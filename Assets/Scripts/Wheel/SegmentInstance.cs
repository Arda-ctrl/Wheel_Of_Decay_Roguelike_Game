using UnityEngine;

public class SegmentInstance : MonoBehaviour
{
    [HideInInspector] public SegmentData data;
    [HideInInspector] public int startSlotIndex;
    private SpriteRenderer sr;
    private const int SegmentOrderInLayer = 10;
    private float hoverTimer = 0f;
    private bool isHovering = false;
    private const float HOVER_DELAY = 0.5f;
    private void Awake() { sr = GetComponent<SpriteRenderer>(); }
    private void Update()
    {
        if (isHovering)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= HOVER_DELAY && !TooltipUI.Instance.IsVisible)
                ShowTooltip();
        }
    }
    public void Init(SegmentData data, int slotIndex)
    {
        this.data = data;
        this.startSlotIndex = slotIndex;
        if (sr != null)
            sr.sortingOrder = SegmentOrderInLayer;
    }
    private void ShowTooltip()
    {
        if (data == null || TooltipUI.Instance == null) return;
        string title = data.segmentID;
        string description = data.description;
        string typeAndRarity = $"Type: {data.type} | Rarity: {data.rarity}";
        TooltipUI.Instance.ShowTooltip(title, description, typeAndRarity, Input.mousePosition);
    }
    private void OnMouseEnter() { isHovering = true; hoverTimer = 0f; }
    private void OnMouseExit() { isHovering = false; hoverTimer = 0f; TooltipUI.Instance?.HideTooltip(); }
}
