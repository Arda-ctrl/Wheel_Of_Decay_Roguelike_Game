using UnityEngine;
using System.Collections;

public class BigMyceloid : BaseSlimeController
{
    [Header("Big Myceloid Settings")]
    [SerializeField] private GameObject mudProjectilePrefab;
    [SerializeField] private GameObject mediumMyceloidPrefab;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform[] weaponVisualPoints;
    
    [Header("Attack Settings")]
    [SerializeField] private float mudProjectileSpeed = 5f;
    [SerializeField] private float mudDamage = 30f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float mudRange = 10f;
    
    [Header("Jumping Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCooldown = 4f;
    [SerializeField] private float jumpRange = 8f;
    [SerializeField] private bool useJumpMovement = true;
    
    [Header("Splitting Settings")]
    [SerializeField] private int splitCount = 2;
    [SerializeField] private float splitForce = 4f;
    
    [Header("Weapon Explosion")]
    [SerializeField] private int weaponCount = 5;
    [SerializeField] private float explosionRadius = 6f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private float weaponThrowForce = 10f;
    
    private float lastMudAttackTime = 0f;
    private float lastJumpTime = 0f;
    private GameObject[] stuckWeapons;
    private bool isJumping = false;
    
    protected override void Start()
    {
        slimeType = SlimeType.Big;
        
        // Set big slime stats
        if (enemyData != null)
        {
            enemyData.maxHealth = 150f;
            enemyData.baseSpeed = 2f; // Slower base movement
            enemyData.baseDamage = mudDamage;
            enemyData.attackRange = mudRange;
            enemyData.attackCooldown = attackCooldown;
            enemyData.detectionRange = 8f;
        }
        
        // Larger settings for big slime
        attackZoneRadius = 6f;
        roamRadius = 10f;
        roamSpeed = 1.5f; // Slower roaming
        
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
                weapon.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-60f, 60f));
                weapon.transform.localScale = Vector3.one * 1.2f; // Bigger weapons for big slime
                
                // Disable collider (visual only)
                Collider2D weaponCollider = weapon.GetComponent<Collider2D>();
                if (weaponCollider != null)
                {
                    weaponCollider.enabled = false;
                }
                
                stuckWeapons[i] = weapon;
            }
        }
    }

    protected override void HandleRoamingState()
    {
        if (useJumpMovement && Time.time >= lastJumpTime + jumpCooldown)
        {
            PerformJump();
        }
        else
        {
            base.HandleRoamingState();
        }
    }

    private void PerformJump()
    {
        if (rb != null && !isJumping)
        {
            isJumping = true;
            lastJumpTime = Time.time;
            
            Vector2 direction = (roamTarget - (Vector2)transform.position).normalized;
            
            // Add upward component for big jump
            Vector2 jumpDirection = new Vector2(direction.x, 0.8f).normalized;
            
            rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
            
            // Trigger jump animation
            if (animator != null)
            {
                animator.SetTrigger("BigJump");
            }
            
            // Update sprite direction
            bool shouldFaceRight = direction.x > 0;
            if (shouldFaceRight != isFacingRight)
            {
                FlipSprite();
            }
            
            StartCoroutine(JumpCoroutine());
        }
    }

    private IEnumerator JumpCoroutine()
    {
        // Wait for landing
        yield return new WaitForSeconds(0.8f);
        
        // Create landing impact
        CreateJumpImpact();
        
        isJumping = false;
    }

    private void CreateJumpImpact()
    {
        // Create visual effect
        if (explosionEffectPrefab != null)
        {
            GameObject impact = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            impact.transform.localScale = Vector3.one * 0.7f; // Smaller than death explosion
        }
        
        // Apply damage to player if in range
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= 3f) // Jump impact radius
            {
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(20f);
                }
                else if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }
                
                // Apply knockback
                Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDir * 8f, ForceMode2D.Impulse);
                }
            }
        }
    }

    protected override bool CanAttack()
    {
        return !isAttacking && !isJumping && Time.time >= lastMudAttackTime + attackCooldown;
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
        StartCoroutine(BigMudSpitAttack());
    }

    private IEnumerator BigMudSpitAttack()
    {
        // Wait for animation wind-up
        yield return new WaitForSeconds(0.4f);
        
        if (playerTransform != null)
        {
            // Calculate target position with prediction
            Vector2 targetPos = PredictPlayerPosition();
            
            // Spawn larger mud projectile
            Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up * 1f;
            GameObject mudProj = Instantiate(mudProjectilePrefab, spawnPos, Quaternion.identity);
            
            // Scale up the projectile for big slime
            mudProj.transform.localScale = Vector3.one * 1.3f;
            
            MudProjectile mudScript = mudProj.GetComponent<MudProjectile>();
            if (mudScript != null)
            {
                mudScript.Initialize(targetPos, mudProjectileSpeed, mudDamage, gameObject, MudType.Normal);
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
        
        // Predict player movement
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float timeToTarget = Vector2.Distance(transform.position, playerPos) / mudProjectileSpeed;
            playerPos += playerRb.linearVelocity * timeToTarget * 0.6f; // Better prediction for big slime
        }
        
        return playerPos;
    }

    protected override void OnSlimeDeath()
    {
        // Split into medium slimes
        SplitIntoMediumSlimes();
        
        // Create weapon explosion
        CreateWeaponExplosion();
    }

    private void SplitIntoMediumSlimes()
    {
        if (mediumMyceloidPrefab == null) return;
        
        for (int i = 0; i < splitCount; i++)
        {
            // Calculate spawn position around the dying slime
            float angle = (360f / splitCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + (Vector3)direction * 2f;
            
            // Create medium slime
            GameObject mediumSlime = Instantiate(mediumMyceloidPrefab, spawnPos, Quaternion.identity);
            
            // Set that it was spawned from medium (big slime) so it will throw weapons when it dies
            MediumMyceloid mediumScript = mediumSlime.GetComponent<MediumMyceloid>();
            if (mediumScript != null)
            {
                mediumScript.SetSpawnedFromMedium(true);
            }
            
            // Add force to split slimes
            Rigidbody2D mediumRb = mediumSlime.GetComponent<Rigidbody2D>();
            if (mediumRb != null)
            {
                mediumRb.AddForce(direction * splitForce, ForceMode2D.Impulse);
            }
            
            Debug.Log($"Big Myceloid split into medium slime at {spawnPos}");
        }
    }

    private void CreateWeaponExplosion()
    {
        // Create main explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * 1.5f; // Large explosion
        }
        
        // Apply explosion damage to player
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= explosionRadius)
            {
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamage);
                }
                else if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }
                
                // Strong knockback from explosion
                Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDir * 15f, ForceMode2D.Impulse);
                }
            }
        }
        
        // Throw weapons in all directions
        ThrowWeaponsInExplosion();
    }

    private void ThrowWeaponsInExplosion()
    {
        if (stuckWeapons == null) return;
        
        for (int i = 0; i < stuckWeapons.Length; i++)
        {
            if (stuckWeapons[i] != null)
            {
                StartCoroutine(ThrowWeaponInExplosion(i));
            }
        }
    }

    private IEnumerator ThrowWeaponInExplosion(int weaponIndex)
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.3f)); // Quick succession
        
        if (stuckWeapons[weaponIndex] == null) yield break;
        
        // Detach weapon
        stuckWeapons[weaponIndex].transform.SetParent(null);
        
        // Add physics
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
        
        // Add weapon projectile script with higher damage for explosion
        WeaponProjectile weaponProj = stuckWeapons[weaponIndex].AddComponent<WeaponProjectile>();
        weaponProj.Initialize(explosionDamage * 0.5f, 4f); // 4 second lifetime
        
        // Calculate explosion direction - radial outward
        float angle = (360f / stuckWeapons.Length) * weaponIndex;
        Vector2 explosionDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        
        weaponRb.AddForce(explosionDirection * weaponThrowForce * 1.5f, ForceMode2D.Impulse); // Stronger throw in explosion
        weaponRb.AddTorque(Random.Range(-300f, 300f)); // High spin
        
        Debug.Log($"Threw weapon {weaponIndex} in explosion direction {explosionDirection}");
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // Set jumping state
        animator.SetBool("IsJumping", isJumping);
        
        // Set health percentage
        float healthPercent = currentHealth / enemyData.maxHealth;
        animator.SetFloat("HealthPercent", healthPercent);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw mud attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mudRange);
        
        // Draw jump range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpRange);
        
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
