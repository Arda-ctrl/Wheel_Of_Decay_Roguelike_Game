using UnityEngine;
using System.Collections;

public class SummonerForestSpriteWizard : VerdantEnemy
{
    [Header("Wizard Sprite Settings")]
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] private float minFleeDistance = 5f;
    [SerializeField] private float maxFleeDistance = 8f;
    [SerializeField] private float panicDistance = 3f; // Distance at which wizard panics and flees faster

    [Header("Summoning Settings")]
    [SerializeField] private GameObject brainlessSpriteMinion; // Reference to brainless sprite prefab
    [SerializeField] private float summonInterval = 5f;
    [SerializeField] private int maxSummonedMinions = 6;
    [SerializeField] private float summonRange = 3f;
    [SerializeField] private float summonCastTime = 1.5f;
    [SerializeField] private GameObject summonEffect; // Visual effect for summoning

    [Header("Staff Settings")]
    [SerializeField] private Transform staffTransform; // Visual staff object
    [SerializeField] private ParticleSystem magicAura; // Magic particles around staff
    [SerializeField] private AudioClip summonSound;
    [SerializeField] private AudioClip teleportSound;

    [Header("Teleport Ability")]
    [SerializeField] private bool canTeleport = true;
    [SerializeField] private float teleportCooldown = 8f;
    [SerializeField] private float teleportDistance = 4f;
    [SerializeField] private GameObject teleportEffect;

    private float lastSummonTime;
    private float lastTeleportTime;
    private bool isSummoning = false;
    private bool isFleeingInPanic = false;
    private int currentMinionCount = 0;

    protected override void Start()
    {
        base.Start();
        
        // Set wizard sprite stats
        if (enemyData != null)
        {
            enemyData.baseSpeed = fleeSpeed;
            enemyData.maxHealth *= 0.8f; // Less health than weaponless summoner
            enemyData.baseDamage = 0f; // No direct damage
            enemyData.attackRange = 0f; // No melee attacks
            enemyData.detectionRange = 10f; // High detection to flee early
            enemyData.hasSpecialAbility = true;
            enemyData.specialAbilityCooldown = summonInterval;
            enemyData.specialAbilityRange = summonRange;
        }
        
        // Disable gravity for flying behavior
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 1.5f; // More drag for controlled movement
        }

        // Initialize current health with the updated max health
        currentHealth = enemyData.maxHealth;
        
        // Start summoning coroutine
        StartCoroutine(SummonBehavior());
    }

    protected override void UpdateAI()
    {
        if (PlayerController.Instance == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        isPlayerInRange = distanceToPlayer <= enemyData.detectionRange;

        if (isPlayerInRange)
        {
            HandlePlayerInRange(distanceToPlayer);
        }
        else
        {
            HandlePlayerOutOfRange();
        }

        // Check if should teleport when in danger
        if (canTeleport && distanceToPlayer < panicDistance && Time.time >= lastTeleportTime + teleportCooldown)
        {
            StartCoroutine(PerformTeleport());
        }
    }

    protected override void HandlePlayerInRange(float distanceToPlayer)
    {
        // Always flee from player
        Vector2 fleeDirection = (transform.position - PlayerController.Instance.transform.position).normalized;
        
        // Determine flee speed based on distance
        float currentFleeSpeed = fleeSpeed;
        if (distanceToPlayer < panicDistance)
        {
            currentFleeSpeed *= 1.5f; // Panic speed
            isFleeingInPanic = true;
        }
        else
        {
            isFleeingInPanic = false;
        }

        // Keep minimum distance from player
        if (distanceToPlayer < minFleeDistance)
        {
            // Flee at full speed
            if (rb != null)
            {
                rb.linearVelocity = fleeDirection * currentFleeSpeed;
            }
        }
        else if (distanceToPlayer > maxFleeDistance)
        {
            // Slow down when far enough
            if (rb != null)
            {
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);
            }
        }
        else
        {
            // Maintain distance
            if (rb != null)
            {
                rb.linearVelocity = fleeDirection * currentFleeSpeed * 0.5f;
            }
        }

        // Update sprite direction
        UpdateSpriteDirection(fleeDirection);
    }

    protected override void HandlePlayerOutOfRange()
    {
        // Hover in place when player is not detected
        if (rb != null)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 3f);
        }
        isFleeingInPanic = false;
    }

    private void UpdateSpriteDirection(Vector2 movement)
    {
        if (spriteRenderer != null)
        {
            isFacingRight = movement.x > 0;
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    private IEnumerator SummonBehavior()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(summonInterval);
            
            if (currentMinionCount < maxSummonedMinions && !isSummoning)
            {
                yield return StartCoroutine(SummonMinion());
            }
        }
    }

    private IEnumerator SummonMinion()
    {
        if (brainlessSpriteMinion == null) yield break;

        isSummoning = true;
        
        // Play summoning animation and effects
        if (animator != null)
        {
            animator.SetBool("IsCasting", true);
        }
        
        if (magicAura != null)
        {
            magicAura.Play();
        }
        
        if (summonSound != null)
        {
            AudioSource.PlayClipAtPoint(summonSound, transform.position);
        }
        
        // Stop movement while casting
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Debug.Log($"🧙‍♀️ {enemyData.enemyName} is summoning a minion...");
        
        yield return new WaitForSeconds(summonCastTime);
        
        // Spawn the minion
        Vector2 summonOffset = Random.insideUnitCircle * summonRange;
        Vector3 summonPosition = transform.position + (Vector3)summonOffset;
        
        GameObject minion = Instantiate(brainlessSpriteMinion, summonPosition, Quaternion.identity);
        currentMinionCount++;
        
        // Spawn summon effect
        if (summonEffect != null)
        {
            Instantiate(summonEffect, summonPosition, Quaternion.identity);
        }
        
        // Setup minion death callback to decrease count
        var minionHealth = minion.GetComponent<BaseEnemy>();
        if (minionHealth != null)
        {
            // Add a component to track when minion dies
            var tracker = minion.AddComponent<MinionTracker>();
            tracker.parentWizard = this;
        }
        
        Debug.Log($"🧚 {enemyData.enemyName} summoned a brainless sprite! ({currentMinionCount}/{maxSummonedMinions})");
        
        lastSummonTime = Time.time;
        isSummoning = false;
        
        if (animator != null)
        {
            animator.SetBool("IsCasting", false);
        }
        
        if (magicAura != null)
        {
            magicAura.Stop();
        }
    }

    private IEnumerator PerformTeleport()
    {
        if (PlayerController.Instance == null) yield break;
        
        lastTeleportTime = Time.time;
        
        // Calculate teleport position (away from player)
        Vector2 playerPos = PlayerController.Instance.transform.position;
        Vector2 currentPos = transform.position;
        Vector2 awayDirection = (currentPos - playerPos).normalized;
        Vector2 teleportPos = currentPos + awayDirection * teleportDistance;
        
        // Play teleport effect at current position
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);
        }
        
        // Brief invisibility/fade effect
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        // Teleport
        transform.position = teleportPos;
        
        // Play teleport effect at new position
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Restore visibility
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
        
        Debug.Log($"🧙‍♀️ {enemyData.enemyName} teleported away from player!");
    }

    public void OnMinionDied()
    {
        currentMinionCount = Mathf.Max(0, currentMinionCount - 1);
        Debug.Log($"Minion died. Remaining: {currentMinionCount}/{maxSummonedMinions}");
    }

    protected override void PerformAttack()
    {
        // Wizard doesn't perform direct attacks, only summons
        if (Time.time >= lastSummonTime + summonInterval && !isSummoning)
        {
            StartCoroutine(SummonMinion());
        }
    }

    protected override void UseSpecialAbility()
    {
        // Special ability is summoning
        if (!isSummoning && currentMinionCount < maxSummonedMinions)
        {
            StartCoroutine(SummonMinion());
        }
    }

    protected override void OnVerdantDamaged(float damage)
    {
        // When damaged, try to teleport if possible
        if (canTeleport && Time.time >= lastTeleportTime + (teleportCooldown * 0.5f))
        {
            StartCoroutine(PerformTeleport());
        }
        
        // Also summon emergency minion if possible
        if (!isSummoning && currentMinionCount < maxSummonedMinions)
        {
            StartCoroutine(SummonMinion());
        }
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator != null)
        {
            animator.SetBool("IsFlying", true);
            animator.SetBool("IsFleeing", isFleeingInPanic);
            animator.SetBool("IsCasting", isSummoning);
            animator.SetFloat("FlightSpeed", rb != null ? rb.linearVelocity.magnitude : 0f);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw flee distances
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, panicDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minFleeDistance);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxFleeDistance);
        
        // Draw summon range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRange);
    }
}

// Helper component to track minion deaths
public class MinionTracker : MonoBehaviour
{
    public SummonerForestSpriteWizard parentWizard;
    
    private void OnDestroy()
    {
        if (parentWizard != null)
        {
            parentWizard.OnMinionDied();
        }
    }
}


