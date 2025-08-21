using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class BaseEnemy : MonoBehaviour, IHealth, IMoveable, IStatusEffect, IEnemy
{
    [Header("Enemy Data")]
    [SerializeField] protected KingdomEnemyData enemyData;
    
    [Header("Components")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    
    [Header("Debug")]
    [SerializeField] protected bool showDebugInfo = true;
    [SerializeField] protected bool showDetectionGizmo = true;

    // Protected fields
    protected float currentHealth;
    protected float speedMultiplier = 1f;
    protected Dictionary<StatusEffectType, float> activeStatusEffects = new Dictionary<StatusEffectType, float>();
    
    // Movement and AI
    protected Transform playerTransform;
    protected Vector2 moveDirection;
    protected bool isPlayerInRange = false;
    protected bool isFacingRight = true;
    protected bool isAttacking = false;
    protected float lastAttackTime = 0f;
    
    // Combat
    protected float currentDamage;
    protected float currentDefense;
    
    // Special abilities
    protected float lastSpecialAbilityTime = 0f;
    protected bool canUseSpecialAbility = true;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        InitializeComponents();
    }

    protected virtual void Start()
    {
        InitializeEnemy();
        OnEnemySpawned();
    }

    protected virtual void Update()
    {
        UpdateStatusEffects();
        UpdateAI();
        UpdateAnimations();
    }

    protected virtual void OnDestroy()
    {
        activeStatusEffects.Clear();
        OnEnemyDestroyed();
    }

    #endregion

    #region Initialization

    protected virtual void InitializeComponents()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    protected virtual void InitializeEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData is null for {gameObject.name}!");
            return;
        }

        // Initialize stats
        currentHealth = enemyData.maxHealth;
        currentDamage = enemyData.baseDamage * enemyData.damageMultiplier;
        currentDefense = enemyData.defenseMultiplier;

        // Get player reference
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }

        // Set initial direction
        isFacingRight = true;
        UpdateSpriteDirection(isFacingRight);
    }

    #endregion

    #region AI and Movement

    protected virtual void UpdateAI()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distanceToPlayer <= enemyData.detectionRange;

        if (isPlayerInRange)
        {
            HandlePlayerInRange(distanceToPlayer);
        }
        else
        {
            HandlePlayerOutOfRange();
        }
    }

    protected virtual void HandlePlayerInRange(float distanceToPlayer)
    {
        // Calculate movement direction
        moveDirection = (playerTransform.position - transform.position).normalized;

        // Update sprite direction
        bool shouldFaceRight = playerTransform.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            FlipSprite();
        }

        // Handle movement and combat
        if (distanceToPlayer > enemyData.stopDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
            
            // Try to attack if in range
            if (CanAttack() && distanceToPlayer <= enemyData.attackRange)
            {
                Attack();
            }
        }

        // Handle special abilities
        if (enemyData.hasSpecialAbility && canUseSpecialAbility && 
            distanceToPlayer <= enemyData.specialAbilityRange)
        {
            TryUseSpecialAbility();
        }
    }

    protected virtual void HandlePlayerOutOfRange()
    {
        StopMoving();
        isAttacking = false;
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (rb != null)
        {
            float currentSpeed = GetCurrentSpeed();
            rb.linearVelocity = moveDirection * currentSpeed;
        }
    }

    protected virtual void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected virtual void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        UpdateSpriteDirection(isFacingRight);
    }

    protected virtual void UpdateSpriteDirection(bool facingRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    #endregion

    #region Combat

    protected virtual bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + enemyData.attackCooldown;
    }

    protected virtual void Attack()
    {
        if (!CanAttack()) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Play attack sound
        if (enemyData.attackSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
        }

        // Perform attack logic
        PerformAttack();

        // Reset attack state after animation
        StartCoroutine(ResetAttackState());
    }

    protected virtual void PerformAttack()
    {
        // Override in derived classes for specific attack logic
        Debug.Log($"{enemyData.enemyName} is attacking!");
    }

    protected virtual IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.5f); // Adjust based on attack animation length
        isAttacking = false;
    }

    protected virtual void TryUseSpecialAbility()
    {
        if (Time.time >= lastSpecialAbilityTime + enemyData.specialAbilityCooldown)
        {
            UseSpecialAbility();
            lastSpecialAbilityTime = Time.time;
        }
    }

    protected virtual void UseSpecialAbility()
    {
        // Override in derived classes
        Debug.Log($"{enemyData.enemyName} used special ability!");
    }

    #endregion

    #region Status Effects

    protected virtual void UpdateStatusEffects()
    {
        List<StatusEffectType> expiredEffects = new List<StatusEffectType>();
        
        foreach (var effect in activeStatusEffects)
        {
            if (Time.time >= effect.Value)
            {
                expiredEffects.Add(effect.Key);
            }
        }

        foreach (var effect in expiredEffects)
        {
            RemoveStatus(effect);
        }
    }

    #endregion

    #region Animation

    protected virtual void UpdateAnimations()
    {
        if (animator == null) return;

        // Update movement animation
        bool isMoving = rb != null && rb.linearVelocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", isAttacking);
    }

    #endregion

    #region IHealth Implementation

    public virtual void TakeDamage(float amount)
    {
        float finalDamage = amount / currentDefense;
        currentHealth -= finalDamage;

        // Play hurt sound
        if (enemyData.hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.hurtSound, transform.position);
        }

        // Trigger hurt animation
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        OnEnemyDamaged(finalDamage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => enemyData.maxHealth;

    #endregion

    #region IMoveable Implementation

    public virtual void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
    }

    public virtual float GetCurrentSpeed() => enemyData.baseSpeed * speedMultiplier;
    public virtual float GetBaseSpeed() => enemyData.baseSpeed;

    #endregion

    #region IStatusEffect Implementation

    public virtual void ApplyStatus(StatusEffectType statusType, float duration)
    {
        activeStatusEffects[statusType] = Time.time + duration;
        OnStatusEffectApplied(statusType, duration);
    }

    public virtual void RemoveStatus(StatusEffectType statusType)
    {
        if (activeStatusEffects.ContainsKey(statusType))
        {
            activeStatusEffects.Remove(statusType);
            OnStatusEffectRemoved(statusType);
        }
    }

    public virtual bool HasStatus(StatusEffectType statusType)
    {
        return activeStatusEffects.ContainsKey(statusType);
    }

    #endregion

    #region Death and Rewards

    protected virtual void Die()
    {
        // Play death sound
        if (enemyData.deathSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.deathSound, transform.position);
        }

        // Spawn death effect
        if (enemyData.deathEffect != null)
        {
            Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
        }

        // Give rewards
        GiveRewards();

        // Trigger death event
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerEnemyDeath(gameObject);
        }

        OnEnemyDeath();

        // Destroy the enemy
        Destroy(gameObject);
    }

    protected virtual void GiveRewards()
    {
        // Override in derived classes for specific reward logic
        Debug.Log($"{enemyData.enemyName} died! Gave {enemyData.experienceReward} XP and {enemyData.goldReward} gold!");
    }

    #endregion

    #region Virtual Methods for Override

    protected virtual void OnEnemySpawned() { }
    protected virtual void OnEnemyDamaged(float damage) { }
    protected virtual void OnEnemyDeath() { }
    protected virtual void OnEnemyDestroyed() { }
    protected virtual void OnStatusEffectApplied(StatusEffectType statusType, float duration) { }
    protected virtual void OnStatusEffectRemoved(StatusEffectType statusType) { }

    #endregion

    #region Gizmos

    protected virtual void OnDrawGizmosSelected()
    {
        if (!showDetectionGizmo || enemyData == null) return;

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

        // Stop distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.stopDistance);

        // Attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

        // Special ability range
        if (enemyData.hasSpecialAbility)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, enemyData.specialAbilityRange);
        }
    }

    #endregion

    #region IEnemy Implementation
    
    public virtual float GetDetectionRange()
    {
        return enemyData != null ? enemyData.detectionRange : 5f;
    }
    
    public virtual void SetDetectionRange(float range)
    {
        if (enemyData != null)
        {
            enemyData.detectionRange = range;
        }
    }
    
    public virtual float GetAttackRange()
    {
        return enemyData != null ? enemyData.attackRange : 2f;
    }
    
    public virtual void SetAttackRange(float range)
    {
        if (enemyData != null)
        {
            enemyData.attackRange = range;
        }
    }
    
    #endregion
    
    #region Debug

    protected virtual void OnGUI()
    {
        if (!showDebugInfo || !Application.isEditor) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        float xPos = screenPos.x - 50f;
        float yPos = Screen.height - screenPos.y - 50f;

        GUI.Label(new Rect(xPos, yPos, 200, 20), 
                 $"HP: {currentHealth:F0}/{enemyData.maxHealth}");
        GUI.Label(new Rect(xPos, yPos + 20, 200, 20), 
                 $"Speed: {GetCurrentSpeed():F1}");
        GUI.Label(new Rect(xPos, yPos + 40, 200, 20), 
                 $"Kingdom: {enemyData.kingdomType}");
    }

    #endregion
} 