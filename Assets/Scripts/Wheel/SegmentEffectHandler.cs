using UnityEngine;

public class SegmentEffectHandler : MonoBehaviour
{
    public static SegmentEffectHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void ActivateSegmentEffect(SegmentData segmentData, int stackCount)
    {
        float totalAmount = segmentData.statAmount * stackCount;
        if (StatsUI.Instance != null)
        {
            switch (segmentData.statType)
            {
                case StatType.Attack:
                    StatsUI.Instance.AddStatBonus(attackDamage: totalAmount);
                    break;
                case StatType.Defence:
                    StatsUI.Instance.AddStatBonus(defense: totalAmount);
                    break;
                case StatType.AttackSpeed:
                    StatsUI.Instance.AddStatBonus(attackSpeed: totalAmount);
                    break;
                case StatType.MovementSpeed:
                    StatsUI.Instance.AddStatBonus(movementSpeed: totalAmount);
                    break;
                case StatType.CriticalChance:
                    StatsUI.Instance.AddStatBonus(criticalChance: totalAmount);
                    break;
            }
        }
    }
    public void DeactivateSegmentEffect(SegmentData segmentData, int stackCount)
    {
        float totalAmount = segmentData.statAmount * stackCount;
        if (StatsUI.Instance != null)
        {
            switch (segmentData.statType)
            {
                case StatType.Attack:
                    StatsUI.Instance.RemoveStatBonus(attackDamage: totalAmount);
                    break;
                case StatType.Defence:
                    StatsUI.Instance.RemoveStatBonus(defense: totalAmount);
                    break;
                case StatType.AttackSpeed:
                    StatsUI.Instance.RemoveStatBonus(attackSpeed: totalAmount);
                    break;
                case StatType.MovementSpeed:
                    StatsUI.Instance.RemoveStatBonus(movementSpeed: totalAmount);
                    break;
                case StatType.CriticalChance:
                    StatsUI.Instance.RemoveStatBonus(criticalChance: totalAmount);
                    break;
            }
        }
    }
} 