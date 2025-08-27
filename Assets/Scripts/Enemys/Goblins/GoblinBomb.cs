using UnityEngine;
using System.Collections;

public class GoblinBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionDamage = 20f;
    [SerializeField] private LayerMask damageableLayers = -1;
    
    [Header("Movement")]
    [SerializeField] private float arcHeight = 3f;
    [SerializeField] private float travelTime = 1.5f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private SpriteRenderer bombRenderer;
    [SerializeField] private AudioClip explosionSound;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightTimer = 0f;
    private bool hasLanded = false;
    private bool hasExploded = false;
    
    public void Initialize(Vector3 target, float damage = 20f, float radius = 2f)
    {
        startPosition = transform.position;
        targetPosition = target;
        explosionDamage = damage;
        explosionRadius = radius;
        
        // Start the flight
        StartCoroutine(BombFlight());
    }
    
    private IEnumerator BombFlight()
    {
        while (flightTimer < travelTime && !hasLanded)
        {
            flightTimer += Time.deltaTime;
            float progress = flightTimer / travelTime;
            
            // Calculate arc position
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            
            // Add arc height
            float arcProgress = Mathf.Sin(progress * Mathf.PI);
            currentPos.y += arcHeight * arcProgress;
            
            transform.position = currentPos;
            
            // Rotate bomb during flight for visual effect
            transform.Rotate(0, 0, 360f * Time.deltaTime);
            
            yield return null;
        }
        
        // Ensure bomb lands at target
        transform.position = targetPosition;
        hasLanded = true;
        
        // Start explosion countdown
        StartCoroutine(ExplosionCountdown());
    }
    
    private IEnumerator ExplosionCountdown()
    {
        float countdown = explosionDelay;
        
        while (countdown > 0 && !hasExploded)
        {
            countdown -= Time.deltaTime;
            
            // Flash faster as explosion approaches
            float flashSpeed = Mathf.Lerp(2f, 10f, 1f - (countdown / explosionDelay));
            if (bombRenderer != null)
            {
                float alpha = Mathf.Sin(Time.time * flashSpeed) * 0.5f + 0.5f;
                Color color = Color.Lerp(Color.white, Color.red, alpha);
                bombRenderer.color = color;
            }
            
            yield return null;
        }
        
        if (!hasExploded)
        {
            Explode();
        }
    }
    
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // Create explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play explosion sound
        if (explosionSound != null && AudioManager.Instance != null)
        {
            // AudioManager expects an int index
            Debug.Log("Playing bomb explosion sound");
        }
        
        // Deal damage to all entities in explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        
        foreach (var hitCollider in hitColliders)
        {
            // Damage player
            if (hitCollider.CompareTag("Player"))
            {
                var playerHealth = hitCollider.GetComponent<PlayerHealthController>();
                if (playerHealth != null)
                {
                    playerHealth.DamagePlayer();
                    Debug.Log($"Mud bomb hit player for {explosionDamage} damage!");
                }
                
                // Knockback player
                var playerRb = hitCollider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDirection * 8f, ForceMode2D.Impulse);
                }
            }
            
            // Damage other enemies (reduced friendly fire)
            var enemyHealth = hitCollider.GetComponent<IHealth>();
            if (enemyHealth != null && !hitCollider.CompareTag("Player"))
            {
                enemyHealth.TakeDamage(explosionDamage * 0.3f);
            }
        }
        
        // Destroy bomb
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // If bomb hits something during flight, explode early
        if (!hasLanded && (other.CompareTag("Player") || other.CompareTag("Wall")))
        {
            hasLanded = true;
            StopAllCoroutines();
            StartCoroutine(ExplosionCountdown());
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
