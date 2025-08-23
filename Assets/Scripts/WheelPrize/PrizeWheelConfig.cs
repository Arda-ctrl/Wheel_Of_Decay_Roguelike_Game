using UnityEngine;
using System.Collections.Generic;

public enum RarityPreset
{
    Balanced,       // 50%, 30%, 15%, 4%, 1%
    CommonFocused,  // 70%, 20%, 8%, 2%, 0%
    RareFocused,    // 30%, 30%, 25%, 10%, 5%
    HighRisk,       // 20%, 20%, 30%, 20%, 10%
    Custom          // Manuel ayarlama
}

public enum SegmentCountPreset
{
    Tiny,      // 3-4 segment
    Small,     // 4-6 segment  
    Medium,    // 5-8 segment
    Large,     // 6-10 segment
    Custom     // Manuel ayarlama
}

[CreateAssetMenu(fileName = "PrizeWheelConfig", menuName = "Prize Wheel/Prize Wheel Config")]
public class PrizeWheelConfig : ScriptableObject
{
    [Header("Segment Angles by Type & Rarity")]
    public StatSegmentAngles statAngles;
    public WheelManipulationAngles wheelAngles;
    public CurseAngles curseAngles;
    public OnRemoveAngles onRemoveAngles;
    
    [Header("Generation Rules")]
    public PrizeWheelRules generationRules;
    
    [Header("Visual Settings")]
    public Color defaultLineColor = Color.black;
    public float defaultLineWidth = 0.02f;
    
    [Header("Rarity Distribution")]
    public RarityPreset rarityPreset = RarityPreset.Balanced;
    
    [Range(0, 100)]
    public float commonChance = 50f;
    [Range(0, 100)]
    public float uncommonChance = 30f;
    [Range(0, 100)]
    public float rareChance = 15f;
    [Range(0, 100)]
    public float epicChance = 4f;
    [Range(0, 100)]
    public float legendaryChance = 1f;
    
    public float totalRarityPercentage = 100f;
    
    public float GetAngleForSegment(SegmentData segmentData)
    {
        switch (segmentData.effectType)
        {
            case SegmentEffectType.StatBoost:
                return statAngles.GetAngleByRarity(segmentData.rarity);
            case SegmentEffectType.WheelManipulation:
                return wheelAngles.GetAngleByRarity(segmentData.rarity);
            case SegmentEffectType.CurseEffect:
                return curseAngles.GetAngleByRarity(segmentData.rarity);
            case SegmentEffectType.OnRemoveEffect:
                return onRemoveAngles.GetAngleByRarity(segmentData.rarity);
            default:
                return 60f; // Varsayılan
        }
    }
    
    public int GetRandomSegmentCount()
    {
        if (generationRules == null) 
        {
            Debug.LogWarning("GenerationRules is null, using default segment count");
            return 4;
        }
        return Random.Range(generationRules.minSegmentCount, generationRules.maxSegmentCount + 1);
    }
    
    private void OnValidate()
    {
        ApplyRarityPreset();
        
        if (generationRules != null)
        {
            generationRules.ApplySegmentCountPreset();
        }
        
        if (Application.isPlaying)
        {
            ValidateRarityPercentages();
        }
    }
    
    public void ApplyRarityPreset()
    {
        switch (rarityPreset)
        {
            case RarityPreset.Balanced:
                commonChance = 50f;
                uncommonChance = 30f;
                rareChance = 15f;
                epicChance = 4f;
                legendaryChance = 1f;
                break;
                
            case RarityPreset.CommonFocused:
                commonChance = 70f;
                uncommonChance = 20f;
                rareChance = 8f;
                epicChance = 2f;
                legendaryChance = 0f;
                break;
                
            case RarityPreset.RareFocused:
                commonChance = 30f;
                uncommonChance = 30f;
                rareChance = 25f;
                epicChance = 10f;
                legendaryChance = 5f;
                break;
                
            case RarityPreset.HighRisk:
                commonChance = 20f;
                uncommonChance = 20f;
                rareChance = 30f;
                epicChance = 20f;
                legendaryChance = 10f;
                break;
                
            case RarityPreset.Custom:
                break;
        }
        
        ValidateRarityPercentages();
    }
    
    public void ValidateRarityPercentages()
    {
        totalRarityPercentage = commonChance + uncommonChance + rareChance + epicChance + legendaryChance;
        
        if (Mathf.Abs(totalRarityPercentage - 100f) > 0.1f)
        {
            Debug.LogWarning($"Rarity percentages don't add up to 100%! Current total: {totalRarityPercentage}%");
        }
    }
    
    public Rarity GetRandomRarity()
    {
        ValidateRarityPercentages();
        
        float random = Random.Range(0f, totalRarityPercentage);
        float cumulative = 0f;
        
        cumulative += legendaryChance;
        if (random <= cumulative) return Rarity.Legendary;
        
        cumulative += epicChance;
        if (random <= cumulative) return Rarity.Epic;
        
        cumulative += rareChance;
        if (random <= cumulative) return Rarity.Rare;
        
        cumulative += uncommonChance;
        if (random <= cumulative) return Rarity.Uncommon;
        
        return Rarity.Common;
    }
}

[System.Serializable]
public class StatSegmentAngles
{
    [Header("Stat Boost Angles")]
    public float common = 80f;
    public float uncommon = 60f;
    public float rare = 40f;
    public float epic = 25f;
    public float legendary = 15f;
    
    public float GetAngleByRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return common;
            case Rarity.Uncommon: return uncommon;
            case Rarity.Rare: return rare;
            case Rarity.Epic: return epic;
            case Rarity.Legendary: return legendary;
            default: return common;
        }
    }
}

[System.Serializable]
public class WheelManipulationAngles
{
    [Header("Wheel Manipulation Angles")]
    public float common = 70f;
    public float uncommon = 50f;
    public float rare = 35f;
    public float epic = 20f;
    public float legendary = 10f;
    
    public float GetAngleByRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return common;
            case Rarity.Uncommon: return uncommon;
            case Rarity.Rare: return rare;
            case Rarity.Epic: return epic;
            case Rarity.Legendary: return legendary;
            default: return common;
        }
    }
}

[System.Serializable]
public class CurseAngles
{
    [Header("Curse Effect Angles")]
    public float common = 30f;
    public float uncommon = 25f;
    public float rare = 20f;
    public float epic = 15f;
    public float legendary = 10f;
    
    public float GetAngleByRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return common;
            case Rarity.Uncommon: return uncommon;
            case Rarity.Rare: return rare;
            case Rarity.Epic: return epic;
            case Rarity.Legendary: return legendary;
            default: return common;
        }
    }
}

[System.Serializable]
public class OnRemoveAngles
{
    [Header("OnRemove Effect Angles")]
    public float common = 60f;
    public float uncommon = 45f;
    public float rare = 30f;
    public float epic = 20f;
    public float legendary = 15f;
    
    public float GetAngleByRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return common;
            case Rarity.Uncommon: return uncommon;
            case Rarity.Rare: return rare;
            case Rarity.Epic: return epic;
            case Rarity.Legendary: return legendary;
            default: return common;
        }
    }
}

[System.Serializable]
public class PrizeWheelRules
{
    [Header("Segment Count Rules")]
    [Range(3, 6)]
    public int minSegmentCount = 3;
    [Range(3, 6)]
    public int maxSegmentCount = 6;
    public SegmentCountPreset segmentCountPreset = SegmentCountPreset.Tiny;
    
    [Header("Type Limits")]
    [Range(0, 5)]
    public int maxCurseSegments = 1;
    [Range(0, 3)]
    public int maxOnRemoveSegments = 1;
    
    [Header("Special Rules")]
    public bool guaranteeAtLeastOneGoodSegment = true;
    public bool avoidAllCurseWheels = true;
    public bool balanceWheelSize = true;
    public bool preventDuplicateSegments = true; // Aynı segment 2 kez gelmesin
    
    [Header("Resource Segment")]
    public bool addResourceSegmentForRemainder = true;
    public int minResourceSegmentAngle = 10;
    public Color resourceSegmentColor = new Color(0.6f, 0.4f, 0.2f, 1f); // Kahve rengi
    
    public bool CanAddSegmentType(SegmentEffectType type, List<PrizeSegment> currentSegments)
    {
        int currentCount = 0;
        foreach (var segment in currentSegments)
        {
            if (segment.segmentReward != null && segment.segmentReward.effectType == type)
                currentCount++;
        }
        
        switch (type)
        {
            case SegmentEffectType.CurseEffect:
                return currentCount < maxCurseSegments;
            case SegmentEffectType.OnRemoveEffect:
                return currentCount < maxOnRemoveSegments;
            default:
                return true;
        }
    }
    
    public void ApplySegmentCountPreset()
    {
        switch (segmentCountPreset)
        {
            case SegmentCountPreset.Tiny:
                minSegmentCount = 3;
                maxSegmentCount = 4;
                break;
                
            case SegmentCountPreset.Small:
                minSegmentCount = 4;
                maxSegmentCount = 6;
                break;
                
            case SegmentCountPreset.Medium:
                minSegmentCount = 5;
                maxSegmentCount = 8;
                break;
                
            case SegmentCountPreset.Large:
                minSegmentCount = 6;
                maxSegmentCount = 10;
                break;
                
            case SegmentCountPreset.Custom:
                break;
        }
    }
}
