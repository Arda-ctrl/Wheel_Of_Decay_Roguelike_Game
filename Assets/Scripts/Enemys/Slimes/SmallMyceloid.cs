using UnityEngine;

public class SmallMyceloid : BaseSlimeController
{
    [Header("Small Myceloid Settings")]
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeAttackCooldown = 1.5f;
    
    [Header("Bouncy Movement")]
    [SerializeField] private float bounceForce = 5f;
    [SerializeField] private float bounceInterval = 2f;
    [SerializeField] private bool useBounceMovement = true;
    
    private float lastBounceTime = 0f;
    private float lastMeleeAttackTime = 0f;

    protected override void Start()
    {
        slimeType = SlimeType.Small;
        
        // Set small slime stats
        if (enemyData != null)
        {
            enemyData.maxHealth = 30f;
            enemyData.baseSpeed = 4f;
            enemyData.baseDamage = meleeDamage;
            enemyData.attackRange = meleeRange;
            enemyData.attackCooldown = meleeAttackCooldown;
            enemyData.detectionRange = 3f;
        }
        
        // Smaller attack zone for small slimes
        attackZoneRadius = 2.5f;
        roamRadius = 5f;
        roamSpeed = 3f;
        
        base.Start();
    }

    protected override void HandleRoamingState()
    {
        if (useBounceMovement)
        {
            HandleBouncyMovement();
        }
        else
        {
            base.HandleRoamingState();
        }
    }

    private void HandleBouncyMovement()
    {
        // Bouncy movement pattern - jump towards roam target
        if (Time.time >= lastBounceTime + bounceInterval)
        {
            PerformBounce();
            lastBounceTime = Time.time;
        }
        
        // Check if reached target or time limit
        if (Vector2.Distance(transform.position, roamTarget) < 1f || stateTimer >= roamTime)
        {
            ChangeSlimeState(SlimeState.Idle);
        }
    }

    private void PerformBounce()
    {
        if (rb != null)
        {
            Vector2 direction = (roamTarget - (Vector2)transform.position).normalized;
            
            // Add upward component for bounce
            Vector2 bounceDirection = new Vector2(direction.x, Mathf.Abs(direction.y) + 0.5f).normalized;
            
            rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
            
            // Update sprite direction
            bool shouldFaceRight = direction.x > 0;
            if (shouldFaceRight != isFacingRight)
            {
                FlipSprite();
            }
            
            // Trigger bounce animation (if Bounce trigger exists)
            if (animator != null)
            {
                // Check if Bounce parameter exists before setting it
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "Bounce" && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        animator.SetTrigger("Bounce");
                        break;
                    }
                }
            }
        }
    }

    protected override bool CanAttack()
    {
        return !isAttacking && Time.time >= lastMeleeAttackTime + meleeAttackCooldown;
    }

    protected override void PerformSlimeAttack()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= meleeRange)
        {
            lastMeleeAttackTime = Time.time;
            
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Poke"); // Use Poke trigger instead of MeleeAttack
            }
            
            // Perform melee attack
            StartCoroutine(MeleeAttackCoroutine());
        }
    }

    private System.Collections.IEnumerator MeleeAttackCoroutine()
    {
        // Wait for animation wind-up
        yield return new WaitForSeconds(0.2f);
        
        // Check if player is still in range
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= meleeRange)
            {
                // Deal damage to player
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(meleeDamage);
                }
                else if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }
                
                // Play attack sound
                if (enemyData.attackSound != null)
                {
                    AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
                }
                
                // Small knockback effect on player
                Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDir * 2f, ForceMode2D.Impulse);
                }
                
                Debug.Log($"Small Myceloid dealt {meleeDamage} melee damage to player");
            }
        }
    }

    protected override void HandleAttackingState()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Face the player
        bool shouldFaceRight = playerTransform.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            FlipSprite();
        }
        
        // Move towards player if not in melee range
        if (distanceToPlayer > meleeRange)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * (roamSpeed * 1.5f); // Move faster when attacking
        }
        else
        {
            StopMoving();
            
            if (CanAttack())
            {
                PerformSlimeAttack();
            }
        }
    }

    protected override void OnSlimeDeath()
    {
        // Small slimes don't split further, just die
        Debug.Log("Small Myceloid died");
        
        // Create small death effect
        if (enemyData.deathEffect != null)
        {
            GameObject effect = Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
            
            // Scale down the effect for small slime
            effect.transform.localScale = Vector3.one * 0.7f;
        }
    }

    protected override void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Update movement animation based on velocity
        bool isMoving = rb != null && rb.linearVelocity.magnitude > 0.1f;
        bool isJogging = isMoving && currentSlimeState == SlimeState.Roaming;
        bool isIdle = !isMoving && currentSlimeState == SlimeState.Idle;
        bool isDead = currentSlimeState == SlimeState.Dead;
        bool isPoking = isAttacking && currentSlimeState == SlimeState.Attacking;
        
        // Set animation parameters to match your animator
        animator.SetBool("IsIdle", isIdle);
        animator.SetBool("IsJogging", isJogging);
        animator.SetBool("IsDead", isDead);
        animator.SetBool("IsPoking", isPoking);
        
        // Set bouncing state (if you want to keep this)
        animator.SetBool("IsBouncing", useBounceMovement && currentSlimeState == SlimeState.Roaming);
        
        // Set attack state
        animator.SetBool("IsInMeleeRange", playerTransform != null && 
                          Vector2.Distance(transform.position, playerTransform.position) <= meleeRange);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw melee attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
