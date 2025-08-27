using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GoblinType 
{
    DaggerGoblin,
    DaggerGoblinBomber,
    TrapperGoblin,
    TrapperGoblinBombs
}

public enum GoblinState
{
    Idle,
    Chasing,
    Attacking,
    Fleeing,
    PlacingTrap,
    Exploding,
    Dead
}

[System.Serializable]
public class GoblinStats
{
    [Header("Base Stats")]
    public float health = 50f;
    public float speed = 6f;
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    
    [Header("Goblin Specific")]
    public bool canFlee = true;
    public float fleeHealthThreshold = 0.3f; // 30% health or below
    public int minAlliesForFlee = 1; // Minimum allies needed to not flee
    public float explosionDamage = 25f;
    public float explosionRadius = 2f;
}

public abstract class GoblinController : EnemyController
{
    [Header("Goblin Settings")]
    [SerializeField] protected GoblinType goblinType;
    [SerializeField] protected GoblinStats goblinStats;
    [SerializeField] protected float allyDetectionRange = 8f;
    
    [Header("Audio")]
    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected AudioClip explosionSound;
    
    protected GoblinState currentState = GoblinState.Idle;
    protected float lastAttackTime;
    protected bool isFleeingAlone;
    protected List<GoblinController> nearbyAllies = new List<GoblinController>();
    protected Coroutine currentStateCoroutine;
    
    // Virtual methods for different goblin behaviors
    protected override void Start()
    {
        // Initialize with goblin stats
        maxHealth = goblinStats.health;
        baseSpeed = goblinStats.speed;
        
        base.Start();
        
        // Start AI coroutine
        StartCoroutine(GoblinAI());
    }
    
    protected override void Update()
    {
        base.Update();
        UpdateAllyDetection();
        UpdateState();
    }
    
    protected virtual void UpdateAllyDetection()
    {
        nearbyAllies.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, allyDetectionRange);
        
        foreach (var collider in colliders)
        {
            var ally = collider.GetComponent<GoblinController>();
            if (ally != null && ally != this && ally.currentState != GoblinState.Dead)
            {
                nearbyAllies.Add(ally);
            }
        }
        
        // Check if should flee when alone
        if (goblinStats.canFlee && GetCurrentHealth() <= goblinStats.health * goblinStats.fleeHealthThreshold)
        {
            isFleeingAlone = nearbyAllies.Count < goblinStats.minAlliesForFlee;
        }
    }
    
    protected virtual void UpdateState()
    {
        if (currentState == GoblinState.Dead) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        // State transitions
        switch (currentState)
        {
            case GoblinState.Idle:
                if (isFleeingAlone)
                {
                    ChangeState(GoblinState.Fleeing);
                }
                else if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(GoblinState.Chasing);
                }
                break;
                
            case GoblinState.Chasing:
                if (isFleeingAlone)
                {
                    ChangeState(GoblinState.Fleeing);
                }
                else if (distanceToPlayer <= goblinStats.attackRange && Time.time >= lastAttackTime + goblinStats.attackCooldown)
                {
                    ChangeState(GoblinState.Attacking);
                }
                else if (distanceToPlayer > detectionRange)
                {
                    ChangeState(GoblinState.Idle);
                }
                break;
                
            case GoblinState.Attacking:
                if (isFleeingAlone)
                {
                    ChangeState(GoblinState.Fleeing);
                }
                else if (distanceToPlayer > goblinStats.attackRange)
                {
                    ChangeState(GoblinState.Chasing);
                }
                break;
                
            case GoblinState.Fleeing:
                if (!isFleeingAlone && distanceToPlayer <= detectionRange)
                {
                    ChangeState(GoblinState.Chasing);
                }
                else if (!isFleeingAlone && distanceToPlayer > detectionRange)
                {
                    ChangeState(GoblinState.Idle);
                }
                break;
        }
    }
    
    protected virtual void ChangeState(GoblinState newState)
    {
        if (currentState == newState) return;
        
        // Exit current state
        OnStateExit(currentState);
        
        // Change state
        GoblinState oldState = currentState;
        currentState = newState;
        
        // Enter new state
        OnStateEnter(newState, oldState);
    }
    
    protected virtual void OnStateEnter(GoblinState newState, GoblinState oldState)
    {
        if (currentStateCoroutine != null)
        {
            StopCoroutine(currentStateCoroutine);
        }
        
        switch (newState)
        {
            case GoblinState.Attacking:
                currentStateCoroutine = StartCoroutine(AttackBehavior());
                break;
            case GoblinState.Fleeing:
                currentStateCoroutine = StartCoroutine(FleeBehavior());
                break;
        }
    }
    
    protected virtual void OnStateExit(GoblinState exitingState)
    {
        if (currentStateCoroutine != null)
        {
            StopCoroutine(currentStateCoroutine);
            currentStateCoroutine = null;
        }
    }
    
    // Abstract methods for specific goblin types to implement
    protected abstract IEnumerator GoblinAI();
    protected abstract IEnumerator AttackBehavior();
    protected virtual void OnGoblinDamaged()
    {
        // Base implementation - can be overridden by specific goblin types
        Debug.Log($"{gameObject.name} took damage! Current health: {GetCurrentHealth()}");
    }
    
    // Common behaviors
    protected virtual IEnumerator FleeBehavior()
    {
        while (currentState == GoblinState.Fleeing)
        {
            if (PlayerController.Instance != null)
            {
                Vector2 fleeDirection = (transform.position - PlayerController.Instance.transform.position).normalized;
                rb.linearVelocity = fleeDirection * GetCurrentSpeed() * 1.5f; // Flee faster
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        
        if (GetCurrentHealth() > 0)
        {
            OnGoblinDamaged();
        }
        else
        {
            ChangeState(GoblinState.Dead);
        }
    }
    
    protected virtual void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            // AudioManager expects an int index, not AudioClip
            // This would need to be implemented based on your audio system
            Debug.Log($"Playing sound for {gameObject.name}");
        }
    }
    
    // Now we can directly access the protected fields from EnemyController
    
    protected virtual void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw ally detection range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, allyDetectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, goblinStats.attackRange);
    }
}
