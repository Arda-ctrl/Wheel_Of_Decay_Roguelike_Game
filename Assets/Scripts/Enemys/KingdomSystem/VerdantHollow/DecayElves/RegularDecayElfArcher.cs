using UnityEngine;

public class RegularDecayElfArcher : DecayElfArcher
{
    [Header("Regular Decay Elf Archer Specific")]
    [SerializeField] private GameObject longBowPrefab;
    [SerializeField] private GameObject crystalArrowPrefab;
    [SerializeField] private float dashAttackChance = 0.3f;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Regular Decay Elf specific attack logic
        if (Random.value < dashAttackChance)
        {
            // Dash backward and shoot
            DashBackwardAndShoot();
        }
    }

    private void DashBackwardAndShoot()
    {
        // Start dash
        StartCoroutine(DashAndShootSequence());
    }

    private System.Collections.IEnumerator DashAndShootSequence()
    {
        // First dash backward
        yield return StartCoroutine(PerformDash());
        
        // Then shoot arrow
        ShootArrow();
        
        Debug.Log($"🏹 {enemyData.enemyName} dashed backward and shot an arrow!");
    }

    protected override void ShootArrow()
    {
        if (arrowSpawnPoint == null)
        {
            arrowSpawnPoint = transform;
        }

        // Calculate direction to player
        Vector2 direction = (playerTransform.position - arrowSpawnPoint.position).normalized;
        
        // Create crystal arrow
        GameObject arrow = Instantiate(crystalArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        
        // Set arrow properties
        var arrowComponent = arrow.GetComponent<EnemyProjectile>();
        if (arrowComponent != null)
        {
            arrowComponent.Initialize(direction, arrowSpeed, arrowDamage, gameObject);
        }
        else
        {
            // Fallback if no EnemyProjectile component
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * arrowSpeed;
            }
        }
        
        Debug.Log($"🏹 {enemyData.enemyName} shot a crystal arrow!");
    }

    protected override void PerformDecayElfSpecialAbility()
    {
        // Regular Decay Elf special ability - Rapid Shot
        Debug.Log($"🏹 {enemyData.enemyName} used Rapid Shot!");
        
        // Shoot multiple arrows quickly
        StartCoroutine(RapidShotSequence());
    }

    private System.Collections.IEnumerator RapidShotSequence()
    {
        for (int i = 0; i < 3; i++)
        {
            ShootArrow();
            yield return new WaitForSeconds(0.2f);
        }
    }

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"🏹 Regular Decay Elf Archer spawned! Kingdom: {enemyData.kingdomType}");
    }
} 