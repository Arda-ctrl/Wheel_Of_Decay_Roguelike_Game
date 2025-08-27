using UnityEngine;
using System.Collections;

public class SlimeProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private GameObject landingEffect;
    [SerializeField] private AudioClip landingSound;
    
    private Vector2 throwDirection;
    private float throwForce;
    private GameObject owner;
    private Rigidbody2D rb;
    private bool hasLanded = false;
    private BaseSlimeController slimeController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        slimeController = GetComponent<BaseSlimeController>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Set physics properties for projectile flight
        rb.gravityScale = 1f; // Normal gravity for arc
        rb.linearDamping = 0.2f; // Some air resistance
    }

    public void Initialize(Vector2 direction, float force, float projectileLifetime, GameObject ownerObject)
    {
        throwDirection = direction.normalized;
        throwForce = force;
        lifetime = projectileLifetime;
        owner = ownerObject;
        
        // Apply initial throw force with arc
        Vector2 velocity = throwDirection * throwForce;
        velocity.y += 3f; // Add upward component for arc trajectory
        
        if (rb != null)
        {
            rb.linearVelocity = velocity;
            rb.AddTorque(Random.Range(-200f, 200f)); // Random spin during flight
        }
        
        // Disable slime controller during flight
        if (slimeController != null)
        {
            slimeController.enabled = false;
        }
        
        // Set up collision detection
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true; // Use trigger for landing detection
        }
        
        // Destroy after lifetime if it doesn't land
        Destroy(gameObject, lifetime);
        
        Debug.Log($"Slime projectile launched with force {throwForce} in direction {throwDirection}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasLanded) return;
        
        // Don't collide with owner
        if (other.gameObject == owner) return;
        
        // Check for landing on ground or hitting player
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            LandSlime();
        }
        else if (other.CompareTag("Player"))
        {
            HitPlayer(other);
        }
    }

    private void LandSlime()
    {
        hasLanded = true;
        
        // Stop physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        
        // Create landing effect
        if (landingEffect != null)
        {
            Instantiate(landingEffect, transform.position, Quaternion.identity);
        }
        
        // Play landing sound
        if (landingSound != null)
        {
            AudioSource.PlayClipAtPoint(landingSound, transform.position);
        }
        
        // Activate slime controller
        if (slimeController != null)
        {
            slimeController.enabled = true;
        }
        
        // Reset collider to normal collision
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
        
        // Slime bounce effect on landing
        StartCoroutine(LandingBounce());
        
        Debug.Log("Slime projectile landed and activated as normal slime");
    }

    private IEnumerator LandingBounce()
    {
        Vector3 originalScale = transform.localScale;
        
        // Squash and stretch effect
        transform.localScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.8f, originalScale.z);
        yield return new WaitForSeconds(0.1f);
        
        transform.localScale = new Vector3(originalScale.x * 0.9f, originalScale.y * 1.1f, originalScale.z);
        yield return new WaitForSeconds(0.1f);
        
        transform.localScale = originalScale;
    }

    private void HitPlayer(Collider2D player)
    {
        hasLanded = true;
        
        // Deal damage to player
        var playerHealth = player.GetComponent<IHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }
        
        // Apply knockback
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockbackDir = throwDirection;
            playerRb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
        }
        
        // Create hit effect
        if (landingEffect != null)
        {
            Instantiate(landingEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log($"Slime projectile hit player for {damage} damage");
        
        // Convert to normal slime after hitting player
        LandSlime();
    }

    private void Update()
    {
        // Rotate the slime during flight for visual effect
        if (!hasLanded && rb != null && rb.linearVelocity.magnitude > 1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnBecameInvisible()
    {
        // If the slime goes off screen during flight, land it anyway
        if (!hasLanded)
        {
            LandSlime();
        }
    }
}
