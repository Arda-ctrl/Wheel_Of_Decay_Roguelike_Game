using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // DOTween için gerekli

public class SegmentPlacementManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject segmentPreviewPanel;
    [SerializeField] private Image segmentPreviewImage;
    [SerializeField] private Text segmentNameText;
    [SerializeField] private Text segmentDescriptionText;
    
    [Header("Placement Settings")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private float previewAlpha = 0.5f;
    [SerializeField] private LayerMask wheelLayerMask = -1; // -1 = tüm layerlar
    
    // Current state
    private PrizeSegment currentSegment;
    private WheelManager wheelManager;
    private Camera mainCamera;
    private GameObject previewSegmentInstance;
    private bool isInPreviewMode = false;
    private int hoveredSlotIndex = -1;
    
    // Tooltip tarzı açıklama için metin
    private readonly string tooltipInstructions = "Çarkın üzerine gelerek segmenti yerleştirebilirsiniz.\nYerleştirmek için tıklayın.";
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();
            
        wheelManager = FindFirstObjectByType<WheelManager>();
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        // UI panelini oluştur veya bul
        if (segmentPreviewPanel == null)
        {
            CreateSegmentPreviewUI();
        }
        
        // Başlangıçta UI'ı gizle
        if (segmentPreviewPanel != null)
            segmentPreviewPanel.SetActive(false);
    }
    
    private void CreateSegmentPreviewUI()
    {
        // Canvas'ı bul
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas bulunamadı! UI oluşturulamıyor.");
            
            // Canvas yoksa bir tane oluştur
            GameObject canvasObj = new GameObject("UI Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Debug.Log("Canvas otomatik oluşturuldu!");
        }
        
        // Ana panel oluştur
        GameObject panelObj = new GameObject("SegmentPreviewPanel");
        panelObj.transform.SetParent(mainCanvas.transform, false);
        
        // Panel boyutları - buradan değiştirebilirsiniz
        float panelWidth = 420f; // Genişlik - daha geniş
        float panelHeight = 700f; // Yükseklik - daha uzun
        float panelPosX = 200f; // Sol kenardan uzaklık
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.5f);
        panelRect.anchorMax = new Vector2(0, 0.5f);
        panelRect.anchoredPosition = new Vector2(panelPosX, 0);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        
        Image panelBG = panelObj.AddComponent<Image>();
        panelBG.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        segmentPreviewPanel = panelObj;
        
        // Segment preview image
        GameObject imageObj = new GameObject("SegmentImage");
        imageObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.1f, 0.78f); // Daha yukarıda
        imageRect.anchorMax = new Vector2(0.9f, 0.95f);
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        
        segmentPreviewImage = imageObj.AddComponent<Image>();
        segmentPreviewImage.color = Color.white;
        
        // Segment name text
        GameObject nameObj = new GameObject("SegmentName");
        nameObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.05f, 0.55f); // Daha aşağıya çekildi
        nameRect.anchorMax = new Vector2(0.95f, 0.65f); // Daha aşağıya çekildi
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        segmentNameText = nameObj.AddComponent<Text>();
        segmentNameText.text = "Segment Name";
        segmentNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        segmentNameText.fontSize = 30; // Çok daha büyük font
        segmentNameText.fontStyle = FontStyle.Bold;
        segmentNameText.color = Color.white;
        segmentNameText.alignment = TextAnchor.MiddleCenter;
        
        // Segment description text
        GameObject descObj = new GameObject("SegmentDescription");
        descObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.4f); // Yukarı çekildi
        descRect.anchorMax = new Vector2(0.95f, 0.5f); // Yukarı çekildi
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;
        
        segmentDescriptionText = descObj.AddComponent<Text>();
        segmentDescriptionText.text = "Description";
        segmentDescriptionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        segmentDescriptionText.fontSize = 26; // Çok daha büyük font
        segmentDescriptionText.color = Color.gray;
        segmentDescriptionText.alignment = TextAnchor.UpperLeft;
        segmentDescriptionText.verticalOverflow = VerticalWrapMode.Overflow;
        
        // Tooltip tarzı açıklama
        GameObject tooltipObj = new GameObject("PlacementInstructions");
        tooltipObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform tooltipRect = tooltipObj.AddComponent<RectTransform>();
        tooltipRect.anchorMin = new Vector2(0.05f, 0.10f); // Daha aşağıda
        tooltipRect.anchorMax = new Vector2(0.95f, 0.20f); // Daha aşağıda
        tooltipRect.offsetMin = Vector2.zero;
        tooltipRect.offsetMax = Vector2.zero;
        
        Text tooltipText = tooltipObj.AddComponent<Text>();
        tooltipText.text = tooltipInstructions; // Tanımladığımız değişkeni kullan
        tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltipText.fontSize = 28; // Çok daha büyük font
        tooltipText.color = Color.white;
        tooltipText.alignment = TextAnchor.MiddleCenter;
        tooltipText.verticalOverflow = VerticalWrapMode.Overflow;
    }
    
    public void StartPlacement(PrizeSegment segment)
    {
        currentSegment = segment;
        
        // Debug.Log($"🎯 Starting placement for segment: {segment.segmentName}");
        
        // UI'ı göster ve güncelle
        ShowSegmentPreviewUI();
        
        // Preview modu başlat
        isInPreviewMode = true;
    }
    
    private void ShowSegmentPreviewUI()
    {
        if (segmentPreviewPanel != null)
        {
            // Önce paneli ekranın dışına yerleştir (soldan gelecek)
            RectTransform panelRect = segmentPreviewPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Orijinal pozisyonu kaydet
                Vector2 targetPos = panelRect.anchoredPosition;
                
                // Ekranın dışına yerleştir (sol tarafta)
                panelRect.anchoredPosition = new Vector2(-panelRect.sizeDelta.x - 50, targetPos.y);
                
                // Paneli aktifleştir
                segmentPreviewPanel.SetActive(true);
                
                // Animasyonla içeri getir
                panelRect.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutBack);
            }
            else
            {
                segmentPreviewPanel.SetActive(true);
            }
            
            // Segment bilgilerini güncelle
            if (segmentNameText != null)
                segmentNameText.text = currentSegment.segmentName;
                
            if (segmentDescriptionText != null)
            {
                string description = "";
                if (currentSegment.segmentReward != null)
                {
                    description = $"Type: {currentSegment.segmentReward.type}\nRarity: {currentSegment.segmentReward.rarity}\nSize: {currentSegment.segmentReward.size}";
                    if (!string.IsNullOrEmpty(currentSegment.segmentReward.description))
                        description += $"\n{currentSegment.segmentReward.description}";
                }
                segmentDescriptionText.text = description;
            }
            
            // Segment rengini ayarla
            if (segmentPreviewImage != null)
            {
                segmentPreviewImage.color = currentSegment.segmentColor;
            }
        }
    }
    
    private void Update()
    {
        if (isInPreviewMode && wheelManager != null)
        {
            HandleMouseHover();
        }
    }
    
    private void HandleMouseHover()
    {
        // Mouse pozisyonunu al
        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        
        // Debug: Ray casting durumunu kontrol et
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, wheelLayerMask);
        
        if (hitSomething)
        {
            // Debug: Neye hit ettiğimizi kontrol et
            Debug.Log($"Hit object: {hit.transform.name}, Parent: {hit.transform.parent?.name}");
            
            // Hit ettiğimiz nesnenin slot olup olmadığını kontrol et
            Transform hitTransform = hit.transform;
            
            // Slot index'ini bul
            int slotIndex = GetSlotIndexFromTransform(hitTransform);
            
            Debug.Log($"Slot Index found: {slotIndex}");
            
            if (slotIndex != -1 && slotIndex != hoveredSlotIndex)
            {
                // Yeni slot'a geçiş
                RemovePreviewSegment();
                
                // Segment boyutunu al
                int segmentSize = currentSegment?.segmentReward?.size ?? 1;
                
                // Slot'ların boş olup olmadığını kontrol et
                int startSlot = slotIndex;
                if (segmentSize > 1)
                {
                    int half = segmentSize / 2;
                    startSlot = (slotIndex - half + wheelManager.slotCount) % wheelManager.slotCount;
                }
                
                if (AreSlotsAvailableForSegment(startSlot, segmentSize))
                {
                    hoveredSlotIndex = slotIndex;
                    ShowPreviewSegment(slotIndex);
                    Debug.Log($"Switched to slot {slotIndex}");
                }
                else
                {
                    hoveredSlotIndex = -1; // Dolu slot - preview gösterme
                    Debug.Log($"Slot {slotIndex} dolu - preview gösterilmiyor");
                }
            }
        }
        else
        {
            // SORUN TESPİTİ: Physics.Raycast çalışmıyor olabilir
            // 1. Çarkın ve slotların collider'ı var mı?
            // 2. LayerMask doğru ayarlanmış mı?
            // 3. Camera.main doğru kamera mı?
            
            // Alternatif yöntem: ScreenToWorldPoint kullanarak pozisyon tespiti
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
            
            // Çark merkezinden uzaklık ve açı hesapla
            if (wheelManager != null && wheelManager.transform != null)
            {
                Vector3 wheelCenter = wheelManager.transform.position;
                float distance = Vector2.Distance(new Vector2(worldPoint.x, worldPoint.y), new Vector2(wheelCenter.x, wheelCenter.y));
                
                // Çark yarıçapı içinde mi?
                float wheelRadius = 5f; // Çark yarıçapını ayarlayın
                if (distance < wheelRadius && distance > 1f)
                {
                    // Açı hesapla
                    Vector2 direction = new Vector2(worldPoint.x - wheelCenter.x, worldPoint.y - wheelCenter.y);
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    
                    // 90 derece offset ekle ve saat yönünün tersine çevir
                    angle = (angle - 90f);  // 90 derece offset
                    angle = (360f - angle); // Saat yönünün tersine çevir
                    
                    // Açıyı 0-360 arasına normalize et
                    angle = ((angle % 360f) + 360f) % 360f;
                    
                    // Çarkın rotasyonunu hesaba kat
                    if (wheelManager.transform.rotation != Quaternion.identity)
                    {
                        float wheelRotation = wheelManager.transform.eulerAngles.z;
                        angle -= wheelRotation;
                        angle = (angle + 360f) % 360f;
                    }
                    
                    Debug.Log($"Raw angle: {Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg}, Adjusted angle: {angle}");
                    
                    // Açıya göre slot index'i hesapla
                    int slotCount = wheelManager.slotCount;
                    float slotAngle = 360f / slotCount;
                    int slotIndex = Mathf.FloorToInt(angle / slotAngle);
                    
                    Debug.Log($"Alternative method - Angle: {angle}, Slot: {slotIndex}");
                    
                    if (slotIndex != hoveredSlotIndex)
                    {
                        RemovePreviewSegment();
                        
                        // Segment boyutunu al
                        int segmentSize = currentSegment?.segmentReward?.size ?? 1;
                        
                        // Slot'ların boş olup olmadığını kontrol et
                        int startSlot = slotIndex;
                        if (segmentSize > 1)
                        {
                            int half = segmentSize / 2;
                            startSlot = (slotIndex - half + wheelManager.slotCount) % wheelManager.slotCount;
                        }
                        
                        if (AreSlotsAvailableForSegment(startSlot, segmentSize))
                        {
                            hoveredSlotIndex = slotIndex;
                            ShowPreviewSegment(slotIndex);
                        }
                        else
                        {
                            hoveredSlotIndex = -1; // Dolu slot - preview gösterme
                        }
                    }
                }
                else if (hoveredSlotIndex != -1)
                {
                    RemovePreviewSegment();
                    hoveredSlotIndex = -1;
                }
            }
        }
        
        // Mouse click kontrolü
        if (Input.GetMouseButtonDown(0) && hoveredSlotIndex != -1)
        {
            Debug.Log($"Clicked on slot {hoveredSlotIndex}");
            PlaceSegmentAtSlot(hoveredSlotIndex);
        }
    }
    
    private int GetSlotIndexFromTransform(Transform transform)
    {
        // WheelManager'daki slot'ları kontrol et
        if (wheelManager != null && wheelManager.slots != null)
        {
            for (int i = 0; i < wheelManager.slots.Length; i++)
            {
                if (wheelManager.slots[i] != null && 
                    (wheelManager.slots[i] == transform || transform.IsChildOf(wheelManager.slots[i])))
                {
                    return i;
                }
            }
        }
        
        // Debug için
        if (wheelManager == null)
            Debug.LogWarning("WheelManager bulunamadı!");
        else if (wheelManager.slots == null)
            Debug.LogWarning("WheelManager slots null!");
        else
            Debug.LogWarning($"Transform {transform.name} hiçbir slot ile eşleşmedi");
            
        return -1;
    }
    
    private void ShowPreviewSegment(int slotIndex)
    {
        if (currentSegment?.segmentReward?.segmentPrefab == null) return;
        
        // Segment boyutunu al
        int segmentSize = currentSegment.segmentReward.size;
        
        // Segment boyutuna göre başlangıç slotunu hesapla
        int startSlot = slotIndex;
        if (segmentSize > 1)
        {
            int half = segmentSize / 2;
            startSlot = (slotIndex - half + wheelManager.slotCount) % wheelManager.slotCount;
            
            // Debug için
            Debug.Log($"Segment boyutu: {segmentSize}, Tıklanan slot: {slotIndex}, Başlangıç slot: {startSlot}");
        }
        
        // Slot'ların boş olup olmadığını kontrol et
        if (!AreSlotsAvailableForSegment(startSlot, segmentSize))
        {
            Debug.Log($"⚠️ Bu slotlara segment yerleştirilemez - dolu slotlar var. Start: {startSlot}, Size: {segmentSize}");
            return; // Dolu slotlarda preview gösterme
        }
        
        // Preview segment'i oluştur
        GameObject prefab = currentSegment.segmentReward.segmentPrefab;
        Transform slotTransform = wheelManager.slots[startSlot];
        
        previewSegmentInstance = Instantiate(prefab, slotTransform);
        previewSegmentInstance.name = "PreviewSegment";
        previewSegmentInstance.transform.localPosition = Vector3.zero;
        previewSegmentInstance.transform.localRotation = Quaternion.identity;
        
        // Yarı saydam yap
        MakeSegmentTransparent(previewSegmentInstance);
        
        Debug.Log($"👻 Preview segment created at slot {startSlot}, Size: {segmentSize}");
    }
    
    // Belirli bir segment için gerekli slotların boş olup olmadığını kontrol et
    private bool AreSlotsAvailableForSegment(int startSlot, int segmentSize)
    {
        if (wheelManager == null || wheelManager.slotOccupied == null) return false;
        
        // Segment boyutuna göre kontrol et
        for (int i = 0; i < segmentSize; i++)
        {
            int slotIndex = (startSlot + i) % wheelManager.slotCount;
            if (wheelManager.slotOccupied[slotIndex])
            {
                Debug.Log($"❌ Slot {slotIndex} dolu - segment yerleştirilemez");
                return false;
            }
        }
        
        return true;
    }
    
    private void MakeSegmentTransparent(GameObject segmentObj)
    {
        Renderer[] renderers = segmentObj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = new Material(materials[i]);
                
                // Transparent rendering mode'a geç
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                
                // Alpha değerini ayarla
                Color color = mat.color;
                color.a = previewAlpha;
                mat.color = color;
                
                materials[i] = mat;
            }
            renderer.materials = materials;
        }
    }
    
    private void RemovePreviewSegment()
    {
        if (previewSegmentInstance != null)
        {
            DestroyImmediate(previewSegmentInstance);
            previewSegmentInstance = null;
        }
    }
    
    private void PlaceSegmentAtSlot(int slotIndex)
    {
        if (currentSegment?.segmentReward == null || wheelManager == null) return;
        
        // Segment boyutunu al
        int segmentSize = currentSegment.segmentReward.size;
        
        // Segment boyutuna göre başlangıç slotunu hesapla
        int startSlot = slotIndex;
        if (segmentSize > 1)
        {
            int half = segmentSize / 2;
            startSlot = (slotIndex - half + wheelManager.slotCount) % wheelManager.slotCount;
            
            Debug.Log($"🔧 Placing segment - Size: {segmentSize}, Clicked slot: {slotIndex}, Start slot: {startSlot}");
        }
        
        // Slot'ların boş olup olmadığını kontrol et
        if (!AreSlotsAvailableForSegment(startSlot, segmentSize))
        {
            Debug.LogWarning($"⚠️ Bu slotlara segment yerleştirilemez - dolu slotlar var. Start: {startSlot}, Size: {segmentSize}");
            return; // Dolu slotlarda yerleştirme yapma
        }
        
        // Preview'ı kaldır
        RemovePreviewSegment();
        
        // WheelManager'ın placement sistemini kullan
        wheelManager.AddSegmentToSlot(currentSegment.segmentReward, startSlot);
        
        // Placement'ı tamamla
        CompletePlacement();
    }
    

    private void CompletePlacement()
    {
        // Preview'ı temizle
        RemovePreviewSegment();
        
        // UI'ı animasyonla gizle
        if (segmentPreviewPanel != null)
        {
            RectTransform panelRect = segmentPreviewPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Animasyonla dışarı çıkar
                Vector2 hidePos = new Vector2(-panelRect.sizeDelta.x - 50, panelRect.anchoredPosition.y);
                panelRect.DOAnchorPos(hidePos, 0.4f).SetEase(Ease.InBack).OnComplete(() => {
                    segmentPreviewPanel.SetActive(false);
                });
            }
            else
            {
                segmentPreviewPanel.SetActive(false);
            }
        }
        
        // State'i temizle
        isInPreviewMode = false;
        hoveredSlotIndex = -1;
        currentSegment = null;
        
        // PrizeWheelManager'a bildir
        PrizeWheelManager prizeWheelManager = FindFirstObjectByType<PrizeWheelManager>();
        if (prizeWheelManager != null)
        {
            prizeWheelManager.CompletePlacement();
        }
        
        // Debug.Log("🎯 Segment placement completed!");
    }
    
    private void OnDestroy()
    {
        RemovePreviewSegment();
    }
}
