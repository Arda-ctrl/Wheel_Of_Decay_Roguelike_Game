using UnityEngine;

public class PrizeSegmentTooltip : MonoBehaviour
{
    [Header("Tooltip Settings")]
    public float hoverDelay = 0.5f;
    
    private PrizeSegment currentSegment;
    private PrizeWheelManager wheelManager;
    
    private float hoverTimer = 0f;
    private bool isHovering = false;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();
            
        wheelManager = GetComponentInParent<PrizeWheelManager>();
        if (wheelManager == null)
            wheelManager = FindFirstObjectByType<PrizeWheelManager>();
    }
    
    void Update()
    {
        if (isHovering)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverDelay && !IsTooltipVisible())
            {
                ShowTooltip();
            }
        }
        
        // Mouse pozisyonunu kontrol et
        CheckMouseHover();
    }
    
    void CheckMouseHover()
    {
        if (mainCamera == null || wheelManager == null || wheelManager.segments.Count == 0) return;
        
        // Mouse pozisyonunu world space'e çevir
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 2D için
        
        // Çarkın merkezine göre açıyı hesapla
        Vector3 wheelCenter = transform.position;
        Vector3 direction = mouseWorldPos - wheelCenter;
        float distance = direction.magnitude;
        
        // Çark içinde mi?
        float wheelRadius = 2.5f; // Çark yarıçapı (ayarlayabilirsin)
        bool insideWheel = distance <= wheelRadius && distance >= 0.2f; // İç boşluk hariç
        
        if (insideWheel)
        {
            // Mouse'un açısını hesapla - Unity koordinat sistemine uygun
            float mouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Açıyı normalize et (0-360)
            if (mouseAngle < 0)
                mouseAngle += 360f;
            
            // Saat yönüne çevirmek için: (360f - angle) % 360f
            mouseAngle = (360f - mouseAngle) % 360f;
            
            // 90 derece offset ekle (yukarıyı 0° yapmak için)
            mouseAngle = (mouseAngle + 90f) % 360f;
            
            // Hangi segment'te olduğumuzu bul
            PrizeSegment hoveredSegment = null;
            foreach (var segment in wheelManager.segments)
            {
                if (segment.ContainsAngle(mouseAngle))
                {
                    hoveredSegment = segment;
                    break;
                }
            }
            
            if (hoveredSegment != null)
            {
                // Yeni segment'te hover
                if (currentSegment != hoveredSegment)
                {
                    currentSegment = hoveredSegment;
                    OnMouseEnter();
                }
                else if (!isHovering)
                {
                    OnMouseEnter();
                }
            }
            else if (isHovering)
            {
                // Hiçbir segment'te değil
                OnMouseExit();
            }
        }
        else if (isHovering)
        {
            // Çark dışına çıktı
            OnMouseExit();
        }
    }
    
    void OnMouseEnter()
    {
        isHovering = true;
        hoverTimer = 0f;
    }
    
    void OnMouseExit()
    {
        isHovering = false;
        hoverTimer = 0f;
        HideTooltip();
    }
    
    void ShowTooltip()
    {
        if (currentSegment == null || TooltipUI.Instance == null) return;
        
        // Tooltip içeriğini hazırla
        string title = currentSegment.segmentName;
        string description = GetSegmentDescription();
        string typeAndRarity = GetSegmentTypeInfo();
        
        // Mouse pozisyonunda göster
        TooltipUI.Instance.ShowTooltip(title, description, typeAndRarity, Input.mousePosition);
    }
    
    void HideTooltip()
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.HideTooltip();
        }
    }
    
    bool IsTooltipVisible()
    {
        return TooltipUI.Instance != null && TooltipUI.Instance.IsVisible;
    }
    
    string GetSegmentDescription()
    {
        switch (currentSegment.prizeType)
        {
            case PrizeType.SegmentReward:
                return currentSegment.segmentReward != null ? currentSegment.segmentReward.description : "Unknown Segment";
                
            case PrizeType.Resource:
                return currentSegment.customRewardText;
                
            case PrizeType.CustomReward:
                return currentSegment.customRewardText;
                
            default:
                return "Mystery Prize";
        }
    }
    
    string GetSegmentTypeInfo()
    {
        switch (currentSegment.prizeType)
        {
            case PrizeType.SegmentReward:
                if (currentSegment.segmentReward != null)
                {
                    return $"Type: {currentSegment.segmentReward.type} | Rarity: {currentSegment.segmentReward.rarity}";
                }
                return "Type: Segment | Rarity: Unknown";
                
            case PrizeType.Resource:
                return $"Type: Resource | Amount: {currentSegment.resourceAmount}";
                
            case PrizeType.CustomReward:
                return "Type: Special Reward";
                
            default:
                return "Type: Unknown";
        }
    }
    
    // Wheel radius'u ayarlamak için (opsiyonel)
    public void SetWheelRadius(float radius)
    {
        // Dinamik radius ayarı için kullanılabilir
    }
}
