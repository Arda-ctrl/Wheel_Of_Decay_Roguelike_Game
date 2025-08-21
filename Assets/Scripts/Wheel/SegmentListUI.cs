using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SegmentListUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject segmentButtonPrefab;
    public Transform contentParent;
    public WheelManager wheelManager;
    public GameObject segmentSelectionUI;
    [Header("Settings")]
    public int numberOfSegmentsToShow = 3;
    private SegmentData[] allSegments;
    private void Start()
    {
        segmentSelectionUI?.SetActive(false);
        LoadAllSegments();
        PopulateList();
    }
    private void LoadAllSegments()
    {
        allSegments = Resources.LoadAll<SegmentData>("Wheel/Segment SO");
        if (allSegments == null || allSegments.Length == 0)
            Debug.LogError("Hiç segment bulunamadı! Resources/Wheel/Segment SO klasörünü kontrol edin.");
    }
    public void ShowSegmentSelectionUI()
    {
        if (segmentSelectionUI != null)
        {
            segmentSelectionUI.SetActive(true);
            ClearList();
            PopulateList();
        }
    }
    private void ClearList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
    }
    private void PopulateList()
    {
        if (allSegments == null || allSegments.Length == 0) return;
        var randomSegments = allSegments.OrderBy(x => Random.value).Take(numberOfSegmentsToShow).ToArray();
        foreach (var segment in randomSegments)
            CreateSegmentButton(segment);
    }
    private void CreateSegmentButton(SegmentData segment)
    {
        GameObject btnGO = Instantiate(segmentButtonPrefab, contentParent);
        btnGO.name = $"SegmentBtn_{segment.segmentID}";
        UpdateButtonUI(btnGO, segment);
        btnGO.GetComponent<Button>().onClick.AddListener(() => OnSegmentSelected(segment));
    }
    private void UpdateButtonUI(GameObject button, SegmentData segment)
    {
        if (button.transform.Find("NameText")?.GetComponent<TMP_Text>() is TMP_Text nameText)
            nameText.text = $"{segment.segmentID}\nType: {segment.type} | Rarity: {segment.rarity}";
        if (button.transform.Find("DescriptionText")?.GetComponent<TMP_Text>() is TMP_Text descText)
            descText.text = segment.description;
        if (button.transform.Find("Icon")?.GetComponent<Image>() is Image iconImage)
        {
            var spriteRenderer = segment.segmentPrefab?.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                iconImage.sprite = spriteRenderer.sprite;
                iconImage.color = segment.segmentColor;
            }
        }
        if (button.transform.Find("InfoText")?.GetComponent<TMP_Text>() is TMP_Text infoText)
            infoText.text = $"Type: {segment.type} | Rarity: {segment.rarity}";
    }
    private void OnSegmentSelected(SegmentData segment)
    {
        wheelManager.SelectSegmentForPlacement(segment);
        segmentSelectionUI?.SetActive(false);
        
        // UI Manager'a segment seçildiğini bildir
        WheelUIManager.Instance?.OnSegmentSelected();
    }
}
