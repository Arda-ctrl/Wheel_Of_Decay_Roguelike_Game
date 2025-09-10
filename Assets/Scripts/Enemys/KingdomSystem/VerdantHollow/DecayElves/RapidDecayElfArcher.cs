using UnityEngine;
using System.Collections;

public class RapidDecayElfArcher : DecayElfArcher
{
    [Header("Rapid Decay Elf Archer Specific")]
    [SerializeField] private GameObject shortBowPrefab;
    [SerializeField] private GameObject smallArrowPrefab;
    [SerializeField] private float rapidFireRate = 0.3f; // Time between shots in rapid fire
    [SerializeField] private int rapidFireCount = 5; // Number of arrows in rapid fire
    [SerializeField] private float rapidFireCooldown = 6f; // Cooldown between rapid fire sequences
    [SerializeField] private float smallArrowSpeed = 12f;
    [SerializeField] private float smallArrowDamage = 8f;
    [SerializeField] private bool canShootWhileMoving = true;
    
    private float lastRapidFireTime = 0f;
    private bool isRapidFiring = false;
    private bool isMoving = false;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Rapid Decay Elf can shoot while moving or do rapid fire
        if (isMoving && canShootWhileMoving)
        {
            ShootWhileMoving();
        }
        else if (Time.time >= lastRapidFireTime + rapidFireCooldown)
        {
            StartRapidFire();
        }
        else
        {
            // Regular single shot
            ShootSmallArrow();
        }
    }

    private void ShootSmallArrow()
    {
        if (arrowSpawnPoint == null)
        {
            arrowSpawnPoint = transform;
        }

        // Calculate direction to player
        Vector2 direction = (playerTransform.position - arrowSpawnPoint.position).normalized;
        
        // Create small arrow
        GameObject arrow = Instantiate(smallArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        
        // Set arrow properties
        var arrowComponent = arrow.GetComponent<EnemyProjectile>();
        if (arrowComponent != null)
        {
            arrowComponent.Initialize(direction, smallArrowSpeed, smallArrowDamage, gameObject);
        }
        else
        {
            // Fallback if no EnemyProjectile component
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * smallArrowSpeed;
            }
        }
        
        Debug.Log($"🏹 {enemyData.enemyName} shot a small arrow!");
    }

    private void ShootWhileMoving()
    {
        // Shoot while moving - less accurate but faster
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        
        // Add some spread when moving
        float spreadAngle = Random.Range(-15f, 15f);
        direction = Quaternion.Euler(0, 0, spreadAngle) * direction;
        
        GameObject arrow = Instantiate(smallArrowPrefab, transform.position, Quaternion.identity);
        
        var arrowComponent = arrow.GetComponent<EnemyProjectile>();
        if (arrowComponent != null)
        {
            arrowComponent.Initialize(direction, smallArrowSpeed * 0.8f, smallArrowDamage * 0.7f, gameObject);
        }
        else
        {
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * smallArrowSpeed * 0.8f;
            }
        }
        
        Debug.Log($"🏃‍♂️ {enemyData.enemyName} shot while moving!");
    }

    private void StartRapidFire()
    {
        if (isRapidFiring) return;
        
        StartCoroutine(RapidFireSequence());
        lastRapidFireTime = Time.time;
    }

    private IEnumerator RapidFireSequence()
    {
        isRapidFiring = true;
        
        Debug.Log($"⚡ {enemyData.enemyName} started rapid fire!");
        
        // Stop movement during rapid fire for accuracy
        Vector2 originalVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.zero;
        
        for (int i = 0; i < rapidFireCount; i++)
        {
            ShootSmallArrow();
            yield return new WaitForSeconds(rapidFireRate);
        }
        
        // Resume movement
        rb.linearVelocity = originalVelocity;
        
        isRapidFiring = false;
        Debug.Log($"⚡ {enemyData.enemyName} finished rapid fire!");
    }

    protected override void UpdateAI()
    {
        if (isRapidFiring) return; // Don't update AI while rapid firing
        
        // Track if we're moving
        isMoving = rb != null && rb.linearVelocity.magnitude > 0.1f;
        
        base.UpdateAI();
    }

    protected override void MoveTowardsPlayer()
    {
        if (isRapidFiring) return; // Don't move while rapid firing
        
        base.MoveTowardsPlayer();
    }

    protected override void PerformDecayElfSpecialAbility()
    {
        // Rapid Decay Elf special ability - Enhanced Rapid Fire
        if (Time.time >= lastRapidFireTime + rapidFireCooldown)
        {
            StartCoroutine(EnhancedRapidFire());
        }
    }

    private IEnumerator EnhancedRapidFire()
    {
        Debug.Log($"⚡ {enemyData.enemyName} used Enhanced Rapid Fire!");
        
        // Enhanced rapid fire with more arrows and faster rate
        int enhancedCount = rapidFireCount + 3;
        float enhancedRate = rapidFireRate * 0.7f;
        
        isRapidFiring = true;
        lastRapidFireTime = Time.time;
        
        // Stop movement for accuracy
        Vector2 originalVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.zero;
        
        for (int i = 0; i < enhancedCount; i++)
        {
            ShootSmallArrow();
            yield return new WaitForSeconds(enhancedRate);
        }
        
        // Resume movement
        rb.linearVelocity = originalVelocity;
        
        isRapidFiring = false;
    }

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"⚡ Rapid Decay Elf Archer spawned! Kingdom: {enemyData.kingdomType}");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw rapid fire range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
}