using UnityEngine;

public class FireBurst : MonoBehaviour
{
    [Header("Fire Burst Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private GameObject fireVFX;
    [SerializeField] private AudioClip fireSound;
    
    private Vector2 direction;
    private GameObject owner;
    private Rigidbody2D rb;
    private bool hasHit = false;

    public void Initialize(Vector2 dir, float dmg, GameObject ownerObj)
    {
        direction = dir.normalized;
        damage = dmg;
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
        
        // Create visual effect
        if (fireVFX != null)
        {
            Instantiate(fireVFX, transform.position, Quaternion.identity, transform);
        }

        // Play sound
        if (fireSound != null)
        {
            AudioSource.PlayClipAtPoint(fireSound, transform.position);
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
        if (hasHit) return;
        hasHit = true;

        // Apply damage to player
        if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }

        // Apply burn status effect
        var playerStatus = PlayerController.Instance?.GetComponent<IStatusEffect>();
        if (playerStatus != null)
        {
            playerStatus.ApplyStatus(StatusEffectType.Burning, 2f);
        }

        OnHit();
    }

    private void OnHit()
    {
        // Create hit effect
        if (fireVFX != null)
        {
            Instantiate(fireVFX, transform.position, Quaternion.identity);
        }

        // Destroy fire burst
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        // Destroy if it goes off screen
        Destroy(gameObject);
    }
} 