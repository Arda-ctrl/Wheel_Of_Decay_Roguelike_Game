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
            SegmentEffectHandler.Instance.ActivateSegmentEffect(selectedSegmentForPlacement, 1);
        }
        RemoveTempSegment();
        selectedSegmentForPlacement = null;
        selectedSlotForPlacement = -1;
    }
    public void SpinWheel()
    {
        if (!isSpinning)
            StartCoroutine(SpinWheelCoroutine());
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
    private void OnSpinEnd() { StartCoroutine(SpinEndSequence()); }
    private IEnumerator SpinEndSequence()
    {
        int selectedSlot = GetSlotUnderIndicator();
        yield return new WaitForSeconds(delayBeforeRemove);
        RemoveSegmentAtSlot(selectedSlot);
        StartCoroutine(SmoothResetWheel());
    }
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
    private void RemoveSegmentAtSlot(int slotIndex)
    {
        int maxSize = 3;
        for (int offset = maxSize - 1; offset >= 0; offset--)
        {
            int i = (slotIndex - offset + slotCount) % slotCount;
            Transform slot = slots[i];
            foreach (Transform child in slot)
            {
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
                        if (inst.data.statAmount != 0)
                        {
                            string effectId = inst.data.segmentID;
                            if (activeEffectCounts.ContainsKey(effectId))
                            {
                                SegmentEffectHandler.Instance.DeactivateSegmentEffect(inst.data, 1);
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
                        return;
                    }
                }
            }
        }
    }
}
