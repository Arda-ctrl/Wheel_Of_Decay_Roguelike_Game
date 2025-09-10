using UnityEngine;
using System.Collections;

public class PoisonDecayElfSpearman : VerdantEnemy
{
    [Header("Poison Decay Elf Spearman Specific")]
    [SerializeField] private GameObject poisonSpearPrefab;
    [SerializeField] private GameObject poisonCrystalPrefab;
    [SerializeField] private float spearRange = 3f;
    [SerializeField] private float spearDamage = 16f;
    [SerializeField] private float lungeDistance = 2f;
    [SerializeField] private float lungeSpeed = 10f;
    [SerializeField] private float lungeCooldown = 4f;
    [SerializeField] private float spearThrowChance = 0.2f; // 20% chance
    [SerializeField] private float spearThrowCooldown = 8f;
    [SerializeField] private float spearThrowRange = 6f;
    [SerializeField] private float spearThrowSpeed = 8f;
    [SerializeField] private float spearRetrieveTime = 2f;
    [SerializeField] private float poisonDamage = 3f;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonAreaRadius = 3f;
    [SerializeField] private float poisonAreaDuration = 8f;
    [SerializeField] private GameObject poisonAreaEffect;
    
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
            Debug.Log($"☠️ {enemyData.enemyName} has no spear!");
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Decide attack type based on distance and cooldowns
        if (distanceToPlayer <= spearRange && Time.time >= lastLungeTime + lungeCooldown)
        {
            PerformPoisonLungeAttack();
        }
        else if (distanceToPlayer > spearRange && distanceToPlayer <= spearThrowRange && 
                 Time.time >= lastSpearThrowTime + spearThrowCooldown && 
                 Random.value < spearThrowChance)
        {
            PerformPoisonSpearThrow();
        }
        else
        {
            // Regular poison spear attack
            PerformRegularPoisonSpearAttack();
        }
    }

    private void PerformPoisonLungeAttack()
    {
        if (isLunging) return;
        
        StartCoroutine(PoisonLungeAttackSequence());
        lastLungeTime = Time.time;
    }

    private IEnumerator PoisonLungeAttackSequence()
    {
        isLunging = true;
        
        Debug.Log($"☠️ {enemyData.enemyName} performs poison lunge attack!");
        
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
        
        // Deal poison damage if player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= spearRange)
        {
            DealPoisonSpearDamage();
        }
        
        isLunging = false;
    }

    private void PerformPoisonSpearThrow()
    {
        if (isThrowingSpear || !hasSpear) return;
        
        StartCoroutine(PoisonSpearThrowSequence());
        lastSpearThrowTime = Time.time;
    }

    private IEnumerator PoisonSpearThrowSequence()
    {
        isThrowingSpear = true;
        hasSpear = false;
        
        Debug.Log($"☠️ {enemyData.enemyName} throws poison spear!");
        
        // Calculate throw direction
        Vector2 throwDirection = (playerTransform.position - transform.position).normalized;
        
        // Create thrown poison spear
        if (poisonSpearPrefab != null)
        {
            thrownSpear = Instantiate(poisonSpearPrefab, transform.position, Quaternion.identity);
            
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
        
        // Create poison area where spear lands
        Vector2 poisonAreaPosition = (Vector2)transform.position + throwDirection * spearThrowRange;
        CreatePoisonArea(poisonAreaPosition);
        
        // Wait for spear to be retrieved
        yield return new WaitForSeconds(spearRetrieveTime);
        
        // Retrieve spear (simplified - in reality, spear would need to return)
        if (thrownSpear != null)
        {
            Destroy(thrownSpear);
        }
        
        hasSpear = true;
        isThrowingSpear = false;
        
        Debug.Log($"☠️ {enemyData.enemyName} retrieved poison spear!");
    }

    private void PerformRegularPoisonSpearAttack()
    {
        Debug.Log($"☠️ {enemyData.enemyName} performs regular poison spear attack!");
        
        // Deal poison damage if player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= spearRange)
        {
            DealPoisonSpearDamage();
        }
    }

    private void DealPoisonSpearDamage()
    {
        // Deal damage to player
        if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }
        
        // Apply poison status effect
        if (PlayerController.Instance != null)
        {
            var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
            if (playerStatus != null)
            {
                playerStatus.ApplyStatus(StatusEffectType.Poisoned, poisonDuration);
                Debug.Log($"☠️ {enemyData.enemyName} poisoned the player!");
            }
        }
        
        Debug.Log($"☠️ {enemyData.enemyName} dealt {spearDamage} poison spear damage!");
    }

    private void CreatePoisonArea(Vector2 position)
    {
        Debug.Log($"☠️ {enemyData.enemyName} created poison area at {position}!");
        
        // Create poison area effect
        if (poisonAreaEffect != null)
        {
            GameObject poisonArea = Instantiate(poisonAreaEffect, position, Quaternion.identity);
            Destroy(poisonArea, poisonAreaDuration);
        }
        
        // Start poison area damage coroutine
        StartCoroutine(PoisonAreaDamage(position));
    }

    private IEnumerator PoisonAreaDamage(Vector2 position)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < poisonAreaDuration)
        {
            // Check for players in poison area
            Collider2D[] targets = Physics2D.OverlapCircleAll(position, poisonAreaRadius);
            
            foreach (Collider2D target in targets)
            {
                if (target.CompareTag("Player"))
                {
                    // Apply poison damage over time
                    if (PlayerHealthController.Instance != null)
                    {
                        PlayerHealthController.Instance.DamagePlayer();
                    }
                    
                    // Apply poison status effect
                    if (PlayerController.Instance != null)
                    {
                        var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
                        if (playerStatus != null)
                        {
                            playerStatus.ApplyStatus(StatusEffectType.Poisoned, poisonDuration);
                        }
                    }
                }
            }
            
            elapsedTime += 1f; // Damage every second
            yield return new WaitForSeconds(1f);
        }
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
        
        // Poison Spearman special ability - Poison Cloud
        StartCoroutine(PoisonCloudAbility());
    }

    private IEnumerator PoisonCloudAbility()
    {
        Debug.Log($"☠️ {enemyData.enemyName} used Poison Cloud!");
        
        // Create multiple poison areas around the spearman
        Vector2[] poisonPositions = {
            transform.position + Vector3.up * 2f,
            transform.position + Vector3.down * 2f,
            transform.position + Vector3.left * 2f,
            transform.position + Vector3.right * 2f
        };
        
        foreach (Vector2 pos in poisonPositions)
        {
            CreatePoisonArea(pos);
            yield return new WaitForSeconds(0.5f);
        }
    }

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"☠️ Poison Decay Elf Spearman spawned! Kingdom: {enemyData.kingdomType}");
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
        
        // Draw poison area radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, poisonAreaRadius);
    }
}
