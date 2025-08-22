using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrizeWheelGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public PrizeWheelConfig config;
    
    [Header("Segment Sources")]
    public string segmentSOPath = "Wheel/Segment SO"; // Resources path
    
    private SegmentData[] allSegments;
    private List<SegmentData> usedSegments = new List<SegmentData>(); // Duplicate kontrolü için
    
    void Awake()
    {
        LoadAllSegments();
    }
    
    void LoadAllSegments()
    {
        allSegments = Resources.LoadAll<SegmentData>(segmentSOPath);
        if (allSegments == null || allSegments.Length == 0)
        {
            Debug.LogError($"No segments found at path: Resources/{segmentSOPath}");
        }
        else
        {
            Debug.Log($"Loaded {allSegments.Length} segments from Resources/{segmentSOPath}");
        }
    }
    
    public List<PrizeSegment> GenerateRandomPrizeWheel()
    {
        if (config == null)
        {
            Debug.LogError("PrizeWheelConfig is null!");
            return new List<PrizeSegment>();
        }
        
        // Eğer segment'ler henüz yüklenmemişse, yükle
        if (allSegments == null || allSegments.Length == 0)
        {
            Debug.LogWarning("Segments not loaded yet, loading now...");
            LoadAllSegments();
        }
        
        if (allSegments == null || allSegments.Length == 0)
        {
            Debug.LogError("No segments loaded!");
            return new List<PrizeSegment>();
        }
        
        List<PrizeSegment> prizeSegments = new List<PrizeSegment>();
        var rules = config.generationRules;
        
        // Segment sayısını belirle (config'ten)
        int targetSegmentCount = config.GetRandomSegmentCount();
        
        // Kullanılan segment'leri temizle
        usedSegments.Clear();
        
        // Segment türlerini ve sayılarını planla
        var segmentPlan = CreateSegmentPlan(targetSegmentCount);
        
        // Her planlanan segment için SegmentData seç ve PrizeSegment oluştur
        foreach (var plannedSegment in segmentPlan)
        {
            SegmentData selectedSegment = SelectRandomSegment(plannedSegment.type, plannedSegment.rarity);
            if (selectedSegment != null)
            {
                PrizeSegment prizeSegment = CreatePrizeSegment(selectedSegment, plannedSegment.angle);
                prizeSegments.Add(prizeSegment);
                
                // Duplicate kontrolü için kaydet
                if (rules.preventDuplicateSegments)
                {
                    usedSegments.Add(selectedSegment);
                }
            }
        }
        
        // Açıları düzenle (360° olacak şekilde)
        NormalizeSegmentAngles(prizeSegments);
        
        // Debug: Duplicate kontrolü sonuçları
        if (rules.preventDuplicateSegments)
        {
            var segmentNames = prizeSegments.Select(s => s.segmentName).ToArray();
            var duplicates = segmentNames.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            
            if (duplicates.Any())
            {
                Debug.LogWarning($"Duplicate segments found: {string.Join(", ", duplicates)}");
            }
            else
            {
                Debug.Log("✅ No duplicate segments - all unique!");
            }
        }
        
        return prizeSegments;
    }
    
    List<PlannedSegment> CreateSegmentPlan(int targetCount)
    {
        var plan = new List<PlannedSegment>();
        var rules = config.generationRules;
        
        // Zorunlu segmentleri ekle
        for (int i = 0; i < rules.minStatSegments; i++)
        {
            plan.Add(new PlannedSegment 
            { 
                type = SegmentEffectType.StatBoost,
                rarity = config.GetRandomRarity(),
                angle = 0 // Sonra hesaplanacak
            });
        }
        
        // Kalan slotları doldur
        int remainingSlots = targetCount - plan.Count;
        
        for (int i = 0; i < remainingSlots; i++)
        {
            SegmentEffectType randomType = GetRandomSegmentType(plan);
            Rarity randomRarity = config.GetRandomRarity();
            
            plan.Add(new PlannedSegment 
            { 
                type = randomType,
                rarity = randomRarity,
                angle = 0 // Sonra hesaplanacak
            });
        }
        
        // Açıları hesapla
        foreach (var segment in plan)
        {
            segment.angle = GetAngleForTypeAndRarity(segment.type, segment.rarity);
        }
        
        return plan;
    }
    
    SegmentEffectType GetRandomSegmentType(List<PlannedSegment> currentPlan)
    {
        var rules = config.generationRules;
        var availableTypes = new List<SegmentEffectType>();
        
        // Hangi türler eklenebilir kontrol et
        if (CanAddType(SegmentEffectType.StatBoost, currentPlan))
            availableTypes.Add(SegmentEffectType.StatBoost);
            
        if (CanAddType(SegmentEffectType.WheelManipulation, currentPlan))
            availableTypes.Add(SegmentEffectType.WheelManipulation);
            
        if (CanAddType(SegmentEffectType.CurseEffect, currentPlan))
            availableTypes.Add(SegmentEffectType.CurseEffect);
            
        if (CanAddType(SegmentEffectType.OnRemoveEffect, currentPlan))
            availableTypes.Add(SegmentEffectType.OnRemoveEffect);
        
        // Eğer hiçbir tür eklenemiyorsa, StatBoost varsayılan
        if (availableTypes.Count == 0)
            return SegmentEffectType.StatBoost;
        
        // Rastgele seç ama StatBoost'a öncelik ver
        if (availableTypes.Contains(SegmentEffectType.StatBoost) && Random.Range(0f, 1f) < 0.5f)
            return SegmentEffectType.StatBoost;
        
        return availableTypes[Random.Range(0, availableTypes.Count)];
    }
    
    bool CanAddType(SegmentEffectType type, List<PlannedSegment> currentPlan)
    {
        var rules = config.generationRules;
        int currentCount = currentPlan.Count(p => p.type == type);
        
        switch (type)
        {
            case SegmentEffectType.CurseEffect:
                return currentCount < rules.maxCurseSegments;
            case SegmentEffectType.OnRemoveEffect:
                return currentCount < rules.maxOnRemoveSegments;
            default:
                return true;
        }
    }
    
    float GetAngleForTypeAndRarity(SegmentEffectType type, Rarity rarity)
    {
        switch (type)
        {
            case SegmentEffectType.StatBoost:
                return config.statAngles.GetAngleByRarity(rarity);
            case SegmentEffectType.WheelManipulation:
                return config.wheelAngles.GetAngleByRarity(rarity);
            case SegmentEffectType.CurseEffect:
                return config.curseAngles.GetAngleByRarity(rarity);
            case SegmentEffectType.OnRemoveEffect:
                return config.onRemoveAngles.GetAngleByRarity(rarity);
            default:
                return 60f;
        }
    }
    
    SegmentData SelectRandomSegment(SegmentEffectType type, Rarity rarity)
    {
        var candidates = allSegments.Where(s => s.effectType == type && s.rarity == rarity).ToArray();
        
        if (candidates.Length == 0)
        {
            // Exact match bulunamazsa, aynı türden farklı rarity dene
            candidates = allSegments.Where(s => s.effectType == type).ToArray();
        }
        
        if (candidates.Length == 0)
        {
            Debug.LogWarning($"No segments found for type: {type}, rarity: {rarity}");
            return null;
        }
        
        // Duplicate kontrolü
        if (config.generationRules.preventDuplicateSegments)
        {
            // Kullanılmamış segment'leri filtrele
            candidates = candidates.Where(s => !usedSegments.Contains(s)).ToArray();
            
            // Eğer tüm segment'ler kullanılmışsa, warning ver ama en azından bir tane seç
            if (candidates.Length == 0)
            {
                Debug.LogWarning($"All segments of type {type} already used, allowing duplicate");
                candidates = allSegments.Where(s => s.effectType == type && s.rarity == rarity).ToArray();
                
                if (candidates.Length == 0)
                {
                    candidates = allSegments.Where(s => s.effectType == type).ToArray();
                }
            }
        }
        
        return candidates[Random.Range(0, candidates.Length)];
    }
    
    PrizeSegment CreatePrizeSegment(SegmentData segmentData, float angle)
    {
        return new PrizeSegment
        {
            segmentName = segmentData.segmentID,
            startAngle = 0, // Sonra hesaplanacak
            endAngle = angle, // Geçici olarak açı büyüklüğünü sakla
            segmentColor = segmentData.segmentColor, // SO'dan rengi al
            prizeType = PrizeType.SegmentReward,
            segmentReward = segmentData,
            segmentIcon = GetSegmentIcon(segmentData) // İkon da varsa al
        };
    }
    
    Sprite GetSegmentIcon(SegmentData segmentData)
    {
        // Eğer segment prefab'ında SpriteRenderer varsa, sprite'ını al
        if (segmentData.segmentPrefab != null)
        {
            SpriteRenderer sr = segmentData.segmentPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                return sr.sprite;
            }
        }
        return null;
    }
    
    void NormalizeSegmentAngles(List<PrizeSegment> segments)
    {
        if (segments.Count == 0) return;
        
        // Kesin açıları kullan (virgülsüz)
        float currentAngle = 0f;
        float totalUsedAngle = 0f;
        
        // Önce fazla segment kontrolü yap
        float runningTotal = 0f;
        for (int i = 0; i < segments.Count; i++)
        {
            float segmentSize = segments[i].endAngle; // Geçici olarak saklanan açı büyüklüğü
            int roundedSize = Mathf.RoundToInt(segmentSize); // Kesin açı
            
            if (runningTotal + roundedSize > 360f)
            {
                // Bu segment'ten sonrakileri kaldır
                int removeCount = segments.Count - i;
                segments.RemoveRange(i, removeCount);
                Debug.LogWarning($"Removed {removeCount} segments to fit within 360°");
                break;
            }
            runningTotal += roundedSize;
        }
        
        // Şimdi segment'lerin gerçek açılarını hesapla - Unity koordinat sistemine uygun
        // Unity'de yukarı 90°, sağa 0°, aşağı -90°, sola 180°
        // Biz iğne üstte (90° Unity koordinatında) istiyoruz
        currentAngle = 0f; // 0°'den başla (Unity'de üst pozisyon)
        totalUsedAngle = 0f;
        
        for (int i = 0; i < segments.Count; i++)
        {
            float desiredAngle = segments[i].endAngle; // Geçici olarak saklanmış açı büyüklüğü
            int roundedAngle = Mathf.RoundToInt(desiredAngle); // Kesin açı
            
            // 0°'den başlayıp saat yönüne doğru yerleştir (Unity koordinat sistemi)
            segments[i].startAngle = currentAngle;
            segments[i].endAngle = currentAngle + roundedAngle;
            
            currentAngle += roundedAngle;
            totalUsedAngle += roundedAngle;
        }
        
        // Kalan açı var mı kontrol et
        float remainingAngle = 360f - totalUsedAngle;
        
        // Config'e göre Resource segment ekle
        if (config.generationRules.addResourceSegmentForRemainder && 
            remainingAngle >= config.generationRules.minResourceSegmentAngle)
        {
            PrizeSegment resourceSegment = new PrizeSegment
            {
                segmentName = "Resource",
                startAngle = currentAngle,
                endAngle = currentAngle + remainingAngle,
                segmentColor = config.generationRules.resourceSegmentColor,
                prizeType = PrizeType.Resource,
                resourceAmount = Mathf.RoundToInt(remainingAngle / 5f), // Açı büyüklüğüne göre resource miktarı
                customRewardText = $"+{Mathf.RoundToInt(remainingAngle / 5f)} Coins"
            };
            
            segments.Add(resourceSegment);
            Debug.Log($"Added Resource segment: {remainingAngle:F0}° (+{resourceSegment.resourceAmount} Coins)");
        }
        else if (remainingAngle > 0)
        {
            // Küçük kalan açıyı son segment'e ekle
            if (segments.Count > 0)
            {
                segments[segments.Count - 1].endAngle += remainingAngle;
                Debug.Log($"Added remaining {remainingAngle:F0}° to last segment");
            }
        }
        
        // Final kontrol - toplam 360° olmalı
        float finalTotal = segments.Sum(s => s.AngleSize);
        Debug.Log($"Final wheel total: {finalTotal}° (Target: 360°)");
    }
    
    // Debug için
    [ContextMenu("Generate Test Wheel")]
    public void GenerateTestWheel()
    {
        var segments = GenerateRandomPrizeWheel();
        
        Debug.Log("=== Generated Prize Wheel (Unity Coordinates - 90° = Top) ===");
        foreach (var segment in segments)
        {
            // Unity koordinat sisteminde açıları göster
            float unityStartAngle = segment.startAngle;
            float unityEndAngle = segment.endAngle;
            
            Debug.Log($"{segment.segmentName}: {unityStartAngle:F1}° - {unityEndAngle:F1}° (Size: {segment.AngleSize:F1}°) Color: #{ColorUtility.ToHtmlStringRGB(segment.segmentColor)}");
        }
        
        // PrizeWheelManager'a uygula
        var wheelManager = FindFirstObjectByType<PrizeWheelManager>();
        if (wheelManager != null)
        {
            wheelManager.segments = segments;
            Debug.Log("Applied to PrizeWheelManager!");
        }
    }
}

[System.Serializable]
public class PlannedSegment
{
    public SegmentEffectType type;
    public Rarity rarity;
    public float angle;
}
