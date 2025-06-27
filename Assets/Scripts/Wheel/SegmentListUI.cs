using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro namespace
using System.Linq; // LINQ için

// SegmentListUI: Segmentlerin listesini UI'da oluşturan ve seçim işlemini yöneten sınıf.
public class SegmentListUI : MonoBehaviour
{
    public GameObject segmentButtonPrefab;
    public Transform contentParent;
    public WheelManager wheelManager;
    public GameObject segmentSelectionUI; // UI panelini referans olarak ekle
    public int numberOfSegmentsToShow = 3; // Gösterilecek segment sayısı

    private SegmentData[] allSegments;

    private void Start()
    {
        if (segmentSelectionUI != null)
            segmentSelectionUI.SetActive(false); // Başlangıçta kapalı olsun
        
        LoadAllSegments();
        PopulateList();
    }

    void LoadAllSegments()
    {
        // Resources/Wheel/Segment SO klasöründen tüm SegmentData'ları yükle
        allSegments = Resources.LoadAll<SegmentData>("Wheel/Segment SO");
        
        if (allSegments == null || allSegments.Length == 0)
        {
            Debug.LogError("Hiç segment bulunamadı! Resources/Wheel/Segment SO klasörünü kontrol edin.");
            return;
        }
    }

    public void ShowSegmentSelectionUI()
    {
        if (segmentSelectionUI != null)
        {
            segmentSelectionUI.SetActive(true);
            // UI açıldığında yeni rastgele segmentler seç
            ClearList();
            PopulateList();
        }
    }

    void ClearList()
    {
        // Mevcut tüm butonları temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    void PopulateList()
    {
        if (allSegments == null || allSegments.Length == 0) return;

        // Tüm segmentlerden rastgele 3 tanesini seç
        var randomSegments = allSegments.OrderBy(x => Random.value).Take(numberOfSegmentsToShow).ToArray();

        foreach (var segment in randomSegments)
        {
            GameObject btnGO = Instantiate(segmentButtonPrefab, contentParent);
            btnGO.name = $"SegmentBtn_{segment.segmentID}";

            TMP_Text textComp = btnGO.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = segment.segmentID;

            Image iconImage = btnGO.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
                iconImage.sprite = segment.icon;

            btnGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                wheelManager.SelectSegmentForPlacement(segment);
                if (segmentSelectionUI != null)
                    segmentSelectionUI.SetActive(false); // Seçimden sonra paneli kapat
            });
        }
    }
}
