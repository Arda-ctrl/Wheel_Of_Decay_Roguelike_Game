using UnityEngine;

/// <summary>
/// ElementalAura - Yakındaki düşmanlara sürekli hasar
/// Sana yakın olan düşmanlar element hasarı alır eğer 3 saniye yakında kalırlarsa aynı elementten 1 stack eklenir
/// </summary>
public class ElementalAura : MonoBehaviour, IAbility
{
    [Header("Elemental Aura Settings")]
    [SerializeField] private string abilityName = "Elemental Aura";
    [SerializeField] private string description = "Yakındaki düşmanlara sürekli hasar verir";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private float auraDamage = 5f;
    [SerializeField] private float auraRadius = 6f;
    [SerializeField] private float auraStackTime = 3f;
    
    private IElement currentElement;
    private ElementalAbilityData abilityData;
    private bool isActive = true;
    private System.Collections.Generic.Dictionary<GameObject, float> enemyAuraTimes = new System.Collections.Generic.Dictionary<GameObject, float>();
    
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
        auraDamage = data.auraDamage;
        auraRadius = data.auraRadius;
        auraStackTime = data.auraStackTime;
    }
    
    private void Update()
    {
        if (!isActive || currentElement == null) return;
        
        // Yakındaki düşmanları bul
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, auraRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                GameObject enemy = collider.gameObject;
                
                // Aura hasarı uygula
                ApplyAuraDamage(enemy);
                
                // Aura süresini takip et
                TrackAuraTime(enemy);
            }
        }
        
        // Aura süresini temizle
        CleanupAuraTimes();
    }
    
    /// <summary>
    /// Aura hasarını uygular
    /// </summary>
    /// <param name="enemy">Düşman GameObject</param>
    private void ApplyAuraDamage(GameObject enemy)
    {
        var health = enemy.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(auraDamage * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Aura süresini takip eder
    /// </summary>
    /// <param name="enemy">Düşman GameObject</param>
    private void TrackAuraTime(GameObject enemy)
    {
        if (!enemyAuraTimes.ContainsKey(enemy))
        {
            enemyAuraTimes[enemy] = 0f;
        }
        
        enemyAuraTimes[enemy] += Time.deltaTime;
        
        // Gerekli süreye ulaşıldıysa stack ekle
        if (enemyAuraTimes[enemy] >= auraStackTime)
        {
            if (currentElement != null)
            {
                currentElement.ApplyElementStack(enemy, 1);
                Debug.Log($"🔥 {enemy.name} stayed in {currentElement.ElementName} aura for {auraStackTime}s, adding stack");
            }
            
            // Süreyi sıfırla
            enemyAuraTimes[enemy] = 0f;
        }
    }
    
    /// <summary>
    /// Aura sürelerini temizler
    /// </summary>
    private void CleanupAuraTimes()
    {
        var keysToRemove = new System.Collections.Generic.List<GameObject>();
        
        foreach (var kvp in enemyAuraTimes)
        {
            if (kvp.Key == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            enemyAuraTimes.Remove(key);
        }
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
        Debug.Log($"{caster.name} için {currentElement?.ElementName} aura aktif");
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
    /// Aura VFX'ini oluşturur
    /// </summary>
    private void CreateAuraVFX()
    {
        if (abilityData?.vfxPrefab != null)
        {
            GameObject auraVFX = Object.Instantiate(abilityData.vfxPrefab, transform.position, Quaternion.identity);
            auraVFX.transform.SetParent(transform);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = auraVFX.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
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
        
        // Element değiştiğinde aura VFX'ini yeniden oluştur
        if (isActive)
        {
            CreateAuraVFX();
        }
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
        
        if (active && currentElement != null)
        {
            CreateAuraVFX();
        }
    }
} 