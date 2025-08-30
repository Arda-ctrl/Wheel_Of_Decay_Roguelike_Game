using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class PrizeWheelManager : MonoBehaviour
{
    [Header("Wheel Components")]
    public Transform wheelTransform;
    public Renderer wheelRenderer;
    public Transform indicatorTransform;
    
    [Header("Wheel Settings")]
    public List<PrizeSegment> segments = new List<PrizeSegment>();
    public float spinDuration = 3f;
    public float minSpinRotations = 3f;
    public float maxSpinRotations = 6f;
    public AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    public Color lineColor = Color.black;
    [Range(0.001f, 0.1f)]
    public float lineWidth = 0.02f;
    public float dividerInnerRadius = 0.1f;
    public float dividerOuterRadius = 0.5f;
    
    [Header("Tooltip Settings")]
    public float wheelRadius = 2.5f; // Tooltip detection için çark yarıçapı
    public float hoverDelay = 0.5f;
    
    [Header("Input Settings")]
    public KeyCode spinKey = KeyCode.Space;
    public KeyCode acceptResultKey = KeyCode.Return;
    
    [Header("Ability System")]
    public PrizeWheelAbilityHandler abilityHandler;
    
    private Material wheelMaterial;
    private bool isSpinning = false;
    private bool isWaitingForAcceptance = false; // Çark sonucunu kabul etmeyi bekliyor mu?
    private PrizeSegment pendingWinningSegment = null; // Kazanılan segment (kabul edilmeyi bekliyor)
    private System.Action<PrizeSegment> onSpinComplete;
    private List<PrizeWheelDivider> dividers = new List<PrizeWheelDivider>();
    private PrizeSegmentTooltip tooltipHandler;
    
    void Start()
    {
        InitializeWheel();
        SetupShader();
        
        // Ability handler'ı bul
        if (abilityHandler == null)
            abilityHandler = FindFirstObjectByType<PrizeWheelAbilityHandler>();
            
        // Bir frame bekle ki diğer objeler de initialize olsun
        StartCoroutine(GenerateRandomWheelDelayed());
    }
    
    System.Collections.IEnumerator GenerateRandomWheelDelayed()
    {
        yield return null; // Bir frame bekle
        GenerateRandomWheelOnStart();
    }
    
    void Update()
    {
        // Tuş kontrolü
        if (Input.GetKeyDown(spinKey) && !isSpinning && !isWaitingForAcceptance)
        {
            SpinWheel();
        }
        
        // Enter tuşu ile çark sonucunu kabul etme
        if (Input.GetKeyDown(acceptResultKey) && isWaitingForAcceptance)
        {
            AcceptWheelResult();
        }
    }
    
    void InitializeWheel()
    {
        if (wheelRenderer == null)
            wheelRenderer = GetComponent<Renderer>();
            
        if (wheelRenderer != null)
        {
            wheelMaterial = wheelRenderer.material;
        }
        
        // Çarkı başlangıç pozisyonuna getir (0 derece)
        if (wheelTransform != null)
        {
            wheelTransform.rotation = Quaternion.identity; // 0,0,0 rotation
        }
        
        // Segments otomatik generate edilecek (GenerateRandomWheelOnStart'ta)
        
        UpdateShaderProperties();
        CreateDividers();
    }
    
    void CreateTestSegments()
    {
        segments.Add(new PrizeSegment 
        { 
            segmentName = "Attack Boost",
            startAngle = 0f, 
            endAngle = 80f, 
            segmentColor = Color.red,
            prizeType = PrizeType.Resource,
            resourceAmount = 2
        });
        
        segments.Add(new PrizeSegment 
        { 
            segmentName = "Defense Boost",
            startAngle = 80f, 
            endAngle = 240f, 
            segmentColor = Color.blue,
            prizeType = PrizeType.Resource,
            resourceAmount = 3
        });
        
        segments.Add(new PrizeSegment 
        { 
            segmentName = "Curse",
            startAngle = 240f, 
            endAngle = 360f, 
            segmentColor = Color.green,
            prizeType = PrizeType.CustomReward,
            customRewardText = "Random Curse"
        });
    }
    
    void SetupShader()
    {
        if (wheelMaterial == null) return;
        
        // Line properties
        wheelMaterial.SetColor("_LineColor", lineColor);
        wheelMaterial.SetFloat("_LineWidth", lineWidth);
    }
    
    public void UpdateShaderProperties()
    {
        if (wheelMaterial == null || segments.Count == 0) return;
        
        // Segment sayısı
        wheelMaterial.SetInt("_SegmentCount", segments.Count);
        
        // Arrays oluştur
        float[] startAngles = new float[10];
        float[] endAngles = new float[10];
        
        for (int i = 0; i < segments.Count && i < 10; i++)
        {
            startAngles[i] = segments[i].startAngle;
            endAngles[i] = segments[i].endAngle;
            
            // Segment rengini ayarla
            string colorProperty = $"_SegmentColor{i + 1}";
            wheelMaterial.SetColor(colorProperty, segments[i].segmentColor);
        }
        
        // Arrays'i shader'a gönder
        wheelMaterial.SetFloatArray("_StartAngles", startAngles);
        wheelMaterial.SetFloatArray("_EndAngles", endAngles);
    }
    
    [ContextMenu("Spin Wheel")]
    public void SpinWheel(System.Action<PrizeSegment> callback = null)
    {
        if (isSpinning) return;
        
        onSpinComplete = callback;
        StartSpin();
    }
    
    void StartSpin()
    {
        isSpinning = true;
        
        // Debug: Başlangıç rotasyonunu kontrol et
        float startRotation = wheelTransform != null ? wheelTransform.eulerAngles.z : 0f;
        Debug.Log($"🚀 DEBUG: Spin başlangıcı - Çark rotasyonu: {startRotation:F1}°");
        
        // Basit ve mantıklı yaklaşım
        // Rastgele dönüş miktarı
        float randomRotations = Random.Range(minSpinRotations, maxSpinRotations);
        float totalRotation = randomRotations * 360f;
        
        // Rastgele hedef açı (0-360)
        float targetAngle = Random.Range(0f, 360f);
        totalRotation += targetAngle;
        
        Debug.Log($"🎯 DEBUG: Hedef açı: {targetAngle:F1}°, Toplam dönüş: {totalRotation:F1}°");
        
        // DOTween ile döndür - normal yönde
        wheelTransform.DORotate(new Vector3(0, 0, totalRotation), spinDuration, RotateMode.LocalAxisAdd)
            .SetEase(spinCurve)
            .OnComplete(() => OnSpinComplete(targetAngle));
    }
    
    void OnSpinComplete(float finalAngle)
    {
        isSpinning = false;
        
        // Debug: Çarkın mevcut rotasyonunu kontrol et
        float currentWheelRotation = wheelTransform != null ? wheelTransform.eulerAngles.z : 0f;
        Debug.Log($"🔍 DEBUG: Çark rotasyonu: {currentWheelRotation:F1}°, Final angle: {finalAngle:F1}°");
        
        // DÜZELTME: İğnenin gösterdiği açıyı hesapla
        // İğne üstte (0 derece) olduğu için, çarkın mevcut rotasyonu = iğnenin gösterdiği açı
        float needleAngle = currentWheelRotation;
        
        Debug.Log($"🎲 Wheel stopped at: {finalAngle}°, Çark rotasyonu: {currentWheelRotation:F1}°, İğne gösteriyor: {needleAngle:F1}°");
        
        // Hangi segment kazandı?
        PrizeSegment winningSegment = GetSegmentAtAngle(needleAngle);
        
        if (winningSegment != null)
        {
            Debug.Log($"🎉 Won: {winningSegment.segmentName} (Range: {winningSegment.startAngle}°-{winningSegment.endAngle}°)");
            onSpinComplete?.Invoke(winningSegment);
            
            // Post-spin ability'leri tetikle
            OnWheelSpinComplete?.Invoke();
            
            // Çark sonucunu kabul etmeyi bekle
            pendingWinningSegment = winningSegment;
            isWaitingForAcceptance = true;
            
            Debug.Log("⏳ Press ENTER to accept the wheel result and continue...");
        }
        else
        {
            Debug.LogWarning($"No segment found at needle angle {needleAngle}");
        }
        
        // Tooltip sistemini güncelle - çark döndükten sonra tooltip'in doğru çalışması için
        if (tooltipHandler != null)
        {
            // Tooltip handler'ı yeniden başlat
            DestroyImmediate(tooltipHandler.gameObject);
            tooltipHandler = null;
        }
        
        // Yeni tooltip handler oluştur
        SetupTooltipSystem();
        
        // Tooltip handler'ı güncelle - segment bilgilerini yenile
        if (tooltipHandler != null)
        {
            tooltipHandler.UpdateSegmentData(segments);
        }
    }
    
    PrizeSegment GetSegmentAtAngle(float angle)
    {
        Debug.Log($"🔍 DEBUG: Açı {angle:F1}° için segment aranıyor...");
        
        foreach (var segment in segments)
        {
            Debug.Log($"  - {segment.segmentName}: {segment.startAngle:F1}° - {segment.endAngle:F1}° (Contains {angle:F1}°: {segment.ContainsAngle(angle)})");
            if (segment.ContainsAngle(angle))
                return segment;
        }
        
        Debug.LogWarning($"❌ DEBUG: Açı {angle:F1}° için hiçbir segment bulunamadı!");
        return null;
    }
    
    // Editor'da segment'leri güncelle
    void OnValidate()
    {
        if (Application.isPlaying && wheelMaterial != null)
        {
            SetupShader();
            UpdateShaderProperties();
        }
    }
    
    // Segment yönetimi
    public void AddSegment(PrizeSegment segment)
    {
        segments.Add(segment);
        UpdateShaderProperties();
    }
    
    public void RemoveSegment(int index)
    {
        if (index >= 0 && index < segments.Count)
        {
            segments.RemoveAt(index);
            RecalculateSegmentSizes();
            UpdateShaderProperties();
        }
    }
    
    public void RecalculateSegmentSizes()
    {
        if (segments.Count == 0) return;
        
        float anglePerSegment = 360f / segments.Count;
        
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].startAngle = i * anglePerSegment;
            segments[i].endAngle = (i + 1) * anglePerSegment;
        }
    }
    
    void CreateDividers()
    {
        // Eski divider'ları temizle
        ClearDividers();
        
        // Her segment başlangıcında divider oluştur
        foreach (var segment in segments)
        {
            GameObject dividerObj = new GameObject($"Divider_{segment.startAngle}");
            dividerObj.transform.SetParent(wheelTransform);
            dividerObj.transform.localPosition = Vector3.zero;
            dividerObj.transform.localRotation = Quaternion.identity;
            dividerObj.transform.localScale = Vector3.one;
            
            PrizeWheelDivider divider = dividerObj.AddComponent<PrizeWheelDivider>();
            divider.innerRadius = dividerInnerRadius;
            divider.outerRadius = dividerOuterRadius;
            divider.lineWidth = lineWidth;
            divider.lineColor = lineColor;
            divider.SetupDivider(segment.startAngle);
            
            dividers.Add(divider);
        }
    }
    
    void ClearDividers()
    {
        foreach (var divider in dividers)
        {
            if (divider != null)
                DestroyImmediate(divider.gameObject);
        }
        dividers.Clear();
    }
    
    void SetupTooltipSystem()
    {
        // Tooltip handler'ı oluştur veya güncelle
        if (tooltipHandler == null)
        {
            GameObject tooltipObj = new GameObject("PrizeWheelTooltipHandler");
            tooltipObj.transform.SetParent(transform);
            tooltipObj.transform.localPosition = Vector3.zero;
            
            tooltipHandler = tooltipObj.AddComponent<PrizeSegmentTooltip>();
            tooltipHandler.hoverDelay = hoverDelay;
        }
        
        // Tooltip handler'ını güncelle
        UpdateTooltipHandler();
    }
    
    void UpdateTooltipHandler()
    {
        if (tooltipHandler != null)
        {
            // Tooltip handler'ı güncelle - segment verilerini yenile
            tooltipHandler.UpdateSegmentData(segments);
        }
    }
    
    void GenerateRandomWheelOnStart()
    {
        // PrizeWheelGenerator'ı bul
        PrizeWheelGenerator generator = FindFirstObjectByType<PrizeWheelGenerator>();
        
        if (generator != null)
        {
            // Random wheel oluştur
            var generatedSegments = generator.GenerateRandomPrizeWheel();
            
            if (generatedSegments != null && generatedSegments.Count > 0)
            {
                segments = generatedSegments;
                UpdateShaderProperties();
                CreateDividers();
                SetupTooltipSystem();
                
                Debug.Log($"Auto-generated prize wheel with {segments.Count} segments on start!");
                
                // Debug için segment listesini göster
                foreach (var segment in segments)
                {
                    string colorHex = ColorUtility.ToHtmlStringRGB(segment.segmentColor);
                    Debug.Log($"- {segment.segmentName}: {segment.startAngle:F0}° - {segment.endAngle:F0}° (Size: {segment.AngleSize:F0}°) Color: #{colorHex}");
                }
            }
            else
            {
                Debug.LogWarning("Failed to generate random wheel, using test segments");
                CreateTestSegments(); // Fallback
            }
        }
        else
        {
            Debug.LogWarning("PrizeWheelGenerator not found! Using test segments");
            CreateTestSegments(); // Fallback
        }
    }
    
    // Segment Placement System
    private PrizeSegment pendingSegment = null;
    private bool isInPlacementMode = false;
    
    // Event delegates
    public System.Action<PrizeSegment> OnSegmentWon;
    public System.Action OnWheelSpinComplete; // Post-spin ability'ler için
    
    bool IsSegmentPlaceable(PrizeSegment segment)
    {
        // Sadece SegmentReward tipindeki segmentler yerleştirilebilir
        return segment.prizeType == PrizeType.SegmentReward && segment.segmentReward != null;
    }
    
    void StartSegmentPlacementSystem(PrizeSegment wonSegment)
    {
        // Debug.Log($"🔧 Starting segment placement system for: {wonSegment.segmentName}");
        
        // Kazanılan segmenti sakla
        pendingSegment = wonSegment;
        isInPlacementMode = true;
        
        // Event'i tetikle - UI sistemine bildir
        OnSegmentWon?.Invoke(wonSegment);
        
        // Prize wheel'i gizle ve normal wheel UI'ını göster
        HidePrizeWheelAndShowWheelUI();
    }
    
    void HidePrizeWheelAndShowWheelUI()
    {
        // Prize wheel'i gizle
        gameObject.SetActive(false);
        
        // Indicator'ı da gizle
        if (indicatorTransform != null)
        {
            indicatorTransform.gameObject.SetActive(false);
        }
        
        // WheelUIAnimator'ı bul ve wheel UI'ını göster
        WheelUIAnimator wheelUIAnimator = FindFirstObjectByType<WheelUIAnimator>();
        if (wheelUIAnimator != null)
        {
            // Tab tuşuna basmış gibi wheel UI'ını göster
            wheelUIAnimator.ShowWheelUI();
        }
        
        // Segment placement UI'ını başlat
        StartSegmentPlacementUI();
    }
    
    void StartSegmentPlacementUI()
    {
        // Segment placement manager'ı oluştur veya aktifleştir
        SegmentPlacementManager placementManager = FindFirstObjectByType<SegmentPlacementManager>();
        if (placementManager == null)
        {
            GameObject placementObj = new GameObject("SegmentPlacementManager");
            placementManager = placementObj.AddComponent<SegmentPlacementManager>();
        }
        
        // Placement sistemini başlat
        placementManager.StartPlacement(pendingSegment);
    }
    
    // Public methods for external access
    public PrizeSegment GetPendingSegment() => pendingSegment;
    public bool IsInPlacementMode() => isInPlacementMode;
    
    public void CompletePlacement()
    {
        pendingSegment = null;
        isInPlacementMode = false;
        // Debug.Log("🎯 Segment placement completed!");
    }
    
    // Prize wheel'i tekrar göstermek için (opsiyonel)
    public void ShowPrizeWheelAgain()
    {
        gameObject.SetActive(true);
        
        // Indicator'ı da tekrar göster
        if (indicatorTransform != null)
        {
            indicatorTransform.gameObject.SetActive(true);
        }
        
        // Debug.Log("🎡 Prize wheel shown again!");
    }
    
    // Enter tuşu ile çark sonucunu kabul etme
    public void AcceptWheelResult()
    {
        if (!isWaitingForAcceptance || pendingWinningSegment == null) return;
        
        Debug.Log("✅ Wheel result accepted! Processing...");
        
        // Eğer kazanılan segment çarka yerleştirilebilir bir segment ise segment placement sistemini başlat
        if (IsSegmentPlaceable(pendingWinningSegment))
        {
            StartSegmentPlacementSystem(pendingWinningSegment);
        }
        else
        {
            Debug.Log($"🎁 Non-placeable segment won: {pendingWinningSegment.segmentName}");
            // Burada resource veya custom reward işlemi yapılabilir
        }
        
        // State'i temizle
        isWaitingForAcceptance = false;
        pendingWinningSegment = null;
    }
    
    // Public properties
    public bool IsSpinning => isSpinning;
    public bool IsWaitingForAcceptance => isWaitingForAcceptance;
    public int SegmentCount => segments.Count;
}
