using UnityEngine;
using System.Collections;

public class TankRotkinsController : EnemyController
{
    [Header("Tank Rotkins Settings")]
    [SerializeField] private KingdomEnemyData enemyData;
    [SerializeField] private float areaAttackRange = 4f;
    [SerializeField] private float areaAttackDamage = 50f;
    [SerializeField] private float branchAttackRange = 3f;
    [SerializeField] private float branchAttackDamage = 30f;
    [SerializeField] private float branchAttackRadius = 1.2f;
    
    [Header("Attack Timings")]
    [SerializeField] private float lastAttackTime;
    [SerializeField] private float lastAreaAttackTime;
    [SerializeField] private float areaAttackCooldown = 6f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string attackAnimationName = "Attack";
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject areaAttackVFX;
    [SerializeField] private Transform attackPoint;
    
    private bool isPerformingAreaAttack = false;
    private bool isAttacking = false;

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
        
        // Attack point referansını ayarla (eğer yoksa transform'u kullan)
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // Saldırı menzilinde ve saldırı yapmıyorsa saldırıya geç
        if (playerTransform != null && !isAttacking && !isPerformingAreaAttack)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Alan saldırısı zamanı geldi mi?
            if (Time.time - lastAreaAttackTime >= areaAttackCooldown && distanceToPlayer <= areaAttackRange)
            {
                StartCoroutine(PerformAreaAttack());
            }
            // Normal dal saldırısı
            else if (distanceToPlayer <= branchAttackRange && Time.time - lastAttackTime >= enemyData.attackCooldown)
            {
                StartCoroutine(PerformBranchAttack());
            }
        }
    }

    private IEnumerator PerformBranchAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Hareket dur
        rb.linearVelocity = Vector2.zero;
        
        // Attack animasyonunu başlat
        if (animator != null)
        {
            animator.Play(attackAnimationName);
        }
        
        // Hasar uygulaması animasyon event'i ile yapılır (OnBranchHit)
        // Animasyon süresi kadar bekle
        yield return new WaitForSeconds(0.5f);
        
        // Saldırı bitişi bekleme
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    private IEnumerator PerformAreaAttack()
    {
        isPerformingAreaAttack = true;
        lastAreaAttackTime = Time.time;
        
        // Hareket dur
        rb.linearVelocity = Vector2.zero;
        
        Debug.Log("Tank Rotkins alan saldırısına başlıyor!");
        
        // Charging animation veya slow attack buildup
        if (animator != null)
        {
            animator.Play(attackAnimationName);
        }
        
        // Yavaş güçlü saldırı için bekleme süresi
        yield return new WaitForSeconds(1.5f);
        
        // Hasar ve VFX animasyon event'i ile yapılır (OnAreaHit)
        
        // Saldırı sonrası recovery time
        yield return new WaitForSeconds(1f);
        
        isPerformingAreaAttack = false;
        
        // İdle animasyonuna dön
        if (animator != null)
        {
            animator.Play(idleAnimationName);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Dal saldırısı vuruş yarıçapını çiz (AttackPoint merkezli)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint != null ? attackPoint.position : transform.position, branchAttackRadius);
        
        // Alan saldırısı menzilini çiz
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackPoint != null ? attackPoint.position : transform.position, areaAttackRange);
    }

    // Animation Event: Dal saldırısının vuruş anında çağrılır
    public void OnBranchHit()
    {
        if (attackPoint == null) attackPoint = transform;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, branchAttackRadius);
        if (hit != null && hit.CompareTag("Player"))
        {
            var health = hit.GetComponent<IHealth>();
            health?.TakeDamage(branchAttackDamage);
            Debug.Log($"Tank Rotkins dal saldırısı! Hasar: {branchAttackDamage}");

            if (enemyData?.attackSound != null)
            {
                AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
            }
        }
    }

    // Animation Event: Alan saldırısının etki anında çağrılır
    public void OnAreaHit()
    {
        if (attackPoint == null) attackPoint = transform;

        // VFX
        if (areaAttackVFX != null)
        {
            Instantiate(areaAttackVFX, attackPoint.position, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, areaAttackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var health = hit.GetComponent<IHealth>();
                health?.TakeDamage(areaAttackDamage);
                
                var playerRb = hit.attachedRigidbody;
                if (playerRb != null)
                {
                    Vector2 dir = (hit.transform.position - attackPoint.position).normalized;
                    playerRb.AddForce(dir * 500f, ForceMode2D.Impulse);
                }
            }
        }

        if (enemyData?.attackSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
        }
    }

    public override void TakeDamage(float amount)
    {
        // Defans bonus uygula
        float finalDamage = amount;
        if (enemyData != null)
        {
            finalDamage *= enemyData.defenseMultiplier;
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
