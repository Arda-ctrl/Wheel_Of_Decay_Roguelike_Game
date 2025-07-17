using UnityEngine;

/// <summary>
/// ElementalAbilityData - 10 temel elemental yetenek tipinin verilerini tutan ScriptableObject
/// Her yetenek için özel ayarlar içerir ve Inspector'dan ayarlanabilir
/// </summary>
[CreateAssetMenu(fileName = "New Elemental Ability", menuName = "Game/Abilities/Elemental Ability")]
public class ElementalAbilityData : ScriptableObject
{
    [Header("Basic Settings")]
    public string abilityName;
    public string description;
    public Sprite icon;
    
    [Header("Ability Settings")]
    public AbilityType abilityType;
    public float cooldownDuration;
    public float manaCost;
    
    [Header("Element Settings")]
    public ElementType elementType;
    
    [Header("Ability Specific Settings")]
    
    [Header("Elemental Strike Settings")]
    public int stackAmount = 1;
    public float strikeDamage = 10f;
    
    [Header("Elemental Buff Settings")]
    public float damageMultiplier = 1.5f;
    
    [Header("Elemental Projectile Settings")]
    public int attackCountForProjectile = 3;
    public float projectileSpeed = 10f;
    public float projectileDamage = 15f;
    public float projectileRange = 10f;
    public GameObject projectilePrefab;
    
    [Header("Elemental Armor Settings")]
    public float damageReductionPercent = 30f;
    public float armorDuration = 10f;
    public float areaDamageRadius = 3f;
    
    [Header("Elemental Area Settings")]
    public int requiredStacksForArea = 5;
    public float areaDamage = 20f;
    public float areaRadius = 5f;
    public float areaDuration = 5f;
    
    [Header("Elemental Lance Barrage Settings")]
    public int lanceCount = 5;
    public float lanceDamage = 25f;
    public float lanceRange = 8f;
    
    [Header("Elemental Overflow Settings")]
    public int overflowStackAmount = 5;
    public float overflowDamage = 30f;
    public int requiredEnemyKills = 20;
    
    [Header("Elemental Burst Settings")]
    public int burstTriggerStacks = 3;
    public float burstDamage = 40f;
    public float burstRadius = 4f;
    
    [Header("Elemental Aura Settings")]
    public float auraDamage = 5f;
    public float auraRadius = 6f;
    public float auraStackTime = 3f;
    
    [Header("Elemental Orb Settings")]
    public float orbDamage = 15f;
    public float orbDuration = 10f;
    public float orbSpeed = 5f;
    public GameObject orbPrefab;
    
    [Header("Visual and Audio")]
    public GameObject vfxPrefab;
    public AudioClip sfxClip;
    
    /// <summary>
    /// Ability'yi oluşturur ve döndürür
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Oluşturulan ability component</returns>
    public IAbility CreateAbility(GameObject caster)
    {
        IAbility ability = null;
        
        switch (abilityType)
        {
            case AbilityType.ElementalStrike:
                ability = CreateElementalStrike(caster);
                break;
            case AbilityType.ElementalBuff:
                ability = CreateElementalBuff(caster);
                break;
            case AbilityType.ElementalProjectile:
                ability = CreateElementalProjectile(caster);
                break;
            case AbilityType.ElementalArmor:
                ability = CreateElementalArmor(caster);
                break;
            case AbilityType.ElementalArea:
                ability = CreateElementalArea(caster);
                break;
            case AbilityType.ElementalLanceBarrage:
                ability = CreateElementalLanceBarrage(caster);
                break;
            case AbilityType.ElementalOverflow:
                ability = CreateElementalOverflow(caster);
                break;
            case AbilityType.ElementalBurst:
                ability = CreateElementalBurst(caster);
                break;
            case AbilityType.ElementalAura:
                ability = CreateElementalAura(caster);
                break;
            case AbilityType.ElementalOrb:
                ability = CreateElementalOrb(caster);
                break;
        }
        
        return ability;
    }
    
    /// <summary>
    /// Element'i oluşturur ve döndürür
    /// </summary>
    /// <returns>Oluşturulan element</returns>
    public IElement CreateElement()
    {
        IElement element = null;
        
        switch (elementType)
        {
            case ElementType.Fire:
                element = new FireElement();
                break;
            case ElementType.Ice:
                element = new IceElement();
                break;
            case ElementType.Poison:
                element = new PoisonElement();
                break;
            case ElementType.Lightning:
                element = new LightningElement();
                break;
            case ElementType.Earth:
                element = new EarthElement();
                break;
            case ElementType.Wind:
                element = new WindElement();
                break;
        }
        
        return element;
    }
    
    // Ability creation methods - her biri ilgili component'i oluşturur ve ayarlar
    private ElementalStrike CreateElementalStrike(GameObject caster)
    {
        var strike = caster.AddComponent<ElementalStrike>();
        strike.Initialize(this);
        return strike;
    }
    
    private ElementalBuff CreateElementalBuff(GameObject caster)
    {
        var buff = caster.AddComponent<ElementalBuff>();
        buff.Initialize(this);
        return buff;
    }
    
    private ElementalProjectile CreateElementalProjectile(GameObject caster)
    {
        var projectile = caster.AddComponent<ElementalProjectile>();
        projectile.Initialize(this);
        return projectile;
    }
    
    private ElementalArmor CreateElementalArmor(GameObject caster)
    {
        var armor = caster.AddComponent<ElementalArmor>();
        armor.Initialize(this);
        return armor;
    }
    
    private ElementalArea CreateElementalArea(GameObject caster)
    {
        var area = caster.AddComponent<ElementalArea>();
        area.Initialize(this);
        return area;
    }
    
    private ElementalLanceBarrage CreateElementalLanceBarrage(GameObject caster)
    {
        var lance = caster.AddComponent<ElementalLanceBarrage>();
        lance.Initialize(this);
        return lance;
    }
    
    private ElementalOverflow CreateElementalOverflow(GameObject caster)
    {
        var overflow = caster.AddComponent<ElementalOverflow>();
        overflow.Initialize(this);
        return overflow;
    }
    
    private ElementalBurst CreateElementalBurst(GameObject caster)
    {
        var burst = caster.AddComponent<ElementalBurst>();
        burst.Initialize(this);
        return burst;
    }
    
    private ElementalAura CreateElementalAura(GameObject caster)
    {
        var aura = caster.AddComponent<ElementalAura>();
        aura.Initialize(this);
        return aura;
    }
    
    private ElementalOrb CreateElementalOrb(GameObject caster)
    {
        var orb = caster.AddComponent<ElementalOrb>();
        orb.Initialize(this);
        return orb;
    }
    
    /// <summary>
    /// Ability'yi kullanır
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    public void UseAbility(GameObject caster, GameObject target)
    {
        IAbility ability = CreateAbility(caster);
        IElement element = CreateElement();
        
        if (ability != null && element != null)
        {
            ability.UseAbility(caster, target, element);
        }
    }
    
    /// <summary>
    /// Ability'nin kullanılıp kullanılamayacağını kontrol eder
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Kullanılabilir mi?</returns>
    public bool CanUseAbility(GameObject caster)
    {
        IAbility ability = CreateAbility(caster);
        return ability?.CanUseAbility(caster) ?? false;
    }
    
    /// <summary>
    /// Ability'nin cooldown progress'ini döndürür
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <returns>0-1 arası progress değeri</returns>
    public float GetCooldownProgress(GameObject caster)
    {
        IAbility ability = CreateAbility(caster);
        return ability?.GetCooldownProgress() ?? 0f;
    }
} 