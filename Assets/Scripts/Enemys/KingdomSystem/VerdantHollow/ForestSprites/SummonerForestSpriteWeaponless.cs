using UnityEngine;
using System.Collections;

public class SummonerForestSpriteWeaponless : VerdantEnemy
{
    [Header("Summoner Sprite Settings")]
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] private float contactDamageRange = 1f;
    [SerializeField] private float flySpeed = 4f;
    [SerializeField] private float hoverDistance = 3f;
    [SerializeField] private float circleRadius = 2f;
    [SerializeField] private float circleSpeed = 2f;

    [Header("Split on Death Settings")]
    [SerializeField] private GameObject brainlessSpriteMinion; // Reference to brainless sprite prefab
    [SerializeField] private int splitCount = 4;
    [SerializeField] private float splitForce = 5f;
    [SerializeField] private float splitRadius = 2f;

    [Header("Visual Settings")]
    [SerializeField] private float scaleFactor = 1.3f; // Make it larger than normal sprites

    private Vector2 circleCenter;
    private float circleAngle = 0f;
    private bool isHoveringAroundPlayer = false;

    protected override void Start()
    {
        base.Start();
        
        // Make sprite larger
        transform.localScale = Vector3.one * scaleFactor;
        
        // Set summoner sprite stats
        if (enemyData != null)
        {
            enemyData.baseSpeed = flySpeed;
            enemyData.maxHealth *= 1.5f; // More health than normal sprites
            enemyData.baseDamage = contactDamage;
            enemyData.attackRange = contactDamageRange;
            enemyData.attackCooldown = 0.8f;
            enemyData.detectionRange = 8f;
        }
        
        // Disable gravity for flying behavior
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 1f; // Some drag for smoother movement
        }

        // Initialize current health with the updated max health
        currentHealth = enemyData.maxHealth;
    }

    protected override void UpdateAI()
    {
        if (PlayerController.Instance == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        isPlayerInRange = distanceToPlayer <= enemyData.detectionRange;

        if (isPlayerInRange)
        {
            HandlePlayerInRange(distanceToPlayer);
        }
        else
        {
            HandlePlayerOutOfRange();
        }

        CheckForPlayerContact();
    }

    protected override void HandlePlayerInRange(float distanceToPlayer)
    {
        // Hover around player at a distance, circling them
        if (distanceToPlayer > hoverDistance + circleRadius)
        {
            // Move closer to player
            Vector2 direction = (PlayerController.Instance.transform.position - transform.position).normalized;
            if (rb != null)
            {
                rb.linearVelocity = direction * flySpeed;
            }
            isHoveringAroundPlayer = false;
        }
        else if (distanceToPlayer < hoverDistance - circleRadius)
        {
            // Move away from player
            Vector2 direction = (transform.position - PlayerController.Instance.transform.position).normalized;
            if (rb != null)
            {
                rb.linearVelocity = direction * flySpeed;
            }
            isHoveringAroundPlayer = false;
        }
        else
        {
            // Circle around player
            CircleAroundPlayer();
            isHoveringAroundPlayer = true;
        }

        // Update sprite direction
        UpdateSpriteDirection();
    }

    protected override void HandlePlayerOutOfRange()
    {
        // Stop moving and idle
        if (rb != null)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);
        }
        isHoveringAroundPlayer = false;
    }

    private void CircleAroundPlayer()
    {
        // Set circle center to player position
        circleCenter = PlayerController.Instance.transform.position;
        
        // Update circle angle
        circleAngle += circleSpeed * Time.fixedDeltaTime;
        if (circleAngle >= 360f) circleAngle = 0f;
        
        // Calculate target position on circle
        Vector2 targetPosition = circleCenter + new Vector2(
            Mathf.Cos(circleAngle * Mathf.Deg2Rad) * circleRadius,
            Mathf.Sin(circleAngle * Mathf.Deg2Rad) * circleRadius
        );
        
        // Move towards target position
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        if (rb != null)
        {
            rb.linearVelocity = direction * flySpeed * 0.8f; // Slightly slower when circling
        }
    }

    private void UpdateSpriteDirection()
    {
        if (spriteRenderer != null && rb != null)
        {
            isFacingRight = rb.linearVelocity.x > 0;
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    private void CheckForPlayerContact()
    {
        if (PlayerController.Instance == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        if (distanceToPlayer <= contactDamageRange)
        {
            DealContactDamage();
        }
    }

    private void DealContactDamage()
    {
        if (PlayerController.Instance != null && CanAttack())
        {
            var playerHealth = PlayerController.Instance.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer();
                lastAttackTime = Time.time;
                
                Debug.Log($"🧚‍♀️ {enemyData.enemyName} dealt contact damage to player!");
                
                // Play contact effect
                if (enemyData.attackSound != null)
                {
                    AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
                }

                // Try to apply verdant effects
                PerformAttack();
            }
        }
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Contact damage is already handled in DealContactDamage
        // This will trigger poison/root effects from VerdantEnemy
    }

    protected override void Die()
    {
        // Before dying, split into multiple brainless sprites
        SplitIntoMinions();
        
        // Then proceed with normal death
        base.Die();
    }

    private void SplitIntoMinions()
    {
        if (brainlessSpriteMinion == null)
        {
            Debug.LogWarning($"BrainlessSpriteMinion prefab is not assigned to {gameObject.name}!");
            return;
        }

        Debug.Log($"🧚‍♀️ {enemyData.enemyName} is splitting into {splitCount} minions!");

        for (int i = 0; i < splitCount; i++)
        {
            // Calculate spawn position around the dying sprite
            float angle = (360f / splitCount) * i * Mathf.Deg2Rad;
            Vector2 spawnOffset = new Vector2(
                Mathf.Cos(angle) * splitRadius,
                Mathf.Sin(angle) * splitRadius
            );
            Vector3 spawnPosition = transform.position + (Vector3)spawnOffset;
            
            // Spawn the minion
            GameObject minion = Instantiate(brainlessSpriteMinion, spawnPosition, Quaternion.identity);
            
            // Add force to spread them out
            Rigidbody2D minionRb = minion.GetComponent<Rigidbody2D>();
            if (minionRb != null)
            {
                Vector2 force = spawnOffset.normalized * splitForce;
                minionRb.AddForce(force, ForceMode2D.Impulse);
            }
            
            // Make minions slightly smaller
            minion.transform.localScale = Vector3.one * 0.8f;
            
            Debug.Log($"🧚 Spawned minion {i + 1} at {spawnPosition}");
        }
    }

    protected override void OnVerdantDamaged(float damage)
    {
        // When damaged, increase circle speed temporarily
        StartCoroutine(TemporaryFrenzy());
    }

    private IEnumerator TemporaryFrenzy()
    {
        float originalCircleSpeed = circleSpeed;
        float originalFlySpeed = flySpeed;
        
        circleSpeed *= 2f;
        flySpeed *= 1.3f;
        
        yield return new WaitForSeconds(3f);
        
        circleSpeed = originalCircleSpeed;
        flySpeed = originalFlySpeed;
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator != null)
        {
            animator.SetBool("IsFlying", true);
            animator.SetBool("IsCircling", isHoveringAroundPlayer);
            animator.SetFloat("FlightSpeed", rb != null ? rb.linearVelocity.magnitude : 0f);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw contact damage range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, contactDamageRange);
        
        // Draw hover distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hoverDistance);
        
        // Draw circle radius
        if (PlayerController.Instance != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(PlayerController.Instance.transform.position, circleRadius);
        }
        
        // Draw split positions
        Gizmos.color = Color.magenta;
        for (int i = 0; i < splitCount; i++)
        {
            float angle = (360f / splitCount) * i * Mathf.Deg2Rad;
            Vector2 splitPos = (Vector2)transform.position + new Vector2(
                Mathf.Cos(angle) * splitRadius,
                Mathf.Sin(angle) * splitRadius
            );
            Gizmos.DrawWireSphere(splitPos, 0.3f);
        }
    }
}


