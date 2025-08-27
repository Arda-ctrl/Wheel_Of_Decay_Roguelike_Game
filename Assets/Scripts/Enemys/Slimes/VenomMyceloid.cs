using UnityEngine;
using System.Collections;

public class VenomMyceloid : BaseSlimeController
{
    [Header("Venom Myceloid Settings")]
    [SerializeField] private GameObject mudProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Material venomFormMaterial; // Material for venom form visual
    [SerializeField] private GameObject poisonCloudPrefab;
    
    [Header("Normal Form Attack Settings")]
    [SerializeField] private float poisonProjectileSpeed = 7f;
    [SerializeField] private float poisonDamage = 25f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float poisonRange = 9f;
    
    [Header("Venom Form Settings")]
    [SerializeField] private float venomFormSpeed = 6f;
    [SerializeField] private float venomFormDamage = 50f;
    [SerializeField] private float venomFormRange = 3f;
    [SerializeField] private float venomFormSize = 1.3f;
    [SerializeField] private Color venomFormColor = new Color(0.5f, 0f, 1f); // Purple
    
    [Header("Transformation Settings")]
    [SerializeField] private float transformationHealthThreshold = 0.5f;
    [SerializeField] private float transformationTime = 2f;
    [SerializeField] private GameObject transformationEffect;
    
    private bool isInVenomForm = false;
    private bool hasTransformed = false;
    private float lastPoisonAttackTime = 0f;
    private Vector3 originalScale;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private bool isTransforming = false;
    
    protected override void Start()
    {
        slimeType = SlimeType.VenomNormal;
        
        // Set venom slime stats
        if (enemyData != null)
        {
            enemyData.maxHealth = 100f;
            enemyData.baseSpeed = 3.5f;
            enemyData.baseDamage = poisonDamage;
            enemyData.attackRange = poisonRange;
            enemyData.attackCooldown = attackCooldown;
            enemyData.detectionRange = 7f;
        }
        
        // Store original appearance
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        
        // Check for transformation trigger
        if (!hasTransformed && !isTransforming)
        {
            float healthPercent = currentHealth / enemyData.maxHealth;
            if (healthPercent <= transformationHealthThreshold)
            {
                StartTransformation();
            }
        }
    }

    private void StartTransformation()
    {
        if (isTransforming) return;
        
        isTransforming = true;
        hasTransformed = true;
        
        Debug.Log("Venom Myceloid starting transformation to venom form!");
        
        StartCoroutine(TransformationSequence());
    }

    private IEnumerator TransformationSequence()
    {
        // Stop all movement during transformation
        StopMoving();
        ChangeSlimeState(SlimeState.Idle);
        
        // Play transformation animation
        if (animator != null)
        {
            animator.SetTrigger("Transform");
            animator.SetBool("IsTransforming", true);
        }
        
        // Create transformation effect
        if (transformationEffect != null)
        {
            Instantiate(transformationEffect, transform.position, Quaternion.identity);
        }
        
        // Gradual transformation over time
        float elapsed = 0f;
        while (elapsed < transformationTime)
        {
            float progress = elapsed / transformationTime;
            
            // Scale up
            transform.localScale = Vector3.Lerp(originalScale, originalScale * venomFormSize, progress);
            
            // Change color to purple
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, venomFormColor, progress);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Complete transformation
        CompleteTransformation();
        
        isTransforming = false;
        
        if (animator != null)
        {
            animator.SetBool("IsTransforming", false);
        }
    }

    private void CompleteTransformation()
    {
        isInVenomForm = true;
        slimeType = SlimeType.VenomForm;
        
        // Update stats for venom form
        if (enemyData != null)
        {
            enemyData.baseSpeed = venomFormSpeed;
            enemyData.baseDamage = venomFormDamage;
            enemyData.attackRange = venomFormRange;
            enemyData.attackCooldown = 1f; // Faster attacks in venom form
        }
        
        // Update roaming behavior for aggressive pursuit
        attackZoneRadius = 8f; // Larger attack zone
        roamSpeed = venomFormSpeed * 0.7f;
        
        Debug.Log("Venom Myceloid transformation complete! Now in venom form.");
    }

    protected override void HandleAttackingState()
    {
        if (isInVenomForm)
        {
            HandleVenomFormAttacking();
        }
        else
        {
            HandleNormalFormAttacking();
        }
    }

    private void HandleNormalFormAttacking()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Face the player
        bool shouldFaceRight = playerTransform.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            FlipSprite();
        }
        
        // Stop moving and use ranged attack
        StopMoving();
        
        if (CanAttack() && distanceToPlayer <= poisonRange)
        {
            PerformSlimeAttack();
        }
    }

    private void HandleVenomFormAttacking()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Face the player
        bool shouldFaceRight = playerTransform.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            FlipSprite();
        }
        
        // Aggressively chase player in venom form
        if (distanceToPlayer > venomFormRange)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * venomFormSpeed;
        }
        else
        {
            StopMoving();
            
            if (CanAttack())
            {
                PerformVenomFormAttack();
            }
        }
    }

    protected override bool CanAttack()
    {
        return !isAttacking && !isTransforming && Time.time >= lastPoisonAttackTime + enemyData.attackCooldown;
    }

    protected override void PerformSlimeAttack()
    {
        if (isInVenomForm)
        {
            PerformVenomFormAttack();
        }
        else
        {
            PerformPoisonSpitAttack();
        }
    }

    private void PerformPoisonSpitAttack()
    {
        if (playerTransform == null || mudProjectilePrefab == null) return;
        
        lastPoisonAttackTime = Time.time;
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("SpitPoison");
        }
        
        StartCoroutine(PoisonSpitCoroutine());
    }

    private IEnumerator PoisonSpitCoroutine()
    {
        // Wait for animation wind-up
        yield return new WaitForSeconds(0.3f);
        
        if (playerTransform != null)
        {
            Vector2 targetPos = PredictPlayerPosition();
            
            // Spawn poison projectile
            Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up * 0.5f;
            GameObject poisonProj = Instantiate(mudProjectilePrefab, spawnPos, Quaternion.identity);
            
            // Give it a purple tint for poison
            SpriteRenderer projRenderer = poisonProj.GetComponent<SpriteRenderer>();
            if (projRenderer != null)
            {
                projRenderer.color = new Color(0.8f, 0.3f, 1f); // Purple tint
            }
            
            MudProjectile mudScript = poisonProj.GetComponent<MudProjectile>();
            if (mudScript != null)
            {
                mudScript.Initialize(targetPos, poisonProjectileSpeed, poisonDamage, gameObject, MudType.Poison);
            }
            
            // Play attack sound
            if (enemyData.attackSound != null)
            {
                AudioSource.PlayClipAtPoint(enemyData.attackSound, transform.position);
            }
        }
    }

    private void PerformVenomFormAttack()
    {
        if (playerTransform == null) return;
        
        lastPoisonAttackTime = Time.time;
        
        // Trigger venom form attack animation
        if (animator != null)
        {
            animator.SetTrigger("VenomFormAttack");
        }
        
        StartCoroutine(VenomFormAttackCoroutine());
    }

    private IEnumerator VenomFormAttackCoroutine()
    {
        // Wait for animation wind-up
        yield return new WaitForSeconds(0.2f);
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= venomFormRange)
            {
                // Deal massive damage
                var playerHealth = playerTransform.GetComponent<IHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(venomFormDamage);
                }
                else if (PlayerHealthController.Instance != null)
                {
                    PlayerHealthController.Instance.DamagePlayer();
                }
                
                // Apply poison status
                var statusTarget = playerTransform.GetComponent<IStatusEffect>();
                if (statusTarget != null)
                {
                    statusTarget.ApplyStatus(StatusEffectType.Poisoned, 8f); // Long poison duration
                }
                
                // Create poison cloud effect
                if (poisonCloudPrefab != null)
                {
                    Instantiate(poisonCloudPrefab, transform.position, Quaternion.identity);
                }
                
                // Strong knockback
                Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDir * 12f, ForceMode2D.Impulse);
                }
                
                Debug.Log($"Venom Form dealt {venomFormDamage} massive damage to player!");
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
            float timeToTarget = Vector2.Distance(transform.position, playerPos) / poisonProjectileSpeed;
            playerPos += playerRb.linearVelocity * timeToTarget * 0.4f;
        }
        
        return playerPos;
    }

    protected override void OnSlimeDeath()
    {
        // Create poison cloud on death
        if (poisonCloudPrefab != null)
        {
            GameObject deathCloud = Instantiate(poisonCloudPrefab, transform.position, Quaternion.identity);
            
            // Larger cloud if died in venom form
            if (isInVenomForm)
            {
                deathCloud.transform.localScale = Vector3.one * 1.5f;
            }
        }
        
        Debug.Log($"Venom Myceloid died in {(isInVenomForm ? "venom" : "normal")} form");
    }

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // Set venom form state
        animator.SetBool("IsInVenomForm", isInVenomForm);
        animator.SetBool("HasTransformed", hasTransformed);
        
        // Set health percentage for transformation trigger
        float healthPercent = currentHealth / enemyData.maxHealth;
        animator.SetFloat("HealthPercent", healthPercent);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (isInVenomForm)
        {
            // Draw venom form attack range
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, venomFormRange);
        }
        else
        {
            // Draw poison spit range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, poisonRange);
        }
        
        // Draw transformation threshold indicator
        if (!hasTransformed)
        {
            Gizmos.color = Color.yellow;
            Vector3 pos = transform.position + Vector3.up * 2f;
            Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
        }
    }
}
