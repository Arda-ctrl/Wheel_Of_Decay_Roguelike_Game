using UnityEngine;

/// <summary>
/// ElementalOverflow - Big, odadaki tüm düşmanlara 5 stack
/// Odadaki tüm düşmanlara hangi element ise 5 stack yükler ve hasar verir
/// </summary>
public class ElementalOverflow : MonoBehaviour, IAbility
{
    [Header("Elemental Overflow Settings")]
    [SerializeField] private string abilityName = "Elemental Overflow";
    [SerializeField] private string description = "Odadaki tüm düşmanlara 5 stack yükler";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 30f;
    [SerializeField] private float manaCost = 0f; // SO'dan ayarlanacak
    [SerializeField] private int overflowStackAmount = 5;
    [SerializeField] private float overflowDamage = 30f;
    [SerializeField] private int requiredEnemyKills = 20;
    
    private bool isOnCooldown;
    private float cooldownTimeRemaining;
    private IElement currentElement;
    private ElementalAbilityData abilityData;
    private int enemyKillCount = 0;
    
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
        overflowStackAmount = data.overflowStackAmount;
        overflowDamage = data.overflowDamage;
        requiredEnemyKills = data.requiredEnemyKills;
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
        
        // Mana tüket
        var manaController = PlayerManaController.Instance;
        if (manaController != null)
        {
            if (!manaController.ConsumeMana(manaCost))
            {
                Debug.Log($"❌ Failed to consume mana for {abilityName}!");
                return;
            }
                         Debug.Log($"💧 {abilityName} consumed {manaCost} mana (from SO settings)");
        }
        
        currentElement = element;
        
        Debug.Log($"🎯 ElementalOverflow: Caster is {caster.name} with tag '{caster.tag}'");
        
        // Odadaki tüm düşmanları bul
        Collider2D[] colliders = Physics2D.OverlapCircleAll(caster.transform.position, 50f); // Geniş alan
        var enemies = new System.Collections.Generic.List<GameObject>();
        
        Debug.Log($"🔍 Found {colliders.Length} total colliders in range");
        
        foreach (var collider in colliders)
        {
            // Sadece Enemy tag'ine sahip objeler VE caster kendisi değil
            if (collider.CompareTag("Enemy") && collider.gameObject != caster)
            {
                enemies.Add(collider.gameObject);
                Debug.Log($"🎯 Found enemy for overflow: {collider.gameObject.name}");
            }
            else if (collider.gameObject == caster)
            {
                Debug.Log($"🚫 Skipping caster (self): {collider.gameObject.name}");
            }
        }
        
        Debug.Log($"✅ Final enemy count for overflow: {enemies.Count}");
        
        if (enemies.Count > 0)
        {
            // Overflow saldırısını başlat
            StartCoroutine(PerformOverflowAttack(caster, enemies));
            
            // Sadece SFX oynat (VFX her düşman için ayrı ayrı oynatılıyor)
            PlayOverflowSFX();
            
            Debug.Log($"💥 {caster.name} performed {currentElement?.ElementName} overflow on {enemies.Count} enemies");
        }
        
        // Cooldown başlat
        StartCooldown();
    }
    
    /// <summary>
    /// Overflow saldırısını gerçekleştirir
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <param name="enemies">Hedef düşmanlar</param>
    private System.Collections.IEnumerator PerformOverflowAttack(GameObject caster, System.Collections.Generic.List<GameObject> enemies)
    {
        foreach (var enemy in enemies)
        {
            // Güvenlik kontrolü: Sadece Enemy tag'ine sahip ve caster olmayan objeler
            if (!enemy.CompareTag("Enemy") || enemy == caster)
            {
                Debug.Log($"⚠️ Skipping invalid target: {enemy.name} (Tag: {enemy.tag})");
                continue;
            }
            
            Debug.Log($"💥 Applying overflow to enemy: {enemy.name}");
            
            // Hasar uygula
            var health = enemy.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(overflowDamage);
                Debug.Log($"💥 Dealt {overflowDamage} damage to {enemy.name}");
            }
            
            // Element stack ekle
            if (currentElement != null)
            {
                currentElement.ApplyElementStack(enemy, overflowStackAmount);
                Debug.Log($"🔥 Applied {overflowStackAmount} {currentElement.ElementType} stacks to {enemy.name}");
            }
            
            // Düşmanın üzerinde VFX oluştur
            if (abilityData?.vfxPrefab != null)
            {
                GameObject overflowVFX = Object.Instantiate(abilityData.vfxPrefab, enemy.transform.position, Quaternion.identity);
                
                // Element rengine göre VFX'i ayarla
                var particleSystem = overflowVFX.GetComponent<ParticleSystem>();
                if (particleSystem != null && currentElement != null)
                {
                    var main = particleSystem.main;
                    main.startColor = currentElement.ElementColor;
                }
                
                // VFX'i kısa süre sonra yok et
                Destroy(overflowVFX, 2f);
                Debug.Log($"✨ Created overflow VFX for {enemy.name} (will destroy in 2s)");
            }
            
            // Kısa bekleme
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    /// <summary>
    /// Düşman öldüğünde kill count'u artırır
    /// </summary>
    /// <param name="deadEnemy">Ölen düşman</param>
    public void OnEnemyKilled(GameObject deadEnemy)
    {
        enemyKillCount++;
        
        // Gerekli kill sayısına ulaşıldıysa cooldown'u sıfırla
        if (enemyKillCount >= requiredEnemyKills)
        {
            ResetCooldown();
            enemyKillCount = 0;
            Debug.Log($"💥 Overflow cooldown reset after {requiredEnemyKills} enemy kills");
        }
    }
    
    /// <summary>
    /// Cooldown'u sıfırlar
    /// </summary>
    private void ResetCooldown()
    {
        isOnCooldown = false;
        cooldownTimeRemaining = 0f;
    }
    
    /// <summary>
    /// Ability'nin kullanılıp kullanılamayacağını kontrol eder
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Kullanılabilir mi?</returns>
    public bool CanUseAbility(GameObject caster)
    {
        // Cooldown kontrolü
        if (isOnCooldown)
        {
            Debug.Log($"❌ {abilityName} is on cooldown: {cooldownTimeRemaining:F1}s remaining");
            return false;
        }
        
        // Mana kontrolü
        var manaController = PlayerManaController.Instance;
        if (manaController != null && !manaController.HasEnoughMana(manaCost))
        {
            Debug.Log($"❌ Not enough mana for {abilityName}! Required: {manaCost}, Current: {manaController.GetCurrentMana()}");
            return false;
        }
        
        return true;
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
    /// Overflow SFX'ini oynatır (VFX yok, sadece ses)
    /// </summary>
    private void PlayOverflowSFX()
    {
        // Sadece SFX oynat - VFX her düşmanın üzerinde ayrı ayrı oluşturuluyor
        if (abilityData?.sfxClip != null)
        {
            //AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
            Debug.Log($"🔊 Playing overflow SFX (no VFX on player)");
        }
    }
    
    /// <summary>
    /// Overflow efektlerini oynatır (VFX ve SFX) - DEPRECATED: Artık kullanılmıyor
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    private void PlayOverflowEffects(GameObject caster)
    {
        // Bu metod artık kullanılmıyor - Player üzerinde VFX oluşturmasını engellemek için
        Debug.Log("⚠️ PlayOverflowEffects deprecated - use PlayOverflowSFX instead");
        
        // Sadece SFX
        PlayOverflowSFX();
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
    /// Kill count'u döndürür
    /// </summary>
    /// <returns>Mevcut kill count</returns>
    public int GetKillCount()
    {
        return enemyKillCount;
    }
} 