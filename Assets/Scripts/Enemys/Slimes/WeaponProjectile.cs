using UnityEngine;
using System.Collections;

public class WeaponProjectile : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSound;
    
    private bool hasHit = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float weaponDamage, float weaponLifetime)
    {
        damage = weaponDamage;
        lifetime = weaponLifetime;
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Check if it hits the player
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other);
        }
        
        // Stop when hitting ground or walls
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            StickToSurface();
        }
    }

    private void DealDamageToPlayer(Collider2D player)
    {
        hasHit = true;
        
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

        // Create hit effect
        CreateHitEffect();
        
        // Destroy the weapon
        Destroy(gameObject);
    }

    private void StickToSurface()
    {
        hasHit = true;
        
        // Stop physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        
        // Weapon remains on ground for a while before disappearing
        StartCoroutine(FadeOutWeapon());
    }

    private IEnumerator FadeOutWeapon()
    {
        yield return new WaitForSeconds(lifetime * 0.5f);
        
        // Fade out effect
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float fadeTime = 1f;
            float elapsed = 0f;
            Color originalColor = sr.color;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
        
        Destroy(gameObject);
    }

    private void CreateHitEffect()
    {
        // Create hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}
