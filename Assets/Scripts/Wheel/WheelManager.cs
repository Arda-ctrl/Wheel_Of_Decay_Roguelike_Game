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
    private bool isSpinning = false;
    public Dictionary<string, int> activeEffectCounts = new Dictionary<string, int>();
    private bool wheelEffectInProgress = false;
    public float lastWheelAngle = 0f;
    // slotOffset ve currentWheelAngle değişkenlerini kaldır

    private IEnumerator HandleWheelEffectSequence(int targetSlot)
    {
        wheelEffectInProgress = true;
        isSpinning = true;
        float anglePerSlot = 360f / slotCount;
        float targetAngle = targetSlot * anglePerSlot + 5f;
        float startAngle = slotParent.localEulerAngles.z;
        startAngle = (startAngle % 360f + 360f) % 360f;
        float delta = Mathf.DeltaAngle(startAngle, targetAngle);
        float endAngle = startAngle + delta;
        float duration = spinDuration * 0.5f;
        float elapsed = 0f;
        Debug.Log($"[WheelEffect] İğne {targetSlot} slotuna smooth döndürülüyor. Başlangıç: {startAngle}, Hedef: {targetAngle}, Delta: {delta}");
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = spinCurve.Evaluate(t);
            float angle = Mathf.LerpAngle(startAngle, endAngle, easedT);
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        slotParent.localEulerAngles = new Vector3(0, 0, targetAngle);
        Debug.Log("[WheelEffect] Hedef slota ulaşıldı, bekleniyor...");
        yield return new WaitForSeconds(delayBeforeRemove);
        // --- SİLME TAM BURADA OLMALI ---
        int slot = GetSlotUnderIndicator();
        RemoveSegmentAtSlot(slot);
        yield return new WaitForSeconds(delayBeforeReset);
        Debug.Log("[WheelEffect] Çark sıfırlanıyor...");
        float resetDuration = 1f;
        float resetElapsed = 0f;
        float resetStart = slotParent.localEulerAngles.z;
        float resetEnd = 0f;
        while (resetElapsed < resetDuration)
        {
            float t = resetElapsed / resetDuration;
            float easedT = Mathf.SmoothStep(0, 1, t);
            float angle = Mathf.LerpAngle(resetStart, resetEnd, easedT);
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            resetElapsed += Time.deltaTime;
            yield return null;
        }
        slotParent.localEulerAngles = new Vector3(0, 0, resetEnd);
        Debug.Log("[WheelEffect] Çark sıfırlandı. Efekt tamamlandı.");
        wheelEffectInProgress = false;
        isSpinning = false;
    }
    private void Start()
    {
        GenerateSlots();
        activeEffectCounts = new Dictionary<string, int>();
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
        if (selectedSegmentForPlacement.statAmount != 0)
        {
            string effectId = selectedSegmentForPlacement.segmentID;
            if (!activeEffectCounts.ContainsKey(effectId))
                activeEffectCounts[effectId] = 0;
            activeEffectCounts[effectId]++;
            SegmentStatBoostHandler.Instance.ActivateSegmentEffect(selectedSegmentForPlacement, 1);
        }
        RemoveTempSegment();
        selectedSegmentForPlacement = null;
        selectedSlotForPlacement = -1;
    }
    public void SpinWheel()
    {
        if (!isSpinning && !wheelEffectInProgress)
        {
            Debug.Log("[SpinWheel] Çark döndürülüyor.");
            isSpinning = true;
            StartCoroutine(SpinWheelCoroutine());
        }
        else
        {
            Debug.LogWarning("[SpinWheel] Çark zaten dönüyor veya efekt devam ediyor!");
        }
    }
    private IEnumerator SpinWheelCoroutine()
    {
        isSpinning = true;
        float duration = spinDuration;
        float elapsed = 0f;
        float startAngle = slotParent.localEulerAngles.z;
        float totalRounds = Random.Range(minSpinRounds, maxSpinRounds);
        float anglePerSlot = 360f / slotCount;
        int targetSlot = Random.Range(0, slotCount);
        float slotAngle = Mathf.Round((targetSlot * anglePerSlot + 5f) * 10f) / 10f;
        float endAngle = startAngle + (360f * Mathf.Floor(totalRounds)) + slotAngle;
        endAngle = Mathf.Round(endAngle * 10f) / 10f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = spinCurve.Evaluate(t);
            float angle = Mathf.Lerp(startAngle, endAngle, easedT);
            angle = Mathf.Round(angle * 10f) / 10f;
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        float finalAngle = (targetSlot * anglePerSlot + 5f);
        finalAngle = Mathf.Round(finalAngle * 10f) / 10f;
        finalAngle = ((finalAngle % 360f) + 360f) % 360f;
        slotParent.localEulerAngles = new Vector3(0, 0, finalAngle);
        OnSpinEnd();
    }
    // Sadece debug için: belirli bir slota döndür
    public void SpinWheelForDebug(int targetSlot)
    {
        if (!isSpinning && !wheelEffectInProgress)
        {
            Debug.Log($"[SpinWheelForDebug] Çark {targetSlot} slotuna döndürülüyor.");
            isSpinning = true;
            StartCoroutine(SpinWheelCoroutine_Debug(targetSlot));
        }
        else
        {
            Debug.LogWarning("[SpinWheelForDebug] Çark zaten dönüyor veya efekt devam ediyor!");
        }
    }

    private IEnumerator SpinWheelCoroutine_Debug(int forcedTargetSlot)
    {
        isSpinning = true;
        float duration = spinDuration;
        float elapsed = 0f;
        float startAngle = slotParent.localEulerAngles.z;
        float totalRounds = Random.Range(minSpinRounds, maxSpinRounds);
        float anglePerSlot = 360f / slotCount;
        int targetSlot = forcedTargetSlot % slotCount;
        float slotAngle = Mathf.Round((targetSlot * anglePerSlot + 5f) * 10f) / 10f;
        float endAngle = startAngle + (360f * Mathf.Floor(totalRounds)) + slotAngle;
        endAngle = Mathf.Round(endAngle * 10f) / 10f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = spinCurve.Evaluate(t);
            float angle = Mathf.Lerp(startAngle, endAngle, easedT);
            angle = Mathf.Round(angle * 10f) / 10f;
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        float finalAngle = (targetSlot * anglePerSlot + 5f);
        finalAngle = Mathf.Round(finalAngle * 10f) / 10f;
        finalAngle = ((finalAngle % 360f) + 360f) % 360f;
        slotParent.localEulerAngles = new Vector3(0, 0, finalAngle);
        OnSpinEnd();
    }
    private void OnSpinEnd() { StartCoroutine(SpinEndSequence()); }
    private IEnumerator SpinEndSequence()
    {
        int selectedSlot = GetSlotUnderIndicator();
        Debug.Log($"[SpinEnd] İğne hangi slota geldi: {selectedSlot}");

        // 1. Tüm WheelManipulation segmentlerini bul
        List<(SegmentInstance, int)> wheelSegments = new List<(SegmentInstance, int)>();
        for (int i = 0; i < slotCount; i++)
        {
            foreach (Transform child in slots[i])
            {
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
            var data = inst.data;
            SegmentWheelManipulationHandler.IWheelEffect effect = null;
            if (data.wheelManipulationType == WheelManipulationType.BlackHole)
            {
                effect = new SegmentWheelManipulationHandler.BlackHoleEffect(data.blackHoleRange);
            }
            else if (data.wheelManipulationType == WheelManipulationType.Redirector)
            {
                effect = new SegmentWheelManipulationHandler.RedirectorEffect(data.redirectDirection);
            }
            else if (data.wheelManipulationType == WheelManipulationType.Repulsor)
            {
                effect = new SegmentWheelManipulationHandler.RepulsorEffect(data.repulsorRange);
            }
            else if (data.wheelManipulationType == WheelManipulationType.MirrorRedirect)
            {
                effect = new SegmentWheelManipulationHandler.MirrorRedirectEffect();
            }
            else if (data.wheelManipulationType == WheelManipulationType.CommonRedirector)
            {
                effect = new SegmentWheelManipulationHandler.CommonRedirectorEffect(
                    data.commonRedirectorRange,
                    data.commonRedirectorMinRarity,
                    data.commonRedirectorMaxRarity
                );
            }
            else if (data.wheelManipulationType == WheelManipulationType.SafeEscape)
            {
                effect = new SegmentWheelManipulationHandler.SafeEscapeEffect(data.safeEscapeRange);
            }
            else if (data.wheelManipulationType == WheelManipulationType.ExplosiveEscape)
            {
                effect = new SegmentWheelManipulationHandler.ExplosiveEscapeEffect(data.explosiveEscapeRange);
            }
            else if (data.wheelManipulationType == WheelManipulationType.SegmentSwapper)
            {
                effect = new SegmentWheelManipulationHandler.SegmentSwapperEffect(data.swapperRange);
            }
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
        if (triggeredEffects.Count > 0)
        {
            int randomIndex = Random.Range(0, triggeredEffects.Count);
            int targetSlot = triggeredEffects[randomIndex].targetSlot;
            StartCoroutine(HandleWheelEffectSequence(targetSlot));
            yield break;
        }

        // 4. Hiçbir efekt tetiklenmediyse normal işlem yap
        yield return new WaitForSeconds(delayBeforeRemove);
        RemoveSegmentAtSlot(selectedSlot);
        yield return new WaitForSeconds(delayBeforeReset);
        StartCoroutine(SmoothResetWheel());
        isSpinning = false;
    }

    // RemoveSegmentAfterEffect fonksiyonunu tamamen kaldır

    private IEnumerator SmoothResetWheel()
    {
        yield return new WaitForSeconds(delayBeforeReset);
        float duration = 1f;
        float elapsed = 0f;
        float startAngle = slotParent.localEulerAngles.z;
        float endAngle = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = Mathf.SmoothStep(0, 1, t);
            float angle = Mathf.Lerp(startAngle, endAngle, easedT);
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        slotParent.localEulerAngles = new Vector3(0, 0, endAngle);
        isSpinning = false;
    }
    private int GetSlotUnderIndicator()
    {
        if (indicatorTip == null) return 0;
        float minDist = float.MaxValue;
        int closestSlot = 0;
        Vector3 tipPos = indicatorTip.position;
        for (int i = 0; i < slots.Length; i++)
        {
            Transform center = slots[i].Find("SlotCenter");
            Vector3 centerPos = center != null ? center.position : slots[i].position;
            float dist = Vector3.Distance(tipPos, centerPos);
            if (dist < minDist)
            {
                minDist = dist;
                closestSlot = i;
            }
        }
        return closestSlot;
    }
    public void RemoveSegmentAtSlot(int slotIndex)
    {
        Debug.Log($"[RemoveSegmentAtSlot] Slot: {slotIndex} siliniyor... Açı: {slotParent.localEulerAngles.z}");
        int maxSize = 3;
        for (int offset = maxSize - 1; offset >= 0; offset--)
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

                        Debug.Log($"[RemoveSegmentAtSlot] Segment siliniyor: {inst.data.segmentID}, StartSlot: {segStart}, Size: {size}");
                        if (inst.data.effectType == SegmentEffectType.StatBoost && inst.data.statAmount != 0)
                        {
                            string effectId = inst.data.segmentID;
                            if (activeEffectCounts.ContainsKey(effectId))
                            {
                                SegmentStatBoostHandler.Instance.DeactivateSegmentEffect(inst.data, 1);
                                activeEffectCounts[effectId]--;
                                if (activeEffectCounts[effectId] <= 0)
                                    activeEffectCounts.Remove(effectId);
                            }
                        }
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
                        return;
                    }
                }
            }
        }
    }

    public void FillWheelWithSegment(SegmentData data)
    {
        if (data == null || data.segmentPrefab == null) return;
        for (int i = 0; i < slotCount; i++)
        {
            PlaceSegment(data, i);
            if (data.statAmount != 0)
            {
                string effectId = data.segmentID;
                if (!activeEffectCounts.ContainsKey(effectId))
                    activeEffectCounts[effectId] = 0;
                activeEffectCounts[effectId]++;
                SegmentStatBoostHandler.Instance.ActivateSegmentEffect(data, 1);
            }
        }
    }

    public void AddSegmentToSlot(SegmentData data, int slotIndex)
    {
        if (data == null || data.segmentPrefab == null) return;
        PlaceSegment(data, slotIndex);
        if (data.statAmount != 0)
        {
            string effectId = data.segmentID;
            if (!activeEffectCounts.ContainsKey(effectId))
                activeEffectCounts[effectId] = 0;
            activeEffectCounts[effectId]++;
            SegmentStatBoostHandler.Instance.ActivateSegmentEffect(data, 1);
        }
    }

    public void ClearWheel()
    {
        for (int i = 0; i < slotCount; i++)
        {
            RemoveSegmentAtSlot(i);
        }
        activeEffectCounts.Clear();
    }

    private void Update()
    {
        lastWheelAngle = slotParent.localEulerAngles.z;
    }
}
