using UnityEngine;

/// <summary>
/// Saldırı gücünü yüzdelik olarak artıran segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "DamagePercentageBoost", menuName = "Segment Effects/Damage Percentage Boost")]
public class DamagePercentageBoost : SegmentEffect
{
    [Header("Hasar Yüzde Artış Ayarları")]
    [Range(0, 100)]
    public float damagePercentage = 50f; // Yüzdelik artış (örn: 50 = %50 artış)
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            float baseAttackDamage = 10f; // StatsUI'dan alınabilir
            float bonusDamage = baseAttackDamage * (damagePercentage / 100f) * stackCount;
            StatsUI.Instance.AddStatBonus(attackDamage: bonusDamage);
        }
        Debug.Log($"Saldırı gücü %{damagePercentage * stackCount} artırıldı!");
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            float baseAttackDamage = 10f; // StatsUI'dan alınabilir
            float bonusDamage = baseAttackDamage * (damagePercentage / 100f) * stackCount;
            StatsUI.Instance.RemoveStatBonus(attackDamage: bonusDamage);
        }
        Debug.Log("Saldırı gücü yüzde bonusu kaldırıldı.");
    }
}

/// <summary>
/// Can yenileyen segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "HealthRegenEffect", menuName = "Segment Effects/Health Regen")]
public class HealthRegenEffect : SegmentEffect
{
    [Header("Can Yenileme Ayarları")]
    public float healAmount = 10f;
    public float healInterval = 2f;
    public float defenseBonus = 10f; // Can yenileme sırasında defans bonusu
    
    private float lastHealTime;
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        lastHealTime = Time.time;
        if (StatsUI.Instance != null)
        {
            StatsUI.Instance.AddStatBonus(defense: defenseBonus * stackCount);
        }
        Debug.Log($"Can yenileme aktif! Her {healInterval} saniyede {healAmount} can yenilenecek. +{defenseBonus * stackCount} defans bonusu.");
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            StatsUI.Instance.RemoveStatBonus(defense: defenseBonus * stackCount);
        }
        Debug.Log("Can yenileme ve defans bonusu kaldırıldı.");
    }
}

/// <summary>
/// Hız artıran segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "SpeedBoostEffect", menuName = "Segment Effects/Speed Boost")]
public class SpeedBoostEffect : SegmentEffect
{
    [Header("Hız Artırma Ayarları")]
    public float speedMultiplier = 1.3f;
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            float moveSpeedBonus = 5f * (speedMultiplier - 1f) * stackCount;
            float attackSpeedBonus = 1f * (speedMultiplier - 1f) * stackCount;
            StatsUI.Instance.AddStatBonus(
                movementSpeed: moveSpeedBonus,
                attackSpeed: attackSpeedBonus
            );
        }
        Debug.Log($"Hız {speedMultiplier}x artırıldı!");
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            float moveSpeedBonus = 5f * (speedMultiplier - 1f) * stackCount;
            float attackSpeedBonus = 1f * (speedMultiplier - 1f) * stackCount;
            StatsUI.Instance.RemoveStatBonus(
                movementSpeed: moveSpeedBonus,
                attackSpeed: attackSpeedBonus
            );
        }
        Debug.Log("Hız bonusu kaldırıldı.");
    }
}

/// <summary>
/// Özel saldırı efekti (örn: ateş, buz, zehir)
/// </summary>
[CreateAssetMenu(fileName = "ElementalEffect", menuName = "Segment Effects/Elemental")]
public class ElementalEffect : SegmentEffect
{
    [Header("Elemental Ayarları")]
    public ElementType elementType;
    public float elementalDamage = 5f;
    public float criticalBonus = 10f;
    
    public enum ElementType
    {
        Fire,   // Ateş
        Ice,    // Buz
        Poison, // Zehir
        Lightning // Şimşek
    }
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            StatsUI.Instance.AddStatBonus(
                attackDamage: elementalDamage * stackCount,
                criticalChance: criticalBonus * stackCount
            );
        }
        Debug.Log($"{elementType} elemental saldırısı aktif! +{elementalDamage * stackCount} hasar ve +{criticalBonus * stackCount}% kritik şansı.");
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData, int stackCount = 1)
    {
        if (StatsUI.Instance != null)
        {
            StatsUI.Instance.RemoveStatBonus(
                attackDamage: elementalDamage * stackCount,
                criticalChance: criticalBonus * stackCount
            );
        }
        Debug.Log($"{elementType} elemental bonusları kaldırıldı.");
    }
}

/// <summary>
/// Sabit hasar artışı veren segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "DamageBoostEffect", menuName = "Segment Effects/Damage Boost")]
public class DamageBoostEffect : SegmentEffect
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