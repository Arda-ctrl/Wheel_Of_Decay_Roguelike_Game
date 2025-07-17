using UnityEngine;

/// <summary>
/// ElementalLanceBarrage - Big, etrafındaki düşmanlara 5 saldırı
/// Etrafındaki düşmanlara 5 adet elementine göre saldırı yapar düşman sayısına göre değişir
/// </summary>
public class ElementalLanceBarrage : MonoBehaviour, IAbility
{
    [Header("Elemental Lance Barrage Settings")]
    [SerializeField] private string abilityName = "Elemental Lance Barrage";
    [SerializeField] private string description = "Etrafındaki düşmanlara 5 saldırı yapar";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 15f;
    [SerializeField] private float manaCost = 50f;
    [SerializeField] private int lanceCount = 5;
    [SerializeField] private float lanceDamage = 25f;
    [SerializeField] private float lanceRange = 8f;
    
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
        lanceCount = data.lanceCount;
        lanceDamage = data.lanceDamage;
        lanceRange = data.lanceRange;
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
        
        // Etrafındaki düşmanları bul
        Collider2D[] colliders = Physics2D.OverlapCircleAll(caster.transform.position, lanceRange);
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
            // Lance saldırılarını başlat
            StartCoroutine(PerformLanceBarrage(caster, enemies));
            
            // VFX ve SFX oynat
            PlayLanceBarrageEffects(caster);
            
            Debug.Log($"⚔️ {caster.name} performed {currentElement?.ElementName} lance barrage on {enemies.Count} enemies");
        }
        
        // Cooldown başlat
        StartCooldown();
    }
    
    /// <summary>
    /// Lance barrage saldırılarını gerçekleştirir
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <param name="enemies">Hedef düşmanlar</param>
    private System.Collections.IEnumerator PerformLanceBarrage(GameObject caster, System.Collections.Generic.List<GameObject> enemies)
    {
        int lancesPerEnemy = lanceCount / enemies.Count;
        int remainingLances = lanceCount % enemies.Count;
        
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = enemies[i];
            int lancesForThisEnemy = lancesPerEnemy + (i < remainingLances ? 1 : 0);
            
            for (int j = 0; j < lancesForThisEnemy; j++)
            {
                // Lance saldırısı
                PerformLanceAttack(caster, enemy);
                
                // Kısa bekleme
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    /// <summary>
    /// Tek bir lance saldırısı gerçekleştirir
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    private void PerformLanceAttack(GameObject caster, GameObject target)
    {
        // Hasar uygula
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(lanceDamage);
        }
        
        // Element stack ekle
        if (currentElement != null)
        {
            currentElement.ApplyElementStack(target, 1);
        }
        
        // Lance VFX'i oluştur
        if (abilityData?.vfxPrefab != null)
        {
            Vector3 direction = (target.transform.position - caster.transform.position).normalized;
            Vector3 lancePosition = caster.transform.position + direction * 2f;
            
            GameObject lanceVFX = Object.Instantiate(abilityData.vfxPrefab, lancePosition, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = lanceVFX.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
            
            // VFX'i kısa süre sonra yok et
            Destroy(lanceVFX, 1f);
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
        isOnCooldown = true;
        cooldownTimeRemaining = cooldownDuration;
    }
    
    /// <summary>
    /// Lance barrage efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="caster">Caster GameObject</param>
    private void PlayLanceBarrageEffects(GameObject caster)
    {
        // Lance barrage VFX'i oynat
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
        
        // Lance barrage SFX'i oynat
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