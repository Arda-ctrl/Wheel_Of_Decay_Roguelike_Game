using UnityEngine;
using System.Collections;

public class SummonerFairyMage : BaseFairyController
{
    [Header("Summoner Fairy (Mage) Settings")]
    [SerializeField] private GameObject brainlessFairyPrefab;
    [SerializeField] private GameObject summonEffectPrefab;
    
    [Header("Mage Stats")]
    [SerializeField] private float mageSize = 1.2f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float minimumFleeDistance = 12f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float wallDetectionDistance = 2.5f;
    [SerializeField] private LayerMask groundLayerMask = -1; // All layers for ground/walls
    [SerializeField] private LayerMask triggerLayerMask = -1; // All layers for trigger colliders (walls)
    
    [Header("Summoning Settings")]
    [SerializeField] private float summonInterval = 5f;
    [SerializeField] private float summonCastTime = 1.5f;
    [SerializeField] private int maxSummonedFairies = 6;
    [SerializeField] private float summonRange = 8f;
    [SerializeField] private bool canSummonWhileFleeing = true;
    
    private float lastSummonTime = 0f;
    private bool isCastingSummon = false;
    private int currentSummonedCount = 0;
    
    protected override void Start()
    {
        fairyType = FairyType.SummonerMage;
        
        // Scale up the mage fairy
        transform.localScale = Vector3.one * mageSize;
        
        // Set mage fairy stats
        if (enemyData != null)
        {
            enemyData.maxHealth = 50f;
            enemyData.baseSpeed = walkSpeed;
            enemyData.baseDamage = contactDamage;
            enemyData.detectionRange = detectionRange;
        }
        
        flySpeed = walkSpeed; // Start with walk speed
        contactDamage = 12f; // Lower than unarmed summoner since it focuses on summoning
        canBounceOffWalls = true; // Mage needs wall detection for ground movement
        
        base.Start();
        
        // Start in fleeing state if player is nearby
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRange)
            {
                ChangeFairyState(FairyState.Fleeing);
            }
        }
        
        Debug.Log("Summoner Forest Fairy (Mage) spawned - will walk on ground, flee from player and summon minions");
    }


    protected override void UpdateFairyState()
    {
        base.UpdateFairyState();
        
        // Check if we should start or stop fleeing based on player distance
        if (playerTransform != null && currentFairyState != FairyState.Dead)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= detectionRange && currentFairyState != FairyState.Fleeing)
            {
                ChangeFairyState(FairyState.Fleeing);
            }
            else if (distanceToPlayer >= minimumFleeDistance && currentFairyState == FairyState.Fleeing)
            {
                ChangeFairyState(FairyState.Idle);
            }
        }
        
        // Handle summoning - can summon from any state
        if (!isCastingSummon && Time.time >= lastSummonTime + summonInterval && currentSummonedCount < maxSummonedFairies)
        {
            StartCoroutine(PerformSummon());
        }
    }

    protected override void HandleFleeing()
    {
        if (playerTransform == null) return;
        
        // Calculate direction away from player
        Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
        
        // Add some variation to avoid predictable movement
        float randomAngle = Random.Range(-20f, 20f);
        fleeDirection = Quaternion.Euler(0, 0, randomAngle) * fleeDirection;
        
        // Smooth movement with acceleration
        if (rb != null)
        {
            Vector2 targetVelocity = fleeDirection * fleeSpeed;
            Vector2 currentVelocity = rb.linearVelocity;
            
            // Smooth acceleration towards target velocity
            Vector2 velocityChange = targetVelocity - currentVelocity;
            Vector2 accelerationForce = velocityChange.normalized * acceleration * Time.deltaTime;
            
            // Limit acceleration to prevent overshooting
            if (accelerationForce.magnitude > velocityChange.magnitude)
            {
                accelerationForce = velocityChange;
            }
            
            rb.linearVelocity += accelerationForce;
            
            // Limit maximum speed
            if (rb.linearVelocity.magnitude > fleeSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * fleeSpeed;
            }
        }
        
        // Update sprite direction
        UpdateSpriteDirection();
        
        // Check for wall collision and change direction smoothly
        CheckWallCollisionForMage();
    }

    private IEnumerator PerformSummon()
    {
        isCastingSummon = true;
        lastSummonTime = Time.time;
        
        Debug.Log("Mage Fairy starting summon cast");
        
        // Change to summoning state
        FairyState previousState = currentFairyState;
        ChangeFairyState(FairyState.Summoning);
        
        // Slow down during cast (but don't stop completely if fleeing)
        if (previousState == FairyState.Fleeing)
        {
            // Reduce speed while casting but keep moving
            if (rb != null)
            {
                rb.linearVelocity *= 0.4f;
            }
        }
        else
        {
            // Stop moving if not fleeing
            if (rb != null)
            {
                rb.linearVelocity *= 0.1f;
            }
        }
        
        // Play casting animation
        if (animator != null)
        {
            animator.SetTrigger("StartSummon");
        }
        
        // Create casting effect
        if (summonEffectPrefab != null)
        {
            GameObject castingEffect = Instantiate(summonEffectPrefab, transform.position, Quaternion.identity);
            castingEffect.transform.SetParent(transform);
            Destroy(castingEffect, summonCastTime);
        }
        
        // Wait for cast time
        yield return new WaitForSeconds(summonCastTime);
        
        // Spawn the fairy
        SpawnBrainlessFairy();
        
        // Return to previous state (Idle or Fleeing)
        ChangeFairyState(previousState);
        
        isCastingSummon = false;
        
        Debug.Log("Mage Fairy completed summon");
    }

    private void SpawnBrainlessFairy()
    {
        if (brainlessFairyPrefab == null)
        {
            Debug.LogWarning("Mage Fairy: No brainless fairy prefab assigned!");
            return;
        }
        
        // Calculate spawn position near the mage but not too close
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = transform.position + (Vector3)randomDirection * Random.Range(2f, summonRange);
        
        // Create brainless fairy
        GameObject summonedFairy = Instantiate(brainlessFairyPrefab, spawnPos, Quaternion.identity);
        
        // Give it some initial velocity
        Rigidbody2D fairyRb = summonedFairy.GetComponent<Rigidbody2D>();
        if (fairyRb != null)
        {
            fairyRb.AddForce(randomDirection * 3f, ForceMode2D.Impulse);
        }
        
        // Track summoned fairy count
        currentSummonedCount++;
        
        // Create spawn effect
        if (summonEffectPrefab != null)
        {
            GameObject spawnEffect = Instantiate(summonEffectPrefab, spawnPos, Quaternion.identity);
            spawnEffect.transform.localScale = Vector3.one * 0.8f;
            Destroy(spawnEffect, 2f);
        }
        
        Debug.Log($"Mage Fairy summoned brainless fairy at {spawnPos}. Total summoned: {currentSummonedCount}");
    }

    private void CheckWallCollisionForMage()
    {
        if (rb == null) return;
        
        Vector2 currentVelocity = rb.linearVelocity;
        if (currentVelocity.magnitude < 0.1f) return; // Not moving
        
        Vector2 moveDirection = currentVelocity.normalized;
        
        // Method 1: Raycast for solid walls
        Vector2[] rayDirections = {
            moveDirection,
            (Vector2)(Quaternion.Euler(0, 0, 15f) * moveDirection),
            (Vector2)(Quaternion.Euler(0, 0, -15f) * moveDirection),
            (Vector2)(Quaternion.Euler(0, 0, 30f) * moveDirection),
            (Vector2)(Quaternion.Euler(0, 0, -30f) * moveDirection)
        };
        
        foreach (Vector2 rayDir in rayDirections)
        {
            // Check solid walls
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, wallDetectionDistance, groundLayerMask);
            
            if (hit.collider != null)
            {
                Debug.Log($"Mage Fairy detected solid wall at {hit.point}, normal: {hit.normal}");
                HandleWallCollisionForMage(hit.normal);
                return; // Only handle one wall collision per frame
            }
        }
        
        // Method 2: Check for trigger colliders in front (for walls like in the image)
        Vector2 currentPos = (Vector2)transform.position;
        Vector2 futurePosition = currentPos + moveDirection * wallDetectionDistance;
        
        // Use OverlapCircle for better detection
        Collider2D triggerCollider = Physics2D.OverlapCircle(futurePosition, 0.5f, triggerLayerMask);
        
        if (triggerCollider != null)
        {
            Debug.Log($"Mage Fairy detected trigger wall at {futurePosition} with collider: {triggerCollider.name}");
            // Calculate wall normal based on movement direction
            Vector2 wallNormal = -moveDirection; // Opposite to movement direction
            HandleWallCollisionForMage(wallNormal);
            return;
        }
        
        // Method 3: Check multiple points in front for trigger colliders
        Vector2[] checkPoints = {
            futurePosition,
            currentPos + (Vector2)(Quaternion.Euler(0, 0, 15f) * moveDirection) * wallDetectionDistance,
            currentPos + (Vector2)(Quaternion.Euler(0, 0, -15f) * moveDirection) * wallDetectionDistance,
            currentPos + (Vector2)(Quaternion.Euler(0, 0, 30f) * moveDirection) * wallDetectionDistance,
            currentPos + (Vector2)(Quaternion.Euler(0, 0, -30f) * moveDirection) * wallDetectionDistance
        };
        
        foreach (Vector2 checkPoint in checkPoints)
        {
            Collider2D triggerColl = Physics2D.OverlapCircle(checkPoint, 0.5f, triggerLayerMask);
            if (triggerColl != null)
            {
                Debug.Log($"Mage Fairy detected trigger wall at {checkPoint} with collider: {triggerColl.name}");
                Vector2 wallNormal = -moveDirection;
                HandleWallCollisionForMage(wallNormal);
                return;
            }
        }
    }
    
    private void HandleWallCollisionForMage(Vector2 wallNormal)
    {
        // Find a new direction that avoids the wall
        if (currentFairyState == FairyState.Fleeing && playerTransform != null)
        {
            // Find a new direction away from player that avoids the wall
            Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
            
            // Try to find a direction that's not blocked by the wall
            Vector2[] testDirections = {
                fleeDirection,
                Quaternion.Euler(0, 0, 45f) * fleeDirection,
                Quaternion.Euler(0, 0, -45f) * fleeDirection,
                Quaternion.Euler(0, 0, 90f) * fleeDirection,
                Quaternion.Euler(0, 0, -90f) * fleeDirection,
                Quaternion.Euler(0, 0, 135f) * fleeDirection,
                Quaternion.Euler(0, 0, -135f) * fleeDirection
            };
            
            Vector2 bestDirection = fleeDirection;
            foreach (Vector2 testDir in testDirections)
            {
                // Check if this direction is not blocked by the wall
                if (Vector2.Dot(testDir, -wallNormal) > 0.3f) // Not going into the wall
                {
                    bestDirection = testDir;
                    break;
                }
            }
            
            currentFlyDirection = bestDirection.normalized;
            
            // Apply the new direction immediately
            if (rb != null)
            {
                rb.linearVelocity = currentFlyDirection * rb.linearVelocity.magnitude;
            }
        }
        else
        {
            // If not fleeing, pick a random direction away from the wall
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            int attempts = 0;
            while (Vector2.Dot(randomDirection, -wallNormal) < 0.3f && attempts < 10)
            {
                randomDirection = Random.insideUnitCircle.normalized;
                attempts++;
            }
            currentFlyDirection = randomDirection;
            
            // Apply the new direction immediately
            if (rb != null)
            {
                rb.linearVelocity = currentFlyDirection * rb.linearVelocity.magnitude;
            }
        }
        
        Debug.Log($"Mage Fairy changed direction to avoid wall: {currentFlyDirection}");
    }

    protected override void BounceOffWall(Vector2 wallNormal)
    {
        // Use the new wall collision handler
        HandleWallCollisionForMage(wallNormal);
    }

    protected override void DealContactDamage(Collider2D player)
    {
        base.DealContactDamage(player);
        
        // Mage fairy is startled by contact and tries to flee immediately
        if (currentFairyState != FairyState.Fleeing)
        {
            ChangeFairyState(FairyState.Fleeing);
        }
        
        // Apply less knockback since mage is more fragile
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(knockbackDir * 2f, ForceMode2D.Impulse);
        }
    }

    protected override void OnFairyDeath()
    {
        Debug.Log("Summoner Forest Fairy (Mage) died");
        
        // Create death effect
        if (enemyData.deathEffect != null)
        {
            GameObject effect = Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 1.1f;
        }
        
        // Play death sound
        if (enemyData.deathSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.deathSound, transform.position);
        }
        
        // Mage fairy doesn't spawn fairies on death, but maybe creates a magical explosion effect
        if (summonEffectPrefab != null)
        {
            GameObject deathMagic = Instantiate(summonEffectPrefab, transform.position, Quaternion.identity);
            deathMagic.transform.localScale = Vector3.one * 1.5f;
            Destroy(deathMagic, 3f);
        }
    }

    protected override void HandleIdle()
    {
        // Smooth deceleration to stop
        if (rb != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            
            // Apply deceleration
            Vector2 decelerationForce = -currentVelocity.normalized * deceleration * Time.deltaTime;
            
            // Don't overshoot zero velocity
            if (decelerationForce.magnitude > currentVelocity.magnitude)
            {
                decelerationForce = -currentVelocity;
            }
            
            rb.linearVelocity += decelerationForce;
        }
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // Mage fairy has 4 animations: Idle, Summon, Jog (walk), Death
        // IsMoving is set in base class for walking
        // IsSummoning is set in base class for summoning
        // Death is handled by base enemy class
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw minimum flee distance
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position, minimumFleeDistance);
        
        // Draw summon range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRange);
        
        // Draw flee direction if fleeing
        if (currentFairyState == FairyState.Fleeing && playerTransform != null)
        {
            Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, fleeDirection * 3f);
        }
        
        // Draw wall detection rays
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            Vector2 moveDirection = rb.linearVelocity.normalized;
            
            // Draw raycast rays
            Vector2[] rayDirections = {
                moveDirection,
                (Vector2)(Quaternion.Euler(0, 0, 15f) * moveDirection),
                (Vector2)(Quaternion.Euler(0, 0, -15f) * moveDirection),
                (Vector2)(Quaternion.Euler(0, 0, 30f) * moveDirection),
                (Vector2)(Quaternion.Euler(0, 0, -30f) * moveDirection)
            };
            
            Gizmos.color = Color.cyan;
            foreach (Vector2 rayDir in rayDirections)
            {
                Gizmos.DrawRay(transform.position, rayDir * wallDetectionDistance);
            }
            
            // Draw trigger collider check points
            Vector2 currentPos = (Vector2)transform.position;
            Vector2[] checkPoints = {
                currentPos + moveDirection * wallDetectionDistance,
                currentPos + (Vector2)(Quaternion.Euler(0, 0, 15f) * moveDirection) * wallDetectionDistance,
                currentPos + (Vector2)(Quaternion.Euler(0, 0, -15f) * moveDirection) * wallDetectionDistance,
                currentPos + (Vector2)(Quaternion.Euler(0, 0, 30f) * moveDirection) * wallDetectionDistance,
                currentPos + (Vector2)(Quaternion.Euler(0, 0, -30f) * moveDirection) * wallDetectionDistance
            };
            
            Gizmos.color = Color.yellow;
            foreach (Vector2 checkPoint in checkPoints)
            {
                Gizmos.DrawWireSphere(checkPoint, 0.5f);
            }
        }
    }
}
