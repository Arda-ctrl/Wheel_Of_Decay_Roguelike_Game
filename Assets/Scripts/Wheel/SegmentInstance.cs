using UnityEngine;

// SegmentInstance: Yerleştirilen segmentin görselini ve bilgisini tutar.
public class SegmentInstance : MonoBehaviour
{
    [HideInInspector] public SegmentData data;
    [HideInInspector] public int startSlotIndex;

    private SpriteRenderer sr;
    private const int SegmentOrderInLayer = 10; // Slotlar 0 ise segmentler 10'da olacak

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// SegmentData ve başlangıç slotunu ata, görseli güncelle.
    /// </summary>
    public void Init(SegmentData data, int slotIndex)
    {
        this.data = data;
        this.startSlotIndex = slotIndex;
        UpdateVisual();
        if (sr != null)
            sr.sortingOrder = SegmentOrderInLayer;
    }

    private void UpdateVisual()
    {
        if (sr != null && data != null && data.icon != null)
            sr.sprite = data.icon;
    }
}
