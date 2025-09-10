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
    [SerializeField] protected GoblinStatsSO statsAsset;
    [SerializeField] protected bool allowSOTypeOverride = false;
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
    protected bool isPerformingAttack;
    
    [Header("Death Settings")]
    [SerializeField] protected float deathDestroyDelay = 0.8f;
    
    // Animator & unified parameters
    [SerializeField] protected Animator animator; // atanabilir; yoksa Start'ta bulunur
    protected static readonly int AnimIsIdle = Animator.StringToHash("IsIdle");
    protected static readonly int AnimIsJogging = Animator.StringToHash("IsJogging");
    protected static readonly int AnimAttack = Animator.StringToHash("Attack");
    protected static readonly int AnimThrow = Animator.StringToHash("Throw");
    protected static readonly int AnimIsSettingUp = Animator.StringToHash("IsSettingUp");
    protected static readonly int AnimIsDead = Animator.StringToHash("IsDead");
    protected static readonly int AnimIsAttacking = Animator.StringToHash("IsAttacking");
    
    // Virtual methods for different goblin behaviors
    protected override void Start()
    {
        // Load from ScriptableObject if provided
        if (statsAsset != null)
        {
            goblinStats = new GoblinStats
            {
                health = statsAsset.stats.health,
                speed = statsAsset.stats.speed,
                attackDamage = statsAsset.stats.attackDamage,
                attackRange = statsAsset.stats.attackRange,
                attackCooldown = statsAsset.stats.attackCooldown,
                canFlee = statsAsset.stats.canFlee,
                fleeHealthThreshold = statsAsset.stats.fleeHealthThreshold,
                minAlliesForFlee = statsAsset.stats.minAlliesForFlee,
                explosionDamage = statsAsset.stats.explosionDamage,
                explosionRadius = statsAsset.stats.explosionRadius
            };
            if (allowSOTypeOverride && statsAsset.overrideGoblinType)
            {
                goblinType = statsAsset.goblinType;
            }
        }

        // Initialize with goblin stats
        maxHealth = goblinStats.health;
        baseSpeed = goblinStats.speed;
        
        base.Start();
        
        // Cache Animator (prefab üstünden atanmışsa onu kullan; yoksa bul)
        // Öncelik: child 'Body' objesindeki Animator
        Transform body = transform.Find("Body");
        if (body != null)
        {
            var bodyAnim = body.GetComponent<Animator>();
            if (bodyAnim != null) animator = bodyAnim;
            // SpriteRenderer da Body üzerinde olabilir
            if (spriteRenderer == null)
            {
                var sr = body.GetComponent<SpriteRenderer>();
                if (sr != null) spriteRenderer = sr;
            }
        }
        // Yine bulunamadıysa kökte ya da çocuklarda ara
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[GoblinController] Animator bulunamadı: {name}. Parametreler güncellenmeyecek.");
        }

        // Hız ölçümü için başlangıç değeri
        _lastPosition = transform.position;
        _smoothedSpeed = 0f;
        _joggingState = false;
        
        // Start AI coroutine
        StartCoroutine(GoblinAI());
    }
    
    protected override void Update()
    {
        // Attacking sırasında takip/hareket AI'ını tamamen devre dışı bırak
        if (currentState == GoblinState.Attacking)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            // Yalnızca anim parametrelerini güncelle
            UpdateAnimatorBase();
            return;
        }

        base.Update();
        UpdateAllyDetection();
        UpdateState();
        UpdateAnimatorBase();
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
        // Saldırı sırasında state değişimlerine kilit
        if (currentState == GoblinState.Attacking && isPerformingAttack) return;
        
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
        
        // Sync death flag immediately
        if (animator != null && newState == GoblinState.Dead)
        {
            animator.SetBool(AnimIsDead, true);
        }
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
                isPerformingAttack = true;
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
        if (exitingState == GoblinState.Attacking)
        {
            isPerformingAttack = false;
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
        // Base sınıf anında Destroy ettiği için burada kendi ölüm akışımızı yönetiyoruz
        currentHealth -= amount;
        if (currentHealth > 0f)
        {
            OnGoblinDamaged();
            return;
        }

        if (currentState != GoblinState.Dead)
        {
            currentHealth = 0f;
            ChangeState(GoblinState.Dead);
            StartCoroutine(HandleDeathSequence());
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

    // --- Animator helpers ---
    private Vector3 _lastPosition;
    private float _smoothedSpeed;
    private bool _joggingState;
    protected void UpdateAnimatorBase()
    {
        if (animator == null) return;
        // Daha güvenilir hız ölçümü: pozisyon farkı
        float measuredSpeed = (transform.position - _lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPosition = transform.position;
        // Low-pass filter: ani sıçramaları yumuşat
        float smoothFactor = 10f; // daha yüksek = daha hızlı tepki
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, measuredSpeed, Mathf.Clamp01(smoothFactor * Time.deltaTime));

        // Histerezis: gir/çık eşikleri farklı
        const float jogEnter = 0.12f;
        const float jogExit  = 0.06f;
        bool canJogType = (goblinType == GoblinType.DaggerGoblin || goblinType == GoblinType.DaggerGoblinBomber);
        if (canJogType)
        {
            if (_joggingState)
            {
                // Çıkış eşiği
                if (_smoothedSpeed < jogExit) _joggingState = false;
            }
            else
            {
                // Giriş eşiği
                if (_smoothedSpeed > jogEnter) _joggingState = true;
            }
        }
        else
        {
            _joggingState = false;
        }

        bool isIdle = _smoothedSpeed < jogExit || currentState == GoblinState.Idle;

        // Saldırırken hareket animasyonlarını kilitle (koşma/idle geçişleri olmasın)
        if (currentState == GoblinState.Attacking)
        {
            _joggingState = false;
            isIdle = true;
        }

        animator.SetBool(AnimIsIdle, isIdle && !_joggingState);
        animator.SetBool(AnimIsJogging, _joggingState);
        animator.SetBool(AnimIsDead, currentState == GoblinState.Dead);
        animator.SetBool(AnimIsAttacking, currentState == GoblinState.Attacking);
    }

    private IEnumerator HandleDeathSequence()
    {
        // Hareketi ve çarpışmaları durdur
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        var colls = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colls.Length; i++) colls[i].enabled = false;

        // Ölüm animasyonu için zaman tanı
        yield return new WaitForSeconds(Mathf.Max(0.05f, deathDestroyDelay));
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
