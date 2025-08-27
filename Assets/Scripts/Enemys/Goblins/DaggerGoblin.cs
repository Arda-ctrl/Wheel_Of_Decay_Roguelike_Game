using UnityEngine;
using System.Collections;

public class DaggerGoblin : GoblinController
{
    [Header("Dagger Goblin Settings")]
    [SerializeField] private float rushSpeed = 8f;
    [SerializeField] private float rushDistance = 3f;
    [SerializeField] private float rushCooldown = 2f;
    [SerializeField] private float daggerDamage = 20f;
    [SerializeField] private LayerMask playerLayer = 1;
    
    private bool isRushing = false;
    private float lastRushTime;
    private Vector2 rushStartPosition;
    
    protected override void Start()
    {
        goblinType = GoblinType.DaggerGoblin;
        
        // Set specific stats for Dagger Goblin
        goblinStats.health = 40f;
        goblinStats.speed = 5f;
        goblinStats.attackDamage = daggerDamage;
        goblinStats.attackRange = 1.2f;
        goblinStats.attackCooldown = 1f;
        goblinStats.canFlee = true;
        goblinStats.fleeHealthThreshold = 0.4f; // Flee when below 40% health
        goblinStats.minAlliesForFlee = 1;
        
        base.Start();
    }
    
    protected override IEnumerator GoblinAI()
    {
        while (currentState != GoblinState.Dead)
        {
            switch (currentState)
            {
                case GoblinState.Idle:
                    // Look around, idle animation
                    rb.linearVelocity = Vector2.zero;
                    break;
                    
                case GoblinState.Chasing:
                    if (!isRushing && Time.time >= lastRushTime + rushCooldown)
                    {
                        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
                        if (distanceToPlayer <= rushDistance && distanceToPlayer > goblinStats.attackRange)
                        {
                            StartCoroutine(RushAttack());
                        }
                    }
                    
                    if (!isRushing)
                    {
                        // Normal chase behavior
                        Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
                        rb.linearVelocity = directionToPlayer * GetCurrentSpeed();
                    }
                    break;
                    
                case GoblinState.Fleeing:
                    // Fleeing behavior is handled in base class
                    break;
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    protected override IEnumerator AttackBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        
        // Play attack animation
        yield return new WaitForSeconds(0.3f); // Wind-up time
        
        // Perform dagger attack
        PerformDaggerAttack();
        
        // Play attack sound
        PlaySound(attackSound);
        
        lastAttackTime = Time.time;
        
        // Attack recovery time
        yield return new WaitForSeconds(0.5f);
        
        // Return to chasing if player is still in range
        if (currentState == GoblinState.Attacking)
        {
            ChangeState(GoblinState.Chasing);
        }
    }
    
    protected override void OnGoblinDamaged()
    {
        // Flash red or play hurt animation
        Debug.Log($"{gameObject.name} took damage! Current health: {GetCurrentHealth()}");
        
        // Check if should start fleeing
        if (goblinStats.canFlee && GetCurrentHealth() <= goblinStats.health * goblinStats.fleeHealthThreshold)
        {
            if (nearbyAllies.Count < goblinStats.minAlliesForFlee)
            {
                Debug.Log($"{gameObject.name} is fleeing - low health and no allies nearby!");
            }
        }
    }
    
    private IEnumerator RushAttack()
    {
        isRushing = true;
        lastRushTime = Time.time;
        rushStartPosition = transform.position;
        
        // Calculate rush direction
        Vector2 rushDirection = (PlayerController.Instance.transform.position - transform.position).normalized;
        
        // Rush towards player
        float rushTimer = 0f;
        float maxRushTime = rushDistance / rushSpeed;
        
        while (rushTimer < maxRushTime && isRushing)
        {
            rb.linearVelocity = rushDirection * rushSpeed;
            
            // Check if hit player during rush
            Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, goblinStats.attackRange, playerLayer);
            if (hitPlayer != null && hitPlayer.CompareTag("Player"))
            {
                PerformDaggerAttack();
                break;
            }
            
            rushTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        isRushing = false;
        
        // Brief stop after rush
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);
    }
    
    private void PerformDaggerAttack()
    {
        // Check for player in attack range
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, goblinStats.attackRange, playerLayer);
        
        if (hitPlayer != null && hitPlayer.CompareTag("Player"))
        {
            // Deal damage to player
            var playerHealth = hitPlayer.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer();
                Debug.Log($"{gameObject.name} hit player with dagger!");
            }
            
            // Small knockback
            var playerRb = hitPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDirection = (hitPlayer.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
            }
        }
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isRushing)
        {
            PerformDaggerAttack();
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw rush distance
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rushDistance);
    }
}
