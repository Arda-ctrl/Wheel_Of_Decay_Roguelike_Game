using UnityEngine;
using System.Collections;

public class LavaPool : MonoBehaviour
{
    [Header("Lava Pool Settings")]
    [SerializeField] private float tickRate = 0.5f;
    [SerializeField] private float radius = 4f;
    [SerializeField] private float duration = 8f;
    [SerializeField] private GameObject lavaVFX;
    [SerializeField] private AudioClip lavaSound;
    
    private float damage;
    private GameObject owner;
    private float lastTickTime = 0f;
    private bool isActive = true;

    public void Initialize(float dmg, float dur, GameObject ownerObj)
    {
        damage = dmg;
        duration = dur;
        owner = ownerObj;
        
        // Create visual effect
        if (lavaVFX != null)
        {
            Instantiate(lavaVFX, transform.position, Quaternion.identity, transform);
        }

        // Play sound
        if (lavaSound != null)
        {
            AudioSource.PlayClipAtPoint(lavaSound, transform.position);
        }

        // Start lava pool lifecycle
        StartCoroutine(LavaPoolLifecycle());
    }

    private IEnumerator LavaPoolLifecycle()
    {
        // Active phase
        float elapsedTime = 0f;
        while (elapsedTime < duration && isActive)
        {
            // Check for targets in range
            CheckForTargets();
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fade out phase
        float fadeTime = 2f;
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            // Gradually reduce damage
            damage = Mathf.Lerp(damage, 0f, elapsedTime / fadeTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the lava pool
        Destroy(gameObject);
    }

    private void CheckForTargets()
    {
        if (Time.time < lastTickTime + tickRate) return;

        lastTickTime = Time.time;

        // Find all targets in range
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D target in targets)
        {
            if (target.CompareTag("Player"))
            {
                // Apply lava damage to player
                if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }

                // Apply burn status effect
                var playerStatus = PlayerController.Instance?.GetComponent<IStatusEffect>();
                if (playerStatus != null)
                {
                    playerStatus.ApplyStatus(StatusEffectType.Burning, tickRate * 2f);
                }
            }
            else if (target.CompareTag("Enemy") && target.gameObject != owner)
            {
                // Could damage other enemies (friendly fire)
                var enemyHealth = target.GetComponent<IHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage * tickRate);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw lava pool range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Deactivate()
    {
        isActive = false;
    }
} 