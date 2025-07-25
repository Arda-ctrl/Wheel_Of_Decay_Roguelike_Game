using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ElementalBurst - 3-6-9 stackte 3 kere patlayacak sonra patlamayacak
/// Bir düşmanda aynı elementten 3 stack olduğu zaman altında küçük bir element patlaması olur hasar verir.
/// Her düşman için 3 kez patlayabilir: 3, 6, 9 stack'te. Sonra o düşman için deactive olur.
/// </summary>
public class ElementalBurst : MonoBehaviour, IAbility
{
    [Header("Elemental Burst Settings")]
    [SerializeField] private string abilityName = "Elemental Burst";
    [SerializeField] private string description = "3-6-9 stackte 3 kere patlayacak sonra patlamayacak";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private int baseBurstTriggerStacks = 3;
    [SerializeField] private float burstDamage = 40f;
    [SerializeField] private float burstRadius = 4f;
    [SerializeField] private int maxBurstCount = 3; // Maksimum patlama sayısı
    
    private IElement currentElement;
    private ElementalAbilityData abilityData;
    private bool isActive = true;
    
    // Her düşman için patlama takibi
    private Dictionary<GameObject, int> enemyBurstCounts = new Dictionary<GameObject, int>(); // Her düşmanın kaç kez patladığı
    private Dictionary<GameObject, int> enemyCurrentStacks = new Dictionary<GameObject, int>(); // Her düşmanın mevcut stack sayısı
    
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
        baseBurstTriggerStacks = data.burstTriggerStacks;
        burstDamage = data.burstDamage;
        burstRadius = data.burstRadius;
        // maxBurstCount için varsayılan değer kullan (3)
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
        if (elementType != currentElement.ElementType) return;
        
        // Düşman tracking başlat
        if (!enemyBurstCounts.ContainsKey(target))
        {
            enemyBurstCounts[target] = 0;
            enemyCurrentStacks[target] = 0;
        }
        
        // Mevcut stack sayısını güncelle
        enemyCurrentStacks[target] = stackCount;
        
        // Kaç kez patladığını kontrol et
        int currentBurstCount = enemyBurstCounts[target];
        
        // Eğer maksimum patlama sayısına ulaştıysa çık
        if (currentBurstCount >= maxBurstCount)
        {
            return;
        }
        
        // Gereken stack sayısını hesapla (1. patlama: 3, 2. patlama: 6, 3. patlama: 9)
        int requiredStacks = baseBurstTriggerStacks * (currentBurstCount + 1);
        
        // Stack sayısı yeterliyse patlama yap
        if (stackCount >= requiredStacks)
        {
            CreateBurst(target);
            Debug.Log($"💥 {target.name} burst #{currentBurstCount + 1} - {stackCount}/{requiredStacks} {elementType} stacks");
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
        
        // Patlama sonrası işlemler
        HandlePostBurst(target);
    }
    
    /// <summary>
    /// Patlama sonrası işlemleri yapar (stack reset, counter artırma)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void HandlePostBurst(GameObject target)
    {
        if (!enemyBurstCounts.ContainsKey(target)) return;
        
        // Patlama sayısını artır
        enemyBurstCounts[target]++;
        
        // Stack'i reset et
        ResetElementStacks(target);
        
        int burstCount = enemyBurstCounts[target];
        
        if (burstCount >= maxBurstCount)
        {
            Debug.Log($"🚫 {target.name} maksimum patlama sayısına ulaştı ({burstCount}/{maxBurstCount}) - artık patlama olmayacak");
        }
        else
        {
            int nextRequiredStacks = baseBurstTriggerStacks * (burstCount + 1);
            Debug.Log($"⚡ {target.name} sonraki patlama için {nextRequiredStacks} stack gerekli (patlama {burstCount + 1}/{maxBurstCount})");
        }
    }
    
    /// <summary>
    /// Hedef düşmanın element stack'lerini reset eder
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ResetElementStacks(GameObject target)
    {
        // ElementStack'ten belirli element türünü temizle
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null && currentElement != null)
        {
            int currentStacks = elementStack.GetElementStack(currentElement.ElementType);
            if (currentStacks > 0)
            {
                elementStack.RemoveElementStack(currentElement.ElementType, currentStacks);
                Debug.Log($"🧹 {target.name} - {currentElement.ElementType} stack'leri temizlendi: {currentStacks} -> 0");
            }
        }
        
        // İç tracking'i de reset et
        if (enemyCurrentStacks.ContainsKey(target))
        {
            enemyCurrentStacks[target] = 0;
        }
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
                    Debug.Log($"💥 {collider.name} burst hasarı aldı: {burstDamage}");
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
    
    /// <summary>
    /// Ability'nin aktif olup olmadığını döndürür
    /// </summary>
    /// <returns>Aktif mi?</returns>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Düşman öldüğünde veya yok edildiğinde tracking verilerini temizler
    /// </summary>
    /// <param name="enemy">Temizlenecek düşman</param>
    public void CleanupEnemyData(GameObject enemy)
    {
        if (enemyBurstCounts.ContainsKey(enemy))
        {
            enemyBurstCounts.Remove(enemy);
        }
        
        if (enemyCurrentStacks.ContainsKey(enemy))
        {
            enemyCurrentStacks.Remove(enemy);
        }
        
        Debug.Log($"🧹 {enemy.name} için burst verileri temizlendi");
    }
    
    /// <summary>
    /// Tüm tracking verilerini temizler
    /// </summary>
    public void ClearAllEnemyData()
    {
        enemyBurstCounts.Clear();
        enemyCurrentStacks.Clear();
        Debug.Log("🧹 Tüm düşman burst verileri temizlendi");
    }
    
    /// <summary>
    /// Düşmanın mevcut burst durumunu döndürür
    /// </summary>
    /// <param name="enemy">Düşman</param>
    /// <returns>Burst sayısı ve gereken stack bilgisi</returns>
    public (int burstCount, int requiredStacks, bool canBurst) GetEnemyBurstStatus(GameObject enemy)
    {
        if (!enemyBurstCounts.ContainsKey(enemy))
        {
            return (0, baseBurstTriggerStacks, true);
        }
        
        int burstCount = enemyBurstCounts[enemy];
        bool canBurst = burstCount < maxBurstCount;
        int requiredStacks = canBurst ? baseBurstTriggerStacks * (burstCount + 1) : -1;
        
        return (burstCount, requiredStacks, canBurst);
    }
    
    private void OnDestroy()
    {
        ClearAllEnemyData();
    }
} 