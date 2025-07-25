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
    [SerializeField] private float cooldownDuration = 0f;
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private int attackCountForProjectile = 3;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float projectileRange = 10f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ElementType targetElementType = ElementType.Fire;
    
    private IElement currentElement;
    private bool isActive = true;
    private int attackCounter = 0;
    private Transform playerTransform;
    private ElementalAbilityData abilityData;
    private ElementalAbilityManager elementalAbilityManager;
    
    // IAbility Interface Implementation
    public string AbilityName => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public float CooldownDuration => cooldownDuration;
    public float ManaCost => manaCost;
    
    /// <summary>
    /// Ability'yi ElementalAbilityData ile başlatır
    /// </summary>
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
        targetElementType = data.elementType;
        
        // Debug için varsayılan olarak aktif yap
        isActive = true;
        
        // ElementalAbilityManager referansını al
        elementalAbilityManager = GetComponent<ElementalAbilityManager>();
        if (elementalAbilityManager == null)
        {
            elementalAbilityManager = FindObjectOfType<ElementalAbilityManager>();
        }
    }
    
    private void Start()
    {
        // Player transform'unu doğru şekilde al
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            playerTransform = transform;
        }
    }
    
    public void UseAbility(GameObject caster, GameObject target, IElement element)
    {
        currentElement = element;
    }
    
    public bool CanUseAbility(GameObject caster)
    {
        return isActive;
    }
    
    public float GetCooldownProgress()
    {
        return 0f;
    }
    
    /// <summary>
    /// Saldırı sayacını artırır ve gerekirse projectile gönderir
    /// </summary>
    public void OnAttack()
    {
        if (!isActive || currentElement == null) return;
        
        attackCounter++;
        
        if (attackCounter >= attackCountForProjectile)
        {
            // Dinamik coordination: İlk aktif element coordinate etsin
            if (ShouldCoordinate())
            {
                CoordinateAllProjectiles();
            }
            attackCounter = 0;
        }
    }
    
    /// <summary>
    /// Tüm aktif element'lerin projectile'larını koordine eder
    /// </summary>
    private void CoordinateAllProjectiles()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            // Tüm aktif element'leri topla
            var activeElements = GetAllActiveElements();
            int totalProjectiles = activeElements.Count;
            
            Vector3 enemyPos = nearestEnemy.transform.position;
            Vector3 playerPos = playerTransform.position;
            Vector3 baseDirection = (enemyPos - playerPos).normalized;
            float distance = Vector3.Distance(playerPos, enemyPos);
            
            if (totalProjectiles == 1)
            {
                // Tek element - normal gitsin
                SendElementProjectile(nearestEnemy, baseDirection, 0, 0, activeElements[0].Value, activeElements[0].Key);
            }
            else if (totalProjectiles == 2)
            {
                // 2 element - V şeklinde
                SendElementProjectile(nearestEnemy, baseDirection, -1, distance, activeElements[0].Value, activeElements[0].Key); // Sol
                SendElementProjectile(nearestEnemy, baseDirection, +1, distance, activeElements[1].Value, activeElements[1].Key); // Sağ
            }
            else
            {
                // 3+ element - yayılmış V
                float totalSpread = 40f; // Daha geniş spread
                float angleStep = totalSpread / (totalProjectiles - 1);
                float startAngle = -totalSpread / 2f;
                
                for (int i = 0; i < totalProjectiles; i++)
                {
                    float currentAngle = startAngle + (angleStep * i);
                    int side = (int)Mathf.Sign(currentAngle == 0 ? 0 : currentAngle);
                    SendElementProjectile(nearestEnemy, baseDirection, side, distance, activeElements[i].Value, activeElements[i].Key);
                }
            }
        }
    }
    
    /// <summary>
    /// Tüm aktif element'leri döndürür - DİNAMİK SYSTEM
    /// </summary>
    private List<KeyValuePair<ElementType, IElement>> GetAllActiveElements()
    {
        var activeElements = new List<KeyValuePair<ElementType, IElement>>();
        
        if (elementalAbilityManager == null)
        {
            // Fallback: En azından bu element'i ekle
            activeElements.Add(new KeyValuePair<ElementType, IElement>(targetElementType, currentElement));
            return activeElements;
        }
        
        // TÜM ELEMENT TİPLERİNİ DİNAMİK KONTROL ET
        foreach (ElementType elementType in System.Enum.GetValues(typeof(ElementType)))
        {
            if (elementType == ElementType.None) continue;
            
            // Bu element'in projectile ability'si aktif mi?
            if (elementalAbilityManager.IsAbilityActive(elementType, AbilityType.ElementalProjectile))
            {
                // Element'in kendi projectile ability'sinden element instance'ını al
                var projectileAbility = elementalAbilityManager.GetAbility(elementType, AbilityType.ElementalProjectile) as ElementalProjectile;
                
                if (projectileAbility != null)
                {
                    var element = projectileAbility.GetCurrentElement();
                    if (element != null)
                    {
                        activeElements.Add(new KeyValuePair<ElementType, IElement>(elementType, element));
                    }
                }
            }
        }
        
        // Fallback: Hiç aktif element yoksa bu element'i ekle
        if (activeElements.Count == 0)
        {
            activeElements.Add(new KeyValuePair<ElementType, IElement>(targetElementType, currentElement));
        }
        
        return activeElements;
    }
    
    /// <summary>
    /// Bu element'in coordinate etmesi gerekip gerekmediğini kontrol eder
    /// İlk aktif element (alphabetic order) coordinate eder
    /// </summary>
    private bool ShouldCoordinate()
    {
        if (elementalAbilityManager == null) return true;
        
        // Aktif element'leri alphabetic sırada kontrol et
        ElementType[] elementOrder = { 
            ElementType.Earth, 
            ElementType.Fire, 
            ElementType.Ice, 
            ElementType.Lightning, 
            ElementType.Poison, 
            ElementType.Wind 
        };
        
        foreach (ElementType elementType in elementOrder)
        {
            if (elementalAbilityManager.IsAbilityActive(elementType, AbilityType.ElementalProjectile))
            {
                // İlk aktif element bu mu?
                bool isFirstActive = (elementType == targetElementType);
                return isFirstActive;
            }
        }
        
        return true; // Fallback
    }
    
    /// <summary>
    /// Belirli bir element için projectile gönderir
    /// </summary>
    private void SendElementProjectile(GameObject target, Vector3 baseDirection, int side, float totalDistance, IElement element, ElementType elementType)
    {
        GameObject projectile = CreateProjectile();
        
        if (projectile != null)
        {
            Vector3 direction;
            
            if (totalDistance > 0)
            {
                // Converging projectile
                float initialSpread = 25f;
                Vector3 initialDirection = RotateVector(baseDirection, side * initialSpread);
                direction = initialDirection;
                
                var projectileComponent = projectile.GetComponent<ElementalProjectileObject>();
                if (projectileComponent != null)
                {
                    projectileComponent.InitializeConverging(
                        initialDirection,
                        baseDirection,
                        target.transform.position,
                        totalDistance,
                        projectileSpeed,
                        projectileDamage,
                        element,
                        projectileRange,
                        abilityData,
                        target.transform.position  // ← FIXED DESTINATION
                    );
                }
            }
            else
            {
                // Normal projectile
                direction = baseDirection;
                
                var projectileComponent = projectile.GetComponent<ElementalProjectileObject>();
                if (projectileComponent != null)
                {
                    projectileComponent.Initialize(
                        direction,
                        projectileSpeed,
                        projectileDamage,
                        element,
                        projectileRange,
                        abilityData,
                        target.transform.position  // ← FIXED DESTINATION
                    );
                }
            }
            
            // SMART CIRCLE SPAWN SYSTEM - Player etrafında hayali circle
            Vector3 spawnPosition;
            
            if (totalDistance > 0) // Multiple projectiles
            {
                // Circle parameters
                float circleRadius = 1.5f; // Player'dan 1.5 birim uzak
                
                // Enemy direction'ına göre circle üzerinde base angle
                float enemyAngle = Mathf.Atan2(baseDirection.y, baseDirection.x);
                
                // Side index'ini düzgün hesapla
                int actualSide = side;
                if (elementType == ElementType.Ice) actualSide = -1;
                else if (elementType == ElementType.Fire) actualSide = 0;
                else if (elementType == ElementType.Poison) actualSide = 1;
                else if (elementType == ElementType.Lightning) actualSide = -2;
                else if (elementType == ElementType.Earth) actualSide = 2;
                else if (elementType == ElementType.Wind) actualSide = -3;
                
                // Her projectile için angle offset (15 derece arayla)
                float angleOffset = actualSide * 15f * Mathf.Deg2Rad; // 15 derece = 0.26 radian
                float finalAngle = enemyAngle + angleOffset;
                
                // Circle üzerinde spawn position hesapla
                Vector3 circleOffset = new Vector3(
                    Mathf.Cos(finalAngle) * circleRadius,
                    Mathf.Sin(finalAngle) * circleRadius,
                    0f
                );
                
                spawnPosition = playerTransform.position + circleOffset;
            }
            else
            {
                // Single projectile - enemy yönünde circle üzerinde
                float circleRadius = 1.5f;
                Vector3 circleOffset = baseDirection.normalized * circleRadius;
                spawnPosition = playerTransform.position + circleOffset;
            }
            
            projectile.transform.position = spawnPosition;
            
            // RANGE FIX: Start position'ı spawn position'a ayarla
            var rangeFixer = projectile.GetComponent<ElementalProjectileObject>();
            if (rangeFixer != null)
            {
                // Start position'ı circle spawn point yapıyoruz, player position değil!
                rangeFixer.SetStartPosition(spawnPosition);
            }
            
            // Rotation ayarla
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            PlayProjectileEffects();
        }
    }
    
    private GameObject FindNearestEnemy()
    {
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
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
    
    private GameObject CreateProjectile()
    {
        if (projectilePrefab != null)
        {
            var instantiated = Object.Instantiate(projectilePrefab);
            
            // Prefab'ta component var mı kontrol et
            var prefabComponent = instantiated.GetComponent<ElementalProjectileObject>();
            
            // Eğer prefab'ta component yoksa ekle
            if (prefabComponent == null)
            {
                prefabComponent = instantiated.AddComponent<ElementalProjectileObject>();
            }
            
            // Prefab için de collision ayarları
            var prefabCollider = instantiated.GetComponent<Collider2D>();
            if (prefabCollider != null)
            {
                prefabCollider.isTrigger = true;
            }
            
            return instantiated;
        }
        else
        {
            GameObject projectile = new GameObject("ElementalProjectile");
            
            var rb = projectile.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            var collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.1f;
            
            // Projectile layer'ı - diğer projectile'larla çarpışmasın
            projectile.layer = LayerMask.NameToLayer("Default"); // Veya özel projectile layer
            
            var spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = icon;
            
            var projectileComponent = projectile.AddComponent<ElementalProjectileObject>();
            
            return projectile;
        }
    }
    
    private void PlayProjectileEffects()
    {
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, playerTransform.position, Quaternion.identity);
            
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && currentElement != null)
            {
                var main = particleSystem.main;
                main.startColor = currentElement.ElementColor;
            }
        }
        
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
    }
    
    public void SetElement(IElement element)
    {
        currentElement = element;
    }
    
    public IElement GetCurrentElement()
    {
        return currentElement;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            attackCounter = 0;
        }
    }
    
    public void ResetAttackCounter()
    {
        attackCounter = 0;
    }
    
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
    
    public ElementType GetTargetElementType()
    {
        return targetElementType;
    }

    /// <summary>
    /// Aktif projectile ability sayısını döndürür - DİNAMİK
    /// </summary>
    private int GetActiveProjectileCount()
    {
        if (elementalAbilityManager == null) return 1;
        
        int count = 0;
        
        // TÜM ELEMENT TİPLERİNİ DİNAMİK KONTROL ET
        foreach (ElementType elementType in System.Enum.GetValues(typeof(ElementType)))
        {
            if (elementType == ElementType.None) continue;
            
            if (elementalAbilityManager.IsAbilityActive(elementType, AbilityType.ElementalProjectile))
            {
                count++;
            }
        }
        
        return count > 0 ? count : 1; // En az 1 döndür
    }
    
    /// <summary>
    /// Bu projectile'ın sırasını döndürür (converging için)
    /// </summary>
    private int GetProjectileIndex()
    {
        // Bu element'in index'ini bul
        switch (targetElementType)
        {
            case ElementType.Fire: return 0;
            case ElementType.Ice: return 1;
            case ElementType.Poison: return 2;
            case ElementType.Lightning: return 3;
            case ElementType.Earth: return 4;
            case ElementType.Wind: return 5;
            default: return 0;
        }
    }
    
    /// <summary>
    /// Vector'ü Z ekseni etrafında döndürür
    /// </summary>
    private Vector3 RotateVector(Vector3 vector, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);
        
        return new Vector3(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos,
            vector.z
        );
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
    private float startTime;
    private Vector3 lastPosition;
    private Vector3 lastFramePosition;
    
    // Converging projectile variables
    private bool isConverging = false;
    private Vector3 initialDirection;
    private Vector3 finalDirection;
    private Vector3 targetPosition;
    private float totalDistance;
    private float maxSpreadDistance; // En geniş olacağı mesafe
    
    // FIXED DESTINATION SYSTEM
    private Vector3 fixedDestination; // Sabit hedef pozisyon
    private bool hasFixedDestination = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.freezeRotation = true;
        }
    }
    
    public void Initialize(Vector3 dir, float spd, float dmg, IElement elem, float rng, ElementalAbilityData data, Vector3 destination)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        element = elem;
        range = rng;
        abilityData = data;
        startPosition = transform.position;
        lastPosition = transform.position;
        lastFramePosition = transform.position;
        startTime = Time.time;
        isInitialized = true;
        
        // FIXED DESTINATION SETUP
        fixedDestination = destination;
        hasFixedDestination = true;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            
            Vector3 normalizedDirection = direction.normalized;
            Vector2 velocity2D = new Vector2(normalizedDirection.x, normalizedDirection.y) * speed;
            rb.linearVelocity = velocity2D;
        }
    }
    
    /// <summary>
    /// Converging projectile olarak initialize eder (V şeklinde açılıp hedefe birleşir)
    /// </summary>
    public void InitializeConverging(Vector3 initDir, Vector3 finalDir, Vector3 targetPos, float totalDist, 
                                   float spd, float dmg, IElement elem, float rng, ElementalAbilityData data, Vector3 destination)
    {
        // Normal initialize
        direction = initDir;
        speed = spd;
        damage = dmg;
        element = elem;
        range = rng;
        abilityData = data;
        startPosition = transform.position;
        lastPosition = transform.position;
        lastFramePosition = transform.position;
                startTime = Time.time;
        isInitialized = true;
        
        // FIXED DESTINATION SETUP  
        fixedDestination = destination;
        hasFixedDestination = true;
        
        // Converging özel ayarlar
        isConverging = true;
        initialDirection = initDir;
        finalDirection = finalDir;
        targetPosition = targetPos;
        totalDistance = totalDist;
        maxSpreadDistance = totalDistance * 0.6f; // %60'ında en geniş olsun
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            
            Vector2 velocity2D = new Vector2(initialDirection.x, initialDirection.y) * speed;
            rb.linearVelocity = velocity2D;
        }
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        
        if (distanceTraveled >= range)
        {
            DestroyProjectile();
        }
        
        // DESTINATION CHECK
        if (hasFixedDestination)
        {
            float distanceToDestination = Vector3.Distance(transform.position, fixedDestination);
            if (distanceToDestination <= 0.5f) // 0.5 birim yakınlığa gelince
            {
                CheckDestinationForEnemy();
                DestroyProjectile();
                return;
            }
        }
        
        // SMART TRAJECTORY UPDATE
        if (isConverging && rb != null)
        {
            // Converging mode - sophisticated trajectory
            UpdateConvergingTrajectory();
        }
        
        // VELOCITY CORRUPTION DETECTOR
        if (rb != null && Time.time - startTime < 1f)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 expectedVelocity = new Vector2(direction.x, direction.y) * speed;
            
            float velocityDifference = Vector2.Distance(currentVelocity, expectedVelocity);
            
            if (velocityDifference > 0.5f)
            {
                rb.linearVelocity = expectedVelocity;
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (!isInitialized || rb == null) return;
        
        if (rb.linearVelocity.magnitude < speed * 0.1f)
        {
            Vector3 normalizedDirection = direction.normalized;
            Vector2 velocity2D = new Vector2(normalizedDirection.x, normalizedDirection.y) * speed;
            rb.linearVelocity = velocity2D;
        }
        
        if (rb.linearVelocity.magnitude < 1f && Time.time - startTime > 0.5f)
        {
            Vector3 normalizedDirection = direction.normalized;
            transform.Translate(normalizedDirection * speed * Time.fixedDeltaTime, Space.World);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized) return;
        
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            
            if (element != null)
            {
                element.ApplyElementStack(other.gameObject, 1);
                ApplyElementSpecificEffects(other.gameObject);
            }
            
            PlayHitEffects(transform.position);
            DestroyProjectile();
        }
        else if (other.CompareTag("Wall"))
        {
            PlayHitEffects(transform.position);
            DestroyProjectile();
        }
        else if (other.CompareTag("Player"))
        {
            // Player'la çarpışmayı ignore et - patlama yapma!
            return; // ← IGNORE PLAYER COLLISION
        }
        else if (other.gameObject.name.Contains("ElementalProjectile"))
        {
            // Projectile'lar birbirine çarpmamalı!
            return; // ← IGNORE PROJECTILE COLLISION
        }
        else
        {
            // Bilinmeyen collision'ları da ignore et
            return;
        }
    }
    
    private void ApplyElementSpecificEffects(GameObject target)
    {
        if (abilityData == null || element == null) return;
        
        switch (element.ElementType)
        {
            case ElementType.Fire:
                ApplyBurnEffect(target);
                break;
            case ElementType.Ice:
                ApplySlowEffect(target);
                break;
            case ElementType.Poison:
                ApplyPoisonEffect(target);
                break;
        }
    }
    
    private void ApplyBurnEffect(GameObject target)
    {
        var existingBurn = target.GetComponent<TempBurnEffect>();
        if (existingBurn != null)
        {
            Object.Destroy(existingBurn);
        }
        
        var burnEffect = target.AddComponent<TempBurnEffect>();
        burnEffect.damagePerTick = abilityData.fireBurnDamage;
        burnEffect.duration = abilityData.fireBurnDuration;
        burnEffect.tickRate = abilityData.fireBurnTickRate;
    }
    
    private void ApplySlowEffect(GameObject target)
    {
        var existingSlow = target.GetComponent<TempSlowEffect>();
        if (existingSlow != null)
        {
            Object.Destroy(existingSlow);
        }
        
        var slowEffect = target.AddComponent<TempSlowEffect>();
        slowEffect.slowPercent = abilityData.iceSlowPercentProjectile / 100f;
        slowEffect.duration = abilityData.iceSlowDurationProjectile;
        
        if (Random.Range(0f, 1f) <= abilityData.iceFreezeChance)
        {
            var moveable = target.GetComponent<IMoveable>();
            if (moveable != null)
            {
                var existingFreeze = target.GetComponent<TempFreezeEffect>();
                if (existingFreeze != null)
                {
                    Object.Destroy(existingFreeze);
                }
                
                var freezeEffect = target.AddComponent<TempFreezeEffect>();
                freezeEffect.duration = 2f;
            }
        }
    }
    
    private void ApplyPoisonEffect(GameObject target)
    {
        var existingPoison = target.GetComponent<TempPoisonEffect>();
        if (existingPoison != null)
        {
            Object.Destroy(existingPoison);
        }
        
        var poisonEffect = target.AddComponent<TempPoisonEffect>();
        poisonEffect.damagePerTick = abilityData.poisonDamageProjectile;
        poisonEffect.duration = abilityData.poisonDurationProjectile;
        poisonEffect.tickRate = abilityData.poisonTickRateProjectile;
        poisonEffect.slowPercent = 0.2f;
    }
    
    private void PlayHitEffects(Vector3 hitPosition)
    {
        if (abilityData?.vfxPrefab != null)
        {
            GameObject vfxInstance = Object.Instantiate(abilityData.vfxPrefab, hitPosition, Quaternion.identity);
            
            var particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null && element != null)
            {
                var main = particleSystem.main;
                main.startColor = element.ElementColor;
            }
        }
        
        if (abilityData?.sfxClip != null)
        {
            AudioManager.Instance?.PlaySFX(abilityData.sfxClip);
        }
    }
    
    private void DestroyProjectile()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Converging trajectory'yi günceller - V şeklinde açılıp hedefe birleşir
    /// </summary>
    private void UpdateConvergingTrajectory()
    {
        float currentDistance = Vector3.Distance(startPosition, transform.position);
        float progressRatio = currentDistance / totalDistance;
        
        Vector3 newDirection;
        
        if (currentDistance < maxSpreadDistance)
        {
            // İlk kısım: Spread out (açılma fazı)
            float spreadProgress = currentDistance / maxSpreadDistance;
            newDirection = initialDirection; // İlk direction'da devam et
        }
        else
        {
            // İkinci kısım: Converge (birleşme fazı)
            float convergeProgress = (currentDistance - maxSpreadDistance) / (totalDistance - maxSpreadDistance);
            convergeProgress = Mathf.Clamp01(convergeProgress);
            
            // FIXED DESTINATION FOR CONVERGING
            if (hasFixedDestination)
            {
                targetPosition = fixedDestination; // Fixed destination!
            }
            
            // Şu anki pozisyondan hedefe direction hesapla
            Vector3 currentToTarget = (targetPosition - transform.position).normalized;
            
            // CONVERGING MATH DEBUG
            if (float.IsNaN(currentToTarget.x) || float.IsNaN(currentToTarget.y))
            {
                newDirection = initialDirection; // Fallback
            }
            else
            {
                // Initial direction'dan target direction'a geçiş yap
                newDirection = Vector3.Slerp(initialDirection, currentToTarget, convergeProgress * 2f);
                newDirection = newDirection.normalized;
            }
        }
        
        // Direction değiştiyse velocity'yi güncelle
        if (Vector3.Distance(direction, newDirection) > 0.1f)
        {
            direction = newDirection;
            Vector2 newVelocity = new Vector2(direction.x, direction.y) * speed;
            rb.linearVelocity = newVelocity;
            
            // Rotation'ı da güncelle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void CheckDestinationForEnemy()
    {
        if (!hasFixedDestination) return;
        
        // Destination çevresinde enemy var mı kontrol et (0.8f radius)
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(fixedDestination, 0.8f);
        
        foreach (Collider2D collider in nearbyColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                // Enemy bulundu! Damage ver ve effect uygula
                var health = collider.GetComponent<IHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                
                if (element != null)
                {
                    element.ApplyElementStack(collider.gameObject, 1);
                    ApplyElementSpecificEffects(collider.gameObject);
                }
                
                PlayHitEffects(fixedDestination);
                return; // İlk enemy'yi bulunca çık
            }
        }
        
        // Enemy yok, sadece effect oyna
        PlayHitEffects(fixedDestination);
    }

    public void SetStartPosition(Vector3 newStartPosition)
    {
        startPosition = newStartPosition;
    }
} 