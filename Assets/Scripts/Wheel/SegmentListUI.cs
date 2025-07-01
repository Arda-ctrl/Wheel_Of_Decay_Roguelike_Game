using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro namespace
using System.Linq; // LINQ için

// SegmentListUI: Segmentlerin listesini UI'da oluşturan ve seçim işlemini yöneten sınıf.
public class SegmentListUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject segmentButtonPrefab;
    public Transform contentParent;
    public WheelManager wheelManager;
    public GameObject segmentSelectionUI; // UI panelini referans olarak ekle
    
    [Header("Settings")]
    public int numberOfSegmentsToShow = 3; // Gösterilecek segment sayısı

    private SegmentData[] allSegments;

    private void Start()
    {
        segmentSelectionUI?.SetActive(false);
        LoadAllSegments();
        PopulateList();
    }

    private void LoadAllSegments()
    {
        // Resources/Wheel/Segment SO klasöründen tüm SegmentData'ları yükle
        allSegments = Resources.LoadAll<SegmentData>("Wheel/Segment SO");
        
        if (allSegments == null || allSegments.Length == 0)
        {
            Debug.LogError("Hiç segment bulunamadı! Resources/Wheel/Segment SO klasörünü kontrol edin.");
        }
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
        // Mevcut tüm butonları temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateList()
    {
        if (allSegments == null || allSegments.Length == 0) return;

        var randomSegments = allSegments.OrderBy(x => Random.value).Take(numberOfSegmentsToShow).ToArray();

        foreach (var segment in randomSegments)
        {
            CreateSegmentButton(segment);
        }
    }

    private void CreateSegmentButton(SegmentData segment)
    {
        GameObject btnGO = Instantiate(segmentButtonPrefab, contentParent);
        btnGO.name = $"SegmentBtn_{segment.segmentID}";

        // UI elementlerini güncelle
        UpdateButtonUI(btnGO, segment);
        
        // Click event'i ekle
        btnGO.GetComponent<Button>().onClick.AddListener(() => OnSegmentSelected(segment));
    }

    private void UpdateButtonUI(GameObject button, SegmentData segment)
    {
        // Name Text
        if (button.transform.Find("NameText")?.GetComponent<TMP_Text>() is TMP_Text nameText)
        {
            nameText.text = $"{segment.segmentID}\nType: {segment.type} | Rarity: {segment.rarity}";
        }

        // Description Text
        if (button.transform.Find("DescriptionText")?.GetComponent<TMP_Text>() is TMP_Text descText)
        {
            descText.text = segment.description;
        }

        // Icon
        if (button.transform.Find("Icon")?.GetComponent<Image>() is Image iconImage)
        {
            // Prefab'dan sprite'ı al
            var spriteRenderer = segment.segmentPrefab?.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                iconImage.sprite = spriteRenderer.sprite;
                iconImage.color = segment.segmentColor; // Segment rengini uygula
            }
        }

        // Info Text
        if (button.transform.Find("InfoText")?.GetComponent<TMP_Text>() is TMP_Text infoText)
        {
            infoText.text = $"Type: {segment.type} | Rarity: {segment.rarity}";
        }
    }

    private void OnSegmentSelected(SegmentData segment)
    {
        wheelManager.SelectSegmentForPlacement(segment);
        segmentSelectionUI?.SetActive(false);
    }
}
