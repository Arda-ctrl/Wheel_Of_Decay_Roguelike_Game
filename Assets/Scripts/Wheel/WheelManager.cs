// WheelManager.cs
// Çark üzerindeki slotları ve segment yerleştirmeyi yöneten ana sınıf.
using UnityEngine;
using System.Collections;

public class WheelManager : MonoBehaviour
{
    [Header("Slot Ayarları")]
    [SerializeField] private GameObject wheelSlotPrefab; // Tek dilim prefab'ı
    [SerializeField] private Transform slotParent;       // Tüm slotların child'ı olacak obje
    [SerializeField] private int slotCount = 36;
        
    [Header("Segment Yerleştirme")]
    [SerializeField] private GameObject segmentPrefab;   // SegmentInstance prefab'ı
    [SerializeField] private SegmentData testSegment;    // Test amaçlı inspector'dan atanacak segment

    [Header("Çark Döndürme Ayarları")]
    [SerializeField] private float spinDuration = 3f; // Dönüş süresi (saniye)
    [SerializeField] private float minSpinRounds = 3f; // Minimum tam tur
    [SerializeField] private float maxSpinRounds = 6f; // Maksimum tam tur
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0,0,1,1); // Hız eğrisi
    [SerializeField] private float delayBeforeRemove = 0.5f; // Segment silinmeden önce bekleme süresi
    [SerializeField] private float delayBeforeReset = 0.5f; // Çark eski haline dönmeden önce bekleme süresi
    public Transform indicatorTip; // İğnenin ucu referansı

    [HideInInspector]
    public Transform[] slots;

    [HideInInspector]
    public bool[] slotOccupied;

    private SegmentData selectedSegmentForPlacement;
    private int selectedSlotForPlacement = -1;

    private GameObject tempSegmentInstance = null; // Geçici segment görseli

    private bool isSpinning = false;

    private void Start()
    {
        GenerateSlots();
        // Test amaçlı otomatik segment yerleştirme kaldırıldı.
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

            // SlotController ayarla
            SlotController sc = slot.GetComponent<SlotController>();
            if (sc != null)
            {
                sc.slotIndex = i;
                sc.wheelManager = this;
            }
        }
    }

    /// <summary>
    /// Belirtilen segmenti, slotIndex'ten başlayarak uygun slotlara yerleştirir.
    /// </summary>
    public void PlaceSegment(SegmentData data, int slotIndex)
    {
        if (data == null) return;
        int size = data.size;
        int half = size / 2;
        int startSlot = (slotIndex - half + slotCount) % slotCount;
        if (!AreSlotsAvailable(startSlot, size)) return;
        for (int i = 0; i < size; i++)
        {
            int idx = (startSlot + i) % slotCount;
            slotOccupied[idx] = true;
        }
        GameObject go = Instantiate(segmentPrefab, slots[startSlot]);
        go.name = $"Segment_{startSlot}_{data.segmentID}";
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        var inst = go.GetComponent<SegmentInstance>();
        if (inst != null)
            inst.Init(data, startSlot);
    }

    /// <summary>
    /// Belirtilen slot aralığı segment yerleşimi için uygun mu?
    /// </summary>
    private bool AreSlotsAvailable(int startIndex, int size)
    {
        if (size > slotCount) return false;
        for (int i = 0; i < size; i++)
        {
            int idx = (startIndex + i) % slotCount;
            if (slotOccupied[idx])
            {
                Debug.LogWarning($"Slot {idx} zaten dolu. Segment yerleştirilemez.");
                return false;
            }
        }
        return true;
    }

    // Inspector'dan atanacak Canvas veya panel

    public void SelectSegmentForPlacement(SegmentData segment)
    {
        selectedSegmentForPlacement = segment;
        selectedSlotForPlacement = -1;
        Debug.Log($"Segment seçildi: {segment.segmentID}");
        RemoveTempSegment(); // Önceki geçici segmenti kaldır
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (selectedSegmentForPlacement == null)
        {
            Debug.Log("Önce segment seçmelisin.");
            return;
        }
        int size = selectedSegmentForPlacement.size;
        int half = size / 2;
        int startSlot = (slotIndex - half + slotCount) % slotCount;
        if (!AreSlotsAvailable(startSlot, size))
            return;
        selectedSlotForPlacement = slotIndex;
        Debug.Log($"Segment yerleştirilecek slot seçildi: {slotIndex} (başlangıç: {startSlot})");
        ShowTempSegment(selectedSegmentForPlacement, startSlot);
    }

    private void ShowTempSegment(SegmentData data, int slotIndex)
    {
        RemoveTempSegment();
        if (data == null || slotIndex < 0 || slotIndex >= slots.Length) return;
        tempSegmentInstance = Instantiate(segmentPrefab, slots[slotIndex]);
        tempSegmentInstance.name = $"TempSegment_{slotIndex}_{data.segmentID}";
        tempSegmentInstance.transform.localPosition = Vector3.zero;
        tempSegmentInstance.transform.localRotation = Quaternion.identity;
        var inst = tempSegmentInstance.GetComponent<SegmentInstance>();
        if (inst != null)
            inst.Init(data, slotIndex);
        // Şeffaflık kaldırıldı, segment tam opak olacak
    }

    private void RemoveTempSegment()
    {
        if (tempSegmentInstance != null)
            Destroy(tempSegmentInstance);
        tempSegmentInstance = null;
    }

    public void ConfirmPlacement()
    {
        if (selectedSegmentForPlacement == null || selectedSlotForPlacement == -1)
        {
            Debug.LogWarning("Segment veya slot seçilmedi.");
            return;
        }
        // Kalıcı olarak yerleştir
        PlaceSegment(selectedSegmentForPlacement, selectedSlotForPlacement);
        RemoveTempSegment();
        selectedSegmentForPlacement = null;
        selectedSlotForPlacement = -1;
    }

    public void SpinWheel()
    {
        if (!isSpinning)
            StartCoroutine(SpinWheelCoroutine());
    }

    private System.Collections.IEnumerator SpinWheelCoroutine()
    {
        isSpinning = true;
        float duration = spinDuration;
        float elapsed = 0f;
        float startAngle = slotParent.localEulerAngles.z;
        float totalRounds = Random.Range(minSpinRounds, maxSpinRounds);
        float anglePerSlot = 360f / slotCount;
        int targetSlot = Random.Range(0, slotCount); // Rastgele bir slot seç
        
        // Hedef açıyı tam sayıya yuvarlayalım
        float slotAngle = Mathf.Round((targetSlot * anglePerSlot + 5f) * 10f) / 10f; // 0.1 hassasiyetle yuvarlama
        float endAngle = startAngle + (360f * Mathf.Floor(totalRounds)) + slotAngle;
        endAngle = Mathf.Round(endAngle * 10f) / 10f; // 0.1 hassasiyetle yuvarlama

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = spinCurve.Evaluate(t);
            float angle = Mathf.Lerp(startAngle, endAngle, easedT);
            // Animasyon sırasında da açıyı yuvarla
            angle = Mathf.Round(angle * 10f) / 10f; // 0.1 hassasiyetle yuvarlama
            slotParent.localEulerAngles = new Vector3(0, 0, angle);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Animasyon sonunda kesinlikle hedef açıda olalım
        float finalAngle = (targetSlot * anglePerSlot + 5f);
        finalAngle = Mathf.Round(finalAngle * 10f) / 10f; // 0.1 hassasiyetle yuvarlama
        // 360'a göre modulo al
        finalAngle = ((finalAngle % 360f) + 360f) % 360f;
        
        slotParent.localEulerAngles = new Vector3(0, 0, finalAngle);
        
        isSpinning = false;
        OnSpinEnd();
    }

    private void OnSpinEnd()
    {
        StartCoroutine(SpinEndSequence());
    }

    private IEnumerator SpinEndSequence()
    {
        int selectedSlot = GetSlotUnderIndicator();
        yield return new WaitForSeconds(delayBeforeRemove);
        RemoveSegmentAtSlot(selectedSlot);
        StartCoroutine(SmoothResetWheel());
    }

    private IEnumerator SmoothResetWheel()
    {
        yield return new WaitForSeconds(delayBeforeReset); // Çark eski haline dönmeden önce bekle
        float duration = 1f; // Sıfırlama animasyon süresi
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
    }

    private int GetSlotUnderIndicator()
    {
        if (indicatorTip == null)
        {
            Debug.LogWarning("indicatorTip atanmadı!");
            return 0;
        }
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
        int maxSize = 3; // En büyük segment boyutu
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
                    // Dairesel aralık kontrolü
                    bool inRange = false;
                    if (segStart <= segEnd)
                        inRange = (slotIndex >= segStart && slotIndex <= segEnd);
                    else
                        inRange = (slotIndex >= segStart || slotIndex <= segEnd);
                    if (inRange)
                    {
                        Destroy(child.gameObject);
                        for (int j = 0; j < size; j++)
                        {
                            int idx = (segStart + j) % slotCount;
                            slotOccupied[idx] = false;
                        }
                        Debug.Log($"Slot {slotIndex} segmentin parçasıydı, segment {segStart}-{segEnd} silindi.");
                        return;
                    }
                }
            }
        }
        Debug.Log($"Slot {slotIndex} zaten boş.");
    }
}
