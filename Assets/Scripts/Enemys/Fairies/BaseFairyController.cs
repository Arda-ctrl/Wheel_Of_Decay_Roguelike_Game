using UnityEngine;
using System.Collections;

public enum FairyType
{
    Brainless,
    SummonerUnarmed,
    SummonerMage
}

public enum FairyState
{
    Idle,
    Flying,
    Fleeing,
    Summoning,
    Dead
}

public abstract class BaseFairyController : BaseEnemy
{
    [Header("Fairy Settings")]
    [SerializeField] protected FairyType fairyType;
    [SerializeField] protected float flySpeed = 6f;
    [SerializeField] protected float contactDamage = 15f;
    [SerializeField] protected bool canBounceOffWalls = true;
    
    [Header("Flight Pattern")]
    [SerializeField] protected float directionChangeInterval = 2f;
    [SerializeField] protected float wallBounceForce = 8f;
    [SerializeField] protected LayerMask wallLayerMask = 1; // Default layer
    
    protected FairyState currentFairyState = FairyState.Idle;
    protected Vector2 currentFlyDirection;
    protected float directionTimer = 0f;
    protected bool hasContactedPlayerThisFrame = false;
    
    protected override void Start()
    {
        base.Start();
        
        // Fairies don't use gravity
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
        
        // Set initial random direction
        ChangeFlightDirection();
        ChangeFairyState(FairyState.Idle);
        
        Debug.Log($"{fairyType} Fairy spawned in Verdant Hallow");
    }

    protected override void UpdateAI()
    {
        if (playerTransform == null) return;
        
        UpdateFairyState();
        
        // Reset contact flag each frame
        hasContactedPlayerThisFrame = false;
    }

    protected virtual void UpdateFairyState()
    {
        directionTimer += Time.deltaTime;
        
        switch (currentFairyState)
        {
            case FairyState.Idle:
                HandleIdle();
                break;
                
            case FairyState.Flying:
                HandleFlying();
                break;
                
            case FairyState.Fleeing:
                HandleFleeing();
                break;
                
            case FairyState.Summoning:
                HandleSummoning();
                break;
                
            case FairyState.Dead:
                // Dead state handled by base class
                break;
        }
    }

    protected virtual void HandleFlying()
    {
        // Move in current direction
        if (rb != null)
        {
            rb.linearVelocity = currentFlyDirection * flySpeed;
        }
        
        // Change direction periodically
        if (directionTimer >= directionChangeInterval)
        {
            ChangeFlightDirection();
            directionTimer = 0f;
        }
        
        // Check for wall collisions
        CheckWallCollision();
        
        // Update sprite direction
        UpdateSpriteDirection();
    }

    protected virtual void HandleIdle()
    {
        // Default implementation - just stop moving
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected virtual void HandleFleeing()
    {
        // Default implementation - override in subclasses
        HandleFlying();
    }

    protected virtual void HandleSummoning()
    {
        // Default implementation - override in subclasses
        HandleFlying();
    }

    protected virtual void ChangeFairyState(FairyState newState)
    {
        currentFairyState = newState;
        
        switch (newState)
        {
            case FairyState.Flying:
                ChangeFlightDirection();
                break;
        }
    }

    protected virtual void ChangeFlightDirection()
    {
        // Generate random diagonal direction
        float angle = Random.Range(0f, 360f);
        currentFlyDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        
        Debug.Log($"Fairy changed direction to {currentFlyDirection}");
    }

    protected virtual void CheckWallCollision()
    {
        if (!canBounceOffWalls) return;
        
        // Cast rays in movement direction to detect walls
        float rayDistance = 1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, currentFlyDirection, rayDistance, wallLayerMask);
        
        if (hit.collider != null)
        {
            BounceOffWall(hit.normal);
        }
    }

    protected virtual void BounceOffWall(Vector2 wallNormal)
    {
        // Reflect the direction off the wall
        currentFlyDirection = Vector2.Reflect(currentFlyDirection, wallNormal);
        
        // Add some randomness to prevent getting stuck
        float randomAngle = Random.Range(-30f, 30f);
        currentFlyDirection = Quaternion.Euler(0, 0, randomAngle) * currentFlyDirection;
        currentFlyDirection.Normalize();
        
        // Apply bounce force
        if (rb != null)
        {
            rb.AddForce(currentFlyDirection * wallBounceForce, ForceMode2D.Impulse);
        }
        
        Debug.Log($"Fairy bounced off wall, new direction: {currentFlyDirection}");
    }

    protected virtual void UpdateSpriteDirection()
    {
        if (currentFlyDirection.x != 0)
        {
            bool shouldFaceRight = currentFlyDirection.x > 0;
            if (shouldFaceRight != isFacingRight)
            {
                FlipSprite();
            }
        }
    }

    protected override void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        UpdateSpriteDirection(isFacingRight);
    }

    // Contact damage system
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (hasContactedPlayerThisFrame) return;
        
        if (other.CompareTag("Player"))
        {
            DealContactDamage(other);
            hasContactedPlayerThisFrame = true;
        }
    }

    protected virtual void DealContactDamage(Collider2D player)
    {
        var playerHealth = player.GetComponent<IHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
        else if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }
        
        // Apply small knockback
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(knockbackDir * 3f, ForceMode2D.Impulse);
        }
        
        Debug.Log($"{fairyType} Fairy dealt {contactDamage} contact damage to player");
    }

    protected override void UpdateAnimations()
    {
        if (animator == null) return;

        // Update movement animation based on fairy type
        if (fairyType == FairyType.SummonerMage)
        {
            // Mage fairy walks on ground - has Idle, Summon, Jog, Death
            bool isMoving = rb != null && rb.linearVelocity.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsSummoning", currentFairyState == FairyState.Summoning);
            animator.SetBool("IsIdle", currentFairyState == FairyState.Idle);
            animator.SetBool("IsJogging", currentFairyState == FairyState.Fleeing);
        }
        else
        {
            // Other fairies only have Idle and Death animations
            // They move but stay in Idle animation
            // No additional animation parameters needed
        }
        
        // Set fairy type for animation controller
        animator.SetFloat("FairyType", (float)fairyType);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw flight direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, currentFlyDirection * 2f);
        
        // Draw contact damage range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw wall detection range
        if (canBounceOffWalls)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, currentFlyDirection * 1f);
        }
    }

    // Abstract methods for derived classes
    protected abstract void OnFairyDeath();
    
    protected override void PerformAttack()
    {
        // Fairies don't have traditional attacks, they use contact damage
        // Override in subclasses if needed
    }

    protected override void OnEnemyDeath()
    {
        ChangeFairyState(FairyState.Dead);
        
        // Disable collider to prevent further contact damage
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Trigger death animation immediately when health reaches 0
        if (animator != null)
        {
            Debug.Log($"Triggering Death animation for {fairyType} Fairy");
            animator.SetTrigger("Death");
            
            // Start checking for death animation completion
            StartCoroutine(CheckDeathAnimation());
        }
        else
        {
            Debug.LogError($"No animator found on {fairyType} Fairy!");
            // No animator, destroy immediately
            OnFairyDeath();
            Destroy(gameObject);
        }
    }
    
    private IEnumerator CheckDeathAnimation()
    {
        // Wait a frame for the animation to start
        yield return null;
        
        // Keep checking until death animation is no longer playing
        while (true)
        {
            if (animator == null) break;
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isInDeathState = stateInfo.IsName("Death");
            string currentStateName = animator.GetCurrentAnimatorStateInfo(0).IsName("Death") ? "Death" : "Other";
            
            Debug.Log($"Checking death animation - CurrentState: {currentStateName}, IsInDeath: {isInDeathState}, NormalizedTime: {stateInfo.normalizedTime:F2}");
            
            // If not in death state anymore, animation finished
            // OR if normalizedTime is greater than 1.0 (animation completed at least once)
            if (!isInDeathState || stateInfo.normalizedTime >= 1.0f)
            {
                Debug.Log($"Death animation finished - IsInDeath: {isInDeathState}, NormalizedTime: {stateInfo.normalizedTime:F2}, destroying fairy");
                break;
            }
            
            // Wait one frame before checking again
            yield return null;
        }
        
        // Call fairy-specific death behavior
        OnFairyDeath();
        
        // Destroy the fairy
        Destroy(gameObject);
    }
}
