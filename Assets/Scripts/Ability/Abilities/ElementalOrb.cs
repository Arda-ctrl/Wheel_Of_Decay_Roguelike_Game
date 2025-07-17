using UnityEngine;

/// <summary>
/// ElementalOrb - Otomatik saldıran küre
/// Düşmanlara olduğu element ile otomatik saldıran bir küre çıkarır. 10 saniye sonra yok olur
/// </summary>
public class ElementalOrb : MonoBehaviour, IAbility
{
    [Header("Elemental Orb Settings")]
    [SerializeField] private string abilityName = "Elemental Orb";
    [SerializeField] private string description = "Otomatik saldıran küre oluşturur";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 20f;
    [SerializeField] private float manaCost = 75f;
    [SerializeField] private float orbDamage = 15f;
    [SerializeField] private float orbDuration = 10f;
    [SerializeField] private float orbSpeed = 5f;
    [SerializeField] private GameObject orbPrefab;
    
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
        orbDamage = data.orbDamage;
        orbDuration = data.orbDuration;
        orbSpeed = data.orbSpeed;
        orbPrefab = data.orbPrefab;
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
    /// Ability'yi kullanır
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="element">Kullanılacak element</param>
    public void UseAbility(GameObject caster, GameObject target, IElement element)
    {
        if (!CanUseAbility(caster)) return;
        
        currentElement = element;
        
        // Orb oluştur
        CreateOrb(caster);
        
        // VFX ve SFX oynat
        PlayOrbEffects(caster);
        
        Debug.Log($"🔮 {caster.name} created {currentElement?.ElementName} orb");
        
        // Cooldown başlat
        StartCooldown();
    }
    
    /// <summary>
    /// Orb oluşturur
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    private void CreateOrb(GameObject caster)
    {
        GameObject orb;
        
        if (orbPrefab != null)
        {
            orb = Object.Instantiate(orbPrefab, caster.transform.position, Quaternion.identity);
        }
        else
        {
            // Varsayılan orb oluştur
            orb = new GameObject("Elemental Orb");
            orb.transform.position = caster.transform.position;
            
            // Sprite renderer ekle
            var spriteRenderer = orb.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = icon;
            spriteRenderer.color = currentElement?.ElementColor ?? Color.white;
            
            // Collider ekle
            var collider = orb.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
        }
        
        // Orb controller ekle
        var orbController = orb.AddComponent<ElementalOrbController>();
        orbController.Initialize(currentElement, orbDamage, orbSpeed, orbDuration, abilityData);
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
        isOnCooldown = true;
        cooldownTimeRemaining = cooldownDuration;
    }
    
    /// <summary>
    /// Orb efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    private void PlayOrbEffects(GameObject caster)
    {
        // Orb VFX'i oynat
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, caster.transform.position, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
        }
        
        // Orb SFX'i oynat
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
}

/// <summary>
/// ElementalOrbController - Orb'un davranışını kontrol eder
/// </summary>
public class ElementalOrbController : MonoBehaviour
{
    private IElement element;
    private float damage;
    private float speed;
    private float duration;
    private ElementalAbilityData abilityData;
    private float lifeTime;
    private GameObject nearestEnemy;
    
    /// <summary>
    /// Orb'u başlatır
    /// </summary>
    /// <param name="element">Element</param>
    /// <param name="damage">Hasar</param>
    /// <param name="speed">Hız</param>
    /// <param name="duration">Süre</param>
    /// <param name="abilityData">Ability verileri</param>
    public void Initialize(IElement element, float damage, float speed, float duration, ElementalAbilityData abilityData)
    {
        this.element = element;
        this.damage = damage;
        this.speed = speed;
        this.duration = duration;
        this.abilityData = abilityData;
        this.lifeTime = 0f;
    }
    
    private void Update()
    {
        lifeTime += Time.deltaTime;
        
        // Süre dolduysa yok et
        if (lifeTime >= duration)
        {
            Destroy(gameObject);
            return;
        }
        
        // En yakın düşmanı bul
        FindNearestEnemy();
        
        // Düşmana doğru hareket et
        if (nearestEnemy != null)
        {
            Vector3 direction = (nearestEnemy.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }
    
    /// <summary>
    /// En yakın düşmanı bulur
    /// </summary>
    private void FindNearestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10f);
        float nearestDistance = float.MaxValue;
        nearestEnemy = null;
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = collider.gameObject;
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Hasar uygula
            var health = other.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            
            // Element stack ekle
            if (element != null)
            {
                element.ApplyElementStack(other.gameObject, 1);
            }
            
            // Orb VFX'i oluştur
            if (abilityData?.vfxPrefab != null)
            {
                GameObject hitVFX = Object.Instantiate(abilityData.vfxPrefab, transform.position, Quaternion.identity);
                
                // Element rengine göre VFX'i ayarla
                var particleSystem = hitVFX.GetComponent<ParticleSystem>();
                if (particleSystem != null && element != null)
                {
                    var main = particleSystem.main;
                    main.startColor = element.ElementColor;
                }
                
                // VFX'i kısa süre sonra yok et
                Destroy(hitVFX, 1f);
            }
            
            // Orb'u yok et
            Destroy(gameObject);
        }
    }
} 