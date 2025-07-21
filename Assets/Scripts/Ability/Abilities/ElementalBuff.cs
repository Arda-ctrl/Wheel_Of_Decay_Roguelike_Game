using UnityEngine;

/// <summary>
/// ElementalBuff - Düşmanın üzerinde eşleşen elemental stack varsa normal hasarın 1.5x olur
/// Sadece elemental hasarı buff'layacak, diğer damage türleri etkilenmeyecek
/// Kullanım: Player'ın damage calculation'ına entegre edilir
/// </summary>
public class ElementalBuff : MonoBehaviour, IAbility
{
    [Header("Elemental Buff Settings")]
    [SerializeField] private string abilityName = "Elemental Buff";
    [SerializeField] private string description = "Eşleşen elemental stack varsa hasarı 1.5x yapar";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability olduğu için cooldown yok
    [SerializeField] private float manaCost = 0f; // Pasif ability olduğu için mana maliyeti yok
    [SerializeField] private float damageMultiplier = 1.5f; // Hasar çarpanı
    [SerializeField] private float flatBonusDamage = 10f; // Sabit bonus hasar
    [SerializeField] private ElementType targetElementType = ElementType.Fire; // Bu buff hangi element için
    
    private IElement currentElement;
    private bool isActive = true;
    private ElementalAbilityData abilityData;
    
    // IAbility Interface Implementation
    public string AbilityName => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public float CooldownDuration => cooldownDuration;
    public float ManaCost => manaCost;
    
    /// <summary>
    /// Ability'yi ElementalAbilityData ile başlatır
    /// </summary>
    /// <param name="data">Ability verileri</param>
    public void Initialize(ElementalAbilityData data)
    {
        abilityData = data;
        abilityName = data.abilityName;
        description = data.description;
        icon = data.icon;
        cooldownDuration = data.cooldownDuration;
        manaCost = data.manaCost;
        damageMultiplier = data.damageMultiplier;
        flatBonusDamage = data.flatBonusDamage;
        targetElementType = data.elementType; // Element tipini ayarla
    }
    
    /// <summary>
    /// Ability'yi kullanır (pasif ability olduğu için sadece element ayarlar)
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="element">Kullanılacak element</param>
    public void UseAbility(GameObject caster, GameObject target, IElement element)
    {
        currentElement = element;
        Debug.Log($"{caster.name} için {currentElement?.ElementName} buff'ı aktif");
    }
    
    /// <summary>
    /// Ability'nin kullanılıp kullanılamayacağını kontrol eder
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Kullanılabilir mi?</returns>
    public bool CanUseAbility(GameObject caster)
    {
        return isActive;
    }
    
    /// <summary>
    /// Ability'nin cooldown progress'ini döndürür
    /// </summary>
    /// <returns>0-1 arası progress değeri</returns>
    public float GetCooldownProgress()
    {
        return 0f; // Pasif ability olduğu için cooldown yok
    }
    
    /// <summary>
    /// Hasar hesaplaması yapar ve buff uygular
    /// </summary>
    /// <param name="baseDamage">Temel hasar</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="elementType">Hasarın element türü</param>
    /// <returns>Buff'lanmış hasar</returns>
    public float CalculateBuffDamage(float baseDamage, GameObject target, ElementType elementType)
    {
        if (!isActive || currentElement == null) return baseDamage;
        
        // Sadece bu buff'ın hedef elementi için çalış
        if (elementType != targetElementType) return baseDamage;
        
        // Hedefin element stack'lerini kontrol et
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack == null) return baseDamage;
        
        // Eşleşen element stack'i var mı kontrol et
        if (elementStack.HasElementStack(elementType))
        {
            // Sabit flat bonus hasar (stack sayısına bakmaksızın)
            float buffedDamage = baseDamage + flatBonusDamage;
            
            // Buff VFX'i oynat (sadece ilk kez)
            if (!target.GetComponent<BuffApplied>())
            {
                PlayBuffEffects(target);
                target.AddComponent<BuffApplied>();
            }
            
            Debug.Log($"🛡️ {target.name} has {elementType} stack, {targetElementType} buff applied: {baseDamage} -> {buffedDamage} (fixed {damageMultiplier}x)");
            
            return buffedDamage;
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// Buff efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayBuffEffects(GameObject target)
    {
        // Buff VFX'i oynat
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, target.transform.position, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
        }
        
        // Buff SFX'i oynat
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
    }
    
    /// <summary>
    /// Mevcut elementi ayarlar
    /// </summary>
    /// <param name="element">Yeni element</param>
    public void SetElement(IElement element)
    {
        currentElement = element;
    }
    
    /// <summary>
    /// Mevcut elementi döndürür
    /// </summary>
    /// <returns>Mevcut element</returns>
    public IElement GetCurrentElement()
    {
        return currentElement;
    }
    
    /// <summary>
    /// Buff'ı aktif/pasif yapar
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetActive(bool active)
    {
        isActive = active;
        Debug.Log($"🛡️ {targetElementType} Buff ability {(active ? "activated" : "deactivated")}");
    }
    
    /// <summary>
    /// Hasar çarpanını ayarlar
    /// </summary>
    /// <param name="multiplier">Yeni çarpan</param>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }
    
    /// <summary>
    /// Buff'ın aktif olup olmadığını kontrol eder
    /// </summary>
    /// <returns>Buff aktif mi?</returns>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Bu buff'ın hangi element için olduğunu döndürür
    /// </summary>
    /// <returns>Element tipi</returns>
    public ElementType GetTargetElementType()
    {
        return targetElementType;
    }
} 

/// <summary>
/// Buff'ın uygulandığını işaretlemek için kullanılan marker component
/// </summary>
public class BuffApplied : MonoBehaviour
{
    // Bu component sadece buff'ın uygulandığını işaretlemek için kullanılır
    // Herhangi bir özellik içermez
} 