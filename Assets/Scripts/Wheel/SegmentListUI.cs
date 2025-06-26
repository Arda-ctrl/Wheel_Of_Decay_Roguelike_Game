using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro namespace

// SegmentListUI: Segmentlerin listesini UI'da oluşturan ve seçim işlemini yöneten sınıf.
public class SegmentListUI : MonoBehaviour
{
    public GameObject segmentButtonPrefab;
    public Transform contentParent;
    public WheelManager wheelManager;
    public SegmentData[] allSegments;
    public GameObject segmentSelectionUI; // UI panelini referans olarak ekle

    private void Start()
    {
        if (segmentSelectionUI != null)
            segmentSelectionUI.SetActive(false); // Başlangıçta kapalı olsun
        PopulateList();
    }

    public void ShowSegmentSelectionUI()
    {
        if (segmentSelectionUI != null)
            segmentSelectionUI.SetActive(true);
    }

    void PopulateList()
    {
        foreach (var segment in allSegments)
        {
            GameObject btnGO = Instantiate(segmentButtonPrefab, contentParent);
            btnGO.name = $"SegmentBtn_{segment.segmentName}";

            TMP_Text textComp = btnGO.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = segment.segmentName;

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
