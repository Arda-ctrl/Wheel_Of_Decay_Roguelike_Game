using UnityEngine;
using System.Collections;

public class ShadowDualDaggerDecayElf : VerdantEnemy
{
    [Header("Shadow Dual Dagger Decay Elf Specific")]
    [SerializeField] private GameObject shadowDaggerPrefab;
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private GameObject shadowEffectPrefab;
    [SerializeField] private float daggerDamage = 14f;
    [SerializeField] private float daggerRange = 1.5f;
    [SerializeField] private float backAttackRange = 2f;
    [SerializeField] private float backAttackDamage = 25f;
    [SerializeField] private float shurikenChance = 0.3f; // 30% chance
    [SerializeField] private float shurikenCooldown = 5f;
    [SerializeField] private float shurikenSpeed = 12f;
    [SerializeField] private float shurikenDamage = 10f;
    [SerializeField] private float shadowTeleportCooldown = 6f;
    [SerializeField] private float shadowTeleportRange = 8f;
    [SerializeField] private float shadowDuration = 4f;
    [SerializeField] private float shadowInvisibility = 0.1f;
    [SerializeField] private float shadowAppearDelay = 1f;
    
    private float lastShurikenTime = 0f;
    private float lastShadowTeleportTime = 0f;
    private bool isInShadow = false;
    private bool isAttackingFromBehind = false;
    private bool isShadowTeleporting = false;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Check if we can attack from behind
        if (IsBehindPlayer() && distanceToPlayer <= backAttackRange)
        {
            PerformShadowBackAttack();
        }
        else if (distanceToPlayer <= daggerRange)
        {
            PerformShadowDaggerAttack();
        }
        else if (Time.time >= lastShurikenTime + shurikenCooldown && Random.value < shurikenChance)
        {
            PerformShadowShurikenThrow();
        }
        else if (Time.time >= lastShadowTeleportTime + shadowTeleportCooldown)
        {
            // Use shadow teleport to get behind player
            StartCoroutine(ShadowTeleportBehindPlayer());
        }
        else
        {
            // Move closer for dagger attack
            MoveTowardsPlayer();
        }
    }

    private bool IsBehindPlayer()
    {
        if (playerTransform == null) return false;
        
        // Calculate if elf is behind player
        Vector2 playerForward = playerTransform.right; // Assuming player faces right by default
        Vector2 toElf = (transform.position - playerTransform.position).normalized;
        
        // If dot product is negative, elf is behind player
        return Vector2.Dot(playerForward, toElf) < 0;
    }

    private void PerformShadowBackAttack()
    {
        if (isAttackingFromBehind) return;
        
        StartCoroutine(ShadowBackAttackSequence());
    }

    private IEnumerator ShadowBackAttackSequence()
    {
        isAttackingFromBehind = true;
        
        Debug.Log($"🌑 {enemyData.enemyName} performs shadow back attack!");
        
        // Create shadow effect
        if (shadowEffectPrefab != null)
        {
            GameObject shadow = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity);
            Destroy(shadow, 1f);
        }
        
        // Stop movement
        StopMoving();
        
        // Deal back attack damage
        if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }
        
        // Apply poison status effect (black liquid on daggers)
        if (PlayerController.Instance != null)
        {
            var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
            if (playerStatus != null)
            {
                playerStatus.ApplyStatus(StatusEffectType.Poisoned, 4f);
                Debug.Log($"☠️ {enemyData.enemyName} poisoned the player with shadow black liquid!");
            }
        }
        
        Debug.Log($"🌑 {enemyData.enemyName} dealt {backAttackDamage} shadow back attack damage!");
        
        // Brief pause after back attack
        yield return new WaitForSeconds(0.5f);
        
        isAttackingFromBehind = false;
    }

    private void PerformShadowDaggerAttack()
    {
        Debug.Log($"🌑 {enemyData.enemyName} performs shadow dagger attack!");
        
        // Create shadow effect
        if (shadowEffectPrefab != null)
        {
            GameObject shadow = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity);
            Destroy(shadow, 0.5f);
        }
        
        // Deal dagger damage
        if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }
        
        // Apply poison status effect
        if (PlayerController.Instance != null)
        {
            var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
            if (playerStatus != null)
            {
                playerStatus.ApplyStatus(StatusEffectType.Poisoned, 3f);
            }
        }
        
        Debug.Log($"🌑 {enemyData.enemyName} dealt {daggerDamage} shadow dagger damage!");
    }

    private void PerformShadowShurikenThrow()
    {
        if (Time.time < lastShurikenTime + shurikenCooldown) return;
        
        StartCoroutine(ShadowShurikenThrowSequence());
        lastShurikenTime = Time.time;
    }

    private IEnumerator ShadowShurikenThrowSequence()
    {
        Debug.Log($"⭐ {enemyData.enemyName} throws shadow shuriken!");
        
        // Create shadow effect
        if (shadowEffectPrefab != null)
        {
            GameObject shadow = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity);
            Destroy(shadow, 0.5f);
        }
        
        // Calculate throw direction
        Vector2 throwDirection = (playerTransform.position - transform.position).normalized;
        
        // Create shadow shuriken
        if (shurikenPrefab != null)
        {
            GameObject shuriken = Instantiate(shurikenPrefab, transform.position, Quaternion.identity);
            
            // Set shuriken properties
            var shurikenComponent = shuriken.GetComponent<EnemyProjectile>();
            if (shurikenComponent != null)
            {
                shurikenComponent.Initialize(throwDirection, shurikenSpeed, shurikenDamage, gameObject);
            }
            else
            {
                Rigidbody2D shurikenRb = shuriken.GetComponent<Rigidbody2D>();
                if (shurikenRb != null)
                {
                    shurikenRb.linearVelocity = throwDirection * shurikenSpeed;
                }
            }
        }
        
        yield return null;
    }

    private IEnumerator ShadowTeleportBehindPlayer()
    {
        if (isShadowTeleporting) yield break;
        
        isShadowTeleporting = true;
        lastShadowTeleportTime = Time.time;
        
        Debug.Log($"🌑 {enemyData.enemyName} uses shadow teleport!");
        
        // Create shadow effect at current position
        if (shadowEffectPrefab != null)
        {
            GameObject shadow = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity);
            Destroy(shadow, shadowDuration);
        }
        
        // Enter shadow form
        isInShadow = true;
        SetElfVisibility(shadowInvisibility);
        
        // Stop movement
        StopMoving();
        
        // Wait briefly
        yield return new WaitForSeconds(0.3f);
        
        // Teleport behind player
        Vector2 playerForward = playerTransform.right;
        Vector2 behindPosition = (Vector2)playerTransform.position - playerForward * 2f;
        transform.position = behindPosition;
        
        // Create shadow effect at new position
        if (shadowEffectPrefab != null)
        {
            GameObject shadow = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity);
            Destroy(shadow, shadowDuration);
        }
        
        // Wait in shadow form
        yield return new WaitForSeconds(shadowAppearDelay);
        
        // Exit shadow form
        isInShadow = false;
        SetElfVisibility(1f);
        
        // Perform back attack if still behind player
        if (IsBehindPlayer())
        {
            PerformShadowBackAttack();
        }
        
        isShadowTeleporting = false;
        
        Debug.Log($"🌑 {enemyData.enemyName} appeared from shadows!");
    }

    protected override void UpdateAI()
    {
        if (isAttackingFromBehind || isInShadow || isShadowTeleporting) return; // Don't update AI during special attacks
        
        base.UpdateAI();
    }

    protected override void MoveTowardsPlayer()
    {
        if (isAttackingFromBehind || isInShadow || isShadowTeleporting) return; // Don't move during special attacks
        
        // Try to get behind player
        if (!IsBehindPlayer())
        {
            // Move to get behind player
            Vector2 playerForward = playerTransform.right;
            Vector2 behindPosition = (Vector2)playerTransform.position - playerForward * 2f;
            Vector2 moveDirection = (behindPosition - (Vector2)transform.position).normalized;
            
            if (rb != null)
            {
                rb.linearVelocity = moveDirection * GetCurrentSpeed();
            }
        }
        else
        {
            // Already behind player, move closer
            base.MoveTowardsPlayer();
        }
    }

    protected override void UseSpecialAbility()
    {
        base.UseSpecialAbility();
        
        // Shadow special ability - Shadow Mastery
        StartCoroutine(ShadowMasteryAbility());
    }

    private IEnumerator ShadowMasteryAbility()
    {
        Debug.Log($"🌑 {enemyData.enemyName} used Shadow Mastery!");
        
        // Create multiple shadow effects
        for (int i = 0; i < 3; i++)
        {
            // Create shadow effect
            if (shadowEffectPrefab != null)
            {
                GameObject shadow = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity);
                Destroy(shadow, shadowDuration);
            }
            
            // Enter shadow form briefly
            isInShadow = true;
            SetElfVisibility(shadowInvisibility);
            
            yield return new WaitForSeconds(0.5f);
            
            // Exit shadow form
            isInShadow = false;
            SetElfVisibility(1f);
            
            // Teleport to a random position
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 newPosition = (Vector2)transform.position + randomDirection * 3f;
            transform.position = newPosition;
            
            yield return new WaitForSeconds(0.5f);
        }
        
        // Final teleport behind player
        yield return StartCoroutine(ShadowTeleportBehindPlayer());
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

    protected override void OnEnemySpawned()
    {
        base.OnEnemySpawned();
        Debug.Log($"🌑 Shadow Dual Dagger Decay Elf spawned! Kingdom: {enemyData.kingdomType}");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw dagger range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, daggerRange);
        
        // Draw back attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, backAttackRange);
        
        // Draw shadow teleport range
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, shadowTeleportRange);
    }
}
