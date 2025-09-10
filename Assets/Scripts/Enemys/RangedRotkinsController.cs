using UnityEngine;
using System.Collections;

public class RangedRotkinsController : EnemyController
{
    [Header("Ranged Rotkins Settings")]
    [SerializeField] private KingdomEnemyData enemyData;
    [SerializeField] private GameObject crystalProjectilePrefab;
    [SerializeField] private float crystalDamage = 25f;
    [SerializeField] private float crystalSpeed = 12f;
    [SerializeField] private float crystalRange = 8f;
    
    [Header("Root Attack")]
    [SerializeField] private float rootAttackRange = 3f;
    [SerializeField] private float rootAttackDamage = 20f;
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
    [SerializeField] private Transform[] crystalPositions; // Kafasındaki kristal pozisyonları
    [SerializeField] private GameObject crystalGlowVFX;
    
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
        
        // Crystal glow effect
        StartCoroutine(CrystalGlowEffect());
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
            // Uzak mesafe crystal attack
            else if (distanceToPlayer <= crystalRange && distanceToPlayer > rootAttackRange && 
                     Time.time - lastCrystalAttackTime >= enemyData.attackCooldown)
            {
                StartCoroutine(PerformCrystalAttack());
            }
        }
    }

    private IEnumerator CrystalGlowEffect()
    {
        while (gameObject != null)
        {
            // Kristallerin üzerinde glow efekti
            if (crystalGlowVFX != null && crystalPositions != null)
            {
                foreach (var crystalPos in crystalPositions)
                {
                    if (crystalPos != null)
                    {
                        var glow = Instantiate(crystalGlowVFX, crystalPos.position, Quaternion.identity, crystalPos);
                        Destroy(glow, 1f);
                    }
                }
            }
            
            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator PerformCrystalAttack()
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
        
        Debug.Log("Ranged Rotkins Crystal Attack!");
        
        // Kristal hazırlık süresi
        yield return new WaitForSeconds(0.4f);
        
        // Crystal projectile fırlat
        if (crystalProjectilePrefab != null && playerTransform != null)
        {
            Vector2 direction = (playerTransform.position - crystalLaunchPoint.position).normalized;
            
            GameObject crystal = Instantiate(crystalProjectilePrefab, crystalLaunchPoint.position, Quaternion.identity);
            
            // Crystal'ın RotkinsProjectile component'ini ayarla
            var projectile = crystal.GetComponent<RotkinsProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(direction, crystalSpeed, crystalDamage, false); // false = normal projectile
            }
            
            // Ses efekti çal
            if (enemyData?.attackSound != null)
            {
                AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
            }
        }
        
        // Saldırı sonrası bekleme
        yield return new WaitForSeconds(0.3f);
        
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
        
        Debug.Log("Ranged Rotkins Root Attack!");
        
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
                
                // Player'a hasar ver
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(rootAttackDamage);
                    Debug.Log($"Root Attack Hit! Damage: {rootAttackDamage}");
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
        
        // Crystal attack menzilini çiz
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, crystalRange);
        
        // Root attack menzilini çiz
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rootAttackRange);
    }

    public override void TakeDamage(float amount)
    {
        // Zayıf defans
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
