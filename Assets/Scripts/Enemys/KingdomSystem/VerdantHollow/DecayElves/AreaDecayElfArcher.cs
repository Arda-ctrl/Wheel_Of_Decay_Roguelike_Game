using UnityEngine;
using System.Collections;

public class AreaDecayElfArcher : DecayElfArcher
{
    [Header("Area Decay Elf Archer Specific")]
    [SerializeField] private GameObject bigBowPrefab;
    [SerializeField] private GameObject explosiveArrowPrefab;
    [SerializeField] private GameObject smokeBombPrefab;
    [SerializeField] private float explosiveArrowSpeed = 8f;
    [SerializeField] private float explosiveArrowDamage = 20f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float smokeBombRange = 6f;
    [SerializeField] private float smokeBombCooldown = 8f;
    [SerializeField] private float smokeBombDuration = 2f;
    [SerializeField] private float teleportDistance = 5f;
    
    private float lastSmokeBombTime = 0f;
    private bool isInSmoke = false;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Area Decay Elf shoots explosive arrows
        ShootExplosiveArrow();
    }

    private void ShootExplosiveArrow()
    {
        if (arrowSpawnPoint == null)
        {
            arrowSpawnPoint = transform;
        }

        // Calculate direction to player
        Vector2 direction = (playerTransform.position - arrowSpawnPoint.position).normalized;
        
        // Create explosive arrow
        GameObject arrow = Instantiate(explosiveArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        
        // Set arrow properties using ExplosiveArrow component
        var explosiveArrowComponent = arrow.GetComponent<ExplosiveArrow>();
        if (explosiveArrowComponent != null)
        {
            explosiveArrowComponent.Initialize(direction, explosiveArrowSpeed, explosiveArrowDamage, explosionRadius, gameObject);
        }
        else
        {
            // Fallback if no ExplosiveArrow component
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * explosiveArrowSpeed;
            }
        }
        
        Debug.Log($"💥 {enemyData.enemyName} shot an explosive arrow!");
    }

    protected override void PerformDecayElfSpecialAbility()
    {
        // Area Decay Elf special ability - Smoke Bomb Escape
        if (Time.time >= lastSmokeBombTime + smokeBombCooldown)
        {
            StartCoroutine(SmokeBombEscape());
            lastSmokeBombTime = Time.time;
        }
    }

    private IEnumerator SmokeBombEscape()
    {
        Debug.Log($"💨 {enemyData.enemyName} used Smoke Bomb Escape!");
        
        // Create smoke bomb effect
        if (smokeBombPrefab != null)
        {
            GameObject smoke = Instantiate(smokeBombPrefab, transform.position, Quaternion.identity);
            Destroy(smoke, smokeBombDuration);
        }
        
        // Make elf invisible/semi-transparent during smoke
        isInSmoke = true;
        SetElfVisibility(0.3f);
        
        // Stop movement briefly
        StopMoving();
        
        // Wait a moment in smoke
        yield return new WaitForSeconds(0.5f);
        
        // Teleport to a random position away from player
        Vector2 teleportDirection = Random.insideUnitCircle.normalized;
        Vector2 teleportPosition = (Vector2)transform.position + teleportDirection * teleportDistance;
        
        // Ensure teleport position is within reasonable bounds
        teleportPosition = ClampPositionToBounds(teleportPosition);
        
        // Teleport
        transform.position = teleportPosition;
        
        // Wait in smoke a bit longer
        yield return new WaitForSeconds(smokeBombDuration - 0.5f);
        
        // Restore visibility
        isInSmoke = false;
        SetElfVisibility(1f);
        
        Debug.Log($"💨 {enemyData.enemyName} teleported away!");
    }

    private void SetElfVisibility(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private Vector2 ClampPositionToBounds(Vector2 position)
    {
        // Simple bounds checking - you might want to make this more sophisticated
        // based on your level boundaries
        float maxDistance = 20f; // Adjust based on your level size
        Vector2 center = Vector2.zero; // Adjust based on your level center
        
        if (Vector2.Distance(position, center) > maxDistance)
        {
            position = center + (position - center).normalized * maxDistance;
        }
        
        return position;
    }

    protected override void UpdateAI()
    {
        if (isInSmoke) return; // Don't update AI while in smoke
        
        base.UpdateAI();
    }

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"💥 Area Decay Elf Archer spawned! Kingdom: {enemyData.kingdomType}");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw smoke bomb range
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, smokeBombRange);
        
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}