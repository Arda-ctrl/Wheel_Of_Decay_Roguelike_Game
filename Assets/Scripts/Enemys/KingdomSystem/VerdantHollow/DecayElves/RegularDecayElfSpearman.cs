using UnityEngine;
using System.Collections;

public class RegularDecayElfSpearman : VerdantEnemy
{
    [Header("Regular Decay Elf Spearman Specific")]
    [SerializeField] private GameObject longSpearPrefab;
    [SerializeField] private GameObject crystalSpearPrefab;
    [SerializeField] private float spearRange = 3f;
    [SerializeField] private float spearDamage = 18f;
    [SerializeField] private float lungeDistance = 2f;
    [SerializeField] private float lungeSpeed = 10f;
    [SerializeField] private float lungeCooldown = 4f;
    [SerializeField] private float spearThrowChance = 0.15f; // 15% chance
    [SerializeField] private float spearThrowCooldown = 8f;
    [SerializeField] private float spearThrowRange = 6f;
    [SerializeField] private float spearThrowSpeed = 8f;
    [SerializeField] private float spearRetrieveTime = 2f;
    
    private float lastLungeTime = 0f;
    private float lastSpearThrowTime = 0f;
    private bool isLunging = false;
    private bool isThrowingSpear = false;
    private bool hasSpear = true;
    private GameObject thrownSpear = null;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        if (!hasSpear)
        {
            // Can't attack without spear
            Debug.Log($"🗡️ {enemyData.enemyName} has no spear!");
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Decide attack type based on distance and cooldowns
        if (distanceToPlayer <= spearRange && Time.time >= lastLungeTime + lungeCooldown)
        {
            PerformLungeAttack();
        }
        else if (distanceToPlayer > spearRange && distanceToPlayer <= spearThrowRange && 
                 Time.time >= lastSpearThrowTime + spearThrowCooldown && 
                 Random.value < spearThrowChance)
        {
            PerformSpearThrow();
        }
        else
        {
            // Regular spear attack
            PerformRegularSpearAttack();
        }
    }

    private void PerformLungeAttack()
    {
        if (isLunging) return;
        
        StartCoroutine(LungeAttackSequence());
        lastLungeTime = Time.time;
    }

    private IEnumerator LungeAttackSequence()
    {
        isLunging = true;
        
        Debug.Log($"🗡️ {enemyData.enemyName} performs lunge attack!");
        
        // Calculate lunge direction
        Vector2 lungeDirection = (playerTransform.position - transform.position).normalized;
        
        // Store original speed
        float originalSpeed = GetCurrentSpeed();
        
        // Set lunge speed
        SetSpeedMultiplier(lungeSpeed / enemyData.baseSpeed);
        
        // Perform lunge
        float lungeTime = lungeDistance / lungeSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < lungeTime)
        {
            if (rb != null)
            {
                rb.linearVelocity = lungeDirection * lungeSpeed;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Restore original speed
        SetSpeedMultiplier(originalSpeed / enemyData.baseSpeed);
        
        // Deal damage if player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= spearRange)
        {
            DealSpearDamage();
        }
        
        isLunging = false;
    }

    private void PerformSpearThrow()
    {
        if (isThrowingSpear || !hasSpear) return;
        
        StartCoroutine(SpearThrowSequence());
        lastSpearThrowTime = Time.time;
    }

    private IEnumerator SpearThrowSequence()
    {
        isThrowingSpear = true;
        hasSpear = false;
        
        Debug.Log($"🗡️ {enemyData.enemyName} throws spear!");
        
        // Calculate throw direction
        Vector2 throwDirection = (playerTransform.position - transform.position).normalized;
        
        // Create thrown spear
        if (crystalSpearPrefab != null)
        {
            thrownSpear = Instantiate(crystalSpearPrefab, transform.position, Quaternion.identity);
            
            // Set spear properties
            var spearComponent = thrownSpear.GetComponent<EnemyProjectile>();
            if (spearComponent != null)
            {
                spearComponent.Initialize(throwDirection, spearThrowSpeed, spearDamage, gameObject);
            }
            else
            {
                Rigidbody2D spearRb = thrownSpear.GetComponent<Rigidbody2D>();
                if (spearRb != null)
                {
                    spearRb.linearVelocity = throwDirection * spearThrowSpeed;
                }
            }
        }
        
        // Wait for spear to be retrieved
        yield return new WaitForSeconds(spearRetrieveTime);
        
        // Retrieve spear (simplified - in reality, spear would need to return)
        if (thrownSpear != null)
        {
            Destroy(thrownSpear);
        }
        
        hasSpear = true;
        isThrowingSpear = false;
        
        Debug.Log($"🗡️ {enemyData.enemyName} retrieved spear!");
    }

    private void PerformRegularSpearAttack()
    {
        Debug.Log($"🗡️ {enemyData.enemyName} performs regular spear attack!");
        
        // Deal damage if player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= spearRange)
        {
            DealSpearDamage();
        }
    }

    private void DealSpearDamage()
    {
        // Deal damage to player
        if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }
        
        Debug.Log($"🗡️ {enemyData.enemyName} dealt {spearDamage} spear damage!");
    }

    protected override void UpdateAI()
    {
        if (isLunging || isThrowingSpear) return; // Don't update AI during special attacks
        
        base.UpdateAI();
    }

    protected override void MoveTowardsPlayer()
    {
        if (isLunging || isThrowingSpear) return; // Don't move during special attacks
        
        // Keep distance from player - spearman prefers to maintain range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > spearRange + 1f) // Stay just outside spear range
        {
            base.MoveTowardsPlayer();
        }
        else if (distanceToPlayer < spearRange - 1f) // Back away if too close
        {
            Vector2 retreatDirection = (transform.position - playerTransform.position).normalized;
            if (rb != null)
            {
                rb.linearVelocity = retreatDirection * GetCurrentSpeed() * 0.5f;
            }
        }
        else
        {
            // Stop and maintain distance
            StopMoving();
        }
    }

    protected override void UseSpecialAbility()
    {
        base.UseSpecialAbility();
        
        // Spearman special ability - Enhanced Lunge
        if (Time.time >= lastLungeTime + lungeCooldown)
        {
            StartCoroutine(EnhancedLungeAttack());
        }
    }

    private IEnumerator EnhancedLungeAttack()
    {
        Debug.Log($"🗡️ {enemyData.enemyName} used Enhanced Lunge!");
        
        // Enhanced lunge with longer range and more damage
        float enhancedDistance = lungeDistance * 1.5f;
        float enhancedSpeed = lungeSpeed * 1.2f;
        float enhancedDamage = spearDamage * 1.3f;
        
        isLunging = true;
        lastLungeTime = Time.time;
        
        Vector2 lungeDirection = (playerTransform.position - transform.position).normalized;
        float originalSpeed = GetCurrentSpeed();
        
        SetSpeedMultiplier(enhancedSpeed / enemyData.baseSpeed);
        
        float lungeTime = enhancedDistance / enhancedSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < lungeTime)
        {
            if (rb != null)
            {
                rb.linearVelocity = lungeDirection * enhancedSpeed;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        SetSpeedMultiplier(originalSpeed / enemyData.baseSpeed);
        
        // Deal enhanced damage
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= spearRange)
        {
            if (PlayerHealthController.Instance != null)
            {
                PlayerHealthController.Instance.DamagePlayer();
            }
            Debug.Log($"🗡️ {enemyData.enemyName} dealt {enhancedDamage} enhanced spear damage!");
        }
        
        isLunging = false;
    }

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"🗡️ Regular Decay Elf Spearman spawned! Kingdom: {enemyData.kingdomType}");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw spear range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, spearRange);
        
        // Draw spear throw range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spearThrowRange);
        
        // Draw lunge distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lungeDistance);
    }
}
