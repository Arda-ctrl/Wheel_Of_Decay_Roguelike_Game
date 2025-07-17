using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Game/Abilities/New Ability")]
public class AbilityData : ScriptableObject
{
    [Header("Basic Settings")]
    public string abilityName;
    public string description;
    public Sprite icon;

    [Header("Gameplay Settings")]
    public float damage;
    public float cooldownDuration;
    public AbilityEffectType effectType;
    
    [Header("Visual and Audio")]
    public GameObject vfxPrefab;
    public AudioClip sfxClip;
    
    [Header("Effect Settings")]
    public float effectDuration = 3f;
    public float effectRadius = 1f;

    [Header("Poison Settings")]
    public float poisonSlowAmount = 0.2f;
    public float poisonTickRate = 1f;
    public int maxPoisonStacks = 3;
    public float stackDamageMultiplier = 0.5f;

    [Header("Fire Settings")]
    public float initialBurstDamage = 20f;
    public float igniteDuration = 3f;
    public float igniteTickRate = 0.25f; // 4 ticks per second
    public bool hasAreaEffect = false;
    public float areaEffectRadius = 3f;

    [Header("Ice Settings")]
    public float freezeChance = 0.2f;
    public float freezeDuration = 2f;
    public float chillSlowAmount = 0.5f;
    public float chillDuration = 4f;
    
    [Header("Elemental Stack Settings")]
    public bool hasElementalAbility = false;
    public bool hasElementalBuff = false;
    public int maxElementalStacks = 5;
    public float elementalStackDecayTime = 15f;
    public float elementalDamageMultiplierPerStack = 0.5f;
    public float normalElementalDamage1Stack = 10f;
    public float normalElementalDamage2PlusStacks = 15f;
    public float normalElementalDamagePerAdditionalStack = 5f;
} 