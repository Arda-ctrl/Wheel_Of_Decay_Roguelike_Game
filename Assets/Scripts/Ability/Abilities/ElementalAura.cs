using UnityEngine;

/// <summary>
/// ElementalAura - Yakındaki düşmanlara element stack ekler
/// Sana yakın olan düşmanlar eğer 2 saniye yakında kalırlarsa aynı elementten 1 stack eklenir
/// </summary>
public class ElementalAura : MonoBehaviour, IAbility
{
    [Header("Elemental Aura Settings")]
    [SerializeField] private string abilityName = "Elemental Aura";
    [SerializeField] private string description = "Yakındaki düşmanlara element stack ekler";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private float auraRadius = 6f;
    [SerializeField] private float auraStackTime = 2f; // 2 saniye
    
    private IElement currentElement;
    private ElementalAbilityData abilityData;
    private bool isActive = true;
    private System.Collections.Generic.Dictionary<GameObject, float> enemyAuraTimes = new System.Collections.Generic.Dictionary<GameObject, float>();
    
    // Player'a atandığını kontrol etmek için
    private bool isAttachedToPlayer = false;
    
    // IAbility Interface Implementation
    public string AbilityName => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public float CooldownDuration => cooldownDuration;
    public float ManaCost => manaCost;
    
    private void Start()
    {
        // Player'a atandığını kontrol et
        CheckIfAttachedToPlayer();
    }
    
    /// <summary>
    /// Player'a atandığını kontrol eder
    /// </summary>
    private void CheckIfAttachedToPlayer()
    {
        // PlayerController component'i varsa player'a atanmış demektir
        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            isAttachedToPlayer = true;
            Debug.Log("🔥 ElementalAura attached to Player!");
        }
    }
    
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
        
        // SO'dan aura ayarlarını al
        auraRadius = data.auraRadius;
        auraStackTime = data.auraStackTime;
        
        Debug.Log($"🔥 ElementalAura initialized - Radius: {auraRadius}, StackTime: {auraStackTime}");
    }
    
    private void Update()
    {
        if (!isActive || currentElement == null) return;
        
        // Yakındaki düşmanları bul
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, auraRadius);
        
        // Şu anki frame'de aura içinde olan düşmanları takip et
        var currentFrameEnemies = new System.Collections.Generic.HashSet<GameObject>();
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                GameObject enemy = collider.gameObject;
                currentFrameEnemies.Add(enemy);
                
                // Aura süresini takip et ve stack ekle
                TrackAuraTime(enemy);
            }
        }
        
        // Artık aura içinde olmayan düşmanları temizle
        CleanupAuraTimes(currentFrameEnemies);
    }
    
    /// <summary>
    /// Aura süresini takip eder ve stack ekler
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
                Debug.Log($"🔥 {enemy.name} stayed in {currentElement.ElementName} aura for {auraStackTime}s, adding 1 stack");
            }
            
            // Süreyi sıfırla (her 2 saniyede bir stack eklemek için)
            enemyAuraTimes[enemy] = 0f;
        }
    }
    
    /// <summary>
    /// Aura sürelerini temizler
    /// </summary>
    /// <param name="currentFrameEnemies">Bu frame'de aura içinde olan düşmanlar</param>
    private void CleanupAuraTimes(System.Collections.Generic.HashSet<GameObject> currentFrameEnemies)
    {
        var keysToRemove = new System.Collections.Generic.List<GameObject>();
        
        foreach (var kvp in enemyAuraTimes)
        {
            // Null olan veya artık aura içinde olmayan düşmanları kaldır
            if (kvp.Key == null || !currentFrameEnemies.Contains(kvp.Key))
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            enemyAuraTimes.Remove(key);
            if (key != null)
            {
                Debug.Log($"🔥 {key.name} left the aura, resetting timer");
            }
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
        Debug.Log($"🔥 {caster.name} için {currentElement?.ElementName} aura aktif");
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
        
        Debug.Log($"🔥 ElementalAura element set to: {currentElement?.ElementName}");
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
        
        Debug.Log($"🔥 ElementalAura {(active ? "ACTIVATED" : "DEACTIVATED")}");
    }
    
    /// <summary>
    /// Aktif durumunu döndürür
    /// </summary>
    /// <returns>Aktif mi?</returns>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Aura radius'unu görselleştirmek için gizmos çizer
    /// Player'a atandığında ve seçildiğinde radius'u gösterir
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (isAttachedToPlayer)
        {
            // Aura radius'unu çiz
            Gizmos.color = currentElement != null ? currentElement.ElementColor : Color.yellow;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f); // Transparanlık ekle
            Gizmos.DrawWireSphere(transform.position, auraRadius);
            
            // İç daire çiz
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.1f);
            Gizmos.DrawSphere(transform.position, auraRadius);
        }
    }
    
    /// <summary>
    /// Her zaman görünür aura radius'u (sadece Player'dayken)
    /// </summary>
    private void OnDrawGizmos()
    {
        // Sadece Player'a atandığında ve aktifken çiz
        if (isAttachedToPlayer && isActive && currentElement != null)
        {
            // İnce wireframe çiz
            Gizmos.color = currentElement.ElementColor;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.15f);
            Gizmos.DrawWireSphere(transform.position, auraRadius);
        }
    }
} 