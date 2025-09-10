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
    [SerializeField] private float attackRangeTolerance = 0.15f;
    [SerializeField] private bool attackDebugLogs = false;
    
    private bool isRushing = false;
    private float lastRushTime;
    private Vector2 rushStartPosition;
    private bool hasDealtHitThisAttack = false;
    
    protected override void Start()
    {
        goblinType = GoblinType.DaggerGoblin;
        
        // If statsAsset is assigned in base, it will override these defaults.
        // Provide defaults only when not using SO to keep behavior consistent.
        if (statsAsset == null)
        {
            goblinStats.health = 40f;
            goblinStats.speed = 5.5f;
            goblinStats.attackDamage = daggerDamage;
            goblinStats.attackRange = 1.2f;
            goblinStats.attackCooldown = 1f;
            goblinStats.canFlee = true;
            goblinStats.fleeHealthThreshold = 0.4f; // Flee when below 40% health
            goblinStats.minAlliesForFlee = 1;
        }
        
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
        // Saldırı sırasında tamamen dur
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        // Kollizyonlardan itilmeyi de engellemek için hız çarpanı düşür (varsa)
        SetSpeedMultiplier(0f);
        
        // unified: trigger Attack animation
        if (animator != null)
        {
            animator.SetTrigger(AnimAttack);
            animator.SetBool(AnimIsAttacking, true);
        }
        hasDealtHitThisAttack = false; // event bekleniyor
        // Küçük bir wind-up penceresi (event ile senkron için)
        yield return new WaitForSeconds(0.1f);
        
        // Play attack sound
        PlaySound(attackSound);
        
        lastAttackTime = Time.time;
        
        // Attack state'i bitene kadar kilitli kal
        float safety = 2f; // olası çok uzun klipler için güvenlik
        while (animator != null && safety > 0f)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName("Attacking") || info.normalizedTime >= 0.98f)
                break;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            yield return null;
            safety -= Time.deltaTime;
        }
        // Event gelmediyse tek seferlik fallback hasar uygula
        if (!hasDealtHitThisAttack)
        {
            PerformDaggerAttack();
            hasDealtHitThisAttack = true;
        }
        if (animator != null) animator.SetBool(AnimIsAttacking, false);
        SetSpeedMultiplier(1f);
        
        // Saldırı tamamlandıysa normal akışa dön
        if (currentState == GoblinState.Attacking) ChangeState(GoblinState.Chasing);
    }

    // Body'deki attack klibine Animation Event ekle:
    // Function: InvokeOnParent, String: OnDaggerAttackHitEvent
    public void OnDaggerAttackHitEvent()
    {
        if (currentState != GoblinState.Attacking) return;
        if (hasDealtHitThisAttack) return;
        PerformDaggerAttack();
        hasDealtHitThisAttack = true;
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
        
        // Optional: treat as an attack start
        if (animator != null) animator.SetTrigger(AnimAttack);

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
        // Player referansına göre mesafe kontrolü (daha güvenilir)
        Transform playerT = PlayerController.Instance != null ? PlayerController.Instance.transform : null;
        if (playerT == null) return;

        float dist = Vector2.Distance(transform.position, playerT.position);
        bool inRange = dist <= (goblinStats.attackRange + attackRangeTolerance);

        if (inRange)
        {
            // Deal damage to player (prefer IHealth with proper amount)
            var targetHealth = playerT.GetComponent<IHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(goblinStats.attackDamage);
                if (attackDebugLogs) Debug.Log($"{gameObject.name} dagger hit for {goblinStats.attackDamage} (dist {dist:F2})");
            }
            else
            {
                var playerHealth = playerT.GetComponent<PlayerHealthController>();
                if (playerHealth != null)
                {
                    playerHealth.DamagePlayer(); // fallback to legacy method
                    if (attackDebugLogs) Debug.Log($"{gameObject.name} dagger hit (legacy)");
                }
            }
            
            // Small knockback
            var playerRb = playerT.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDirection = (playerT.position - transform.position).normalized;
                playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (attackDebugLogs) Debug.Log($"{gameObject.name} dagger miss (dist {dist:F2} > range {goblinStats.attackRange + attackRangeTolerance:F2})");
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
