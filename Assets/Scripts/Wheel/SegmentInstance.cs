using UnityEngine;
using System.Collections.Generic;

public class SegmentInstance : MonoBehaviour
{
    [HideInInspector] public SegmentData data;
    [HideInInspector] public int startSlotIndex;
    public float _appliedStatBoost = 0f;
    public float _baseStatAmount = 0f; // Persistent için orijinal değer
    public float _currentStatAmount = 0f; // Persistent için runtime boost
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
        _appliedStatBoost = 0f;
        _baseStatAmount = data.statAmount; // ilk değer burada saklanır
        _currentStatAmount = data.statAmount;
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
