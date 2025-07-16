using UnityEngine;

/// <summary>
/// ElementalStrike - Her vuruş hedefe elemental stack yerleştirir
/// Her element için çalışabilir
/// Kullanım: Player'ın normal saldırısına entegre edilir
/// </summary>
public class ElementalStrike : MonoBehaviour, IAbility
{
    [Header("Elemental Strike Settings")]
    [SerializeField] private string abilityName = "Elemental Strike";
    [SerializeField] private string description = "Her vuruş hedefe elemental stack yerleştirir";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Normal saldırı için cooldown yok
    [SerializeField] private float manaCost = 0f; // Normal saldırı için mana maliyeti yok
    [SerializeField] private int stackAmount = 1; // Her vuruşta eklenecek stack miktarı
    
    private bool isOnCooldown;
    private float cooldownTimeRemaining;
    private IElement currentElement;
    
    // IAbility Interface Implementation
    public string AbilityName => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public float CooldownDuration => cooldownDuration;
    public float ManaCost => manaCost;
    
    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimeRemaining -= Time.deltaTime;
            if (cooldownTimeRemaining <= 0)
            {
                isOnCooldown = false;
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
        
        // Element stack'ini hedefe uygula
        if (currentElement != null)
        {
            currentElement.ApplyElementStack(target, stackAmount);
            
            // VFX ve SFX oynat
            PlayStrikeEffects(caster, target);
            
            Debug.Log($"⚔️ {caster.name} applied {currentElement.ElementName} stack to {target.name}");
        }
        
        // Cooldown başlat
        StartCooldown();
    }
    
    /// <summary>
    /// Ability'nin kullanılıp kullanılamayacağını kontrol eder
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Kullanılabilir mi?</returns>
    public bool CanUseAbility(GameObject caster)
    {
        return !isOnCooldown;
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
        if (cooldownDuration > 0)
        {
            isOnCooldown = true;
            cooldownTimeRemaining = cooldownDuration;
        }
    }
    
    /// <summary>
    /// Strike efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    private void PlayStrikeEffects(GameObject caster, GameObject target)
    {
        // Strike VFX'i oynat
        var strikeVFX = Resources.Load<GameObject>("Prefabs/Effects/ElementalStrikeVFX");
        if (strikeVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(strikeVFX, target.transform.position, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
        }
        
        // Strike SFX'i oynat
        AudioManager.Instance?.PlaySFX(18);
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
    /// Stack miktarını ayarlar
    /// </summary>
    /// <param name="amount">Yeni stack miktarı</param>
    public void SetStackAmount(int amount)
    {
        stackAmount = amount;
    }
} 