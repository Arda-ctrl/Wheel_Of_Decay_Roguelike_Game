using UnityEngine;

public class RapidDecayElfArcher : DecayElfArcher
{
    [Header("Rapid Decay Elf Archer Specific")]
    [SerializeField] private GameObject shortBowPrefab;
    [SerializeField] private GameObject smallArrowPrefab;
    [SerializeField] private float rapidFireRate = 0.1f;
    [SerializeField] private int rapidFireCount = 5;
    [SerializeField] private float movingAttackChance = 0.6f;
    [SerializeField] private bool canShootWhileMoving = true;

    private bool isRapidFiring = false;
    private bool isMovingAndShooting = false;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Rapid Decay Elf specific attack logic
        if (Random.value < movingAttackChance && canShootWhileMoving)
        {
            // Shoot while moving
            StartCoroutine(MovingAttackSequence());
        }
        else
        {
            // Rapid fire from standing position
            StartCoroutine(RapidFireSequence());
        }
    }

    private System.Collections.IEnumerator RapidFireSequence()
    {
        if (isRapidFiring) yield break;
        
        isRapidFiring = true;
        
        for (int i = 0; i < rapidFireCount; i++)
        {
            ShootRapidArrow();
            yield return new WaitForSeconds(rapidFireRate);
        }
        
        isRapidFiring = false;
    }

    private System.Collections.IEnumerator MovingAttackSequence()
    {
        if (isMovingAndShooting) yield break;
        
        isMovingAndShooting = true;
        
        // Move in a pattern while shooting
        Vector2[] moveDirections = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        
        for (int i = 0; i < 4; i++)
        {
            // Move in direction
            if (rb != null)
            {
                rb.linearVelocity = moveDirections[i] * GetCurrentSpeed();
            }
            
            // Shoot while moving
            ShootRapidArrow();
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        isMovingAndShooting = false;
    }

    private void ShootRapidArrow()
    {
        if (arrowSpawnPoint == null)
        {
            arrowSpawnPoint = transform;
        }

        // Calculate direction to player
        Vector2 direction = (playerTransform.position - arrowSpawnPoint.position).normalized;
        
        // Create small arrow
        GameObject arrow = Instantiate(smallArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        
        // Set arrow properties
        var arrowComponent = arrow.GetComponent<EnemyProjectile>();
        if (arrowComponent != null)
        {
            arrowComponent.Initialize(direction, arrowSpeed * 1.2f, arrowDamage * 0.8f, gameObject);
        }
        else
        {
            // Fallback if no EnemyProjectile component
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * arrowSpeed * 1.2f;
            }
        }
        
        Debug.Log($"🏹 {enemyData.enemyName} shot a rapid arrow!");
    }

    protected override void UpdateAI()
    {
        if (isRapidFiring || isMovingAndShooting) return; // Don't update AI while in special attack mode
        
        base.UpdateAI();
    }

    protected override void PerformDecayElfSpecialAbility()
    {
        // Rapid Decay Elf special ability - Barrage
        Debug.Log($"🏹 {enemyData.enemyName} used Barrage!");
        
        // Shoot a barrage of arrows in all directions
        StartCoroutine(BarrageSequence());
    }

    private System.Collections.IEnumerator BarrageSequence()
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            
            // Create small arrow
            GameObject arrow = Instantiate(smallArrowPrefab, transform.position, Quaternion.identity);
            
            var arrowComponent = arrow.GetComponent<EnemyProjectile>();
            if (arrowComponent != null)
            {
                arrowComponent.Initialize(direction, arrowSpeed * 1.5f, arrowDamage * 0.6f, gameObject);
            }
            
            yield return new WaitForSeconds(0.05f);
        }
    }

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"🏹 Rapid Decay Elf Archer spawned! Kingdom: {enemyData.kingdomType}");
    }
} 