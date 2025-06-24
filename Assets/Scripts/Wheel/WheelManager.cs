using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [Header("Slot Ayarları")]
    public GameObject wheelSlotPrefab; // Tek dilim prefab’ı
    public Transform slotParent;       // Tüm slotların child’ı olacak obje
    public int slotCount = 36;
        
    public GameObject segmentSelectionUI;

    [Header("Segment Yerleştirme")]
    public GameObject segmentPrefab;   // SegmentInstance prefab’ı
    public SegmentData testSegment;    // Test amaçlı inspector’dan atanacak segment

    [Header("Debug / Test Ayarları")]
    [Tooltip("Test segmentin yerleştirileceği slot indeksi (0 - slotCount-1)")]
    public int selectedSlotIndex = 0;

    [HideInInspector]
    public Transform[] slots;

    [HideInInspector]
    public bool[] slotOccupied;

    private SegmentData selectedSegmentForPlacement;
    private int selectedSlotForPlacement = -1;


    private void Start()
    {
        GenerateSlots();

        // Inspector’dan girilen index ile segmenti yerleştir
        PlaceSegment(testSegment, selectedSlotIndex);
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
            slot.transform.localRotation = Quaternion.Euler(0f, 0f, -i * angleStep);
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
    /// Segmenti belirtilen başlangıç slotuna yerleştirir. Segmentin boyutuna göre birden fazla slot kaplayabilir.
    /// </summary>
    public void PlaceSegment(SegmentData data, int slotIndex)
    {
        if (data == null) return;
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        int size = data.size;

        // Slot dışına taşıyor mu?
        if (slotIndex + size > slotCount)
        {
            Debug.LogWarning("Segment sığmıyor: Slot dışına taşıyor.");
            return;
        }

        // O slotlar boş mu?
        for (int i = 0; i < size; i++)
        {
            if (slotOccupied[slotIndex + i])
            {
                Debug.LogWarning($"Slot {slotIndex + i} zaten dolu. Segment yerleştirilemez.");
                return;
            }
        }

        // Slotları meşgul et
        for (int i = 0; i < size; i++)
        {
            slotOccupied[slotIndex + i] = true;
        }

        // Segmenti yerleştir (görsel olarak sadece ilk slota)
        GameObject go = Instantiate(segmentPrefab, slots[slotIndex]);
        go.name = $"Segment_{slotIndex}_{data.segmentName}";

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var inst = go.GetComponent<SegmentInstance>();
        if (inst != null)
            inst.Init(data, slotIndex);
    }

 // Inspector’dan atanacak Canvas veya panel

    public void SelectSegmentForPlacement(SegmentData segment)
    {
        selectedSegmentForPlacement = segment;
        selectedSlotForPlacement = -1;
        Debug.Log($"Segment seçildi: {segment.segmentName}");

        if (segmentSelectionUI != null)
            segmentSelectionUI.SetActive(false);
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (selectedSegmentForPlacement == null)
        {
            Debug.Log("Önce segment seçmelisin.");
            return;
        }

        int size = selectedSegmentForPlacement.size;

        if (slotIndex + size > slotCount)
        {
            Debug.LogWarning("Segment seçilen slotta sığmıyor.");
            return;
        }

        for (int i = 0; i < size; i++)
        {
            if (slotOccupied[slotIndex + i])
            {
                Debug.LogWarning("Seçilen slotlar dolu.");
                return;
            }
        }

        selectedSlotForPlacement = slotIndex;
        Debug.Log($"Segment yerleştirilecek slot seçildi: {slotIndex}");
    }

    
    public void ConfirmPlacement()
    {
        if (selectedSegmentForPlacement == null || selectedSlotForPlacement == -1)
        {
            Debug.LogWarning("Segment veya slot seçilmedi.");
            return;
        }

        PlaceSegment(selectedSegmentForPlacement, selectedSlotForPlacement);

        selectedSegmentForPlacement = null;
        selectedSlotForPlacement = -1;
    }



}
