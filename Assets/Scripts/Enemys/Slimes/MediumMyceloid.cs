using UnityEngine;
using System.Collections;

public class MediumMyceloid : BaseSlimeController
{
    [Header("Medium Myceloid Settings")]
    [SerializeField] private GameObject mudProjectilePrefab;
    [SerializeField] private GameObject smallMyceloidPrefab;
    [SerializeField] private GameObject weaponPrefab; // Weapons stuck in the slime
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform[] weaponVisualPoints; // Points where weapons appear stuck
    
    [Header("Attack Settings")]
    [SerializeField] private float mudProjectileSpeed = 6f;
    [SerializeField] private float mudDamage = 20f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float mudRange = 8f;
    
    [Header("Splitting Settings")]
    [SerializeField] private int splitCount = 2;
    [SerializeField] private float splitForce = 3f;
    [SerializeField] private bool wasSpawnedFromMedium = false; // Track if this came from a big slime
    
    [Header("Weapon Throwing")]
    [SerializeField] private int weaponCount = 3;
    [SerializeField] private float weaponThrowForce = 8f;
    [SerializeField] private float weaponThrowRadius = 5f;
    [SerializeField] private float weaponDamage = 15f;
    
    private float lastMudAttackTime = 0f;
    private GameObject[] stuckWeapons;
    
    protected override void Start()
    {
        slimeType = SlimeType.Medium;
        
        // Set medium slime stats
        if (enemyData != null)
        {
            enemyData.maxHealth = 80f;
            enemyData.baseSpeed = 3f;
            enemyData.baseDamage = mudDamage;
            enemyData.attackRange = mudRange;
            enemyData.attackCooldown = attackCooldown;
            enemyData.detectionRange = 6f;
        }
        
        base.Start();
        
        // Initialize stuck weapons visuals
        InitializeWeaponVisuals();
    }

    private void InitializeWeaponVisuals()
    {
        if (weaponPrefab == null || weaponVisualPoints == null) return;
        
        stuckWeapons = new GameObject[weaponVisualPoints.Length];
        
        for (int i = 0; i < weaponVisualPoints.Length && i < weaponCount; i++)
        {
            if (weaponVisualPoints[i] != null)
            {
                GameObject weapon = Instantiate(weaponPrefab, weaponVisualPoints[i]);
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
                
                // Disable collider if it has one (these are just visuals)
                Collider2D weaponCollider = weapon.GetComponent<Collider2D>();
                if (weaponCollider != null)
                {
                    weaponCollider.enabled = false;
                }
                
                stuckWeapons[i] = weapon;
            }
        }
    }

    protected override bool CanAttack()
    {
        return !isAttacking && Time.time >= lastMudAttackTime + attackCooldown;
    }

    protected override void PerformSlimeAttack()
    {
        if (playerTransform == null || mudProjectilePrefab == null) return;
        
        lastMudAttackTime = Time.time;
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("SpitMud");
        }
        
        // Start attack coroutine
        StartCoroutine(MudSpitAttack());
    }

    private IEnumerator MudSpitAttack()
    {
        // Wait for animation wind-up
        yield return new WaitForSeconds(0.3f);
        
        if (playerTransform != null)
        {
            // Calculate target position with prediction
            Vector2 targetPos = PredictPlayerPosition();
            
            // Spawn mud projectile
            if (projectileSpawnPoint != null)
            {
                GameObject mudProj = Instantiate(mudProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                MudProjectile mudScript = mudProj.GetComponent<MudProjectile>();
                
                if (mudScript != null)
                {
                    mudScript.Initialize(targetPos, mudProjectileSpeed, mudDamage, gameObject, MudType.Normal);
                }
            }
            else
            {
                // Fallback to slime position if no spawn point set
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                GameObject mudProj = Instantiate(mudProjectilePrefab, spawnPos, Quaternion.identity);
                MudProjectile mudScript = mudProj.GetComponent<MudProjectile>();
                
                if (mudScript != null)
                {
                    mudScript.Initialize(targetPos, mudProjectileSpeed, mudDamage, gameObject, MudType.Normal);
                }
            }
            
            // Play attack sound
            if (enemyData.attackSound != null)
            {
                AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
            }
        }
    }

    private Vector2 PredictPlayerPosition()
    {
        if (playerTransform == null) return Vector2.zero;
        
        Vector2 playerPos = playerTransform.position;
        
        // Simple prediction based on player movement
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float timeToTarget = Vector2.Distance(transform.position, playerPos) / mudProjectileSpeed;
            playerPos += playerRb.linearVelocity * timeToTarget * 0.5f; // Predict 50% of movement
        }
        
        return playerPos;
    }

    protected override void OnSlimeDeath()
    {
        // Split into smaller slimes
        SplitIntoSmallSlimes();
        
        // Throw weapons if this slime was spawned from a medium (meaning it came from a Big slime)
        if (wasSpawnedFromMedium)
        {
            ThrowStuckWeapons();
        }
    }

    private void SplitIntoSmallSlimes()
    {
        if (smallMyceloidPrefab == null) return;
        
        for (int i = 0; i < splitCount; i++)
        {
            // Calculate spawn position around the dying slime
            float angle = (360f / splitCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + (Vector3)direction * 1.5f;
            
            // Create small slime
            GameObject smallSlime = Instantiate(smallMyceloidPrefab, spawnPos, Quaternion.identity);
            
            // Add force to split slimes
            Rigidbody2D smallRb = smallSlime.GetComponent<Rigidbody2D>();
            if (smallRb != null)
            {
                smallRb.AddForce(direction * splitForce, ForceMode2D.Impulse);
            }
            
            Debug.Log($"Medium Myceloid split into small slime at {spawnPos}");
        }
    }

    private void ThrowStuckWeapons()
    {
        if (stuckWeapons == null) return;
        
        for (int i = 0; i < stuckWeapons.Length; i++)
        {
            if (stuckWeapons[i] != null)
            {
                // Create throwable weapon projectile
                StartCoroutine(ThrowWeapon(i));
            }
        }
    }

    private IEnumerator ThrowWeapon(int weaponIndex)
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f)); // Stagger weapon throws
        
        if (stuckWeapons[weaponIndex] == null) yield break;
        
        // Detach weapon from slime
        stuckWeapons[weaponIndex].transform.SetParent(null);
        
        // Add physics components for throwing
        Rigidbody2D weaponRb = stuckWeapons[weaponIndex].GetComponent<Rigidbody2D>();
        if (weaponRb == null)
        {
            weaponRb = stuckWeapons[weaponIndex].AddComponent<Rigidbody2D>();
        }
        
        Collider2D weaponCollider = stuckWeapons[weaponIndex].GetComponent<Collider2D>();
        if (weaponCollider == null)
        {
            weaponCollider = stuckWeapons[weaponIndex].AddComponent<CircleCollider2D>();
            weaponCollider.isTrigger = true;
        }
        else
        {
            weaponCollider.enabled = true;
            weaponCollider.isTrigger = true;
        }
        
        // Add weapon projectile script
        WeaponProjectile weaponProj = stuckWeapons[weaponIndex].AddComponent<WeaponProjectile>();
        weaponProj.Initialize(weaponDamage, 3f); // 3 second lifetime
        
        // Calculate random throw direction
        Vector2 throwDirection = Random.insideUnitCircle.normalized;
        weaponRb.AddForce(throwDirection * weaponThrowForce, ForceMode2D.Impulse);
        weaponRb.AddTorque(Random.Range(-200f, 200f)); // Random spin
        
        Debug.Log($"Threw weapon {weaponIndex} in direction {throwDirection}");
    }

    public void SetSpawnedFromMedium(bool spawnedFromMedium)
    {
        wasSpawnedFromMedium = spawnedFromMedium;
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // Set health percentage for potential health-based animations
        float healthPercent = currentHealth / enemyData.maxHealth;
        animator.SetFloat("HealthPercent", healthPercent);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw mud attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mudRange);
        
        // Draw weapon throw radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, weaponThrowRadius);
    }
}
