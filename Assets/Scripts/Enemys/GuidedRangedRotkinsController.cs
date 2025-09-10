using UnityEngine;
using System.Collections;

public class GuidedRangedRotkinsController : EnemyController
{
    [Header("Guided Ranged Rotkins Settings")]
    [SerializeField] private KingdomEnemyData enemyData;
    [SerializeField] private GameObject guidedCrystalProjectilePrefab;
    [SerializeField] private float crystalDamage = 30f;
    [SerializeField] private float crystalSpeed = 10f;
    [SerializeField] private float crystalRange = 9f;
    
    [Header("Root Attack")]
    [SerializeField] private float rootAttackRange = 3f;
    [SerializeField] private float rootAttackDamage = 22f;
    [SerializeField] private GameObject rootAttackVFX;
    
    [Header("Attack Timings")]
    [SerializeField] private float lastCrystalAttackTime;
    [SerializeField] private float lastRootAttackTime;
    [SerializeField] private float rootAttackCooldown = 4f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string rangedAttackAnimationName = "RangedAttack";
    [SerializeField] private string rootAttackAnimationName = "RootAttack";
    
    [Header("Visual Components")]
    [SerializeField] private Transform crystalLaunchPoint;
    [SerializeField] private Transform[] crystalPositions; // Kafasındaki kristal pozisyonları (farklı renk)
    [SerializeField] private GameObject redCrystalGlowVFX; // Kırmızı glow (homing için)
    
    private bool isPerformingCrystalAttack = false;
    private bool isPerformingRootAttack = false;

    protected override void Start()
    {
        base.Start();
        
        // EnemyData'dan stats'ları al
        if (enemyData != null)
        {
            maxHealth = enemyData.maxHealth;
            baseSpeed = enemyData.baseSpeed;
            detectionRange = enemyData.detectionRange;
            stopDistance = enemyData.stopDistance;
            currentHealth = maxHealth;
        }
        
        // Animator referansını al
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Crystal launch point referansını ayarla
        if (crystalLaunchPoint == null)
        {
            crystalLaunchPoint = transform;
        }
        
        // Red crystal glow effect (homing variant)
        StartCoroutine(RedCrystalGlowEffect());
    }

    protected override void Update()
    {
        base.Update();
        
        // Saldırı kontrolleri
        if (playerTransform != null && !isPerformingCrystalAttack && !isPerformingRootAttack)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Yakın mesafe root attack
            if (distanceToPlayer <= rootAttackRange && Time.time - lastRootAttackTime >= rootAttackCooldown)
            {
                StartCoroutine(PerformRootAttack());
            }
            // Uzak mesafe guided crystal attack
            else if (distanceToPlayer <= crystalRange && distanceToPlayer > rootAttackRange && 
                     Time.time - lastCrystalAttackTime >= enemyData.attackCooldown)
            {
                StartCoroutine(PerformGuidedCrystalAttack());
            }
        }
    }

    private IEnumerator RedCrystalGlowEffect()
    {
        while (gameObject != null)
        {
            // Kırmızı kristallerin üzerinde glow efekti
            if (redCrystalGlowVFX != null && crystalPositions != null)
            {
                foreach (var crystalPos in crystalPositions)
                {
                    if (crystalPos != null)
                    {
                        var glow = Instantiate(redCrystalGlowVFX, crystalPos.position, Quaternion.identity, crystalPos);
                        Destroy(glow, 1.5f);
                    }
                }
            }
            
            yield return new WaitForSeconds(2.5f); // Daha sık glow (homing threat indicator)
        }
    }

    private IEnumerator PerformGuidedCrystalAttack()
    {
        isPerformingCrystalAttack = true;
        lastCrystalAttackTime = Time.time;
        
        // Hareket dur
        rb.linearVelocity = Vector2.zero;
        
        // Ranged attack animasyonu
        if (animator != null)
        {
            animator.Play(rangedAttackAnimationName);
        }
        
        Debug.Log("Guided Ranged Rotkins Homing Crystal Attack!");
        
        // Kristal hazırlık süresi (guided için biraz daha uzun)
        yield return new WaitForSeconds(0.6f);
        
        // Guided crystal projectile fırlat
        if (guidedCrystalProjectilePrefab != null && playerTransform != null)
        {
            Vector2 direction = (playerTransform.position - crystalLaunchPoint.position).normalized;
            
            GameObject crystal = Instantiate(guidedCrystalProjectilePrefab, crystalLaunchPoint.position, Quaternion.identity);
            
            // Crystal'ın RotkinsProjectile component'ini ayarla (HOMING ENABLED)
            var projectile = crystal.GetComponent<RotkinsProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(direction, crystalSpeed, crystalDamage, true); // true = homing projectile
            }
            
            // Ses efekti çal (daha ominous ses için)
            if (enemyData?.attackSound != null)
            {
                AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
            }
        }
        
        // Saldırı sonrası bekleme
        yield return new WaitForSeconds(0.4f);
        
        isPerformingCrystalAttack = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    private IEnumerator PerformRootAttack()
    {
        isPerformingRootAttack = true;
        lastRootAttackTime = Time.time;
        
        // Hareket dur
        rb.linearVelocity = Vector2.zero;
        
        // Root attack animasyonu
        if (animator != null)
        {
            animator.Play(rootAttackAnimationName);
        }
        
        Debug.Log("Guided Ranged Rotkins Root Attack!");
        
        // Root saldırısı hazırlık
        yield return new WaitForSeconds(0.5f);
        
        // Player pozisyonunda root VFX ve hasar
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= rootAttackRange)
            {
                // Root VFX spawn et
                if (rootAttackVFX != null)
                {
                    Instantiate(rootAttackVFX, playerTransform.position, Quaternion.identity);
                }
                
                // Player'a hasar ver (biraz daha güçlü)
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(rootAttackDamage);
                    Debug.Log($"Guided Root Attack Hit! Damage: {rootAttackDamage}");
                }
                
                // Ses efekti çal
                if (enemyData?.attackSound != null)
                {
                    AudioSource.PlayClipAtPoint(enemyData.attackSound, playerTransform.position);
                }
            }
        }
        
        // Saldırı sonrası bekleme
        yield return new WaitForSeconds(0.8f);
        
        isPerformingRootAttack = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Guided crystal attack menzilini çiz
        Gizmos.color = Color.red; // Kırmızı = homing
        Gizmos.DrawWireSphere(transform.position, crystalRange);
        
        // Root attack menzilini çiz
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rootAttackRange);
    }

    public override void TakeDamage(float amount)
    {
        // Zayıf defans (normal ranged ile aynı)
        float finalDamage = amount;
        if (enemyData != null)
        {
            finalDamage /= enemyData.defenseMultiplier; // 0.8 means takes more damage
        }
        
        base.TakeDamage(finalDamage);
        
        // Hasar alma ses efekti
        if (enemyData?.hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.hurtSound, transform.position);
        }
    }

    protected override void Die()
    {
        // Ölüm efekti
        if (enemyData?.deathEffect != null)
        {
            Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
        }
        
        // Ölüm ses efekti
        if (enemyData?.deathSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.deathSound, transform.position);
        }
        
        base.Die();
    }
}
