using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WheelManager : MonoBehaviour
{
    [Header("Slot Ayarları")]
    [SerializeField] private GameObject wheelSlotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] public int slotCount = 36;
    [Header("Çark Döndürme Ayarları")]
    [SerializeField] public float spinDuration = 3f;
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
    private bool isProcessingSelfBondingCurse = false; // SelfBondingCurse processing flag
    private bool isProcessingBondingCurse = false; // BondingCurse processing flag
    
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
        
        // BlurredMemoryCurse kontrolü - segment eklendiğinde tooltip'leri kapat
        if (data.effectType == SegmentEffectType.CurseEffect && data.curseEffectType == CurseEffectType.BlurredMemoryCurse)
        {
            DisableAllTooltips();
        }
        
        // BondingCurse aktivasyonu - segment eklendiğinde bonding yap
        if (data.effectType == SegmentEffectType.CurseEffect && data.curseEffectType == CurseEffectType.BondingCurse)
        {
            ActivateBondingCurse(inst);
        }
        
        // SelfBondingCurse aktivasyonu - segment eklendiğinde self-bonding yap
        if (data.effectType == SegmentEffectType.CurseEffect && data.curseEffectType == CurseEffectType.SelfBondingCurse)
        {
            ActivateSelfBondingCurse(inst);
        }
        
        // Yeni segment eklendikten sonra curse'lerin eksik bond'larını kontrol et
        CheckAndUpdateCurseBonds();
        
        // Persistent segment ise initialize et
        SegmentStatBoostHandler.Instance?.InitializePersistentSegment(inst);
        
        // Segment tamamen yerleştirildikten sonra stat boostları güncelle
        SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
    }
    
    private void DisableAllTooltips()
    {
        // Global tooltip disable flag'ini aktif et
        PlayerPrefs.SetInt("GlobalTooltipDisabled", 1);
        
        // Tüm mevcut segmentlerin tooltip'lerini kapat
        for (int i = 0; i < slotCount; i++)
        {
            if (slotOccupied[i])
            {
                foreach (Transform child in slots[i])
                {
                    var segment = child.GetComponent<SegmentInstance>();
                    if (segment != null && segment.data != null)
                    {
                        segment.data.tooltipDisabled = true;
                    }
                }
            }
        }
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
            // UI Manager'a spin başladığını bildir
            WheelUIManager.Instance?.OnWheelSpinStarted();
            
            isSpinning = true;
            spinCoroutine = StartCoroutine(SpinWheelCoroutine(onSpinComplete));
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
            isSpinning = true;
            spinCoroutine = StartCoroutine(SpinWheelCoroutine_Debug(targetSlot, onSpinComplete));
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
    public IEnumerator SpinEndSequence()
    {
        if (isDestroyed) yield break;
        
        int selectedSlot = GetSlotUnderIndicator();

        // 1. Tüm WheelManipulation ve CurseEffect segmentlerini bul
        List<(SegmentInstance, int)> wheelSegments = new List<(SegmentInstance, int)>();
        List<(SegmentInstance, int)> curseSegments = new List<(SegmentInstance, int)>();
        
        for (int i = 0; i < slotCount && !isDestroyed; i++)
        {
            foreach (Transform child in slots[i])
            {
                if (isDestroyed) yield break;
                if (child == null) continue; // Null check ekle
                
                var inst = child.GetComponent<SegmentInstance>();
                if (inst != null && inst.data != null)
                {
                    if (inst.data.effectType == SegmentEffectType.WheelManipulation)
                    {
                        wheelSegments.Add((inst, inst.startSlotIndex));
                    }
                    else if (inst.data.effectType == SegmentEffectType.CurseEffect)
                    {
                        curseSegments.Add((inst, inst.startSlotIndex));
                    }
                }
            }
        }

        // 2. Tetiklenebilecek tüm WheelManipulation efektlerini topla
        List<(int targetSlot, SegmentData data)> triggeredEffects = new List<(int targetSlot, SegmentData data)>();

        foreach (var (inst, mySlot) in wheelSegments)
        {
            if (isDestroyed) yield break;
            if (inst == null || inst.data == null) continue; // Null check ekle
            
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

        // 3. Tetiklenen WheelManipulation efekt varsa, rastgele birini seç ve uygula
        if (triggeredEffects.Count > 0 && !isDestroyed)
        {
            int randomIndex = Random.Range(0, triggeredEffects.Count);
            int targetSlot = triggeredEffects[randomIndex].targetSlot;
            wheelEffectCoroutine = StartCoroutine(HandleWheelEffectSequence(targetSlot));
            yield break;
        }

        // 4. CurseEffect'leri kontrol et
        bool curseEffectTriggered = false;
        SegmentInstance triggeredCurseSegment = null;
        int triggeredCurseSlot = -1;
        
        foreach (var (inst, mySlot) in curseSegments)
        {
            if (isDestroyed) yield break;
            if (inst == null || inst.data == null) continue; // Null check ekle
            
            var data = inst.data;
            if (SegmentCurseEffectHandler.Instance == null) continue; // Instance null check ekle
            
            bool triggered = SegmentCurseEffectHandler.Instance.HandleCurseEffect(data, selectedSlot, mySlot, slotCount);
            if (triggered)
            {
                curseEffectTriggered = true;
                triggeredCurseSegment = inst;
                triggeredCurseSlot = mySlot;
                break; // İlk tetiklenen curse effect'i kullan
            }
        }

        // 5. CurseEffect tetiklendiyse segment silme
        if (curseEffectTriggered)
        {
            yield return new WaitForSeconds(0.1f); // Kısa bekleme
            
            // SelfBondingCurse segment'i tetiklendiyse, normal akışta silinmesini sağla
            if (triggeredCurseSegment != null && triggeredCurseSegment.data.curseEffectType == CurseEffectType.SelfBondingCurse)
            {
                // SelfBondingCurse processing flag'ini set et
                isProcessingSelfBondingCurse = true;
                
                // SelfBondingCurse segment'ini normal akışta sil
                RemoveSegmentAtSlot(triggeredCurseSlot);
                
                // Flag'i reset et
                isProcessingSelfBondingCurse = false;
            }
            // BondingCurse segment'i tetiklendiyse, normal akışta silinmesini sağla
            else if (triggeredCurseSegment != null && triggeredCurseSegment.data.curseEffectType == CurseEffectType.BondingCurse)
            {
                // BondingCurse processing flag'ini set et
                isProcessingBondingCurse = true;
                
                // BondingCurse segment'ini normal akışta sil
                RemoveSegmentAtSlot(triggeredCurseSlot);
                
                // Flag'i reset et
                isProcessingBondingCurse = false;
            }
            
            // Curse effect'lerden sonra stat boostları yeniden hesapla
            // Özellikle ExplosiveCurse gibi segment yok eden effect'ler için gerekli
            if (SegmentStatBoostHandler.Instance != null)
            {
                // Bir frame bekle, segmentlerin tam olarak yok edilmesi için
                yield return null;
                SegmentStatBoostHandler.Instance.RecalculateAllStatBoosts();
            }
            
            // ReSpin efektleri aktifse geri dönüşü engelle
            if (!isReSpinEffectActive)
            {
                resetCoroutine = StartCoroutine(SmoothResetWheel());
            }
            
            yield break; // Segment silme işlemini durdur
        }
        
        // 5. Hiçbir efekt tetiklenmediyse normal işlem yap
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
                if (inst != null && inst.data != null && inst.gameObject.activeInHierarchy)
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
                        // Bonded segment handling - segment silinmeden önce kontrol et
                        CheckAndHandleBondedSegments(inst);
                        
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
                        // CurseEffect kontrolü - segment silindiğinde tetiklenen effect'ler
                        if (inst.data.effectType == SegmentEffectType.CurseEffect)
                        {
                            // SelfBondingCurse için özel kontrol - sonsuz döngüyü önlemek için
                            if (inst.data.curseEffectType == CurseEffectType.SelfBondingCurse)
                            {
                                // SelfBondingCurse segment'i silindiğinde tekrar curse effect'leri tetikleme
                                // Sadece bonded segment'lerin silinmesi yeterli
                            }
                            // Sadece silinme sırasında tetiklenen effect'ler (ReSpin, RandomEscape, BlurredMemory)
                            else if (inst.data.curseEffectType != CurseEffectType.TeleportEscapeCurse)
                            {
                                // SelfBondingCurse ve BondingCurse processing sırasında tekrar curse effect'leri tetikleme
                                if (!isProcessingSelfBondingCurse && !isProcessingBondingCurse)
                                {
                                    SegmentCurseEffectHandler.Instance.HandleCurseEffect(inst.data, segStart, segStart, slotCount);
                                }
                            }
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
        // PlaceSegment içinde zaten RecalculateAllStatBoosts çağrılıyor, tekrar çağırma
        // SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
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

    // BondingCurse aktivasyonu
    private void ActivateBondingCurse(SegmentInstance curseSegment)
    {
        if (curseSegment == null || curseSegment.data == null) return;
        
        // Çarktaki diğer segmentleri bul (curse segment hariç)
        List<SegmentInstance> availableSegments = new List<SegmentInstance>();
        
        for (int i = 0; i < slotCount; i++)
        {
            if (slotOccupied[i])
            {
                var segment = GetSegmentAtSlot(i);
                if (segment != null && segment != curseSegment)
                {
                    // Zaten bond'lanmış segmentleri hariç tut
                    if (!SegmentCurseEffectHandler.BondingCurseEffect.bondedPairs.ContainsKey(segment) &&
                        !SegmentCurseEffectHandler.BondingCurseEffect.reverseBondedPairs.ContainsKey(segment))
                    {
                        availableSegments.Add(segment);
                    }
                }
            }
        }
        
        // En az 2 segment gerekli (curse hariç)
        if (availableSegments.Count >= 2)
        {
            // Rastgele 2 segment seç
            var segment1 = availableSegments[Random.Range(0, availableSegments.Count)];
            availableSegments.Remove(segment1);
            var segment2 = availableSegments[Random.Range(0, availableSegments.Count)];
            
            // Bond'u oluştur
            SegmentCurseEffectHandler.BondingCurseEffect.bondedPairs[segment1] = segment2;
            SegmentCurseEffectHandler.BondingCurseEffect.reverseBondedPairs[segment2] = segment1;
            
            // Bu bond'u hangi curse segment'inin oluşturduğunu kaydet
            SegmentCurseEffectHandler.BondingCurseEffect.bondCreator[segment1] = curseSegment;
            SegmentCurseEffectHandler.BondingCurseEffect.bondCreator[segment2] = curseSegment;
            
            Debug.Log($"BondingCurse activated: {segment1.data.segmentName} (Slot {segment1.startSlotIndex}) <-> {segment2.data.segmentName} (Slot {segment2.startSlotIndex})");
        }
    }
    
    // Tüm curse'lerin eksik bond'larını kontrol et ve tamamla
    private void CheckAndUpdateCurseBonds()
    {
        // Tüm aktif curse'leri bul
        for (int i = 0; i < slotCount; i++)
        {
            if (slotOccupied[i])
            {
                var segment = GetSegmentAtSlot(i);
                if (segment != null && segment.data.effectType == SegmentEffectType.CurseEffect)
                {
                    if (segment.data.curseEffectType == CurseEffectType.BondingCurse)
                    {
                        // BondingCurse için eksik bond kontrol et
                        CheckBondingCurseBonds(segment);
                    }
                    else if (segment.data.curseEffectType == CurseEffectType.SelfBondingCurse)
                    {
                        // SelfBondingCurse için eksik bond kontrol et
                        CheckSelfBondingCurseBonds(segment);
                    }
                }
            }
        }
    }
    
    // BondingCurse için eksik bond'ları kontrol et
    private void CheckBondingCurseBonds(SegmentInstance curseSegment)
    {
        // Bu curse'ün aktif bond'u var mı kontrol et
        bool hasActiveBond = false;
        
        // bondCreator'da bu curse'ün oluşturduğu bond'ları kontrol et
        foreach (var pair in SegmentCurseEffectHandler.BondingCurseEffect.bondCreator)
        {
            if (pair.Value == curseSegment)
            {
                hasActiveBond = true;
                break;
            }
        }
        
        // Aktif bond yoksa yeni bond oluştur
        if (!hasActiveBond)
        {
            ActivateBondingCurse(curseSegment);
        }
    }
    
    // SelfBondingCurse için eksik bond'ları kontrol et
    private void CheckSelfBondingCurseBonds(SegmentInstance curseSegment)
    {
        // Bu curse'ün mevcut bond sayısını kontrol et
        int currentBondCount = 0;
        if (SegmentCurseEffectHandler.SelfBondingCurseEffect.selfBondedSegments.ContainsKey(curseSegment))
        {
            currentBondCount = SegmentCurseEffectHandler.SelfBondingCurseEffect.selfBondedSegments[curseSegment].Count;
        }
        
        // Hedef bond sayısı
        int targetBondCount = curseSegment.data.selfBondingCount;
        
        // Eksik bond varsa tamamla
        if (currentBondCount < targetBondCount)
        {
            // CreateNewSelfBond metodunu çağır (SelfBondingCurseEffect'ten)
            SegmentCurseEffectHandler.SelfBondingCurseEffect.CreateNewSelfBond(curseSegment);
        }
    }
    
    // SelfBondingCurse aktivasyonu
    private void ActivateSelfBondingCurse(SegmentInstance curseSegment)
    {
        if (curseSegment == null || curseSegment.data == null) return;
        
        int bondingCount = curseSegment.data.selfBondingCount;
        
        // Çarktaki diğer segmentleri bul (curse segment hariç)
        List<SegmentInstance> availableSegments = new List<SegmentInstance>();
        
        for (int i = 0; i < slotCount; i++)
        {
            if (slotOccupied[i])
            {
                var segment = GetSegmentAtSlot(i);
                if (segment != null && segment != curseSegment)
                {
                    // Zaten self-bond'lanmış segmentleri hariç tut
                    if (!SegmentCurseEffectHandler.SelfBondingCurseEffect.reverseSelfBondedSegments.ContainsKey(segment))
                    {
                        availableSegments.Add(segment);
                    }
                }
            }
        }
        
        // Mevcut segment sayısı kadar bond yap
        int actualBondingCount = Mathf.Min(bondingCount, availableSegments.Count);
        
        if (actualBondingCount > 0)
        {
            List<SegmentInstance> bondedSegments = new List<SegmentInstance>();
            
            // Rastgele segmentleri seç ve bond'la
            for (int i = 0; i < actualBondingCount; i++)
            {
                var randomSegment = availableSegments[Random.Range(0, availableSegments.Count)];
                availableSegments.Remove(randomSegment);
                
                bondedSegments.Add(randomSegment);
                SegmentCurseEffectHandler.SelfBondingCurseEffect.reverseSelfBondedSegments[randomSegment] = curseSegment;
            }
            
            // Curse segment'in bond listesini oluştur
            SegmentCurseEffectHandler.SelfBondingCurseEffect.selfBondedSegments[curseSegment] = bondedSegments;
            
            Debug.Log($"SelfBondingCurse activated: {curseSegment.data.segmentName} (Slot {curseSegment.startSlotIndex}) bonded to {actualBondingCount} segments");
            foreach (var bonded in bondedSegments)
            {
                Debug.Log($"  -> {bonded.data.segmentName} (Slot {bonded.startSlotIndex})");
            }
        }
    }
    
    // Bonded segment handling
    private void CheckAndHandleBondedSegments(SegmentInstance segmentToRemove)
    {
        if (segmentToRemove == null || segmentToRemove.gameObject == null || !segmentToRemove.gameObject.activeInHierarchy) return;
        
        // BondingCurse handling
        SegmentCurseEffectHandler.BondingCurseEffect.HandleBondedSegmentRemoval(segmentToRemove);
        
        // SelfBondingCurse handling
        SegmentCurseEffectHandler.SelfBondingCurseEffect.HandleSelfBondedSegmentRemoval(segmentToRemove);
    }
    
    // Slot'taki segmenti bul (helper method)
    private SegmentInstance GetSegmentAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return null;
        if (!slotOccupied[slotIndex]) return null;
        
        foreach (Transform child in slots[slotIndex])
        {
            var segment = child.GetComponent<SegmentInstance>();
            if (segment != null) return segment;
        }
        return null;
    }

}
