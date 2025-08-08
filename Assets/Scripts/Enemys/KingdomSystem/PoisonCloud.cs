using UnityEngine;
using System.Collections;

public class PoisonCloud : MonoBehaviour
{
    [Header("Poison Cloud Settings")]
    [SerializeField] private float tickRate = 1f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private GameObject poisonVFX;
    [SerializeField] private AudioClip poisonSound;
    
    private float duration;
    private float damage;
    private GameObject owner;
    private float lastTickTime = 0f;
    private bool isActive = true;

    public void Initialize(float dur, float dmg, GameObject ownerObj)
    {
        duration = dur;
        damage = dmg;
        owner = ownerObj;
        
        // Start the poison cloud
        StartCoroutine(PoisonCloudLifecycle());
    }

    private IEnumerator PoisonCloudLifecycle()
    {
        // Create visual effect
        if (poisonVFX != null)
        {
            Instantiate(poisonVFX, transform.position, Quaternion.identity, transform);
        }

        // Play sound
        if (poisonSound != null)
        {
            AudioSource.PlayClipAtPoint(poisonSound, transform.position);
        }

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
        float fadeTime = 1f;
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            // Gradually reduce damage
            damage = Mathf.Lerp(damage, 0f, elapsedTime / fadeTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the cloud
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
                // Apply poison damage to player
                if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }

                // Apply poison status effect
                var playerStatus = PlayerController.Instance?.GetComponent<IStatusEffect>();
                if (playerStatus != null)
                {
                    playerStatus.ApplyStatus(StatusEffectType.Poisoned, tickRate * 2f);
                }
            }
            else if (target.CompareTag("Enemy") && target.gameObject != owner)
            {
                // Could apply poison to other enemies (friendly fire)
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
        // Draw poison cloud range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Deactivate()
    {
        isActive = false;
    }
} 