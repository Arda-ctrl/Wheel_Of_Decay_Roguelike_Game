using UnityEngine;

public class SegmentInstance : MonoBehaviour
{
    [HideInInspector] public SegmentData data;
    [HideInInspector] public int startSlotIndex;

    private SpriteRenderer sr;

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
        if (sr != null && data.icon != null)
            sr.sprite = data.icon;
    }
}
