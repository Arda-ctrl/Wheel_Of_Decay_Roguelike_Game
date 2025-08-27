using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapperGoblin : GoblinController
{
    [Header("Trapper Settings")]
    [SerializeField] private GameObject bearTrapPrefab;
    [SerializeField] private float trapPlacementRange = 6f;
    [SerializeField] private float trapCooldown = 8f;
    [SerializeField] private int maxTraps = 3;
    [SerializeField] private float minTrapDistance = 2f;
    [SerializeField] private float trapPlacementTime = 1.5f;
    
    [Header("Behavior Settings")]
    [SerializeField] private float keepDistanceFromPlayer = 4f;
    [SerializeField] private float retreatDistance = 6f;
    [SerializeField] private LayerMask groundLayer = 1;
    
    protected float lastTrapTime;
    protected List<GameObject> placedTraps = new List<GameObject>();
    protected bool isPlacingTrap = false;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
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
        
        // SpriteRenderer kaldırıldı - test için
    }
    
    protected override IEnumerator GoblinAI()
    {
        while (currentState != GoblinState.Dead)
        {
            switch (currentState)
            {
                case GoblinState.Idle:
                    // Pozisyon kaymasını önle - velocity'yi sıfırla
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    
                    // Look for opportunities to place traps
                    if (ShouldPlaceTrap())
                    {
                        ChangeState(GoblinState.PlacingTrap);
                    }
                    break;
                    
                case GoblinState.Chasing:
                    HandleChasing();
                    break;
                    
                case GoblinState.PlacingTrap:
                    if (!isPlacingTrap)
                    {
                        StartCoroutine(PlaceTrapBehavior());
                    }
                    break;
                    
                case GoblinState.Fleeing:
                    // Fleeing behavior handled in base class
                    // But also try to place traps while fleeing
                    if (ShouldPlaceTrap())
                    {
                        StartCoroutine(PlaceTrapWhileFleeing());
                    }
                    break;
            }
            
            // Clean up destroyed traps from list
            placedTraps.RemoveAll(trap => trap == null);
            
            // Animasyonları güncelle
            UpdateAnimation();
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    // Basit animasyon kontrolü - mevcut animasyonlara göre
    private void UpdateAnimation()
    {
        if (animator != null)
        {
            // Hareket hızı - Rigidbody2D velocity kullan
            float currentSpeed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
            
            // Durum parametreleri - mevcut animasyonlara göre ayarla
            animator.SetBool("IsChasing", currentState == GoblinState.Chasing);
            animator.SetBool("IsPlacingTrap", isPlacingTrap);
            animator.SetBool("IsDead", currentState == GoblinState.Dead);
            
            // Can durumu
            animator.SetFloat("Health", currentHealth);
        }
    }
    
    private void HandleChasing()
    {
        if (PlayerController.Instance == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        // Try to maintain distance from player while placing traps
        if (distanceToPlayer < keepDistanceFromPlayer)
        {
            // Move away from player
            Vector2 retreatDirection = (transform.position - PlayerController.Instance.transform.position).normalized;
            rb.linearVelocity = retreatDirection * GetCurrentSpeed();
        }
        else if (distanceToPlayer > retreatDistance)
        {
            // Move closer to player (but not too close)
            Vector2 approachDirection = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.linearVelocity = approachDirection * GetCurrentSpeed() * 0.7f; // Move slower when approaching
        }
        else
        {
            // In optimal range, stop and consider placing trap
            rb.linearVelocity = Vector2.zero;
            
            if (ShouldPlaceTrap())
            {
                ChangeState(GoblinState.PlacingTrap);
            }
        }
    }
    
    protected virtual bool ShouldPlaceTrap()
    {
        if (Time.time < lastTrapTime + trapCooldown) return false;
        if (placedTraps.Count >= maxTraps) return false;
        if (PlayerController.Instance == null) return false;
        
        // Check if player is in range and there's a good spot for a trap
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        if (distanceToPlayer > trapPlacementRange) return false;
        
        // Find a good trap placement position
        Vector2 trapPosition = FindTrapPlacementPosition();
        return trapPosition != Vector2.zero;
    }
    
    protected virtual Vector2 FindTrapPlacementPosition()
    {
        if (PlayerController.Instance == null) return Vector2.zero;
        
        // Try to place trap between goblin and player, or near player's path
        Vector2 playerPos = PlayerController.Instance.transform.position;
        Vector2 goblinPos = transform.position;
        
        // Calculate potential positions
        Vector2[] candidatePositions = new Vector2[]
        {
            Vector2.Lerp(goblinPos, playerPos, 0.6f), // Between goblin and player
            playerPos + (Vector2)PlayerController.Instance.transform.right * 2f, // To the side of player
            playerPos + (Vector2)PlayerController.Instance.transform.right * -2f, // Other side of player
            playerPos + (Vector2)PlayerController.Instance.transform.up * 2f, // Above player
            playerPos + (Vector2)PlayerController.Instance.transform.up * -2f // Below player
        };
        
        foreach (Vector2 pos in candidatePositions)
        {
            if (IsValidTrapPosition(pos))
            {
                return pos;
            }
        }
        
        return Vector2.zero;
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
        
        // Check if position is on valid ground (you might want to add ground detection here)
        // For now, just check if it's not too far from the goblin
        if (Vector2.Distance(position, transform.position) > trapPlacementRange)
        {
            return false;
        }
        
        return true;
    }
    
    protected virtual IEnumerator PlaceTrapBehavior()
    {
        isPlacingTrap = true;
        rb.linearVelocity = Vector2.zero;
        
        // Trap Placement animasyonunu başlat
        if (animator != null)
        {
            animator.SetBool("IsPlacingTrap", true);
        }
        
        // Find placement position
        Vector2 trapPosition = FindTrapPlacementPosition();
        
        if (trapPosition == Vector2.zero)
        {
            // No valid position found, return to previous state
            isPlacingTrap = false;
            ChangeState(GoblinState.Chasing);
            yield break;
        }
        
        // Play trap placement animation
        yield return new WaitForSeconds(trapPlacementTime);
        
        // Create the trap
        if (bearTrapPrefab != null)
        {
            GameObject newTrap = Instantiate(bearTrapPrefab, trapPosition, Quaternion.identity);
            placedTraps.Add(newTrap);
            
            // Configure the trap
            var trapComponent = newTrap.GetComponent<GoblinTrap>();
            if (trapComponent != null)
            {
                trapComponent.SetTrapType(TrapType.BearTrap);
                trapComponent.SetTrapDamage(20f);
            }
            
            Debug.Log($"{gameObject.name} placed a bear trap at {trapPosition}");
        }
        
        lastTrapTime = Time.time;
        isPlacingTrap = false;
        
        // Reset animation state to prevent freezing
        if (animator != null)
        {
            animator.SetBool("IsPlacingTrap", false);
        }
        
        // Return to chasing
        ChangeState(GoblinState.Chasing);
    }
    
    protected virtual IEnumerator PlaceTrapWhileFleeing()
    {
        if (isPlacingTrap) yield break;
        
        Vector2 trapPosition = transform.position; // Place trap at current position while fleeing
        
        if (bearTrapPrefab != null && placedTraps.Count < maxTraps)
        {
            GameObject newTrap = Instantiate(bearTrapPrefab, trapPosition, Quaternion.identity);
            placedTraps.Add(newTrap);
            
            var trapComponent = newTrap.GetComponent<GoblinTrap>();
            if (trapComponent != null)
            {
                trapComponent.SetTrapType(TrapType.BearTrap);
                trapComponent.SetTrapDamage(15f); // Slightly less damage when placed while fleeing
            }
            
            lastTrapTime = Time.time;
            Debug.Log($"{gameObject.name} placed a bear trap while fleeing!");
        }
    }
    
    protected override IEnumerator AttackBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        
        // Saldırı animasyonunu başlat
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Trapper goblin "attacks" by swinging its bear trap weapon
        yield return new WaitForSeconds(0.5f);
        
        PerformTrapAttack();
        PlaySound(attackSound);
        
        lastAttackTime = Time.time;
        
        yield return new WaitForSeconds(0.5f);
        
        if (currentState == GoblinState.Attacking)
        {
            ChangeState(GoblinState.Chasing);
        }
    }
    
    protected virtual void PerformTrapAttack()
    {
        // Melee attack with the bear trap weapon
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, goblinStats.attackRange);
        
        if (hitPlayer != null && hitPlayer.CompareTag("Player"))
        {
            var playerHealth = hitPlayer.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer();
                Debug.Log($"{gameObject.name} hit player with bear trap weapon for {goblinStats.attackDamage} damage!");
            }
            
            // Apply brief slow effect
            var playerMoveable = hitPlayer.GetComponent<IMoveable>();
            if (playerMoveable != null)
            {
                StartCoroutine(ApplyBriefSlow(playerMoveable));
            }
        }
    }
    
    protected virtual IEnumerator ApplyBriefSlow(IMoveable target)
    {
        target.SetSpeedMultiplier(0.7f);
        yield return new WaitForSeconds(2f);
        target.SetSpeedMultiplier(1f);
    }
    
    protected override void OnGoblinDamaged()
    {
        base.OnGoblinDamaged();
        
        // When damaged, try to place a trap if possible
        if (!isPlacingTrap && ShouldPlaceTrap())
        {
            ChangeState(GoblinState.PlacingTrap);
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw trap placement range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, trapPlacementRange);
        
        // Draw keep distance range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, keepDistanceFromPlayer);
    }
}
