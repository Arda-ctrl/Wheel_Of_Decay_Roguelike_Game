using UnityEngine;
using System.Collections;

public class MudArea : MonoBehaviour
{
    [Header("Mud Area Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private bool appliesSlowEffect = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject mudParticles;
    [SerializeField] private AudioClip bubbleSound;
    [SerializeField] private float bubbleSoundInterval = 2f;
    
    private bool isActive = true;
    private float nextBubbleTime = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Start mud particles if available
        if (mudParticles != null)
        {
            mudParticles.SetActive(true);
        }
        
        // Start the duration countdown
        StartCoroutine(MudAreaLifetime());
        
        Debug.Log($"Mud area created with {duration}s duration, {damagePerSecond} DPS");
    }

    public void Initialize(float areaDuration, float areaDamagePerSecond, bool applySlow = true)
    {
        duration = areaDuration;
        damagePerSecond = areaDamagePerSecond;
        appliesSlowEffect = applySlow;
    }

    private void Update()
    {
        // Play bubble sounds periodically
        if (Time.time >= nextBubbleTime && bubbleSound != null)
        {
            AudioSource.PlayClipAtPoint(bubbleSound, transform.position, 0.3f); // Quieter volume
            nextBubbleTime = Time.time + bubbleSoundInterval;
        }
    }

    private IEnumerator MudAreaLifetime()
    {
        float elapsed = 0f;
        
        while (elapsed < duration && isActive)
        {
            elapsed += Time.deltaTime;
            
            // Fade out the mud area over time
            if (spriteRenderer != null)
            {
                float alpha = Mathf.Lerp(1f, 0.2f, elapsed / duration);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            
            yield return null;
        }
        
        // Deactivate and destroy
        DeactivateMudArea();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            // Start applying mud effects to player
            StartCoroutine(ApplyMudEffectsToPlayer(other));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Remove slow effect when player leaves mud
            if (appliesSlowEffect)
            {
                var moveable = other.GetComponent<IMoveable>();
                if (moveable != null)
                {
                    moveable.SetSpeedMultiplier(1f); // Restore normal speed
                }
            }
        }
    }

    private IEnumerator ApplyMudEffectsToPlayer(Collider2D player)
    {
        var playerHealth = player.GetComponent<IHealth>();
        var playerMoveable = player.GetComponent<IMoveable>();
        
        // Apply slow effect immediately
        if (appliesSlowEffect && playerMoveable != null)
        {
            playerMoveable.SetSpeedMultiplier(slowMultiplier);
        }
        
        // Apply damage over time while player is in mud
        while (isActive)
        {
            // Check if player is still in the mud area
            Collider2D mudCollider = GetComponent<Collider2D>();
            if (mudCollider != null && !mudCollider.bounds.Contains(player.transform.position))
            {
                break; // Player left the mud area
            }
            
            // Apply damage
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
            }
            else if (PlayerHealthController.Instance != null)
            {
                // For simpler health systems, apply damage less frequently
                yield return new WaitForSeconds(1f);
                PlayerHealthController.Instance.DamagePlayer();
            }
            
            yield return null;
        }
        
        // Remove slow effect when done
        if (appliesSlowEffect && playerMoveable != null)
        {
            playerMoveable.SetSpeedMultiplier(1f);
        }
    }

    private void DeactivateMudArea()
    {
        isActive = false;
        
        // Stop particle effects
        if (mudParticles != null)
        {
            mudParticles.SetActive(false);
        }
        
        // Find any players still in the area and remove effects
        Collider2D[] overlapping = Physics2D.OverlapCircleAll(transform.position, 
                                   GetComponent<Collider2D>().bounds.size.x * 0.5f);
        
        foreach (var collider in overlapping)
        {
            if (collider.CompareTag("Player"))
            {
                var moveable = collider.GetComponent<IMoveable>();
                if (moveable != null)
                {
                    moveable.SetSpeedMultiplier(1f); // Restore normal speed
                }
            }
        }
        
        // Destroy the mud area
        Destroy(gameObject, 1f); // Small delay to let effects finish
        
        Debug.Log("Mud area deactivated and cleaned up");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw mud area radius
        Gizmos.color = new Color(0.6f, 0.3f, 0f, 0.3f); // Brown with transparency
        
        Collider2D mudCollider = GetComponent<Collider2D>();
        if (mudCollider != null)
        {
            Gizmos.DrawSphere(transform.position, mudCollider.bounds.size.x * 0.5f);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 2f); // Default radius
        }
    }
}
