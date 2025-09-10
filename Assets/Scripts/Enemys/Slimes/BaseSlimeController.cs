using UnityEngine;
using System.Collections;

public enum SlimeState
{
    Idle,
    Roaming,
    Attacking,
    Dead
}

public enum SlimeType
{
    Small,
    Medium,
    Big,
    VenomNormal,
    VenomForm,
    Elite
}

public abstract class BaseSlimeController : BaseEnemy
{
    [Header("Slime Settings")]
    [SerializeField] protected SlimeType slimeType;
    [SerializeField] protected float roamRadius = 8f;
    [SerializeField] protected float roamSpeed = 2f;
    [SerializeField] protected float idleTime = 3f;
    [SerializeField] protected float roamTime = 5f;
    [SerializeField] protected float attackZoneRadius = 4f;
    
    [Header("Random Movement")]
    [SerializeField] protected float directionChangeInterval = 2f;
    [SerializeField] protected bool useRandomDirectionChange = true;
    
    protected SlimeState currentSlimeState = SlimeState.Idle;
    protected Vector2 spawnPosition;
    protected Vector2 roamTarget;
    protected float stateTimer = 0f;
    protected float directionTimer = 0f;
    protected Vector2 currentRoamDirection;
    
    protected override void Start()
    {
        base.Start();
        spawnPosition = transform.position;
        ChangeSlimeState(SlimeState.Idle);
        GenerateNewRoamTarget();
    }

    protected override void UpdateAI()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool playerInAttackZone = distanceToPlayer <= attackZoneRadius;
        
        // Handle state transitions based on player proximity
        if (playerInAttackZone && currentSlimeState != SlimeState.Attacking && currentSlimeState != SlimeState.Dead)
        {
            ChangeSlimeState(SlimeState.Attacking);
        }
        else if (!playerInAttackZone && currentSlimeState == SlimeState.Attacking)
        {
            ChangeSlimeState(SlimeState.Idle);
        }
        
        // Update current state
        UpdateSlimeState();
    }
    
    // Override BaseEnemy's UpdateAI to prevent conflicts
    protected override void Update()
    {
        if (isDead) return;
        UpdateStatusEffects();
        UpdateAI(); // Use our custom AI instead of BaseEnemy's
        UpdateAnimations();
    }

    protected virtual void UpdateSlimeState()
    {
        stateTimer += Time.deltaTime;
        
        switch (currentSlimeState)
        {
            case SlimeState.Idle:
                HandleIdleState();
                break;
                
            case SlimeState.Roaming:
                HandleRoamingState();
                break;
                
            case SlimeState.Attacking:
                HandleAttackingState();
                break;
                
            case SlimeState.Dead:
                // Dead state handled by base class
                break;
        }
    }

    protected virtual void HandleIdleState()
    {
        StopMoving();
        
        if (stateTimer >= idleTime)
        {
            ChangeSlimeState(SlimeState.Roaming);
        }
    }

    protected virtual void HandleRoamingState()
    {
        MoveTowardsRoamTarget();
        
        // Change direction randomly if enabled
        if (useRandomDirectionChange)
        {
            directionTimer += Time.deltaTime;
            if (directionTimer >= directionChangeInterval)
            {
                ChangeRoamDirection();
                directionTimer = 0f;
            }
        }
        
        // Check if reached target or time limit
        if (Vector2.Distance(transform.position, roamTarget) < 1f || stateTimer >= roamTime)
        {
            ChangeSlimeState(SlimeState.Idle);
        }
    }

    protected virtual void HandleAttackingState()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Face the player
        bool shouldFaceRight = playerTransform.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            FlipSprite();
        }
        
        // Stop moving and attack
        StopMoving();
        
        if (CanAttack() && distanceToPlayer <= enemyData.attackRange)
        {
            Attack();
        }
    }

    protected virtual void ChangeSlimeState(SlimeState newState)
    {
        currentSlimeState = newState;
        stateTimer = 0f;
        
        switch (newState)
        {
            case SlimeState.Roaming:
                GenerateNewRoamTarget();
                break;
        }
    }

    protected virtual void GenerateNewRoamTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(2f, roamRadius);
        roamTarget = spawnPosition + randomDirection * randomDistance;
        
        // Ensure target is within roam radius
        if (Vector2.Distance(spawnPosition, roamTarget) > roamRadius)
        {
            roamTarget = spawnPosition + (roamTarget - spawnPosition).normalized * roamRadius;
        }
    }

    protected virtual void ChangeRoamDirection()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        currentRoamDirection = randomDirection;
        
        // Update roam target to continue in new direction
        roamTarget = (Vector2)transform.position + currentRoamDirection * Random.Range(2f, 5f);
        
        // Keep within roam radius
        if (Vector2.Distance(spawnPosition, roamTarget) > roamRadius)
        {
            Vector2 directionFromSpawn = roamTarget - spawnPosition;
            roamTarget = spawnPosition + directionFromSpawn.normalized * roamRadius;
        }
    }

    protected virtual void MoveTowardsRoamTarget()
    {
        if (rb != null)
        {
            Vector2 direction = (roamTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * roamSpeed;
            
            // Update sprite direction
            bool shouldFaceRight = direction.x > 0;
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

    protected override void UpdateAnimations()
    {
        if (animator == null) return;

        // Update movement animation
        bool isMoving = currentSlimeState == SlimeState.Roaming && rb != null && rb.linearVelocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsIdle", currentSlimeState == SlimeState.Idle);
        
        // Set slime type for animation controller
        animator.SetFloat("SlimeType", (float)slimeType);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw roam radius
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        Gizmos.DrawWireSphere(center, roamRadius);
        
        // Draw attack zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackZoneRadius);
        
        // Draw roam target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(roamTarget, 0.5f);
            Gizmos.DrawLine(transform.position, roamTarget);
        }
    }

    // Abstract methods for derived classes
    protected abstract void PerformSlimeAttack();
    protected abstract void OnSlimeDeath();
    
    protected override void PerformAttack()
    {
        PerformSlimeAttack();
    }

    protected override void OnEnemyDeath()
    {
        ChangeSlimeState(SlimeState.Dead);
        OnSlimeDeath();
    }
}
