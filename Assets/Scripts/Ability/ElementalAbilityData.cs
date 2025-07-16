using UnityEngine;

/// <summary>
/// ElementalAbilityData - Elemental ability'lerin verilerini tutan ScriptableObject
/// Hem IAbility hem de IElement referansı içerir ve Inspector'dan ayarlanabilir
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
    [Header("Strike Settings")]
    public int stackAmount = 1;
    
    [Header("Buff Settings")]
    public float damageMultiplier = 1.5f;
    
    [Header("Projectile Settings")]
    public int attackCountForProjectile = 3;
    public float projectileSpeed = 10f;
    public float projectileDamage = 15f;
    public float projectileRange = 10f;
    public GameObject projectilePrefab;
    
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
            case AbilityType.Strike:
                ability = CreateElementalStrike(caster);
                break;
            case AbilityType.Buff:
                ability = CreateElementalBuff(caster);
                break;
            case AbilityType.Projectile:
                ability = CreateElementalProjectile(caster);
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
        }
        
        return element;
    }
    
    /// <summary>
    /// ElementalStrike ability'sini oluşturur
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <returns>ElementalStrike component</returns>
    private ElementalStrike CreateElementalStrike(GameObject caster)
    {
        var strike = caster.AddComponent<ElementalStrike>();
        
        // Settings'i ayarla
        var serializedStrike = strike as MonoBehaviour;
        if (serializedStrike != null)
        {
            // Reflection ile private field'ları ayarla
            var stackAmountField = typeof(ElementalStrike).GetField("stackAmount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stackAmountField?.SetValue(strike, stackAmount);
        }
        
        return strike;
    }
    
    /// <summary>
    /// ElementalBuff ability'sini oluşturur
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <returns>ElementalBuff component</returns>
    private ElementalBuff CreateElementalBuff(GameObject caster)
    {
        var buff = caster.AddComponent<ElementalBuff>();
        
        // Settings'i ayarla
        var serializedBuff = buff as MonoBehaviour;
        if (serializedBuff != null)
        {
            // Reflection ile private field'ları ayarla
            var damageMultiplierField = typeof(ElementalBuff).GetField("damageMultiplier", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            damageMultiplierField?.SetValue(buff, damageMultiplier);
        }
        
        return buff;
    }
    
    /// <summary>
    /// ElementalProjectile ability'sini oluşturur
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <returns>ElementalProjectile component</returns>
    private ElementalProjectile CreateElementalProjectile(GameObject caster)
    {
        var projectile = caster.AddComponent<ElementalProjectile>();
        
        // Settings'i ayarla
        var serializedProjectile = projectile as MonoBehaviour;
        if (serializedProjectile != null)
        {
            // Reflection ile private field'ları ayarla
            var attackCountField = typeof(ElementalProjectile).GetField("attackCountForProjectile", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackCountField?.SetValue(projectile, attackCountForProjectile);
            
            var speedField = typeof(ElementalProjectile).GetField("projectileSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            speedField?.SetValue(projectile, projectileSpeed);
            
            var damageField = typeof(ElementalProjectile).GetField("projectileDamage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            damageField?.SetValue(projectile, projectileDamage);
            
            var rangeField = typeof(ElementalProjectile).GetField("projectileRange", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            rangeField?.SetValue(projectile, projectileRange);
            
            var prefabField = typeof(ElementalProjectile).GetField("projectilePrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prefabField?.SetValue(projectile, projectilePrefab);
        }
        
        return projectile;
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