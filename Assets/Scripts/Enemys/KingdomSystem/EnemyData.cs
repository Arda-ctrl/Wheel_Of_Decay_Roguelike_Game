using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game/Enemies/Enemy Data")]
public class KingdomEnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public string description;
    public Sprite enemySprite;
    public KingdomType kingdomType;
    public EnemyType enemyType;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float baseSpeed = 5f;
    public float baseDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float stopDistance = 2f;
    public float chaseSpeed = 7f;

    [Header("Combat")]
    public float damageMultiplier = 1f;
    public float defenseMultiplier = 1f;
    public bool canUseAbilities = false;
    public bool isRanged = false;
    public float projectileSpeed = 10f;

    [Header("Rewards")]
    public int experienceReward = 10;
    public int goldReward = 5;
    public GameObject[] possibleDrops;

    [Header("Special Abilities")]
    public bool hasSpecialAbility = false;
    public float specialAbilityCooldown = 5f;
    public float specialAbilityRange = 5f;

    [Header("Visual & Audio")]
    public GameObject deathEffect;
    public AudioClip deathSound;
    public AudioClip attackSound;
    public AudioClip hurtSound;
} 