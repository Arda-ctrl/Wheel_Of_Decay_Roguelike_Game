using UnityEngine;

/// <summary>
/// Sabit hasar artışı veren segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "DamageBoost", menuName = "Segment Effects/Damage Boost")]
public class DamageBoost : SegmentEffect
{
    [Header("Hasar Artırma Ayarları")]
    public float damageBonus = 2f; // Sabit hasar artışı
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            // Sadece yeni eklenen stack için bonus ver
            float bonus = damageBonus * stackCount;
            StatsUI.Instance.AddStatBonus(attackDamage: bonus);
            Debug.Log($"Saldırı gücü {bonus} arttı!");
        }
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            // Sadece silinen stack için bonusu kaldır
            float bonus = damageBonus * stackCount;
            StatsUI.Instance.RemoveStatBonus(attackDamage: bonus);
            Debug.Log($"Saldırı gücü bonusu ({bonus}) kaldırıldı.");
        }
    }
}

/// <summary>
/// Saldırı gücünü yüzdelik olarak artıran segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "DamagePercentageBoost", menuName = "Segment Effects/Damage Percentage Boost")]
public class DamagePercentageBoost : SegmentEffect
{
    [Header("Hasar Yüzde Artış Ayarları")]
    [Range(0, 100)]
    public float damagePercentage = 10f; // Yüzdelik artış (örn: 10 = %10 artış)
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            // Her stack için yüzdelik artışı ekle (10 = 0.1 yani %10)
            float percentageBoost = (damagePercentage / 100f) * stackCount;
            StatsUI.Instance.AddDamagePercentageBoost(percentageBoost);
            Debug.Log($"Saldırı gücü %{damagePercentage * stackCount} arttı!");
        }
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            float percentageBoost = (damagePercentage / 100f) * stackCount;
            StatsUI.Instance.RemoveDamagePercentageBoost(percentageBoost);
            Debug.Log($"Saldırı gücü yüzde bonusu (%{damagePercentage * stackCount}) kaldırıldı.");
        }
    }
}