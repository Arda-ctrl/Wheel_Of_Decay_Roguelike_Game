using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSound;
    
    private Vector2 direction;
    private GameObject owner;
    private Rigidbody2D rb;
    private bool isInitialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
    }

    public void Initialize(Vector2 dir, float spd, float dmg, GameObject ownerObj)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        owner = ownerObj;
        isInitialized = true;

        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

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
            DealDamageToPlayer();
        }

        // Check if it's another enemy (for some special cases)
        if (other.CompareTag("Enemy") && other.gameObject != owner)
        {
            // Could implement friendly fire logic here
        }

        // Hit any solid object
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            OnHit();
        }
    }

    private void DealDamageToPlayer()
    {
        // Apply damage to player
        if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }

        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        OnHit();
    }

    private void OnHit()
    {
        // Spawn hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Destroy projectile
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        // Destroy if it goes off screen
        Destroy(gameObject);
    }
} 