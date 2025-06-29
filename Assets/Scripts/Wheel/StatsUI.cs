using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("Stats Text References")]
    public TMP_Text attackDamageText;
    public TMP_Text attackSpeedText;
    public TMP_Text movementSpeedText;
    public TMP_Text defenseText;
    public TMP_Text criticalChanceText;

    [Header("Base Stats")]
    public float baseAttackDamage = 10f;
    public float baseAttackSpeed = 1f;
    public float baseMovementSpeed = 5f;
    public float baseDefense = 5f;
    public float baseCriticalChance = 5f;

    // Bonus değerleri
    private float bonusAttackDamage = 0f;
    private float bonusAttackSpeed = 0f;
    private float bonusMovementSpeed = 0f;
    private float bonusDefense = 0f;
    private float bonusCriticalChance = 0f;
    private float damagePercentageBoost = 0f;

    private static StatsUI instance;
    public static StatsUI Instance => instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddStatBonus(float attackDamage = 0f, float attackSpeed = 0f, float movementSpeed = 0f, float defense = 0f, float criticalChance = 0f)
    {
        bonusAttackDamage += attackDamage;
        bonusAttackSpeed += attackSpeed;
        bonusMovementSpeed += movementSpeed;
        bonusDefense += defense;
        bonusCriticalChance += criticalChance;
        UpdateUI();
    }

    public void RemoveStatBonus(float attackDamage = 0f, float attackSpeed = 0f, float movementSpeed = 0f, float defense = 0f, float criticalChance = 0f)
    {
        bonusAttackDamage -= attackDamage;
        bonusAttackSpeed -= attackSpeed;
        bonusMovementSpeed -= movementSpeed;
        bonusDefense -= defense;
        bonusCriticalChance -= criticalChance;
        UpdateUI();
    }

    public void AddDamagePercentageBoost(float percentage)
    {
        damagePercentageBoost += percentage;
        UpdateUI();
    }

    public void RemoveDamagePercentageBoost(float percentage)
    {
        damagePercentageBoost -= percentage;
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateStatText(attackDamageText, baseAttackDamage + bonusAttackDamage, bonusAttackDamage, "Attack Damage", true);
        UpdateStatText(attackSpeedText, baseAttackSpeed + bonusAttackSpeed, bonusAttackSpeed, "Attack Speed");
        UpdateStatText(movementSpeedText, baseMovementSpeed + bonusMovementSpeed, bonusMovementSpeed, "Movement Speed");
        UpdateStatText(defenseText, baseDefense + bonusDefense, bonusDefense, "Defense");
        UpdateStatText(criticalChanceText, baseCriticalChance + bonusCriticalChance, bonusCriticalChance, "Critical Chance", false, "%");
    }

    private void UpdateStatText(TMP_Text textComponent, float totalValue, float bonusValue, string statName, bool isDamage = false, string suffix = "")
    {
        if (textComponent == null) return;

        if (isDamage)
        {
            float flatDamage = baseAttackDamage + bonusAttackDamage;
            float totalDamage = flatDamage * (1f + damagePercentageBoost);
            float totalBonus = totalDamage - baseAttackDamage;
            textComponent.text = $"{statName}: {totalDamage:F1} ({(totalBonus >= 0 ? "+" : "")}{totalBonus:F1})";
        }
        else
        {
            textComponent.text = $"{statName}: {totalValue:F1} ({(bonusValue >= 0 ? "+" : "")}{bonusValue:F1}){suffix}";
        }
    }
} 