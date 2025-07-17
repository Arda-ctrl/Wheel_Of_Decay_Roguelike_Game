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
    [SerializeField] private float manaCost = 100f;
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
        
        currentElement = element;
        
        // Odadaki tüm düşmanları bul
        Collider2D[] colliders = Physics2D.OverlapCircleAll(caster.transform.position, 50f); // Geniş alan
        var enemies = new System.Collections.Generic.List<GameObject>();
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                enemies.Add(collider.gameObject);
            }
        }
        
        if (enemies.Count > 0)
        {
            // Overflow saldırısını başlat
            StartCoroutine(PerformOverflowAttack(caster, enemies));
            
            // VFX ve SFX oynat
            PlayOverflowEffects(caster);
            
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
            // Hasar uygula
            var health = enemy.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(overflowDamage);
            }
            
            // Element stack ekle
            if (currentElement != null)
            {
                currentElement.ApplyElementStack(enemy, overflowStackAmount);
            }
            
            // Overflow VFX'i oluştur
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
    /// Overflow efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    private void PlayOverflowEffects(GameObject caster)
    {
        // Overflow VFX'i oynat
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
        
        // Overflow SFX'i oynat
        if (abilityData?.sfxClip != null)
        {
            //AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
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
    /// Kill count'u döndürür
    /// </summary>
    /// <returns>Mevcut kill count</returns>
    public int GetKillCount()
    {
        return enemyKillCount;
    }
} 