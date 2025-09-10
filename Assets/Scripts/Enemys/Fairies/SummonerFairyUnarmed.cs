using UnityEngine;
using System.Collections;

public class SummonerFairyUnarmed : BaseFairyController
{
    [Header("Summoner Fairy (Unarmed) Settings")]
    [SerializeField] private GameObject brainlessFairyPrefab;
    [SerializeField] private float summonerSize = 1.3f;
    [SerializeField] private float enhancedContactDamage = 20f;
    
    [Header("Death Spawning")]
    [SerializeField] private int fairySpawnCount = 4;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private float spawnForce = 6f;
    [SerializeField] private float spawnDelay = 0.2f;
    
    protected override void Start()
    {
        fairyType = FairyType.SummonerUnarmed;
        
        // Scale up the summoner fairy to be bigger
        transform.localScale = Vector3.one * summonerSize;
        
        // Set summoner fairy stats
        if (enemyData != null)
        {
            enemyData.maxHealth = 60f;
            enemyData.baseSpeed = flySpeed;
            enemyData.baseDamage = enhancedContactDamage;
            enemyData.detectionRange = 0f; // No detection needed, just flies around
        }
        
        contactDamage = enhancedContactDamage;
        flySpeed = 5f; // Slightly slower than brainless fairies
        canBounceOffWalls = true;
        
        base.Start();
        
        Debug.Log("Summoner Forest Fairy (Unarmed) spawned - larger and more dangerous");
    }

    protected override void HandleFlying()
    {
        base.HandleFlying();
        
        // Summoner fairy has more erratic flight pattern
        if (directionTimer >= directionChangeInterval * 0.7f) // Change direction more frequently
        {
            if (Random.value < 0.3f) // 30% chance to change direction early
            {
                ChangeFlightDirection();
                directionTimer = 0f;
            }
        }
    }

    protected override void ChangeFlightDirection()
    {
        // Summoner fairy has more varied movement patterns
        float angle = Random.Range(0f, 360f);
        currentFlyDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        
        // Add slight bias towards diagonal movement (but not as strict as brainless)
        if (Random.value < 0.6f) // 60% chance for diagonal-ish movement
        {
            // Adjust angle to be closer to diagonal
            float[] diagonalAngles = { 45f, 135f, 225f, 315f };
            float closestDiagonal = diagonalAngles[0];
            float minDifference = Mathf.Abs(Mathf.DeltaAngle(angle, closestDiagonal));
            
            foreach (float diagonalAngle in diagonalAngles)
            {
                float difference = Mathf.Abs(Mathf.DeltaAngle(angle, diagonalAngle));
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestDiagonal = diagonalAngle;
                }
            }
            
            // Blend towards the closest diagonal
            angle = Mathf.LerpAngle(angle, closestDiagonal, 0.7f);
            currentFlyDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        }
        
        Debug.Log($"Summoner Fairy (Unarmed) changed direction to {currentFlyDirection}");
    }

    protected override void DealContactDamage(Collider2D player)
    {
        base.DealContactDamage(player);
        
        // Summoner fairy applies stronger knockback
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse); // Stronger knockback
        }
    }

    protected override void OnFairyDeath()
    {
        Debug.Log("Summoner Forest Fairy (Unarmed) died - spawning brainless fairies");
        
        // Spawn 4 brainless fairies
        StartCoroutine(SpawnBrainlessFairies());
        
        // Create death effect
        if (enemyData.deathEffect != null)
        {
            GameObject effect = Instantiate(enemyData.deathEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 1.2f; // Bigger effect for bigger fairy
        }
        
        // Play death sound
        if (enemyData.deathSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyData.deathSound, transform.position);
        }
    }

    private IEnumerator SpawnBrainlessFairies()
    {
        if (brainlessFairyPrefab == null)
        {
            Debug.LogWarning("Summoner Fairy: No brainless fairy prefab assigned!");
            yield break;
        }
        
        for (int i = 0; i < fairySpawnCount; i++)
        {
            // Calculate spawn position around the dying fairy
            float angle = (360f / fairySpawnCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + (Vector3)direction * spawnRadius;
            
            // Create brainless fairy
            GameObject spawnedFairy = Instantiate(brainlessFairyPrefab, spawnPos, Quaternion.identity);
            
            // Give the spawned fairy some initial velocity
            Rigidbody2D fairyRb = spawnedFairy.GetComponent<Rigidbody2D>();
            if (fairyRb != null)
            {
                Vector2 spawnVelocity = direction * spawnForce;
                fairyRb.AddForce(spawnVelocity, ForceMode2D.Impulse);
            }
            
            // Set the fairy to start with the spawn direction
            BrainlessFairy fairyScript = spawnedFairy.GetComponent<BrainlessFairy>();
            if (fairyScript != null)
            {
                // The fairy will set its own direction in Start()
            }
            
            Debug.Log($"Spawned brainless fairy {i + 1}/{fairySpawnCount} at {spawnPos}");
            
            // Small delay between spawns for visual effect
            yield return new WaitForSeconds(spawnDelay);
        }
        
        Debug.Log($"Summoner Fairy spawned all {fairySpawnCount} brainless fairies");
    }

    protected override void BounceOffWall(Vector2 wallNormal)
    {
        base.BounceOffWall(wallNormal);
        
        // Summoner fairy has more dramatic wall bounces
        if (rb != null)
        {
            rb.AddForce(currentFlyDirection * wallBounceForce * 1.5f, ForceMode2D.Impulse);
        }
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // Summoner fairy (unarmed) only has Idle and Death animations
        // Movement is handled by physics, animation stays in Idle
        // Same as brainless fairy but bigger
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw spawn radius for death fairies
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw spawn positions
        for (int i = 0; i < fairySpawnCount; i++)
        {
            float angle = (360f / fairySpawnCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + (Vector3)direction * spawnRadius;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPos, 0.3f);
            Gizmos.DrawLine(transform.position, spawnPos);
        }
    }
}
