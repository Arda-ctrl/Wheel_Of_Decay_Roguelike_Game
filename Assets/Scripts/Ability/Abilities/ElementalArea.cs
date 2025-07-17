using UnityEngine;

/// <summary>
/// ElementalArea - 5+ stack ile ölen düşman alan hasarı
/// Bir düşmanın üzerinde 5 den fazla elemental stack ile ölürse öldüğü alana dümanlara aynı elementten hasar veren ve stack yerleştiren bir alan oluşturur
/// </summary>
public class ElementalArea : MonoBehaviour, IAbility
{
    [Header("Elemental Area Settings")]
    [SerializeField] private string abilityName = "Elemental Area";
    [SerializeField] private string description = "5+ stack ile ölen düşman alan hasarı oluşturur";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private int requiredStacksForArea = 5;
    [SerializeField] private float areaDamage = 20f;
    [SerializeField] private float areaRadius = 5f;
    [SerializeField] private float areaDuration = 5f;
    [SerializeField] private float damageInterval = 0.5f; // Hasar uygulama aralığı
    
    private IElement currentElement;
    private ElementalAbilityData abilityData;
    private bool isActive = true;
    
    private void Start()
    {
        // Düşman ölüm event'lerini dinle
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEnemyDeath += OnEnemyDeath;
        }
    }
    
    private void OnDestroy()
    {
        // Event listener'ı temizle
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEnemyDeath -= OnEnemyDeath;
        }
    }
    
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
        requiredStacksForArea = data.requiredStacksForArea;
        areaDamage = data.areaDamage;
        areaRadius = data.areaRadius;
        areaDuration = data.areaDuration;
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
        Debug.Log($"{caster.name} için {currentElement?.ElementName} area ability aktif");
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
    /// Düşman öldüğünde alan hasarı kontrol eder ve uygular
    /// </summary>
    /// <param name="deadEnemy">Ölen düşman GameObject</param>
    public void OnEnemyDeath(GameObject deadEnemy)
    {
        if (!isActive || currentElement == null || deadEnemy == null) return;
        
        // Null check'leri ekle
        if (deadEnemy.transform == null) return;
        
        Debug.Log($"🔍 ElementalArea checking enemy death: {deadEnemy.name}");
        
        // Düşmanın element stack'lerini kontrol et
        var elementStack = deadEnemy.GetComponent<ElementStack>();
        if (elementStack == null) 
        {
            Debug.Log($"❌ {deadEnemy.name} has no ElementStack component");
            return;
        }
        
        int stackCount = elementStack.GetElementStackCount(currentElement.ElementType);
        Debug.Log($"📊 {deadEnemy.name} has {stackCount} {currentElement.ElementName} stacks (required: {requiredStacksForArea})");
        
        // Gerekli stack miktarı var mı kontrol et
        if (stackCount >= requiredStacksForArea)
        {
            // Güvenli şekilde pozisyon al
            Vector3 position = deadEnemy.transform.position;
            CreateAreaDamage(position);
            Debug.Log($"💥 {deadEnemy.name} died with {stackCount} {currentElement.ElementName} stacks, creating area damage at {position}");
        }
        else
        {
            Debug.Log($"❌ {deadEnemy.name} didn't have enough stacks for area damage");
        }
    }
    
    /// <summary>
    /// Alan hasarı oluşturur
    /// </summary>
    /// <param name="centerPosition">Merkez pozisyon</param>
    private void CreateAreaDamage(Vector3 centerPosition)
    {
        Debug.Log($"💥 Creating area damage at {centerPosition}");
        
        // Alan hasarı VFX'i oluştur
        if (abilityData?.vfxPrefab != null)
        {
            GameObject areaVFX = Object.Instantiate(abilityData.vfxPrefab, centerPosition, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = areaVFX.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
            
            // VFX'i belirli süre sonra yok et
            Destroy(areaVFX, areaDuration);
        }
        
        // Alan hasarı SFX'i oynat
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
        
        // Alan hasarı uygula (coroutine'i güvenli şekilde başlat)
        if (gameObject != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(ApplyAreaDamageOverTime(centerPosition));
        }
    }
    
    /// <summary>
    /// Zaman içinde alan hasarı uygular
    /// </summary>
    /// <param name="centerPosition">Merkez pozisyon</param>
    private System.Collections.IEnumerator ApplyAreaDamageOverTime(Vector3 centerPosition)
    {
        float elapsedTime = 0f;
        float lastDamageTime = 0f;
        
        while (elapsedTime < areaDuration)
        {
            // Hasar uygulama zamanı geldi mi kontrol et
            if (Time.time - lastDamageTime >= damageInterval)
            {
                // Alan içindeki düşmanları bul
                Collider2D[] colliders = Physics2D.OverlapCircleAll(centerPosition, areaRadius);
                
                foreach (var collider in colliders)
                {
                    if (collider != null && collider.CompareTag("Enemy"))
                    {
                        var health = collider.GetComponent<IHealth>();
                        if (health != null)
                        {
                            // Hasar uygula (interval'e göre ayarla)
                            float damagePerTick = areaDamage * damageInterval;
                            health.TakeDamage(damagePerTick);
                            
                            // Element stack ekle (sadece bir kez)
                            if (currentElement != null)
                            {
                                var elementStack = collider.GetComponent<ElementStack>();
                                if (elementStack != null)
                                {
                                    currentElement.ApplyElementStack(collider.gameObject, 1);
                                }
                            }
                        }
                    }
                }
                
                lastDamageTime = Time.time;
            }
            
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.1f); // 0.1 saniye bekle
        }
        
        Debug.Log($"💥 Area damage effect ended at {centerPosition}");
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
    /// Ability'nin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <returns>Ability aktif mi?</returns>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetActive(bool active)
    {
        isActive = active;
        Debug.Log($"🔥 ElementalArea {(active ? "ACTIVATED" : "DEACTIVATED")}");
    }
} 