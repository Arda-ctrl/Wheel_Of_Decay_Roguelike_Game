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
    
    private IElement currentElement;
    private bool isActive = true;
    private int attackCounter = 0;
    private Transform playerTransform;
    
    // IAbility Interface Implementation
    public string AbilityName => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public float CooldownDuration => cooldownDuration;
    public float ManaCost => manaCost;
    
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
        Debug.Log($"🎯 Attack counter: {attackCounter}/{attackCountForProjectile}");
        
        if (attackCounter >= attackCountForProjectile)
        {
            SendProjectile();
            attackCounter = 0;
            Debug.Log($"🎯 Projectile sent! Counter reset to 0");
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
                        projectileRange
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
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Projectile");
            
            return projectile;
        }
    }
    
    /// <summary>
    /// Projectile efektlerini oynatır (VFX ve SFX)
    /// </summary>
    private void PlayProjectileEffects()
    {
        // Projectile VFX'i oynat
        var projectileVFX = Resources.Load<GameObject>("Prefabs/Effects/ElementalProjectileVFX");
        if (projectileVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(projectileVFX, playerTransform.position, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
        }
        
        // Projectile SFX'i oynat
        AudioManager.Instance?.PlaySFX(20);
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
    /// Saldırı sayacını sıfırlar
    /// </summary>
    public void ResetAttackCounter()
    {
        attackCounter = 0;
    }
    
    /// <summary>
    /// Projectile ayarlarını günceller
    /// </summary>
    /// <param name="attackCount">Saldırı sayısı</param>
    /// <param name="speed">Projectile hızı</param>
    /// <param name="damage">Projectile hasarı</param>
    /// <param name="range">Projectile menzili</param>
    public void UpdateProjectileSettings(int attackCount, float speed, float damage, float range)
    {
        attackCountForProjectile = attackCount;
        projectileSpeed = speed;
        projectileDamage = damage;
        projectileRange = range;
    }

    public bool IsActive()
    {
        return isActive;
    }
}

/// <summary>
/// ElementalProjectileObject - Projectile'ın hareket ve çarpışma davranışlarını yönetir (Elemental sistem için)
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
    
    public void Initialize(Vector3 dir, float spd, float dmg, IElement elem, float rng)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        element = elem;
        range = rng;
        distanceTraveled = 0f;
        startPosition = transform.position;
        
        // Projectile'ı element rengine göre ayarla
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && element != null)
        {
            spriteRenderer.color = element.ElementColor;
        }
    }
    
    private void Update()
    {
        // Projectile'ı hareket ettir
        transform.position += direction * speed * Time.deltaTime;
        
        // Menzil kontrolü
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= range)
        {
            DestroyProjectile();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Düşmana çarptı mı kontrol et
        if (other.CompareTag("Enemy"))
        {
            // Hasar ver
            var health = other.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            
            // Element stack'i uygula
            if (element != null)
            {
                element.ApplyElementStack(other.gameObject, 1);
            }
            
            // Çarpışma VFX'i oynat
            PlayHitEffects(other.transform.position);
            
            // Projectile'ı yok et
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// Çarpışma efektlerini oynatır
    /// </summary>
    /// <param name="hitPosition">Çarpışma pozisyonu</param>
    private void PlayHitEffects(Vector3 hitPosition)
    {
        // Hit VFX'i oynat
        var hitVFX = Resources.Load<GameObject>("Prefabs/Effects/ElementalProjectileHitVFX");
        if (hitVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(hitVFX, hitPosition, Quaternion.identity);
            
            // Element rengine göre VFX'i ayarla
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && element != null)
            {
                var main = particleSystem.main;
                main.startColor = element.ElementColor;
            }
        }
        
        // Hit SFX'i oynat
        AudioManager.Instance?.PlaySFX(15);
    }
    
    /// <summary>
    /// Projectile'ı yok eder
    /// </summary>
    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
} 