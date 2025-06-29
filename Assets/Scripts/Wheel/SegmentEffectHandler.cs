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
                ActivateDamageBoost(stackCount);
                break;
                
            case 2: // DamagePercentageBoost
                ActivateDamagePercentageBoost(stackCount);
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
                DeactivateDamageBoost(stackCount);
                break;
                
            case 2: // DamagePercentageBoost
                DeactivateDamagePercentageBoost(stackCount);
                break;
                
            default:
                Debug.LogWarning($"Bilinmeyen effect ID: {segmentData.effectID}");
                break;
        }
    }
    
    #region Effect Methods
    private void ActivateDamageBoost(int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float bonus = 2f * stackCount; // Sabit 2f bonus
            StatsUI.Instance.AddStatBonus(attackDamage: bonus);
            Debug.Log($"Effect ID 1 (DamageBoost): Saldırı gücü {bonus} arttı!");
        }
    }
    
    private void DeactivateDamageBoost(int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float bonus = 2f * stackCount; // Sabit 2f bonus
            StatsUI.Instance.RemoveStatBonus(attackDamage: bonus);
            Debug.Log($"Effect ID 1 (DamageBoost): Saldırı gücü bonusu ({bonus}) kaldırıldı.");
        }
    }
    
    private void ActivateDamagePercentageBoost(int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float percentageBoost = (10f / 100f) * stackCount; // Sabit %10 bonus
            StatsUI.Instance.AddDamagePercentageBoost(percentageBoost);
            Debug.Log($"Effect ID 2 (DamagePercentageBoost): Saldırı gücü %{10f * stackCount} arttı!");
        }
    }
    
    private void DeactivateDamagePercentageBoost(int stackCount)
    {
        if (StatsUI.Instance != null)
        {
            float percentageBoost = (10f / 100f) * stackCount; // Sabit %10 bonus
            StatsUI.Instance.RemoveDamagePercentageBoost(percentageBoost);
            Debug.Log($"Effect ID 2 (DamagePercentageBoost): Saldırı gücü yüzde bonusu (%{10f * stackCount}) kaldırıldı.");
        }
    }
    #endregion
} 