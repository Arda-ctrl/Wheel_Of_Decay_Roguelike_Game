using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }
    [Header("References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text typeAndRarityText;
    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(20f, 20f);
    [SerializeField] private float padding = 10f;
    public bool IsVisible => tooltipPanel.activeSelf;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        HideTooltip();
    }
    public void ShowTooltip(string title, string description, string typeAndRarity, Vector2 position)
    {
        tooltipPanel.SetActive(true);
        titleText.text = title;
        descriptionText.text = description;
        typeAndRarityText.text = typeAndRarity;
        PositionTooltip(position);
    }
    private void PositionTooltip(Vector2 position)
    {
        Vector2 tooltipPosition = position + offset;
        RectTransform panelRect = tooltipPanel.GetComponent<RectTransform>();
        if (tooltipPosition.x + panelRect.rect.width > Screen.width)
            tooltipPosition.x = Screen.width - panelRect.rect.width - padding;
        if (tooltipPosition.y + panelRect.rect.height > Screen.height)
            tooltipPosition.y = Screen.height - panelRect.rect.height - padding;
        tooltipPanel.transform.position = tooltipPosition;
    }
    public void HideTooltip() { tooltipPanel?.SetActive(false); }
} 