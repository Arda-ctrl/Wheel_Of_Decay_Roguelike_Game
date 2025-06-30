using UnityEngine;

/// <summary>
/// Tüm segment effectlerini yöneten sınıf
/// </summary>
public class SegmentEffectHandler : MonoBehaviour
{
    public static SegmentEffectHandler Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Segment effect'ini aktifleştir
    /// </summary>
    public void ActivateSegmentEffect(SegmentData segmentData, int stackCount)
    {
        switch (segmentData.effectID)
        {
            case 1: // DamageBoost
                ActivateDamageBoost(segmentData, stackCount);
                break;
                
            case 2: // DamagePercentageBoost
                ActivateDamagePercentageBoost(segmentData, stackCount);
                break;
                
            default:
                Debug.LogWarning($"Bilinmeyen effect ID: {segmentData.effectID}");
                break;
        }
    }
    
    /// <summary>
    /// Segment effect'ini deaktifleştir
    /// </summary>
    public void DeactivateSegmentEffect(SegmentData segmentData, int stackCount)
    {
        switch (segmentData.effectID)
        {
            case 1: // DamageBoost
                DeactivateDamageBoost(segmentData, stackCount);
                break;
                
            case 2: // DamagePercentageBoost
                DeactivateDamagePercentageBoost(segmentData, stackCount);
                break;
                
            default:
                Debug.LogWarning($"Bilinmeyen effect ID: {segmentData.effectID}");
                break;
        }
    }
    
    #region Effect Methods
    private void ActivateDamageBoost(SegmentData segmentData, int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float bonus = segmentData.damageBoostAmount * stackCount;
            StatsUI.Instance.AddStatBonus(attackDamage: bonus);
            Debug.Log($"Effect ID 1 (DamageBoost): Saldırı gücü {bonus} arttı! (SegmentData'dan) ");
        }
    }
    
    private void DeactivateDamageBoost(SegmentData segmentData, int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float bonus = segmentData.damageBoostAmount * stackCount;
            StatsUI.Instance.RemoveStatBonus(attackDamage: bonus);
            Debug.Log($"Effect ID 1 (DamageBoost): Saldırı gücü bonusu ({bonus}) kaldırıldı. (SegmentData'dan)");
        }
    }
    
    private void ActivateDamagePercentageBoost(SegmentData segmentData, int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float percentageBoost = segmentData.damagePercentageBoostAmount * stackCount;
            StatsUI.Instance.AddDamagePercentageBoost(percentageBoost);
            Debug.Log($"Effect ID 2 (DamagePercentageBoost): Saldırı gücü %{percentageBoost * 100f} arttı! (SegmentData'dan)");
        }
    }
    
    private void DeactivateDamagePercentageBoost(SegmentData segmentData, int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float percentageBoost = segmentData.damagePercentageBoostAmount * stackCount;
            StatsUI.Instance.RemoveDamagePercentageBoost(percentageBoost);
            Debug.Log($"Effect ID 2 (DamagePercentageBoost): Saldırı gücü yüzde bonusu (%{percentageBoost * 100f}) kaldırıldı. (SegmentData'dan)");
        }
    }
    #endregion
} 