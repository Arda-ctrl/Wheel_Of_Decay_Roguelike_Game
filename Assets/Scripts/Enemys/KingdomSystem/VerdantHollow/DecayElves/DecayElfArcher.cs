using UnityEngine;

public abstract class DecayElfArcher : VerdantEnemy
{
    [Header("Decay Elf Archer Specific")]
    [SerializeField] protected GameObject arrowPrefab;
    [SerializeField] protected Transform arrowSpawnPoint;
    [SerializeField] protected float arrowSpeed = 10f;
    [SerializeField] protected float arrowDamage = 12f;
    [SerializeField] protected float dashDistance = 3f;
    [SerializeField] protected float dashSpeed = 8f;

    protected bool isDashing = false;
    protected Vector2 dashDirection;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Decay Elf specific attack logic
        if (enemyData.isRanged && arrowPrefab != null)
        {
            ShootArrow();
        }
    }

    protected virtual void ShootArrow()
    {
        if (arrowSpawnPoint == null)
        {
            arrowSpawnPoint = transform;
        }

        // Calculate direction to player
        Vector2 direction = (playerTransform.position - arrowSpawnPoint.position).normalized;
        
        // Create arrow
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        
        // Set arrow properties
        var arrowComponent = arrow.GetComponent<EnemyProjectile>();
        if (arrowComponent != null)
        {
            arrowComponent.Initialize(direction, arrowSpeed, arrowDamage, gameObject);
        }
        else
        {
            // Fallback if no EnemyProjectile component
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * arrowSpeed;
            }
        }
        
        Debug.Log($"🏹 {enemyData.enemyName} shot an arrow!");
    }

    protected virtual void DashBackward()
    {
        if (isDashing) return;

        // Calculate backward direction
        dashDirection = (transform.position - playerTransform.position).normalized;
        
        // Start dash
        StartCoroutine(PerformDash());
    }

    protected virtual System.Collections.IEnumerator PerformDash()
    {
        isDashing = true;
        
        // Store original speed
        float originalSpeed = GetCurrentSpeed();
        
        // Set dash speed
        SetSpeedMultiplier(dashSpeed / enemyData.baseSpeed);
        
        // Move backward
        float dashTime = dashDistance / dashSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < dashTime)
        {
            if (rb != null)
            {
                rb.linearVelocity = dashDirection * dashSpeed;
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
        
        isDashing = false;
    }

    protected override void UpdateAI()
    {
        if (isDashing) return; // Don't update AI while dashing
        
        base.UpdateAI();
    }

    protected override void PerformVerdantSpecialAbility()
    {
        base.PerformVerdantSpecialAbility();
        
        // Decay Elf specific special ability
        PerformDecayElfSpecialAbility();
    }

    protected virtual void PerformDecayElfSpecialAbility()
    {
        // Override in specific decay elf types
        Debug.Log($"🏹 {enemyData.enemyName} used decay elf special ability!");
    }
} 