using UnityEngine;

/// <summary>
/// ElementalStrike - Her vuruş hedefe elemental stack yerleştirir
/// Artık silahın türü ne olursa olsun tüm strike'lar sürekli aktif olur
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
    [SerializeField] private int stackAmount = 1; // Her vuruşta eklenecek stack miktarı (1 mermi = 1 stack)
    [SerializeField] private float strikeDamage = 10f; // Strike hasarı
    [SerializeField] private ElementType targetElementType = ElementType.Fire; // Bu strike hangi element için
    
    [Header("Element Strike Effects")]
    [SerializeField] private float fireStackDamage = 5f; // Fire stack artışında verilen hasar
    [SerializeField] private float iceSlowPercent = 20f; // Ice stack aktifken yavaşlatma yüzdesi
    [SerializeField] private float poisonStackDamage = 5f; // Poison stack artışında verilen hasar
    
    private bool isOnCooldown;
    private float cooldownTimeRemaining;
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
        stackAmount = data.stackAmount;
        strikeDamage = data.strikeDamage;
        targetElementType = data.elementType; // Element tipini ayarla
        
        // Element efektlerini data'dan al (eğer varsa)
        if (data.fireStackDamage > 0) fireStackDamage = data.fireStackDamage;
        if (data.iceSlowPercent > 0) iceSlowPercent = data.iceSlowPercent;
        if (data.poisonStackDamage > 0) poisonStackDamage = data.poisonStackDamage;
    }
    
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
    /// Ability'yi kullanır - Her strike sadece +1 stack ekler
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="element">Kullanılacak element</param>
    public void UseAbility(GameObject caster, GameObject target, IElement element)
    {
        if (!CanUseAbility(caster)) return;
        
        currentElement = element;
        
        // Element stack'ini hedefe uygula (1 vuruş = 1 stack)
        if (currentElement != null)
        {
            // Önceki stack sayısını kontrol et
            var elementStack = target.GetComponent<ElementStack>();
            int previousStack = 0;
            if (elementStack != null)
            {
                previousStack = elementStack.GetElementStack(targetElementType);
            }
            
            Debug.Log($"🔥 ElementalStrike: Applying +1 {targetElementType} stack to {target.name}");
            currentElement.ApplyElementStack(target, 1); // Her zaman sadece 1 stack
            
            // Yeni stack sayısını kontrol et
            int newStack = 0;
            if (elementStack != null)
            {
                newStack = elementStack.GetElementStack(targetElementType);
            }
            
            // Sadece stack artışında strike hasarını uygula
            if (newStack > previousStack)
            {
                ApplyStrikeDamage(target);
                Debug.Log($"⚔️ {caster.name} applied +1 {currentElement.ElementName} stack to {target.name} + STRIKE DAMAGE");
            }
            else
            {
                Debug.Log($"⚔️ {caster.name} applied +1 {currentElement.ElementName} stack to {target.name}");
            }
            
            // VFX ve SFX oynat
            PlayStrikeEffects(caster, target, currentElement);
        }
        
        // Cooldown başlat
        StartCooldown();
    }
    
    /// <summary>
    /// Strike hasarını hedefe uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyStrikeDamage(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            // Strike hasarını sadece bir kez ver
            float finalDamage = strikeDamage;
            
            // Fire Strike için ekstra hasar ver (sadece bir kez, stack kaç olursa olsun)
            if (targetElementType == ElementType.Fire)
            {
                finalDamage = fireStackDamage;
                Debug.Log($"🔥 Fire Strike sabit damage: {finalDamage}");
            }
            else if (targetElementType == ElementType.Poison)
            {
                finalDamage = poisonStackDamage;
                Debug.Log($"☠️ Poison Strike sabit damage: {finalDamage}");
            }
            else if (targetElementType == ElementType.Ice)
            {
                // Ice Strike sadece slow uygular, ekstra hasar vermez
                Debug.Log($"❄️ Ice Strike: Sadece slow, ekstra damage yok");
                // Slow tekrar tekrar uygulanmasın, sadece ilk stackte uygula
                var moveable = target.GetComponent<IMoveable>();
                if (moveable != null && moveable.GetCurrentSpeed() >= moveable.GetBaseSpeed())
                {
                    float slowPercent = iceSlowPercent / 100f;
                    moveable.SetSpeedMultiplier(1f - slowPercent);
                }
                return;
            }
            
            health.TakeDamage(finalDamage);
        }
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
    /// <param name="element">Element (VFX rengi için)</param>
    private void PlayStrikeEffects(GameObject caster, GameObject target, IElement element)
    {
        // Strike VFX'i oynat
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, target.transform.position, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && element != null)
            {
                var main = particleSystem.main;
                main.startColor = element.ElementColor;
            }
        }
        
        // Strike SFX'i oynat
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
    }
    
    /// <summary>
    /// Mevcut elementi ayarlar (artık kullanılmıyor)
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
    
    /// <summary>
    /// Bu strike'ın hangi element için olduğunu döndürür
    /// </summary>
    /// <returns>Element tipi</returns>
    public ElementType GetTargetElementType()
    {
        return targetElementType;
    }
    
    /// <summary>
    /// Fire stack damage değerini döndürür
    /// </summary>
    /// <returns>Fire stack damage</returns>
    public float GetFireStackDamage()
    {
        return fireStackDamage;
    }
    
    /// <summary>
    /// Ice slow percent değerini döndürür
    /// </summary>
    /// <returns>Ice slow percent</returns>
    public float GetIceSlowPercent()
    {
        return iceSlowPercent;
    }
    
    /// <summary>
    /// Poison stack damage değerini döndürür
    /// </summary>
    /// <returns>Poison stack damage</returns>
    public float GetPoisonStackDamage()
    {
        return poisonStackDamage;
    }
} 