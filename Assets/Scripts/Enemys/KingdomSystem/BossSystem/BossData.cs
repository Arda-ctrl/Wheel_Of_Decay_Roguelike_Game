using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Data", menuName = "Game/Bosses/Boss Data")]
public class BossData : ScriptableObject
{
    [Header("Boss Info")]
    public string bossName;
    public string description;
    public Sprite bossSprite;
    public KingdomType kingdomType;
    public int bossTier = 1; // 1-4 for main bosses, 5+ for mini bosses

    [Header("Boss Stats")]
    public float maxHealth = 500f;
    public float baseSpeed = 4f;
    public float baseDamage = 25f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;

    [Header("Boss Phases")]
    public float phase2HealthThreshold = 0.5f;
    public float enrageHealthThreshold = 0.25f;
    public bool hasPhase2 = true;
    public bool hasEnrage = true;

    [Header("Boss Abilities")]
    public BossAbility[] phase1Abilities;
    public BossAbility[] phase2Abilities;
    public BossAbility[] enrageAbilities;

    [Header("Boss Rewards")]
    public int experienceReward = 100;
    public int goldReward = 50;
    public GameObject[] guaranteedDrops;
    public GameObject[] rareDrops;
    public float rareDropChance = 0.1f;

    [Header("Boss Visual & Audio")]
    public GameObject bossPrefab;
    public GameObject deathEffect;
    public AudioClip bossMusic;
    public AudioClip deathSound;
    public AudioClip[] abilitySounds;

    [Header("Boss Arena")]
    public GameObject arenaPrefab;
    public Vector2 arenaSize = new Vector2(20f, 20f);
    public bool lockPlayerInArena = true;
} 