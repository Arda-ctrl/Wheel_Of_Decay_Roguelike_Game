using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WheelManager : MonoBehaviour
{
    [Header("Slot Ayarları")]
    [SerializeField] private GameObject wheelSlotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 36;
    [Header("Çark Döndürme Ayarları")]
    [SerializeField] private float spinDuration = 3f;
    public float SpinDuration => spinDuration;
    [SerializeField] private float minSpinRounds = 3f;
    [SerializeField] private float maxSpinRounds = 6f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private float delayBeforeRemove = 0.5f;
    [SerializeField] private float delayBeforeReset = 0.5f;
    public Transform indicatorTip;
    [HideInInspector] public Transform[] slots;
    [HideInInspector] public bool[] slotOccupied;
    private SegmentData selectedSegmentForPlacement;
    private int selectedSlotForPlacement = -1;
    private GameObject tempSegmentInstance = null;
    public bool isSpinning = false;
    private bool wheelEffectInProgress = false;
    public float lastWheelAngle = 0f;
    
    // ReSpin efektleri çalışırken geri dönüşü engellemek için
    public bool isReSpinEffectActive = false;
    
    // Coroutine referanslarını sakla
    private Coroutine spinCoroutine = null;
    private Coroutine spinEndCoroutine = null;
    private Coroutine wheelEffectCoroutine = null;
    private Coroutine resetCoroutine = null;
    private bool isDestroyed = false;
    
    private IEnumerator HandleWheelEffectSequence(int targetSlot)
    {
        if (isDestroyed) yield break;
        
        wheelEffectInProgress = true;
        
        // İğneyi hedef slota taşı
        float duration = 1f;
        float elapsed = 0f;
        float startAngle = slotParent.localEulerAngles.z;
        float anglePerSlot = 360f / slotCount;
        float targetAngle = (targetSlot * anglePerSlot + 5f);
        targetAngle = ((targetAngle % 360f) + 360f) % 360f;
        
        while (elapsed < duration && !isDestroyed)
        {
            float t = elapsed / duration;
            float easedT = Mathf.SmoothStep(0, 1, t);
            float angle = Mathf.Lerp(startAngle, targetAngle, easedT);
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!isDestroyed)
        {
            slotParent.localEulerAngles = new Vector3(0, 0, targetAngle);
        }
        
        yield return new WaitForSeconds(delayBeforeRemove);
        
        if (!isDestroyed)
        {
            RemoveSegmentAtSlot(targetSlot);
        }
        
        yield return new WaitForSeconds(delayBeforeReset);
        
        if (!isDestroyed)
        {
            resetCoroutine = StartCoroutine(SmoothResetWheel());
        }
        
        wheelEffectInProgress = false;
        isSpinning = false;
        // Stat boost decay/growth güncelle
        SegmentStatBoostHandler.Instance?.OnSpinEnd();
    }
    
    private void Start()
    {
        GenerateSlots();
        lastWheelAngle = slotParent.localEulerAngles.z;
    }
    
    private void OnDestroy()
    {
        isDestroyed = true;
        StopAllCoroutines();
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void GenerateSlots()
    {
        slots = new Transform[slotCount];
        slotOccupied = new bool[slotCount];
        float angleStep = 360f / slotCount;
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(wheelSlotPrefab, slotParent);
            slot.name = "Slot_" + i;
            slot.transform.localRotation = Quaternion.Euler(0f, 0f, -(i * angleStep));
            slots[i] = slot.transform;
            SlotController sc = slot.GetComponent<SlotController>();
            if (sc != null)
            {
                sc.slotIndex = i;
                sc.wheelManager = this;
            }
        }
    }
    public void PlaceSegment(SegmentData data, int slotIndex)
    {
        if (data == null || data.segmentPrefab == null) return;
        int size = data.size;
        int half = size / 2;
        int startSlot = (slotIndex - half + slotCount) % slotCount;
        if (!AreSlotsAvailable(startSlot, size)) return;
        for (int i = 0; i < size; i++)
        {
            int idx = (startSlot + i) % slotCount;
            slotOccupied[idx] = true;
        }
        GameObject go = Instantiate(data.segmentPrefab, slots[startSlot]);
        go.name = $"Segment_{startSlot}_{data.segmentID}";
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        var inst = go.GetComponent<SegmentInstance>();
        if (inst != null)
            inst.Init(data, startSlot);
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = data.segmentColor;
        // Segment yerleştirildikten sonra stat boostları güncelle
        SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
    }
    private bool AreSlotsAvailable(int startIndex, int size)
    {
        if (size > slotCount) return false;
        for (int i = 0; i < size; i++)
        {
            int idx = (startIndex + i) % slotCount;
            if (slotOccupied[idx]) return false;
        }
        return true;
    }
    public void SelectSegmentForPlacement(SegmentData segment)
    {
        selectedSegmentForPlacement = segment;
        selectedSlotForPlacement = -1;
        RemoveTempSegment();
    }
    public void OnSlotClicked(int slotIndex)
    {
        if (selectedSegmentForPlacement == null) return;
        int size = selectedSegmentForPlacement.size;
        int half = size / 2;
        int startSlot = (slotIndex - half + slotCount) % slotCount;
        if (!AreSlotsAvailable(startSlot, size)) return;
        selectedSlotForPlacement = slotIndex;
        ShowTempSegment(selectedSegmentForPlacement, startSlot);
    }
    private void ShowTempSegment(SegmentData data, int slotIndex)
    {
        RemoveTempSegment();
        if (data == null || data.segmentPrefab == null || slotIndex < 0 || slotIndex >= slots.Length) return;
        tempSegmentInstance = Instantiate(data.segmentPrefab, slots[slotIndex]);
        tempSegmentInstance.name = $"TempSegment_{slotIndex}_{data.segmentID}";
        tempSegmentInstance.transform.localPosition = Vector3.zero;
        tempSegmentInstance.transform.localRotation = Quaternion.identity;
        var sr = tempSegmentInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = data.segmentColor;
    }
    private void RemoveTempSegment()
    {
        if (tempSegmentInstance != null)
            Destroy(tempSegmentInstance);
        tempSegmentInstance = null;
    }
    public void ConfirmPlacement()
    {
        if (selectedSegmentForPlacement == null || selectedSlotForPlacement == -1) return;
        PlaceSegment(selectedSegmentForPlacement, selectedSlotForPlacement);
        RemoveTempSegment();
        selectedSegmentForPlacement = null;
        selectedSlotForPlacement = -1;
    }
    public void SpinWheel(System.Action onSpinComplete = null)
    {
        if (!isSpinning && !wheelEffectInProgress && !isDestroyed)
        {
            isSpinning = true;
            spinCoroutine = StartCoroutine(SpinWheelCoroutine(onSpinComplete));
        }
        else
        {
            Debug.LogWarning("[SpinWheel] Çark zaten dönüyor veya efekt devam ediyor!");
        }
    }
    
    // UI Button için parametresiz versiyon
    public void SpinWheelButton()
    {
        SpinWheel();
    }
    private IEnumerator SpinWheelCoroutine(System.Action onSpinComplete = null)
    {
        if (isDestroyed) yield break;
        
        isSpinning = true;
        float duration = spinDuration;
        float elapsed = 0f;
        
        // Mevcut rotasyonu normalize et
        float startAngle = slotParent.localEulerAngles.z;
        startAngle = ((startAngle % 360f) + 360f) % 360f;
        
        float totalRounds = Random.Range(minSpinRounds, maxSpinRounds);
        float anglePerSlot = 360f / slotCount;
        int targetSlot = Random.Range(0, slotCount);
        
        // ReSpin sırasında offset'i sıfırla
        float offset = isReSpinEffectActive ? 0f : 5f;
        float targetAngle = (targetSlot * anglePerSlot + offset);
        targetAngle = ((targetAngle % 360f) + 360f) % 360f;
        
        // Toplam dönüş açısını hesapla
        float totalSpinAngle = (360f * Mathf.Floor(totalRounds)) + targetAngle;
        float endAngle = startAngle + totalSpinAngle;
        

        
        while (elapsed < duration && !isDestroyed)
        {
            float t = elapsed / duration;
            float easedT = spinCurve.Evaluate(t);
            float angle = Mathf.Lerp(startAngle, endAngle, easedT);
            angle = Mathf.Round(angle * 10f) / 10f;
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!isDestroyed)
        {
            // Final açıyı doğru hesapla
            float finalAngle = startAngle + totalSpinAngle;
            finalAngle = ((finalAngle % 360f) + 360f) % 360f;
            slotParent.localEulerAngles = new Vector3(0, 0, finalAngle);
            

            OnSpinEnd();
        }
        isSpinning = false;
        // Stat boost decay/growth güncelle (normal spin için)
        SegmentStatBoostHandler.Instance?.OnSpinEnd();
        onSpinComplete?.Invoke();
    }
    // Sadece debug için: belirli bir slota döndür
    public void SpinWheelForDebug(int targetSlot, System.Action onSpinComplete = null)
    {
        if (!isSpinning && !wheelEffectInProgress && !isDestroyed)
        {
            Debug.Log($"[SpinWheelForDebug] Çark {targetSlot} slotuna döndürülüyor.");
            isSpinning = true;
            spinCoroutine = StartCoroutine(SpinWheelCoroutine_Debug(targetSlot, onSpinComplete));
        }
        else
        {
            Debug.LogWarning("[SpinWheelForDebug] Çark zaten dönüyor veya efekt devam ediyor!");
        }
    }

    private IEnumerator SpinWheelCoroutine_Debug(int forcedTargetSlot, System.Action onSpinComplete = null)
    {
        if (isDestroyed) yield break;
        
        isSpinning = true;
        float duration = spinDuration;
        float elapsed = 0f;
        
        // Mevcut rotasyonu normalize et
        float startAngle = slotParent.localEulerAngles.z;
        startAngle = ((startAngle % 360f) + 360f) % 360f;
        
        float totalRounds = Random.Range(minSpinRounds, maxSpinRounds);
        float anglePerSlot = 360f / slotCount;
        int targetSlot = forcedTargetSlot % slotCount;
        
        // ReSpin sırasında offset'i sıfırla
        float offset = isReSpinEffectActive ? 0f : 5f;
        float targetAngle = (targetSlot * anglePerSlot + offset);
        targetAngle = ((targetAngle % 360f) + 360f) % 360f;
        
        // Toplam dönüş açısını hesapla
        float totalSpinAngle = (360f * Mathf.Floor(totalRounds)) + targetAngle;
        float endAngle = startAngle + totalSpinAngle;
        
        while (elapsed < duration && !isDestroyed)
        {
            float t = elapsed / duration;
            float easedT = spinCurve.Evaluate(t);
            float angle = Mathf.Lerp(startAngle, endAngle, easedT);
            angle = Mathf.Round(angle * 10f) / 10f;
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!isDestroyed)
        {
            // Final açıyı normalize et
            float finalAngle = targetAngle;
            finalAngle = ((finalAngle % 360f) + 360f) % 360f;
            slotParent.localEulerAngles = new Vector3(0, 0, finalAngle);
            OnSpinEnd();
        }
        isSpinning = false;
        // Stat boost decay/growth güncelle (normal spin için)
        SegmentStatBoostHandler.Instance?.OnSpinEnd();
        onSpinComplete?.Invoke();
    }
    private void OnSpinEnd() { spinEndCoroutine = StartCoroutine(SpinEndSequence()); }
    private IEnumerator SpinEndSequence()
    {
        if (isDestroyed) yield break;
        
        int selectedSlot = GetSlotUnderIndicator();

        // 1. Tüm WheelManipulation segmentlerini bul
        List<(SegmentInstance, int)> wheelSegments = new List<(SegmentInstance, int)>();
        for (int i = 0; i < slotCount && !isDestroyed; i++)
        {
            foreach (Transform child in slots[i])
            {
                if (isDestroyed) yield break;
                var inst = child.GetComponent<SegmentInstance>();
                if (inst != null && inst.data != null && inst.data.effectType == SegmentEffectType.WheelManipulation)
                {
                    wheelSegments.Add((inst, inst.startSlotIndex));
                }
            }
        }

        // 2. Tetiklenebilecek tüm efektleri topla
        List<(int targetSlot, SegmentData data)> triggeredEffects = new List<(int targetSlot, SegmentData data)>();

        foreach (var (inst, mySlot) in wheelSegments)
        {
            if (isDestroyed) yield break;
            var data = inst.data;
            var effect = SegmentWheelManipulationHandler.CreateWheelEffect(data);
            if (effect != null)
            {
                bool effectTriggered = false;
                int effectTargetSlot = -1;
                System.Action<int> moveNeedle = (slot) => { effectTargetSlot = slot; effectTriggered = true; };
                effect.OnNeedleLanded(
                    selectedSlot,
                    mySlot,
                    slotCount,
                    moveNeedle,
                    () => { }
                );
                if (effectTriggered && effectTargetSlot >= 0)
                {
                    triggeredEffects.Add((effectTargetSlot, data));
                }
            }
        }

        // 3. Tetiklenen efekt varsa, rastgele birini seç ve uygula
        if (triggeredEffects.Count > 0 && !isDestroyed)
        {
            int randomIndex = Random.Range(0, triggeredEffects.Count);
            int targetSlot = triggeredEffects[randomIndex].targetSlot;
            wheelEffectCoroutine = StartCoroutine(HandleWheelEffectSequence(targetSlot));
            yield break;
        }

        // 4. Hiçbir efekt tetiklenmediyse normal işlem yap
        if (!isDestroyed)
        {
            yield return new WaitForSeconds(delayBeforeRemove);
            RemoveSegmentAtSlot(selectedSlot);
            yield return new WaitForSeconds(delayBeforeReset);
            
            // ReSpin efektleri aktifse geri dönüşü engelle
            if (!isReSpinEffectActive)
            {
                resetCoroutine = StartCoroutine(SmoothResetWheel());
            }
        }
        isSpinning = false;
    }

    // RemoveSegmentAfterEffect fonksiyonunu tamamen kaldır

    public IEnumerator SmoothResetWheel()
    {
        if (isDestroyed) yield break;
        
        yield return new WaitForSeconds(delayBeforeReset);
        float duration = 1f;
        float elapsed = 0f;
        float startAngle = slotParent.localEulerAngles.z;
        float endAngle = 0f;
        while (elapsed < duration && !isDestroyed)
        {
            float t = elapsed / duration;
            float easedT = Mathf.SmoothStep(0, 1, t);
            float angle = Mathf.Lerp(startAngle, endAngle, easedT);
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!isDestroyed)
        {
            slotParent.localEulerAngles = new Vector3(0, 0, endAngle);
        }
        isSpinning = false;
    }
    private int GetSlotUnderIndicator()
    {
        if (indicatorTip == null) return 0;
        
        // Çarkın rotasyonunu al
        float wheelRotation = slotParent.localEulerAngles.z;
        
        // Slot hesaplaması - Floor kullanarak aşağı yuvarla
        float anglePerSlot = 360f / slotCount;
        int selectedSlot = Mathf.FloorToInt(wheelRotation / anglePerSlot) % slotCount;
        
        // Negatif değerleri düzelt
        if (selectedSlot < 0) selectedSlot += slotCount;
        

        
        return selectedSlot;
    }
    public void RemoveSegmentAtSlot(int slotIndex, bool recalcStatBoosts = true)
    {
        int slotCount = slots.Length;
        for (int offset = 0; offset < slotCount; offset++)
        {
            int i = (slotIndex - offset + slotCount) % slotCount;
            Transform slot = slots[i];
            foreach (Transform child in slot)
            {
                if (child == null) continue; // Segment zaten silindiyse atla
                SegmentInstance inst = child.GetComponent<SegmentInstance>();
                if (inst != null && inst.data != null)
                {
                    int segStart = inst.startSlotIndex;
                    int segEnd = (segStart + inst.data.size - 1) % slotCount;
                    int size = inst.data.size;
                    bool inRange = false;
                    if (segStart <= segEnd)
                        inRange = (slotIndex >= segStart && slotIndex <= segEnd);
                    else
                        inRange = (slotIndex >= segStart || slotIndex <= segEnd);
                    if (inRange)
                    {
                        // OnRemoveEffect bilgilerini sakla
                        bool isOnRemoveEffect = inst.data.effectType == SegmentEffectType.OnRemoveEffect;
                        SegmentData onRemoveData = isOnRemoveEffect ? inst.data : null;
                        int onRemoveStartSlot = isOnRemoveEffect ? segStart : -1;

                        // Persistent segment ise yok etme, boost'u artır
                        if (inst.data.effectType == SegmentEffectType.StatBoost && inst.data.statBonusMode == StatBonusMode.Persistent)
                        {
                            inst._currentStatAmount += inst._baseStatAmount;
                            StartCoroutine(DelayedRecalcStatBoosts());
                            return;
                        }
                        // Stat boost segmenti ise, statı silmeden önce sıfırla
                        if (inst.data.effectType == SegmentEffectType.StatBoost && inst._appliedStatBoost != 0f)
                        {
                            StatType statType = inst.data.statType;
                            SegmentStatBoostHandler.Instance.RemoveStat(inst, inst._appliedStatBoost, statType);
                            inst._appliedStatBoost = 0f;
                        }
                        // Random stat stack'i varsa temizle
                        SegmentStatBoostHandler.RemoveAllRandomStatsFor(inst);
                        Destroy(child.gameObject);
                        for (int j = 0; j < size; j++)
                        {
                            int idx = (segStart + j) % slotCount;
                            slotOccupied[idx] = false;
                        }
                        // Segment silindikten ve slotOccupied güncellendikten sonra ödül segmentlerini ekle
                        if (isOnRemoveEffect && onRemoveData != null)
                        {
                            SegmentOnRemoveEffectHandler.Instance.HandleOnRemoveEffect(onRemoveData, onRemoveStartSlot);
                        }
                        // CurseEffect kontrolü
                        if (inst.data.effectType == SegmentEffectType.CurseEffect)
                        {
                            SegmentCurseEffectHandler.Instance.HandleCurseEffect(inst.data, segStart);
                        }
                        // Stat boostları güncellemek için bir frame bekle
                        if (recalcStatBoosts)
                            StartCoroutine(DelayedRecalcStatBoosts());
                        return;
                    }
                }
            }
        }
    }

    private IEnumerator DelayedRecalcStatBoosts()
    {
        yield return null;
        SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
    }

    public void FillWheelWithSegment(SegmentData data)
    {
        if (data == null || data.segmentPrefab == null) return;
        for (int i = 0; i < slotCount; i++)
        {
            PlaceSegment(data, i);
        }
        // FillWheelWithSegment sonrası stat boostları güncelle
        SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
    }

    public void AddSegmentToSlot(SegmentData data, int slotIndex)
    {
        if (data == null || data.segmentPrefab == null) return;
        PlaceSegment(data, slotIndex);
        // AddSegmentToSlot sonrası stat boostları güncelle
        SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
    }

    public void ClearWheel()
    {
        // Tüm segmentleri doğrudan yok et
        for (int i = 0; i < slotCount; i++)
        {
            foreach (Transform child in slots[i])
            {
                if (child != null)
                {
                    var inst = child.GetComponent<SegmentInstance>();
                    if (inst != null && inst.data != null)
                    {
                        // Stat boost segmenti ise, statı silmeden önce sıfırla
                        if (inst.data.effectType == SegmentEffectType.StatBoost && inst._appliedStatBoost != 0f)
                        {
                            StatType statType = inst.data.statType;
                            SegmentStatBoostHandler.Instance.RemoveStat(inst, inst._appliedStatBoost, statType);
                            inst._appliedStatBoost = 0f;
                        }
                        // Random stat stack'i varsa temizle
                        SegmentStatBoostHandler.RemoveAllRandomStatsFor(inst);
                    }
                    Destroy(child.gameObject);
                }
            }
        }
        
        // Tüm slotları boş olarak işaretle
        for (int i = 0; i < slotCount; i++)
        {
            slotOccupied[i] = false;
        }
        
        // Tüm segmentler silindikten sonra stat boostları güncelle
        StartCoroutine(DelayedRecalcStatBoosts());
    }

    private void Update()
    {
        lastWheelAngle = slotParent.localEulerAngles.z;
    }
}
