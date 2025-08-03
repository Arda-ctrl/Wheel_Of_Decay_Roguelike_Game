using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [Header("Meteor Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 40f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip meteorSound;
    [SerializeField] private AudioClip explosionSound;
    
    private Vector3 targetPosition;
    private GameObject owner;
    private bool hasExploded = false;

    public void Initialize(Vector3 target, float dmg, GameObject ownerObj)
    {
        targetPosition = target;
        damage = dmg;
        owner = ownerObj;
        
        // Play meteor sound
        if (meteorSound != null)
        {
            AudioSource.PlayClipAtPoint(meteorSound, transform.position);
        }
        
        // Start falling animation
        StartCoroutine(FallToTarget());
    }

    private System.Collections.IEnumerator FallToTarget()
    {
        Vector3 startPosition = transform.position;
        float fallTime = Vector3.Distance(startPosition, targetPosition) / speed;
        float elapsedTime = 0f;

        while (elapsedTime < fallTime)
        {
            // Parabolic fall motion
            float t = elapsedTime / fallTime;
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
            currentPosition.y = startPosition.y - (t * t * 10f); // Parabolic fall
            
            transform.position = currentPosition;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Explode when reaching target
        Explode();
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

                // Apply burn status
                var playerStatus = PlayerController.Instance?.GetComponent<IStatusEffect>();
                if (playerStatus != null)
                {
                    playerStatus.ApplyStatus(StatusEffectType.Burning, 3f);
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

        // Destroy meteor
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
} 