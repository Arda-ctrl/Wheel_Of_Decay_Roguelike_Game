using UnityEngine;

// SegmentInstance: Yerleştirilen segmentin görselini ve bilgisini tutar.
public class SegmentInstance : MonoBehaviour
{
    [HideInInspector] public SegmentData data;
    [HideInInspector] public int startSlotIndex;

    private SpriteRenderer sr;
    private const int SegmentOrderInLayer = 10; // Slotlar 0 ise segmentler 10'da olacak

    private float hoverTimer = 0f;
    private bool isHovering = false;
    private const float HOVER_DELAY = 0.5f; // Tooltip gösterilmeden önce bekleme süresi (saniye)

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isHovering)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= HOVER_DELAY && !TooltipUI.Instance.IsVisible)
            {
                ShowTooltip();
            }
        }
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
        if (sr != null && data?.icon != null)
            sr.sprite = data.icon;
    }

    private void ShowTooltip()
    {
        if (data == null || TooltipUI.Instance == null) return;

        string title = data.segmentID;
        string description = data.description;
        string typeAndRarity = $"Type: {data.type} | Rarity: {data.rarity}";

        TooltipUI.Instance.ShowTooltip(title, description, typeAndRarity, Input.mousePosition);
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        hoverTimer = 0f;
    }

    private void OnMouseExit()
    {
        isHovering = false;
        hoverTimer = 0f;
        TooltipUI.Instance?.HideTooltip();
    }
}
