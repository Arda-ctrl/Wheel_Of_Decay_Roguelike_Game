using UnityEngine;

public class AreaDecayElfArcher : DecayElfArcher
{
    [Header("Area Decay Elf Archer Specific")]
    [SerializeField] private GameObject explosiveArrowPrefab;
    [SerializeField] private GameObject smokeBombPrefab;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float smokeBombChance = 0.4f;
    [SerializeField] private float teleportDistance = 5f;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Area Decay Elf specific attack logic
        if (Random.value < smokeBombChance)
        {
            // Use smoke bomb and teleport
            UseSmokeBombAndTeleport();
        }
        else
        {
            // Shoot explosive arrow
            ShootExplosiveArrow();
        }
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
        
        // Set arrow properties
        var arrowComponent = arrow.GetComponent<ExplosiveArrow>();
        if (arrowComponent != null)
        {
            arrowComponent.Initialize(direction, arrowSpeed, arrowDamage, explosionRadius, gameObject);
        }
        else
        {
            // Fallback if no ExplosiveArrow component
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * arrowSpeed;
            }
        }
        
        Debug.Log($"🏹 {enemyData.enemyName} shot an explosive arrow!");
    }

    private void UseSmokeBombAndTeleport()
    {
        StartCoroutine(SmokeBombAndTeleportSequence());
    }

    private System.Collections.IEnumerator SmokeBombAndTeleportSequence()
    {
        // Create smoke bomb
        if (smokeBombPrefab != null)
        {
            GameObject smokeBomb = Instantiate(smokeBombPrefab, transform.position, Quaternion.identity);
            Destroy(smokeBomb, 3f);
        }

        Debug.Log($"🏹 {enemyData.enemyName} used smoke bomb!");

        // Teleport to new location
        yield return new WaitForSeconds(0.5f);
        
        Vector3 teleportPosition = GetRandomTeleportPosition();
        transform.position = teleportPosition;
        
        Debug.Log($"🏹 {enemyData.enemyName} teleported to new position!");
    }

    private Vector3 GetRandomTeleportPosition()
    {
        // Get random position around the player
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 teleportPosition = playerTransform.position + (Vector3)(randomDirection * teleportDistance);
        
        // Ensure position is within bounds (you might want to implement proper bounds checking)
        teleportPosition.x = Mathf.Clamp(teleportPosition.x, -20f, 20f);
        teleportPosition.y = Mathf.Clamp(teleportPosition.y, -20f, 20f);
        
        return teleportPosition;
    }

    protected override void PerformDecayElfSpecialAbility()
    {
        // Area Decay Elf special ability - Multi-Explosion
        Debug.Log($"🏹 {enemyData.enemyName} used Multi-Explosion!");
        
        // Shoot multiple explosive arrows in different directions
        StartCoroutine(MultiExplosionSequence());
    }

    private System.Collections.IEnumerator MultiExplosionSequence()
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            
            // Create explosive arrow
            GameObject arrow = Instantiate(explosiveArrowPrefab, transform.position, Quaternion.identity);
            
            var arrowComponent = arrow.GetComponent<ExplosiveArrow>();
            if (arrowComponent != null)
            {
                arrowComponent.Initialize(direction, arrowSpeed * 0.8f, arrowDamage * 0.7f, explosionRadius, gameObject);
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"🏹 Area Decay Elf Archer spawned! Kingdom: {enemyData.kingdomType}");
    }
} 