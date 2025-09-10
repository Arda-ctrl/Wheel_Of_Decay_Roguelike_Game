using UnityEngine;
using System.Collections;

public class TrapperGoblinBombs : TrapperGoblin
{
    [Header("Bomb Throwing Settings")]
    [SerializeField] private GameObject mudBombPrefab;
    [SerializeField] private float bombThrowRange = 8f;
    [SerializeField] private float bombCooldown = 4f;
    [SerializeField] private float bombDamage = 18f;
    [SerializeField] private float bombRadius = 2.5f;
    [SerializeField] private int maxBombs = 2;
    
    [Header("Combat Behavior")]
    [SerializeField] private float preferredCombatRange = 6f;
    [SerializeField] private float bombAccuracy = 0.8f; // 0-1, how accurately bombs are thrown
    
    private float lastBombTime;
    private int bombsThrown = 0;
    private bool isThrowing = false;
    
    protected override void Start()
    {
        goblinType = GoblinType.TrapperGoblinBombs;
        
        // Slightly different stats from regular trapper
        goblinStats.health = 55f;
        goblinStats.speed = 4.5f;
        goblinStats.attackDamage = 10f; // Lower melee damage since it has ranged attacks
        goblinStats.attackRange = 2f;
        goblinStats.attackCooldown = 2.5f;
        goblinStats.canFlee = true;
        goblinStats.fleeHealthThreshold = 0.4f;
        goblinStats.minAlliesForFlee = 0;
        
        base.Start();

        // Locomotion booleans are driven by base from speed
    }
    
    protected override IEnumerator GoblinAI()
    {
        while (currentState != GoblinState.Dead)
        {
            switch (currentState)
            {
                case GoblinState.Idle:
                    rb.linearVelocity = Vector2.zero;
                    
                    // Look for opportunities to place traps or throw bombs
                    if (ShouldPlaceTrap())
                    {
                        ChangeState(GoblinState.PlacingTrap);
                    }
                    else if (ShouldThrowBomb())
                    {
                        StartCoroutine(ThrowBombBehavior());
                    }
                    break;
                    
                case GoblinState.Chasing:
                    HandleAdvancedChasing();
                    break;
                    
                case GoblinState.PlacingTrap:
                    // Use base class trap placement
                    if (!isPlacingTrap)
                    {
                        StartCoroutine(PlaceTrapBehavior());
                    }
                    break;
                    
                case GoblinState.Fleeing:
                    // Use base class fleeing, but also throw bombs while fleeing
                    if (ShouldThrowBomb())
                    {
                        StartCoroutine(ThrowBombWhileFleeing());
                    }
                    break;
            }
            
            yield return new WaitForFixedUpdate();
        }
    }

    // Trapper Bomber için saldırı kullanılmıyor, Attacking yerine Throw/PlacingTrap tercih edilir
    protected override IEnumerator AttackBehavior()
    {
        // Eğer bomba atılabiliyorsa onu yap, aksi halde hızla Chasing'e dön
        if (ShouldThrowBomb())
        {
            yield return StartCoroutine(ThrowBombBehavior());
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            yield return null;
        }

        if (currentState == GoblinState.Attacking)
        {
            ChangeState(GoblinState.Chasing);
        }
    }
    
    private void HandleAdvancedChasing()
    {
        if (PlayerController.Instance == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        // Prioritize bomb throwing if in range
        if (ShouldThrowBomb() && distanceToPlayer <= bombThrowRange)
        {
            if (!isThrowing)
            {
                StartCoroutine(ThrowBombBehavior());
            }
            return;
        }
        
        // Maintain preferred combat range
        if (distanceToPlayer < preferredCombatRange * 0.7f)
        {
            // Too close, back away
            Vector2 retreatDirection = (transform.position - PlayerController.Instance.transform.position).normalized;
            rb.linearVelocity = retreatDirection * GetCurrentSpeed();
        }
        else if (distanceToPlayer > preferredCombatRange * 1.3f)
        {
            // Too far, move closer
            Vector2 approachDirection = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.linearVelocity = approachDirection * GetCurrentSpeed() * 0.6f;
        }
        else
        {
            // In good range, stop and look for opportunities
            rb.linearVelocity = Vector2.zero;
            
            if (ShouldPlaceTrap())
            {
                ChangeState(GoblinState.PlacingTrap);
            }
        }
    }
    
    private bool ShouldThrowBomb()
    {
        if (Time.time < lastBombTime + bombCooldown) return false;
        if (bombsThrown >= maxBombs) return false;
        if (PlayerController.Instance == null) return false;
        if (mudBombPrefab == null) return false;
        if (isThrowing) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        return distanceToPlayer <= bombThrowRange && distanceToPlayer > goblinStats.attackRange;
    }
    
    private IEnumerator ThrowBombBehavior()
    {
        if (isThrowing) yield break;
        
        isThrowing = true;
        rb.linearVelocity = Vector2.zero;
        
        // unified: trigger Throw animation
        if (animator != null) animator.SetTrigger(AnimThrow);
        
        // Wind-up animation time
        yield return new WaitForSeconds(0.8f);
        
        if (PlayerController.Instance != null && mudBombPrefab != null)
        {
            Vector3 targetPosition = CalculateBombTarget();
            ThrowBomb(targetPosition);
        }
        
        lastBombTime = Time.time;
        bombsThrown++;
        
        // Recovery time
        yield return new WaitForSeconds(0.5f);
        
        isThrowing = false;
        
        // Reset bomb count after a period
        if (bombsThrown >= maxBombs)
        {
            StartCoroutine(ResetBombCount());
        }
    }
    
    private IEnumerator ThrowBombWhileFleeing()
    {
        if (isThrowing || PlayerController.Instance == null) yield break;
        
        Vector3 targetPosition = PlayerController.Instance.transform.position;
        
        // Add some inaccuracy when throwing while fleeing
        float inaccuracy = (1f - bombAccuracy) * 3f;
        targetPosition += new Vector3(
            Random.Range(-inaccuracy, inaccuracy),
            Random.Range(-inaccuracy, inaccuracy),
            0
        );
        
        ThrowBomb(targetPosition);
        lastBombTime = Time.time;
        
        Debug.Log($"{gameObject.name} threw a bomb while fleeing!");
    }
    
    private Vector3 CalculateBombTarget()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        
        // Predict player movement
        var playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector3 playerVelocity = playerRb.linearVelocity;
            float flightTime = 1.5f; // Estimated bomb flight time
            playerPosition += (Vector3)playerVelocity * flightTime * 0.5f; // Partial prediction
        }
        
        // Add inaccuracy based on bombAccuracy
        float inaccuracy = (1f - bombAccuracy) * 2f;
        playerPosition += new Vector3(
            Random.Range(-inaccuracy, inaccuracy),
            Random.Range(-inaccuracy, inaccuracy),
            0
        );
        
        return playerPosition;
    }
    
    private void ThrowBomb(Vector3 targetPosition)
    {
        if (mudBombPrefab == null) return;
        
        // Create bomb at goblin's position
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f; // Slightly above goblin
        GameObject bomb = Instantiate(mudBombPrefab, spawnPosition, Quaternion.identity);
        
        // Initialize the bomb
        var bombComponent = bomb.GetComponent<GoblinBomb>();
        if (bombComponent != null)
        {
            bombComponent.Initialize(targetPosition, bombDamage, bombRadius);
        }
        
        Debug.Log($"{gameObject.name} threw a mud bomb at {targetPosition}!");
    }
    
    private IEnumerator ResetBombCount()
    {
        yield return new WaitForSeconds(10f); // Reset bomb count after 10 seconds
        bombsThrown = 0;
    }
    
    protected override void OnGoblinDamaged()
    {
        base.OnGoblinDamaged();
        
        // When damaged, prioritize throwing a bomb if possible
        if (!isThrowing && ShouldThrowBomb())
        {
            StartCoroutine(ThrowBombBehavior());
        }
    }
    
    
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw bomb throw range
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Orange color
        Gizmos.DrawWireSphere(transform.position, bombThrowRange);
        
        // Draw preferred combat range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredCombatRange);
    }
}
