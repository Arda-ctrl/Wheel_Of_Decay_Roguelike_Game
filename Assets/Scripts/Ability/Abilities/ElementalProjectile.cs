using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ElementalProjectile - Her 3 saldırıda bir kez, elementine göre en yakın düşmana uzaktan projectile gönderir
/// Kullanım: Player'ın saldırı sistemine entegre edilir
/// </summary>
public class ElementalProjectile : MonoBehaviour, IAbility
{
    [Header("Elemental Projectile Settings")]
    [SerializeField] private string abilityName = "Elemental Projectile";
    [SerializeField] private string description = "Her 3 saldırıda bir kez en yakın düşmana projectile gönderir";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability olduğu için cooldown yok
    [SerializeField] private float manaCost = 0f; // Pasif ability olduğu için mana maliyeti yok
    [SerializeField] private int attackCountForProjectile = 3; // Kaç saldırıda bir projectile gönderilecek
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float projectileRange = 10f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ElementType targetElementType = ElementType.Fire; // Bu projectile hangi element için
    
    private IElement currentElement;
    private bool isActive = true;
    private int attackCounter = 0;
    private Transform playerTransform;
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
        attackCountForProjectile = data.attackCountForProjectile;
        projectileSpeed = data.projectileSpeed;
        projectileDamage = data.projectileDamage;
        projectileRange = data.projectileRange;
        projectilePrefab = data.projectilePrefab;
        targetElementType = data.elementType; // Element tipini ayarla
    }
    
    private void Start()
    {
        playerTransform = transform;
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
        Debug.Log($"{caster.name} için {currentElement?.ElementName} projectile'ı aktif");
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
    /// Saldırı sayacını artırır ve gerekirse projectile gönderir
    /// </summary>
    public void OnAttack()
    {
        if (!isActive || currentElement == null) return;
        
        attackCounter++;
        Debug.Log($"🎯 {targetElementType} Attack counter: {attackCounter}/{attackCountForProjectile}");
        
        if (attackCounter >= attackCountForProjectile)
        {
            SendProjectile();
            attackCounter = 0;
            Debug.Log($"🎯 {targetElementType} Projectile sent! Counter reset to 0");
        }
    }
    
    /// <summary>
    /// En yakın düşmana projectile gönderir
    /// </summary>
    private void SendProjectile()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            // Projectile oluştur
            GameObject projectile = CreateProjectile();
            
            if (projectile != null)
            {
                // Projectile'ı hedefe yönlendir
                Vector3 direction = (nearestEnemy.transform.position - playerTransform.position).normalized;
                projectile.transform.position = playerTransform.position;
                
                // Projectile component'ini ayarla
                var projectileComponent = projectile.GetComponent<ElementalProjectileObject>();
                if (projectileComponent != null)
                {
                    projectileComponent.Initialize(
                        direction,
                        projectileSpeed,
                        projectileDamage,
                        currentElement,
                        projectileRange,
                        abilityData
                    );
                }
                
                // VFX ve SFX oynat
                PlayProjectileEffects();
                
                Debug.Log($"🎯 {gameObject.name} sent {currentElement.ElementName} projectile to {nearestEnemy.name}");
            }
        }
    }
    
    /// <summary>
    /// En yakın düşmanı bulur
    /// </summary>
    /// <returns>En yakın düşman GameObject</returns>
    private GameObject FindNearestEnemy()
    {
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        // Tüm düşmanları bul
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
            
            if (distance <= projectileRange && distance < nearestDistance)
            {
                nearestEnemy = enemy;
                nearestDistance = distance;
            }
        }
        
        return nearestEnemy;
    }
    
    /// <summary>
    /// Projectile GameObject'ini oluşturur
    /// </summary>
    /// <returns>Oluşturulan projectile GameObject</returns>
    private GameObject CreateProjectile()
    {
        if (projectilePrefab != null)
        {
            return Object.Instantiate(projectilePrefab);
        }
        else
        {
            // Default projectile oluştur
            GameObject projectile = new GameObject("ElementalProjectile");
            projectile.AddComponent<ElementalProjectileObject>();
            projectile.AddComponent<Rigidbody2D>();
            projectile.AddComponent<CircleCollider2D>();
            
            // Sprite renderer ekle
            var spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = icon;
            
            return projectile;
        }
    }
    
    /// <summary>
    /// Projectile efektlerini oynatır (VFX ve SFX)
    /// </summary>
    private void PlayProjectileEffects()
    {
        // Projectile VFX'i oynat
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, playerTransform.position, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
        }
        
        // Projectile SFX'i oynat
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
    
    /// <summary>
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            attackCounter = 0;
        }
        Debug.Log($"🎯 {targetElementType} Projectile ability {(active ? "activated" : "deactivated")}");
    }
    
    /// <summary>
    /// Saldırı sayacını sıfırlar
    /// </summary>
    public void ResetAttackCounter()
    {
        attackCounter = 0;
        Debug.Log($"🎯 {targetElementType} Attack counter reset to 0");
    }
    
    /// <summary>
    /// Projectile ayarlarını günceller
    /// </summary>
    /// <param name="attackCount">Saldırı sayısı</param>
    /// <param name="speed">Hız</param>
    /// <param name="damage">Hasar</param>
    /// <param name="range">Menzil</param>
    public void UpdateProjectileSettings(int attackCount, float speed, float damage, float range)
    {
        attackCountForProjectile = attackCount;
        projectileSpeed = speed;
        projectileDamage = damage;
        projectileRange = range;
    }
    
    /// <summary>
    /// Ability'nin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <returns>Aktif mi?</returns>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Bu projectile'ın hangi element için olduğunu döndürür
    /// </summary>
    /// <returns>Element tipi</returns>
    public ElementType GetTargetElementType()
    {
        return targetElementType;
    }
}

/// <summary>
/// ElementalProjectileObject - Projectile'ın fiziksel davranışını yönetir
/// </summary>
public class ElementalProjectileObject : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private IElement element;
    private float range;
    private float distanceTraveled;
    private Vector3 startPosition;
    private ElementalAbilityData abilityData;
    private Rigidbody2D rb;
    private bool isInitialized = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }
    
    /// <summary>
    /// Projectile'ı initialize eder
    /// </summary>
    /// <param name="dir">Yön</param>
    /// <param name="spd">Hız</param>
    /// <param name="dmg">Hasar</param>
    /// <param name="elem">Element</param>
    /// <param name="rng">Menzil</param>
    /// <param name="data">Ability data</param>
    public void Initialize(Vector3 dir, float spd, float dmg, IElement elem, float rng, ElementalAbilityData data)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        element = elem;
        range = rng;
        abilityData = data;
        startPosition = transform.position;
        isInitialized = true;
        
        // Hızı ayarla
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // Mesafeyi kontrol et
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        
        if (distanceTraveled >= range)
        {
            DestroyProjectile();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized) return;
        
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
                Debug.Log($"🎯 {element.ElementName} projectile hit {other.gameObject.name} and applied stack!");
            }
            
            // VFX ve SFX oynat
            PlayHitEffects(transform.position);
            
            // Projectile'ı yok et
            DestroyProjectile();
        }
        else if (other.CompareTag("Wall"))
        {
            // Duvar'a çarptığında da yok et
            PlayHitEffects(transform.position);
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// Çarpma efektlerini oynatır
    /// </summary>
    /// <param name="hitPosition">Çarpma pozisyonu</param>
    private void PlayHitEffects(Vector3 hitPosition)
    {
        // VFX oynat
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, hitPosition, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && element != null)
            {
                var main = particleSystem.main;
                main.startColor = element.ElementColor;
            }
        }
        
        // SFX oynat
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
    }
    
    /// <summary>
    /// Projectile'ı yok eder
    /// </summary>
    private void DestroyProjectile()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
} 