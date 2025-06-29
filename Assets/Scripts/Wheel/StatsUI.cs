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

    private static StatsUI instance;
    public static StatsUI Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
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

    private void UpdateUI()
    {
        if (attackDamageText) 
        {
            float totalDamage = baseAttackDamage + bonusAttackDamage;
            attackDamageText.text = $"Attack Damage: {totalDamage:F1} ({(bonusAttackDamage >= 0 ? "+" : "")}{bonusAttackDamage:F1})";
        }

        if (attackSpeedText)
        {
            float totalSpeed = baseAttackSpeed + bonusAttackSpeed;
            attackSpeedText.text = $"Attack Speed: {totalSpeed:F1} ({(bonusAttackSpeed >= 0 ? "+" : "")}{bonusAttackSpeed:F1})";
        }

        if (movementSpeedText)
        {
            float totalMove = baseMovementSpeed + bonusMovementSpeed;
            movementSpeedText.text = $"Movement Speed: {totalMove:F1} ({(bonusMovementSpeed >= 0 ? "+" : "")}{bonusMovementSpeed:F1})";
        }

        if (defenseText)
        {
            float totalDef = baseDefense + bonusDefense;
            defenseText.text = $"Defense: {totalDef:F1} ({(bonusDefense >= 0 ? "+" : "")}{bonusDefense:F1})";
        }

        if (criticalChanceText)
        {
            float totalCrit = baseCriticalChance + bonusCriticalChance;
            criticalChanceText.text = $"Critical Chance: {totalCrit:F1}% ({(bonusCriticalChance >= 0 ? "+" : "")}{bonusCriticalChance:F1}%)";
        }
    }

    private void Start()
    {
        UpdateUI(); // Başlangıç değerlerini göster
    }
} 