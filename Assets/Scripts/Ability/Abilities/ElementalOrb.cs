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
        orbDuration = data.orbDuration;
        orbSpeed = data.orbSpeed;
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
    /// Ability'yi kullanır (artık pasif sistem - manuel kullanılmaz)
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="element">Kullanılacak element</param>
    public void UseAbility(GameObject caster, GameObject target, IElement element)
    {
        // Bu ability artık pasif çalışıyor - manuel kullanım yok
        Debug.Log("🔮 ElementalOrb is now a passive ability - orbs spawn automatically when collecting 10 stacks");
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
        // Pasif ability - manuel kullanılamaz
        return false;
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
    private ElementType elementType;
    private float damage = 15f;
    private float speed;
    private float duration;
    private ElementalAbilityData abilityData;
    private float lifeTime;
    private GameObject nearestEnemy;
    
    // Pasif orb için özel değişkenler
    private bool isPassiveOrb = false;
    private Transform playerTransform;
    private float rotationSpeed = 60f;
    private float orbitRadius = 2f;
    private float orbitAngle = 0f;
    private float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    private float attackRange = 8f; // Saldırı alanı
    
    // Events
    public System.Action OnOrbDestroyed;
    
    /// <summary>
    /// Orb'u başlatır (eski sistem için)
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
        this.isPassiveOrb = false;
    }
    
    /// <summary>
    /// Pasif orb olarak başlatır (yeni sistem)
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <param name="playerTransform">Player transform</param>
    /// <param name="abilityData">Orb verileri</param>
    public void InitializeAsPassiveOrb(ElementType elementType, Transform playerTransform, ElementalAbilityData abilityData)
    {
        this.elementType = elementType;
        this.playerTransform = playerTransform;
        this.isPassiveOrb = true;
        this.duration = float.MaxValue; // Süresiz
        this.lifeTime = 0f;
        this.abilityData = abilityData;
        this.attackRange = abilityData.orbDetectionRadius;
        this.damage = 15f; // Sabit damage değeri
        
        // Element oluştur
        this.element = CreateElementFromType(elementType);
        
        // Orb pozisyonunu belirle (0, 90, 180, 270 derece)
        this.orbitAngle = CalculateOrbAngle();
        
        Debug.Log($"🔮 Passive {elementType} orb initialized - Range: {attackRange}, Damage: {damage}");
    }
    
    private void Update()
    {
        lifeTime += Time.deltaTime;
        
        if (isPassiveOrb)
        {
            UpdatePassiveOrb();
        }
        else
        {
            UpdateActiveOrb();
        }
    }
    
    /// <summary>
    /// Pasif orb'u günceller (karakter etrafında döner)
    /// </summary>
    private void UpdatePassiveOrb()
    {
        if (playerTransform == null)
        {
            DestroyOrb();
            return;
        }
        
        // Karakter etrafında dön
        orbitAngle += rotationSpeed * Time.deltaTime;
        if (orbitAngle >= 360f) orbitAngle -= 360f;
        
        // Pozisyonu hesapla
        float radians = orbitAngle * Mathf.Deg2Rad;
        Vector3 orbitPosition = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            Mathf.Sin(radians) * orbitRadius,
            0f
        );
        
        transform.position = playerTransform.position + orbitPosition;
        
        // Otomatik saldırı
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            FindNearestEnemy();
            if (nearestEnemy != null)
            {
                AttackEnemy(nearestEnemy);
                lastAttackTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// Aktif orb'u günceller (düşmana doğru hareket eder)
    /// </summary>
    private void UpdateActiveOrb()
    {
        // Süre dolduysa yok et
        if (lifeTime >= duration)
        {
            DestroyOrb();
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        float nearestDistance = float.MaxValue;
        nearestEnemy = null;
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < nearestDistance && distance <= attackRange)
                {
                    nearestDistance = distance;
                    nearestEnemy = collider.gameObject;
                }
            }
        }
    }
    
    /// <summary>
    /// Düşmana saldırır (projectile fırlatır)
    /// </summary>
    /// <param name="enemy">Hedef düşman</param>
    private void AttackEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        
        // Projectile fırlat
        FireProjectileAtEnemy(enemy);
        
        Debug.Log($"🔮 {elementType} orb fired projectile at {enemy.name}");
    }
    
    /// <summary>
    /// Düşmana projectile fırlatır
    /// </summary>
    /// <param name="enemy">Hedef düşman</param>
    private void FireProjectileAtEnemy(GameObject enemy)
    {
        if (abilityData == null || enemy == null) return;
        
        GameObject projectileGO;
        
        // SO'dan projectile prefab'i varsa kullan
        if (abilityData.orbProjectilePrefab != null)
        {
            projectileGO = Object.Instantiate(abilityData.orbProjectilePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Varsayılan projectile oluştur
            projectileGO = CreateDefaultProjectile();
        }
        
        // Projectile controller ekle
        var projectileController = projectileGO.GetComponent<OrbProjectileController>();
        if (projectileController == null)
        {
            projectileController = projectileGO.AddComponent<OrbProjectileController>();
        }
        
        // Projectile'ı initialize et
        Vector3 direction = (enemy.transform.position - transform.position).normalized;
        projectileController.Initialize(direction, damage, element, abilityData);
    }
    
    /// <summary>
    /// Varsayılan projectile oluşturur
    /// </summary>
    /// <returns>Projectile GameObject</returns>
    private GameObject CreateDefaultProjectile()
    {
        GameObject projectileGO = new GameObject($"{elementType} Orb Projectile");
        projectileGO.transform.position = transform.position;
        projectileGO.transform.localScale = Vector3.one * 0.3f;
        
        // Sprite renderer ekle
        var spriteRenderer = projectileGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateProjectileSprite();
        spriteRenderer.color = element?.ElementColor ?? Color.white;
        spriteRenderer.sortingOrder = 6;
        
        // Collider ekle
        var collider = projectileGO.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.1f;
        
        return projectileGO;
    }
    
    /// <summary>
    /// Projectile için küçük sprite oluşturur
    /// </summary>
    /// <returns>Projectile sprite</returns>
    private Sprite CreateProjectileSprite()
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return sprite;
    }
    

    
    /// <summary>
    /// Orb'u yok eder
    /// </summary>
    private void DestroyOrb()
    {
        OnOrbDestroyed?.Invoke();
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Element türünden IElement oluşturur
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>IElement instance</returns>
    private IElement CreateElementFromType(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return new FireElement();
            case ElementType.Ice:
                return new IceElement();
            case ElementType.Poison:
                return new PoisonElement();
            case ElementType.Lightning:
                return new LightningElement();
            case ElementType.Earth:
                return new EarthElement();
            case ElementType.Wind:
                return new WindElement();
            default:
                return new FireElement();
        }
    }
    
    /// <summary>
    /// Element türünü döndürür
    /// </summary>
    /// <returns>Element türü</returns>
    public ElementType GetElementType()
    {
        return elementType;
    }
    
    /// <summary>
    /// Orb'un açısını hesaplar (0, 90, 180, 270 derece)
    /// </summary>
    /// <returns>Açı değeri</returns>
    private float CalculateOrbAngle()
    {
        // OrbStackManager'dan mevcut orb sayısını al
        int orbCount = OrbStackManager.Instance?.GetActiveOrbCount() ?? 0;
        
        // 4 orb için 90 derece aralıklarla yerleştir
        float[] angles = { 0f, 90f, 180f, 270f };
        return angles[orbCount % 4];
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece aktif orb'lar için trigger kullan (pasif orb'lar otomatik saldırı yapar)
        if (!isPassiveOrb && other.CompareTag("Enemy"))
        {
            AttackEnemy(other.gameObject);
            DestroyOrb();
        }
    }
    
    /// <summary>
    /// Saldırı alanını görselleştirir
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (isPassiveOrb)
        {
            // Saldırı alanını yeşil renkte göster
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Orb'un orbital yolunu mavi renkte göster
            if (playerTransform != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(playerTransform.position, orbitRadius);
            }
        }
    }
} 