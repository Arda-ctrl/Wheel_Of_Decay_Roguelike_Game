using UnityEngine;

public class ExplosiveArrow : MonoBehaviour
{
    [Header("Explosive Arrow Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    
    private Vector2 direction;
    private GameObject owner;
    private Rigidbody2D rb;
    private bool hasExploded = false;

    public void Initialize(Vector2 dir, float spd, float dmg, float radius, GameObject ownerObj)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        explosionRadius = radius;
        owner = ownerObj;
        
        // Set up rigidbody
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        // Set velocity
        rb.linearVelocity = direction * speed;
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the owner
        if (other.gameObject == owner) return;

        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            Explode();
        }

        // Check if it's another enemy (for some special cases)
        if (other.CompareTag("Enemy") && other.gameObject != owner)
        {
            // Could implement friendly fire logic here
        }

        // Hit any solid object
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Create explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Damage all targets in explosion radius
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Player"))
            {
                // Damage player
                if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }

                // Apply poison status effect
                var playerStatus = PlayerController.Instance?.GetComponent<IStatusEffect>();
                if (playerStatus != null)
                {
                    playerStatus.ApplyStatus(StatusEffectType.Poisoned, 3f);
                }
            }
            else if (target.CompareTag("Enemy") && target.gameObject != owner)
            {
                // Could damage other enemies (friendly fire)
                var enemyHealth = target.GetComponent<IHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage * 0.5f); // Reduced damage to allies
                }
            }
        }

        // Destroy explosive arrow
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        // Destroy if it goes off screen
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
} 