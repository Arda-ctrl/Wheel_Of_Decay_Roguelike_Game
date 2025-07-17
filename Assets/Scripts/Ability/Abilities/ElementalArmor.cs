using UnityEngine;

/// <summary>
/// ElementalArmor - Cooldown ile hasar azaltma yeteneği
/// Aldığın hasar elemente göre %30 azaltır ve küçük bir alana element saldırısı olur
/// 10 saniye sonra bu armor yenilenir
/// </summary>
public class ElementalArmor : MonoBehaviour, IAbility
{
    [Header("Elemental Armor Settings")]
    [SerializeField] private string abilityName = "Elemental Armor";
    [SerializeField] private string description = "Aldığın hasar elemente göre %30 azaltır";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 10f;
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private float damageReductionPercent = 30f;
    [SerializeField] private float armorDuration = 10f;
    [SerializeField] private float areaDamageRadius = 3f;
    
    private bool isOnCooldown;
    private float cooldownTimeRemaining;
    private bool isArmorActive;
    private float armorTimeRemaining;
    private IElement currentElement;
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
        damageReductionPercent = data.damageReductionPercent;
        armorDuration = data.armorDuration;
        areaDamageRadius = data.areaDamageRadius;
    }
    
    private void Update()
    {
        // Cooldown kontrolü
        if (isOnCooldown)
        {
            cooldownTimeRemaining -= Time.deltaTime;
            if (cooldownTimeRemaining <= 0)
            {
                isOnCooldown = false;
            }
        }
        
        // Armor süresi kontrolü
        if (isArmorActive)
        {
            armorTimeRemaining -= Time.deltaTime;
            if (armorTimeRemaining <= 0)
            {
                DeactivateArmor();
            }
        }
    }
    
    /// <summary>
    /// Ability'yi kullanır
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="element">Kullanılacak element</param>
    public void UseAbility(GameObject caster, GameObject target, IElement element)
    {
        if (!CanUseAbility(caster)) return;
        
        currentElement = element;
        ActivateArmor();
        
        // VFX ve SFX oynat
        PlayArmorEffects(caster);
        
        Debug.Log($"🛡️ {caster.name} activated {currentElement?.ElementName} armor");
        
        // Cooldown başlat
        StartCooldown();
    }
    
    /// <summary>
    /// Armor'u aktifleştirir
    /// </summary>
    private void ActivateArmor()
    {
        isArmorActive = true;
        armorTimeRemaining = armorDuration;
    }
    
    /// <summary>
    /// Armor'u deaktifleştirir
    /// </summary>
    private void DeactivateArmor()
    {
        isArmorActive = false;
        Debug.Log("🛡️ Elemental Armor deactivated");
    }
    
    /// <summary>
    /// Gelen hasarı azaltır ve alan hasarı uygular
    /// </summary>
    /// <param name="incomingDamage">Gelen hasar</param>
    /// <param name="damageType">Hasar türü</param>
    /// <param name="attacker">Saldıran GameObject</param>
    /// <returns>Azaltılmış hasar</returns>
    public float ReduceDamage(float incomingDamage, ElementType damageType, GameObject attacker)
    {
        if (!isArmorActive || currentElement == null) return incomingDamage;
        
        // Eşleşen element hasarı ise azalt
        if (damageType == currentElement.ElementType)
        {
            float reducedDamage = incomingDamage * (1f - damageReductionPercent / 100f);
            
            // Alan hasarı uygula
            ApplyAreaDamage(attacker);
            
            Debug.Log($"🛡️ {damageType} damage reduced from {incomingDamage} to {reducedDamage}");
            
            return reducedDamage;
        }
        
        return incomingDamage;
    }
    
    /// <summary>
    /// Alan hasarı uygular
    /// </summary>
    /// <param name="center">Merkez GameObject</param>
    private void ApplyAreaDamage(GameObject center)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center.transform.position, areaDamageRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                var health = collider.GetComponent<IHealth>();
                if (health != null)
                {
                    health.TakeDamage(10f); // Sabit alan hasarı
                    
                    // Element stack ekle
                    if (currentElement != null)
                    {
                        currentElement.ApplyElementStack(collider.gameObject, 1);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Ability'nin kullanılıp kullanılamayacağını kontrol eder
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Kullanılabilir mi?</returns>
    public bool CanUseAbility(GameObject caster)
    {
        return !isOnCooldown && !isArmorActive;
    }
    
    /// <summary>
    /// Ability'nin cooldown progress'ini döndürür
    /// </summary>
    /// <returns>0-1 arası progress değeri</returns>
    public float GetCooldownProgress()
    {
        if (!isOnCooldown) return 0f;
        return cooldownTimeRemaining / cooldownDuration;
    }
    
    /// <summary>
    /// Cooldown'u başlatır
    /// </summary>
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimeRemaining = cooldownDuration;
    }
    
    /// <summary>
    /// Armor efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    private void PlayArmorEffects(GameObject caster)
    {
        // Armor VFX'i oynat
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, caster.transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(caster.transform);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
        }
        
        // Armor SFX'i oynat
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
    }
    
    /// <summary>
    /// Armor'ın aktif olup olmadığını kontrol eder
    /// </summary>
    /// <returns>Armor aktif mi?</returns>
    public bool IsArmorActive()
    {
        return isArmorActive;
    }
    
    /// <summary>
    /// Mevcut elementi ayarlar
    /// </summary>
    /// <param name="element">Yeni element</param>
    public void SetElement(IElement element)
    {
        currentElement = element;
    }
} 