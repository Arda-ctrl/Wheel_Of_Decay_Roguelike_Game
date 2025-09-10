using UnityEngine;
using System.Collections;

public class TankRotkinsShieldController : EnemyController
{
    [Header("Shield Tank Rotkins Settings")]
    [SerializeField] private KingdomEnemyData enemyData;
    [SerializeField] private float shieldBashRange = 2.5f;
    [SerializeField] private float shieldBashDamage = 35f;
    [SerializeField] private float shieldBashKnockback = 800f;
    
    [Header("Shield Mechanics")]
    [SerializeField] private float shieldDirection = 0f; // 0 = right, 180 = left
    [SerializeField] private float shieldAngle = 90f; // Shield coverage angle
    [SerializeField] private float shieldDamageReduction = 0.6f; // 60% damage reduction from shield side
    [SerializeField] private float shieldPullRange = 4f;
    [SerializeField] private float shieldDefenseBonus = 0.8f; // Extra defense when shield is pulled close
    
    [Header("Attack Timings")]
    [SerializeField] private float lastAttackTime;
    [SerializeField] private float lastShieldPullTime;
    [SerializeField] private float shieldPullCooldown = 8f;
    [SerializeField] private float shieldPullDuration = 3f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string attackAnimationName = "Attack";
    [SerializeField] private string shieldPullAnimationName = "ShieldPull";
    
    [Header("Visual Components")]
    [SerializeField] private Transform shieldTransform;
    [SerializeField] private GameObject shieldPullVFX;
    [SerializeField] private Transform attackPoint;
    
    private bool isPerformingShieldBash = false;
    private bool isShieldPulled = false;
    private bool isShieldPulling = false;

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
        
        // Attack point referansını ayarla
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
        
        // Shield direction'ı player'ın yönüne göre ayarla
        UpdateShieldDirection();
    }

    protected override void Update()
    {
        base.Update();
        
        // Shield direction'ı sürekli güncelle
        if (!isShieldPulling && !isPerformingShieldBash)
        {
            UpdateShieldDirection();
        }
        
        // Saldırı kontrolleri
        if (playerTransform != null && !isPerformingShieldBash && !isShieldPulling)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Shield Pull zamanı geldi mi?
            if (Time.time - lastShieldPullTime >= shieldPullCooldown && distanceToPlayer <= shieldPullRange)
            {
                StartCoroutine(PerformShieldPull());
            }
            // Normal kalkan saldırısı
            else if (distanceToPlayer <= shieldBashRange && Time.time - lastAttackTime >= enemyData.attackCooldown)
            {
                StartCoroutine(PerformShieldBash());
            }
        }
    }

    private void UpdateShieldDirection()
    {
        if (playerTransform != null)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            shieldDirection = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            
            // Shield transform'unu güncelle
            if (shieldTransform != null)
            {
                shieldTransform.rotation = Quaternion.Euler(0, 0, shieldDirection);
            }
        }
    }

    private IEnumerator PerformShieldBash()
    {
        isPerformingShieldBash = true;
        lastAttackTime = Time.time;
        
        // Hareket dur
        rb.linearVelocity = Vector2.zero;
        
        // Attack animasyonunu başlat
        if (animator != null)
        {
            animator.Play(attackAnimationName);
        }
        
        Debug.Log("Tank Rotkins Shield Bash!");
        
        // Saldırıya hazırlık
        yield return new WaitForSeconds(0.3f);
        
        // Player'a doğru hızlı hareket (shield bash)
        if (playerTransform != null)
        {
            Vector2 bashDirection = (playerTransform.position - transform.position).normalized;
            rb.AddForce(bashDirection * 1000f, ForceMode2D.Impulse);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        // Saldırıyı uygula
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= shieldBashRange)
            {
                // Player'a hasar ver
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(shieldBashDamage);
                    Debug.Log($"Shield Bash Hit! Damage: {shieldBashDamage}");
                }
                
                // Knockback uygula
                var playerRb = playerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = (playerTransform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDirection * shieldBashKnockback, ForceMode2D.Impulse);
                }
                
                // Ses efekti çal
                if (enemyData?.attackSound != null)
                {
                    AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
                }
            }
        }
        
        // Saldırı sonrası durma
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        
        isPerformingShieldBash = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    private IEnumerator PerformShieldPull()
    {
        isShieldPulling = true;
        isShieldPulled = true;
        lastShieldPullTime = Time.time;
        
        // Hareket dur
        rb.linearVelocity = Vector2.zero;
        
        Debug.Log("Tank Rotkins Shield Pull - Defensive Mode!");
        
        // Shield pull animasyonu
        if (animator != null)
        {
            animator.Play(shieldPullAnimationName);
        }
        
        // VFX spawn et
        if (shieldPullVFX != null)
        {
            Instantiate(shieldPullVFX, transform.position, Quaternion.identity);
        }
        
        // Shield pull süresi boyunca bekle
        yield return new WaitForSeconds(shieldPullDuration);
        
        isShieldPulling = false;
        isShieldPulled = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
        
        Debug.Log("Shield Pull ended - Normal mode");
    }

    public override void TakeDamage(float amount)
    {
        float finalDamage = amount;
        
        // Shield direction kontrolü - eğer saldırı kalkan tarafından geliyorsa hasar azalt
        if (playerTransform != null)
        {
            Vector2 damageDirection = (transform.position - playerTransform.position).normalized;
            Vector2 shieldDir = new Vector2(Mathf.Cos(shieldDirection * Mathf.Deg2Rad), Mathf.Sin(shieldDirection * Mathf.Deg2Rad));
            
            float angle = Vector2.Angle(damageDirection, shieldDir);
            
            // Eğer hasar kalkan açısı içinden geliyorsa
            if (angle <= shieldAngle / 2f)
            {
                finalDamage *= shieldDamageReduction;
                Debug.Log($"Shield blocked! Damage reduced from {amount} to {finalDamage}");
            }
        }
        
        // Shield pull durumunda ekstra defans bonusu
        if (isShieldPulled)
        {
            finalDamage *= shieldDefenseBonus;
            Debug.Log($"Shield pulled! Extra defense - damage: {finalDamage}");
        }
        
        // Enemy data defans bonus
        if (enemyData != null)
        {
            finalDamage /= enemyData.defenseMultiplier;
        }
        
        base.TakeDamage(finalDamage);
        
        // Hasar alma ses efekti
        if (enemyData?.hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.hurtSound, transform.position);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Shield bash menzilini çiz
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shieldBashRange);
        
        // Shield pull menzilini çiz
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shieldPullRange);
        
        // Shield direction ve angle'ını çiz
        Gizmos.color = Color.green;
        Vector3 shieldDir = new Vector3(Mathf.Cos(shieldDirection * Mathf.Deg2Rad), Mathf.Sin(shieldDirection * Mathf.Deg2Rad), 0);
        Gizmos.DrawRay(transform.position, shieldDir * 2f);
        
        // Shield coverage area
        float halfAngle = shieldAngle / 2f;
        Vector3 leftBound = Quaternion.Euler(0, 0, halfAngle) * shieldDir;
        Vector3 rightBound = Quaternion.Euler(0, 0, -halfAngle) * shieldDir;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftBound * 1.5f);
        Gizmos.DrawRay(transform.position, rightBound * 1.5f);
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
