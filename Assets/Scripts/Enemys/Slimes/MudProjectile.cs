using UnityEngine;

public enum MudType
{
    Normal,
    Poison,
    Acid
}

public class MudProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private MudType mudType = MudType.Normal;
    
    [Header("Effects")]
    [SerializeField] private GameObject splashEffect;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private float splashRadius = 1.5f;
    
    [Header("Status Effects")]
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float slowAmount = 0.5f;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonDamage = 5f;
    
    private Vector2 direction;
    private GameObject owner;
    private Rigidbody2D rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0.3f; // Slight gravity for arc
        }
    }

    public void Initialize(Vector2 targetPosition, float projectileSpeed, float projectileDamage, GameObject ownerObject, MudType type = MudType.Normal)
    {
        owner = ownerObject;
        speed = projectileSpeed;
        damage = projectileDamage;
        mudType = type;
        
        // Calculate direction and add arc
        Vector2 startPos = transform.position;
        direction = (targetPosition - startPos).normalized;
        
        // Add upward velocity for arc trajectory
        Vector2 velocity = direction * speed;
        velocity.y += 3f; // Add upward component for arc
        
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Don't hit the owner
        if (other.gameObject == owner) return;

        // Check if it hits player
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other);
        }
        
        // Hit ground or obstacles
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            CreateSplash();
        }
    }

    private void DealDamageToPlayer(Collider2D player)
    {
        hasHit = true;
        
        // Deal damage
        var playerHealth = player.GetComponent<IHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }

        // Apply status effects based on mud type
        ApplyStatusEffects(player);
        
        CreateSplash();
    }

    private void ApplyStatusEffects(Collider2D target)
    {
        var statusTarget = target.GetComponent<IStatusEffect>();
        if (statusTarget == null) return;
        
        switch (mudType)
        {
            case MudType.Normal:
                // Apply slow effect
                statusTarget.ApplyStatus(StatusEffectType.Chilled, slowDuration);
                break;
                
            case MudType.Poison:
                // Apply poison and slow
                statusTarget.ApplyStatus(StatusEffectType.Poisoned, poisonDuration);
                statusTarget.ApplyStatus(StatusEffectType.Chilled, slowDuration);
                break;
                
            case MudType.Acid:
                // Apply damage over time (using poison system)
                statusTarget.ApplyStatus(StatusEffectType.Poisoned, poisonDuration);
                break;
        }
    }

    private void CreateSplash()
    {
        // Create splash effect
        if (splashEffect != null)
        {
            GameObject splash = Instantiate(splashEffect, transform.position, Quaternion.identity);
            
            // Scale splash based on type
            float scale = mudType == MudType.Poison ? 1.2f : 1f;
            splash.transform.localScale = Vector3.one * scale;
        }

        // Play splash sound
        if (splashSound != null)
        {
            AudioSource.PlayClipAtPoint(splashSound, transform.position);
        }

        // Create area effect for poison mud
        if (mudType == MudType.Poison)
        {
            CreatePoisonArea();
        }

        Destroy(gameObject);
    }

    private void CreatePoisonArea()
    {
        // Find all targets in splash radius
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        
        foreach (var target in targets)
        {
            if (target.CompareTag("Player"))
            {
                var statusTarget = target.GetComponent<IStatusEffect>();
                if (statusTarget != null)
                {
                    statusTarget.ApplyStatus(StatusEffectType.Poisoned, poisonDuration * 0.5f); // Reduced duration for area effect
                }
            }
        }
    }

    private void OnBecameInvisible()
    {
        if (!hasHit)
        {
            CreateSplash();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw splash radius
        Gizmos.color = mudType == MudType.Poison ? Color.green : new Color(0.6f, 0.3f, 0f); // Brown color
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
