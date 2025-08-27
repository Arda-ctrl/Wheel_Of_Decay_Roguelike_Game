using UnityEngine;
using System.Collections;

public class BigSlimeElite : BaseSlimeController
{
    [Header("Big Slime Elite Settings")]
    [SerializeField] private GameObject smallMyceloidPrefab;
    [SerializeField] private GameObject mediumMyceloidPrefab;
    [SerializeField] private GameObject mudAreaPrefab;
    [SerializeField] private GameObject jumpImpactEffectPrefab;
    [SerializeField] private Transform[] slimeThrowPoints;
    
    [Header("Elite Stats")]
    [SerializeField] private float eliteSize = 3f;
    [SerializeField] private float eliteHealth = 300f;
    [SerializeField] private float eliteSpeed = 1.5f;
    
    [Header("Slime Throwing Attack")]
    [SerializeField] private float slimeThrowRange = 12f;
    [SerializeField] private float slimeThrowCooldown = 4f;
    [SerializeField] private float slimeThrowForce = 8f;
    [SerializeField] private int slimesPerThrow = 2;
    [SerializeField] private float mediumSlimeChance = 0.3f; // 30% chance to throw medium instead of small
    
    [Header("Jump Attack")]
    [SerializeField] private float jumpAttackRange = 15f;
    [SerializeField] private float jumpAttackCooldown = 10f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpDamage = 60f;
    [SerializeField] private float mudAreaRadius = 8f;
    [SerializeField] private float mudAreaDuration = 10f;
    
    [Header("Death Spawning")]
    [SerializeField] private int deathSpawnCount = 10;
    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private float spawnForce = 5f;
    
    private float lastSlimeThrowTime = 0f;
    private float lastJumpAttackTime = 0f;
    private bool isJumping = false;
    private bool isThrowingSlimes = false;
    
    protected override void Start()
    {
        slimeType = SlimeType.Elite;
        
        // Scale up the elite slime
        transform.localScale = Vector3.one * eliteSize;
        
        // Set elite slime stats
        if (enemyData != null)
        {
            enemyData.maxHealth = eliteHealth;
            enemyData.baseSpeed = eliteSpeed;
            enemyData.baseDamage = 40f;
            enemyData.attackRange = slimeThrowRange;
            enemyData.attackCooldown = slimeThrowCooldown;
            enemyData.detectionRange = 12f;
        }
        
        // Larger settings for elite slime
        attackZoneRadius = 10f;
        roamRadius = 15f;
        roamSpeed = eliteSpeed * 0.8f;
        
        base.Start();
        
        Debug.Log("Big Slime Elite spawned with massive size and power!");
    }

    protected override void UpdateSlimeState()
    {
        base.UpdateSlimeState();
        
        // Check for special attacks
        if (currentSlimeState == SlimeState.Attacking && !isJumping && !isThrowingSlimes)
        {
            ConsiderSpecialAttacks();
        }
    }

    private void ConsiderSpecialAttacks()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Consider jump attack if player is at medium range
        if (distanceToPlayer >= 6f && distanceToPlayer <= jumpAttackRange && 
            Time.time >= lastJumpAttackTime + jumpAttackCooldown)
        {
            StartCoroutine(PerformJumpAttack());
        }
        // Consider slime throwing if player is at long range
        else if (distanceToPlayer >= 4f && distanceToPlayer <= slimeThrowRange && 
                 Time.time >= lastSlimeThrowTime + slimeThrowCooldown)
        {
            StartCoroutine(PerformSlimeThrow());
        }
    }

    protected override bool CanAttack()
    {
        return !isAttacking && !isJumping && !isThrowingSlimes;
    }

    protected override void PerformSlimeAttack()
    {
        // Primary attack is slime throwing
        if (!isThrowingSlimes && Time.time >= lastSlimeThrowTime + slimeThrowCooldown)
        {
            StartCoroutine(PerformSlimeThrow());
        }
    }

    private IEnumerator PerformSlimeThrow()
    {
        isThrowingSlimes = true;
        lastSlimeThrowTime = Time.time;
        
        // Stop moving and face player
        StopMoving();
        if (playerTransform != null)
        {
            bool shouldFaceRight = playerTransform.position.x > transform.position.x;
            if (shouldFaceRight != isFacingRight)
            {
                FlipSprite();
            }
        }
        
        // Play throw animation
        if (animator != null)
        {
            animator.SetTrigger("ThrowSlimes");
        }
        
        // Wind-up time
        yield return new WaitForSeconds(0.8f);
        
        // Throw slimes
        for (int i = 0; i < slimesPerThrow; i++)
        {
            ThrowSlimeProjectile();
            yield return new WaitForSeconds(0.3f); // Slight delay between throws
        }
        
        yield return new WaitForSeconds(0.5f);
        isThrowingSlimes = false;
    }

    private void ThrowSlimeProjectile()
    {
        if (playerTransform == null) return;
        
        // Determine which type of slime to throw
        GameObject slimePrefab = Random.value < mediumSlimeChance ? mediumMyceloidPrefab : smallMyceloidPrefab;
        if (slimePrefab == null) return;
        
        // Choose spawn point
        Transform spawnPoint = slimeThrowPoints != null && slimeThrowPoints.Length > 0 ? 
                              slimeThrowPoints[Random.Range(0, slimeThrowPoints.Length)] : 
                              transform;
        
        Vector3 spawnPos = spawnPoint.position;
        
        // Create slime projectile
        GameObject thrownSlime = Instantiate(slimePrefab, spawnPos, Quaternion.identity);
        
        // Add projectile behavior
        SlimeProjectile slimeProj = thrownSlime.AddComponent<SlimeProjectile>();
        
        // Calculate throw direction with arc
        Vector2 targetPos = PredictPlayerPosition();
        Vector2 throwDirection = (targetPos - (Vector2)spawnPos).normalized;
        
        slimeProj.Initialize(throwDirection, slimeThrowForce, 4f, gameObject); // 4 second flight time
        
        Debug.Log($"Elite threw a {(slimePrefab == mediumMyceloidPrefab ? "medium" : "small")} slime!");
    }

    private Vector2 PredictPlayerPosition()
    {
        if (playerTransform == null) return Vector2.zero;
        
        Vector2 playerPos = playerTransform.position;
        
        // Predict player movement
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerPos += playerRb.linearVelocity * 1.5f; // Predict further ahead for thrown slimes
        }
        
        return playerPos;
    }

    private IEnumerator PerformJumpAttack()
    {
        isJumping = true;
        lastJumpAttackTime = Time.time;
        
        // Stop moving
        StopMoving();
        
        // Play jump animation
        if (animator != null)
        {
            animator.SetTrigger("MegaJump");
        }
        
        // Wind-up time
        yield return new WaitForSeconds(1f);
        
        // Calculate jump direction towards player
        Vector2 jumpDirection = Vector2.up;
        if (playerTransform != null)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            jumpDirection = new Vector2(directionToPlayer.x * 0.5f, 1f).normalized;
        }
        
        // Perform massive jump
        if (rb != null)
        {
            rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        }
        
        // Wait for landing
        yield return new WaitForSeconds(1.5f);
        
        // Create landing impact
        CreateMegaJumpImpact();
        
        isJumping = false;
    }

    private void CreateMegaJumpImpact()
    {
        // Create massive impact effect
        if (jumpImpactEffectPrefab != null)
        {
            GameObject impact = Instantiate(jumpImpactEffectPrefab, transform.position, Quaternion.identity);
            impact.transform.localScale = Vector3.one * 2f; // Massive effect
        }
        
        // Apply damage to player if in range
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= 5f) // Jump impact radius
            {
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(jumpDamage);
                }
                else if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }
                
                // Massive knockback
                Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDir * 20f, ForceMode2D.Impulse);
                }
            }
        }
        
        // Create mud area that persists
        CreateMudArea();
    }

    private void CreateMudArea()
    {
        if (mudAreaPrefab != null)
        {
            GameObject mudArea = Instantiate(mudAreaPrefab, transform.position, Quaternion.identity);
            mudArea.transform.localScale = Vector3.one * mudAreaRadius;
            
            // Set up mud area component if it exists
            MudArea mudScript = mudArea.GetComponent<MudArea>();
            if (mudScript != null)
            {
                mudScript.Initialize(mudAreaDuration, 10f, true); // 10 damage per second, slows player
            }
            
            Debug.Log($"Created persistent mud area with radius {mudAreaRadius}");
        }
    }

    protected override void OnSlimeDeath()
    {
        // Spawn many small slimes
        SpawnDeathSlimes();
        
        // Create massive death explosion
        CreateEliteDeathExplosion();
    }

    private void SpawnDeathSlimes()
    {
        for (int i = 0; i < deathSpawnCount; i++)
        {
            if (smallMyceloidPrefab == null) continue;
            
            // Calculate spawn position in circle around elite
            float angle = (360f / deathSpawnCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + (Vector3)direction * spawnRadius;
            
            // Create small slime
            GameObject spawnedSlime = Instantiate(smallMyceloidPrefab, spawnPos, Quaternion.identity);
            
            // Add outward force
            Rigidbody2D slimeRb = spawnedSlime.GetComponent<Rigidbody2D>();
            if (slimeRb != null)
            {
                slimeRb.AddForce(direction * spawnForce, ForceMode2D.Impulse);
            }
            
            Debug.Log($"Spawned small slime {i + 1}/{deathSpawnCount} at {spawnPos}");
        }
    }

    private void CreateEliteDeathExplosion()
    {
        // Create multiple explosion effects
        if (jumpImpactEffectPrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 explosionPos = transform.position + Random.insideUnitSphere * 2f;
                explosionPos.z = 0f;
                
                GameObject explosion = Instantiate(jumpImpactEffectPrefab, explosionPos, Quaternion.identity);
                explosion.transform.localScale = Vector3.one * Random.Range(1.5f, 2.5f);
            }
        }
        
        // Apply damage to player if nearby
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= 8f) // Large explosion radius
            {
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(80f); // Massive death explosion damage
                }
                else if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }
            }
        }
        
        Debug.Log("Big Slime Elite died with massive explosion!");
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // Set elite-specific animation states
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsThrowingSlimes", isThrowingSlimes);
        
        // Set health percentage for rage states
        float healthPercent = currentHealth / enemyData.maxHealth;
        animator.SetFloat("HealthPercent", healthPercent);
        animator.SetBool("IsLowHealth", healthPercent < 0.3f); // Rage mode at low health
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw slime throw range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slimeThrowRange);
        
        // Draw jump attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpAttackRange);
        
        // Draw mud area radius
        Gizmos.color = new Color(0.6f, 0.3f, 0f); // Brown color
        Gizmos.DrawWireSphere(transform.position, mudAreaRadius);
        
        // Draw death spawn radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
