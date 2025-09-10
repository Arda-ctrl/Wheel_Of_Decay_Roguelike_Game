using UnityEngine;
using System.Collections;

public class DaggerGoblinBomber : DaggerGoblin
{
    [Header("Bomber Settings")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionDamage = 30f;
    [SerializeField] private float explosionDelay = 1.5f;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private LayerMask damageableLayers = -1;
    
    private bool isExploding = false;
    private bool hasExploded = false;
    private bool pendingExplosionEvent = false;
    
    protected override void Start()
    {
        goblinType = GoblinType.DaggerGoblinBomber;
        
        if (statsAsset == null)
        {
            // Defaults when no SO is provided
            goblinStats.health = 35f;
            goblinStats.speed = 5.5f;
            goblinStats.attackDamage = 15f; // Lower dagger damage since it has explosion
            goblinStats.attackRange = 1.2f;
            goblinStats.attackCooldown = 1.2f;
            goblinStats.canFlee = false; // Bombers don't flee, they explode
            goblinStats.explosionDamage = explosionDamage;
            goblinStats.explosionRadius = explosionRadius;
        }
        
        base.Start();
        allowSOTypeOverride = false; // SO goblinType'ı değiştirmesin

        // Locomotion booleans are driven by base from speed
    }
    
    protected override void OnGoblinDamaged()
    {
        base.OnGoblinDamaged();
        
        // When damaged, start explosion sequence
        if (!isExploding && !hasExploded && GetCurrentHealth() > 0)
        {
            StartExplosionSequence();
        }
    }
    
    public override void TakeDamage(float amount)
    {
        if (hasExploded) return;
        
        base.TakeDamage(amount);
        
        // If killed by damage, explode immediately
        if (GetCurrentHealth() <= 0 && !hasExploded)
        {
            StartCoroutine(ExplodeImmediately());
        }
    }
    
    private void StartExplosionSequence()
    {
        if (isExploding || hasExploded) return;
        
        isExploding = true;
        ChangeState(GoblinState.Exploding);
        // No dedicated param; can reuse Throw or Attack as wind-up if desired
        if (animator != null)
        {
            animator.SetTrigger(AnimAttack);
            animator.SetBool(AnimIsAttacking, true);
        }
        StartCoroutine(ExplodeAfterDelay());
    }
    
    private IEnumerator ExplodeAfterDelay()
    {
        // Change behavior - run towards player frantically
        float timer = 0f;
        
        while (timer < explosionDelay && !hasExploded)
        {
            if (PlayerController.Instance != null)
            {
                Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
                rb.linearVelocity = directionToPlayer * (GetCurrentSpeed() * 1.5f); // Move faster when about to explode
                
                // Flash or show visual indicator that it's about to explode
                if (Mathf.Sin(Time.time * 20f) > 0) // Fast flashing effect
                {
                    // This would flash the sprite renderer
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = Color.red;
                    }
                }
                else
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = Color.white;
                    }
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (!hasExploded)
        {
            Explode();
        }
    }
    
    private IEnumerator ExplodeImmediately()
    {
        yield return new WaitForSeconds(0.1f); // Tiny delay for death animation
        if (!hasExploded)
        {
            Explode();
        }
    }
    
    private void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        isExploding = false;
        // Ölümü görünür yap: Death state'e geçir ve patlamayı anim event'e bırak
        ChangeState(GoblinState.Dead);
        if (animator != null) animator.SetBool(AnimIsDead, true);
        pendingExplosionEvent = true;
    }

    // Death animasyonuna eklenecek event
    public void OnBomberDeathEvent()
    {
        Debug.Log($"OnBomberDeathEvent fired for {name}");
        // VFX
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        // SFX
        PlaySound(explosionSound);

        // Damage & knockback
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            var health = hitCollider.GetComponent<IHealth>();
            if (health != null)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    health.TakeDamage(explosionDamage);
                }
                else
                {
                    health.TakeDamage(explosionDamage * 0.5f);
                }
            }

            // Knockback only for player (optional for others)
            if (hitCollider.CompareTag("Player"))
            {
                var playerRb = hitCollider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        
        // If touching player while exploding, explode immediately
        if (other.CompareTag("Player") && isExploding && !hasExploded)
        {
            StopAllCoroutines();
            Explode();
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    
    protected override void OnStateEnter(GoblinState newState, GoblinState oldState)
    {
        base.OnStateEnter(newState, oldState);
        
        if (newState == GoblinState.Exploding)
        {
            // Stop any existing behaviors and focus on explosion
            if (currentStateCoroutine != null)
            {
                StopCoroutine(currentStateCoroutine);
            }
        }
    }
}
