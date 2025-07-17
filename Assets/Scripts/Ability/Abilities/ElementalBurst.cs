using UnityEngine;

/// <summary>
/// ElementalBurst - 3 stack olduğunda patlama
/// Bir düşmanda aynı elementten 3 stack olduğu zaman altında küçük bir element patlaması olur hasar verir ve 1 stack daha ekler
/// </summary>
public class ElementalBurst : MonoBehaviour, IAbility
{
    [Header("Elemental Burst Settings")]
    [SerializeField] private string abilityName = "Elemental Burst";
    [SerializeField] private string description = "3 stack olduğunda patlama oluşturur";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private int burstTriggerStacks = 3;
    [SerializeField] private float burstDamage = 40f;
    [SerializeField] private float burstRadius = 4f;
    
    private IElement currentElement;
    private ElementalAbilityData abilityData;
    private bool isActive = true;
    
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
        burstTriggerStacks = data.burstTriggerStacks;
        burstDamage = data.burstDamage;
        burstRadius = data.burstRadius;
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
        Debug.Log($"{caster.name} için {currentElement?.ElementName} burst ability aktif");
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
    /// Element stack eklendiğinde burst kontrolü yapar
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="elementType">Element türü</param>
    /// <param name="stackCount">Stack sayısı</param>
    public void OnElementStackAdded(GameObject target, ElementType elementType, int stackCount)
    {
        if (!isActive || currentElement == null) return;
        
        // Eşleşen element stack'i kontrol et
        if (elementType == currentElement.ElementType && stackCount >= burstTriggerStacks)
        {
            CreateBurst(target);
            Debug.Log($"💥 {target.name} has {stackCount} {elementType} stacks, creating burst");
        }
    }
    
    /// <summary>
    /// Burst patlaması oluşturur
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void CreateBurst(GameObject target)
    {
        // Burst VFX'i oluştur
        if (abilityData?.vfxPrefab != null)
        {
            GameObject burstVFX = Object.Instantiate(abilityData.vfxPrefab, target.transform.position, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = burstVFX.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
            
            // VFX'i kısa süre sonra yok et
            Destroy(burstVFX, 1f);
        }
        
        // Burst SFX'i oynat
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
        
        // Burst hasarı uygula
        ApplyBurstDamage(target);
    }
    
    /// <summary>
    /// Burst hasarını uygular
    /// </summary>
    /// <param name="center">Merkez GameObject</param>
    private void ApplyBurstDamage(GameObject center)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center.transform.position, burstRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                var health = collider.GetComponent<IHealth>();
                if (health != null)
                {
                    // Hasar uygula
                    health.TakeDamage(burstDamage);
                    
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
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetActive(bool active)
    {
        isActive = active;
    }
} 