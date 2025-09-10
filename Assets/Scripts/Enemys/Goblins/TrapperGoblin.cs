using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapperGoblin : GoblinController
{
    [Header("Trapper Settings")]
    [SerializeField] private GameObject bearTrapPrefab;
    [SerializeField] private Transform trapPlacementArea; // Empty object for trap placement area
    [SerializeField] private float trapPlacementRange = 6f;
    [SerializeField] private float trapCooldown = 5f;
    [SerializeField] private int maxTraps = 3;
    [SerializeField] private float minTrapDistance = 2f;
    [SerializeField] private float trapPlacementTime = 1.5f;
    
    [Header("Behavior Settings")]
    [SerializeField] private float keepDistanceFromPlayer = 4f;
    [SerializeField] private float retreatDistance = 6f;
    [SerializeField] private LayerMask groundLayer = 1;
    
    [Header("Random Movement Settings")]
    [SerializeField] private float roamRadius = 8f;
    [SerializeField] private float roamSpeed = 3f;
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private float roamTime = 3f;
    [SerializeField] private float directionChangeInterval = 1f;
    [SerializeField] private bool useRandomDirectionChange = true;
    [SerializeField] private float fleeSpeedMultiplier = 1.2f;
    
    protected float lastTrapTime;
    protected List<GameObject> placedTraps = new List<GameObject>();
    protected bool isPlacingTrap = false;
    
    // Random movement variables (Myceloid style)
    protected Vector2 spawnPosition;
    protected Vector2 roamTarget;
    protected float stateTimer = 0f;
    protected float directionTimer = 0f;
    protected Vector2 currentRoamDirection;
    
    [Header("Animation (deprecated local refs)")]
    [SerializeField] private Animator animatorOverride; // use base.animator when available
    
    protected override void Start()
    {
        goblinType = GoblinType.TrapperGoblin;
        
        // Trapper goblins are more defensive
        goblinStats.health = 60f;
        goblinStats.speed = 4f; // Slower than dagger goblins
        goblinStats.attackDamage = 12f; // Lower direct damage
        goblinStats.attackRange = 2f; // Longer reach (bear trap)
        goblinStats.attackCooldown = 2f;
        goblinStats.canFlee = true;
        goblinStats.fleeHealthThreshold = 0.5f;
        goblinStats.minAlliesForFlee = 0; // Will flee even with allies
        
        base.Start();
        
        // Ana GameObject'teki Animator'ı bul
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator bulunamadı! Trapper_Goblin'e Animator component'i ekle!");
        }
        else
        {
            Debug.Log($"Animator bulundu: {animator.name}");
        }

        // Initialize random movement (Myceloid style)
        spawnPosition = transform.position;
        GenerateNewRoamTarget();
        
        // Disable detection range for trapper - it doesn't chase
        detectionRange = 0f;
        
        // Start with Idle state
        Debug.Log("TrapperGoblin Start completed, changing to Idle state");
        ChangeState(GoblinState.Idle);
    }
    
    protected override IEnumerator GoblinAI()
    {
        Debug.Log("GoblinAI coroutine started!");
        
        while (currentState != GoblinState.Dead)
        {
            Debug.Log($"GoblinAI loop - Current State: {currentState}, isPlacingTrap: {isPlacingTrap}");
            
            // If placing trap, don't move at all
            if (isPlacingTrap)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            else
            {
                // Update state timer
                stateTimer += Time.fixedDeltaTime;
                
                Debug.Log($"GoblinAI - Current State: {currentState}, Timer: {stateTimer:F2}");
                
                switch (currentState)
                {
                    case GoblinState.Idle:
                        Debug.Log("Handling Idle State");
                        HandleIdleState();
                        break;
                        
                    case GoblinState.Chasing:
                        Debug.Log("Handling Chasing State (Roaming)");
                        HandleRoamingState();
                        break;
                        
                    case GoblinState.PlacingTrap:
                        Debug.Log("Handling PlacingTrap State");
                        if (!isPlacingTrap)
                        {
                            StartCoroutine(PlaceTrapBehavior());
                        }
                        break;
                        
                    case GoblinState.Fleeing:
                        Debug.Log("Handling Fleeing State");
                        HandleFleeingBehavior();
                        break;
                }
            }
            
            // Clean up destroyed traps from list
            placedTraps.RemoveAll(trap => trap == null);
            
            // Animasyonları güncelle
            UpdateAnimation();
            
            yield return new WaitForFixedUpdate();
        }
        
        Debug.Log("GoblinAI coroutine ended!");
    }
    
    // Basit animasyon kontrolü - unified params
    private void UpdateAnimation()
    {
        if (animator != null)
        {
            // locomotion booleans (IsIdle/IsJogging) are updated in base; set additional flags here
            animator.SetBool(AnimIsSettingUp, isPlacingTrap);
            animator.SetBool(AnimIsDead, currentState == GoblinState.Dead);
        }
    }
    
    // Idle state - wait and occasionally place traps
    private void HandleIdleState()
    {
        StopMoving();
        
        Debug.Log($"Idle State - Timer: {stateTimer:F2}/{idleTime}, Current State: {currentState}");
        
        if (stateTimer >= idleTime)
        {
            Debug.Log("Idle time finished, switching to Chasing (Roaming)");
            ChangeState(GoblinState.Chasing); // Use Chasing state for roaming
        }
        
        // Occasionally place traps while idle
        if (ShouldPlaceTrap())
        {
            Debug.Log("Should place trap, switching to PlacingTrap");
            ChangeState(GoblinState.PlacingTrap);
        }
    }
    
    // Roaming state - move randomly around spawn area
    private void HandleRoamingState()
    {
        Debug.Log("HandleRoamingState called");
        
        if (PlayerController.Instance == null) 
        {
            Debug.Log("PlayerController.Instance is null, returning");
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        Debug.Log($"Roaming State - Timer: {stateTimer:F2}/{roamTime}, Target: {roamTarget}, Distance to target: {Vector2.Distance(transform.position, roamTarget):F2}");
        
        // If player is too close, flee
        if (distanceToPlayer < keepDistanceFromPlayer)
        {
            Debug.Log("Player too close, switching to Fleeing");
            ChangeState(GoblinState.Fleeing);
            return;
        }
        
        MoveTowardsRoamTarget();
        
        // Change direction randomly if enabled
        if (useRandomDirectionChange)
        {
            directionTimer += Time.deltaTime;
            if (directionTimer >= directionChangeInterval)
            {
                Debug.Log("Changing roam direction");
                ChangeRoamDirection();
                directionTimer = 0f;
            }
        }
        
        // Check if reached target or time limit
        if (Vector2.Distance(transform.position, roamTarget) < 1f || stateTimer >= roamTime)
        {
            Debug.Log("Roaming finished, switching to Idle");
            ChangeState(GoblinState.Idle);
        }
        
        // Occasionally place traps while roaming
        if (ShouldPlaceTrap())
        {
            Debug.Log("Should place trap while roaming, switching to PlacingTrap");
            ChangeState(GoblinState.PlacingTrap);
        }
    }
    
    // Move towards roam target
    private void MoveTowardsRoamTarget()
    {
        if (rb != null)
        {
            Vector2 direction = (roamTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * roamSpeed;
            
            Debug.Log($"Moving towards target: {roamTarget}, Direction: {direction}, Velocity: {rb.linearVelocity}, Speed: {roamSpeed}");
            
            // Update sprite direction
            bool shouldFaceRight = direction.x > 0;
            if (shouldFaceRight != _isFacingRight)
            {
                FlipSprite();
            }
        }
        else
        {
            Debug.LogError("Rigidbody2D is null!");
        }
    }
    
    // Local facing direction tracking
    private bool _isFacingRight = true;
    
    // Flip sprite method
    private void FlipSprite()
    {
        _isFacingRight = !_isFacingRight;
        
        // Flip the sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !_isFacingRight;
        }
    }
    
    // Generate new roam target
    private void GenerateNewRoamTarget()
    {
        Vector2 basePosition = trapPlacementArea != null ? trapPlacementArea.position : spawnPosition;
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(2f, roamRadius);
        roamTarget = basePosition + randomDirection * randomDistance;
        
        // Ensure target is within roam radius
        if (Vector2.Distance(basePosition, roamTarget) > roamRadius)
        {
            roamTarget = basePosition + (roamTarget - basePosition).normalized * roamRadius;
        }
    }
    
    // Change roam direction
    private void ChangeRoamDirection()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        currentRoamDirection = randomDirection;
        
        // Update roam target to continue in new direction
        roamTarget = (Vector2)transform.position + currentRoamDirection * Random.Range(2f, 5f);
        
        // Keep within roam radius
        Vector2 basePosition = trapPlacementArea != null ? trapPlacementArea.position : spawnPosition;
        if (Vector2.Distance(basePosition, roamTarget) > roamRadius)
        {
            Vector2 directionFromBase = roamTarget - basePosition;
            roamTarget = basePosition + directionFromBase.normalized * roamRadius;
        }
    }
    
    // Stop moving
    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // Override ChangeState to reset stateTimer
    protected new void ChangeState(GoblinState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"Changing state from {currentState} to {newState}");
        
        // Reset state timer when changing states
        stateTimer = 0f;
        
        // Call base ChangeState
        base.ChangeState(newState);
    }
    
    // Fleeing behavior - runs away from player
    private void HandleFleeingBehavior()
    {
        if (PlayerController.Instance == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        // If player is far enough, go back to random movement
        if (distanceToPlayer > retreatDistance)
        {
            ChangeState(GoblinState.Idle);
            return;
        }
        
        // Flee from player
        Vector2 fleeDirection = (transform.position - PlayerController.Instance.transform.position).normalized;
        rb.linearVelocity = fleeDirection * GetCurrentSpeed() * fleeSpeedMultiplier;
        
        // Occasionally place traps while fleeing
        if (ShouldPlaceTrap())
        {
            StartCoroutine(PlaceTrapWhileFleeing());
        }
    }
    
    
    
    protected virtual bool ShouldPlaceTrap()
    {
        if (Time.time < lastTrapTime + trapCooldown) return false;
        if (placedTraps.Count >= maxTraps) return false;
        if (PlayerController.Instance == null) return false;
        
        // Always try to place trap when cooldown is ready (no distance check)
        Vector2 trapPosition = FindTrapPlacementPosition();
        return trapPosition != Vector2.zero;
    }
    
    protected virtual Vector2 FindTrapPlacementPosition()
    {
        // Always place trap at the assigned transform position
        if (trapPlacementArea != null)
        {
            return trapPlacementArea.position;
        }
        else
        {
            // Fallback: place at goblin position
            return transform.position;
        }
    }
    
    protected virtual bool IsValidTrapPosition(Vector2 position)
    {
        // Check if position is too close to existing traps
        foreach (GameObject trap in placedTraps)
        {
            if (trap != null && Vector2.Distance(position, trap.transform.position) < minTrapDistance)
            {
                return false;
            }
        }
        
        return true;
    }

    // Trapper için saldırı kullanılmıyor: Attacking durumuna geçişi engellemek için State güncellemesini özelleştir
    protected new void UpdateState()
    {
        // Disable UpdateState for TrapperGoblin - we handle state transitions in GoblinAI
        // This prevents the base class from interfering with our custom state logic
        return;
    }
    
    protected virtual IEnumerator PlaceTrapBehavior()
    {
        isPlacingTrap = true;
        
        // Stop all movement immediately and keep it stopped
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // Trap Placement animasyonunu başlat
        if (animator != null) animator.SetBool(AnimIsSettingUp, true);
        
        // Find placement position
        Vector2 trapPosition = FindTrapPlacementPosition();
        
        if (trapPosition == Vector2.zero)
        {
            // No valid position found, return to previous state
            isPlacingTrap = false;
            if (animator != null) animator.SetBool(AnimIsSettingUp, false);
            ChangeState(GoblinState.Idle);
            yield break;
        }
        
        // Store trap position for animation event
        _pendingTrapPosition = trapPosition;
        
        // Wait for animation event to place trap
        yield return new WaitForSeconds(trapPlacementTime);
        
        // Fallback: if animation event didn't trigger, place trap anyway
        if (_pendingTrapPosition != Vector2.zero)
        {
            PlaceTrapAtPosition(_pendingTrapPosition);
        }
        
        lastTrapTime = Time.time;
        isPlacingTrap = false;
        
        // Reset animation state to prevent freezing
        if (animator != null) animator.SetBool(AnimIsSettingUp, false);
        
        // Reset state timer and return to idle (random movement)
        stateTimer = 0f;
        ChangeState(GoblinState.Idle);
    }
    
    // Store trap position for animation event
    private Vector2 _pendingTrapPosition = Vector2.zero;
    
    // Animation Event method - called from SettingUp animation
    public void OnTrapPlacementEvent()
    {
        if (_pendingTrapPosition != Vector2.zero)
        {
            PlaceTrapAtPosition(_pendingTrapPosition);
            _pendingTrapPosition = Vector2.zero;
        }
    }
    
    // Place trap at specified position
    private void PlaceTrapAtPosition(Vector2 position)
    {
        if (bearTrapPrefab != null)
        {
            GameObject newTrap = Instantiate(bearTrapPrefab, position, Quaternion.identity);
            placedTraps.Add(newTrap);
            
            // Configure the trap
            var trapComponent = newTrap.GetComponent<GoblinTrap>();
            if (trapComponent != null)
            {
                trapComponent.SetTrapType(TrapType.BearTrap);
                trapComponent.SetTrapDamage(20f);
            }
            
            Debug.Log($"{gameObject.name} placed a bear trap at {position}");
        }
    }
    
    protected virtual IEnumerator PlaceTrapWhileFleeing()
    {
        if (isPlacingTrap) yield break;
        
        Vector2 trapPosition = transform.position; // Place trap at current position while fleeing
        
        if (bearTrapPrefab != null && placedTraps.Count < maxTraps)
        {
            PlaceTrapAtPosition(trapPosition);
            lastTrapTime = Time.time;
            Debug.Log($"{gameObject.name} placed a bear trap while fleeing!");
        }
    }
    
    // Trapper'da saldırı kullanılmaz: olası Attacking state'ine düşerse hızlıca çık
    protected override IEnumerator AttackBehavior()
    {
        // Güvenlik: saldırı davranışı devre dışı, hemen Idle'a dön
        rb.linearVelocity = Vector2.zero;
        yield return null;
        if (currentState == GoblinState.Attacking)
        {
            ChangeState(GoblinState.Idle);
        }
    }
    
    // Trapper melee saldırı kaldırıldı
    
    protected virtual IEnumerator ApplyBriefSlow(IMoveable target)
    {
        target.SetSpeedMultiplier(0.7f);
        yield return new WaitForSeconds(2f);
        target.SetSpeedMultiplier(1f);
    }
    
    protected override void OnGoblinDamaged()
    {
        base.OnGoblinDamaged();
        
        // When damaged, flee immediately
        if (currentState != GoblinState.Fleeing)
        {
            ChangeState(GoblinState.Fleeing);
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw trap placement range
        Vector2 basePosition = trapPlacementArea != null ? trapPlacementArea.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(basePosition, trapPlacementRange);
        
        // Draw keep distance range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, keepDistanceFromPlayer);
        
        // Draw retreat distance range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}
