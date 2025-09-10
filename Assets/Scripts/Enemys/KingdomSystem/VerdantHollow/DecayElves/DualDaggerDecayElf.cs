using UnityEngine;
using System.Collections;

public class DualDaggerDecayElf : VerdantEnemy
{
    [Header("Dual Dagger Decay Elf Specific")]
    [SerializeField] private GameObject dualDaggerPrefab;
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private GameObject smokeBombPrefab;
    [SerializeField] private float daggerDamage = 12f;
    [SerializeField] private float daggerRange = 1.5f;
    [SerializeField] private float backAttackRange = 2f;
    [SerializeField] private float backAttackDamage = 20f;
    [SerializeField] private float shurikenChance = 0.25f; // 25% chance
    [SerializeField] private float shurikenCooldown = 6f;
    [SerializeField] private float shurikenSpeed = 10f;
    [SerializeField] private float shurikenDamage = 8f;
    [SerializeField] private float smokeBombCooldown = 8f;
    [SerializeField] private float smokeBombDuration = 2f;
    [SerializeField] private float teleportDistance = 4f;
    [SerializeField] private float stealthDuration = 3f;
    
    private float lastShurikenTime = 0f;
    private float lastSmokeBombTime = 0f;
    private bool isInStealth = false;
    private bool isAttackingFromBehind = false;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Check if we can attack from behind
        if (IsBehindPlayer() && distanceToPlayer <= backAttackRange)
        {
            PerformBackAttack();
        }
        else if (distanceToPlayer <= daggerRange)
        {
            PerformDaggerAttack();
        }
        else if (Time.time >= lastShurikenTime + shurikenCooldown && Random.value < shurikenChance)
        {
            PerformShurikenThrow();
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

    private void PerformBackAttack()
    {
        if (isAttackingFromBehind) return;
        
        StartCoroutine(BackAttackSequence());
    }

    private IEnumerator BackAttackSequence()
    {
        isAttackingFromBehind = true;
        
        Debug.Log($"🗡️ {enemyData.enemyName} performs back attack!");
        
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
                playerStatus.ApplyStatus(StatusEffectType.Poisoned, 3f);
                Debug.Log($"☠️ {enemyData.enemyName} poisoned the player with black liquid!");
            }
        }
        
        Debug.Log($"🗡️ {enemyData.enemyName} dealt {backAttackDamage} back attack damage!");
        
        // Brief pause after back attack
        yield return new WaitForSeconds(0.5f);
        
        isAttackingFromBehind = false;
    }

    private void PerformDaggerAttack()
    {
        Debug.Log($"🗡️ {enemyData.enemyName} performs dagger attack!");
        
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
                playerStatus.ApplyStatus(StatusEffectType.Poisoned, 2f);
            }
        }
        
        Debug.Log($"🗡️ {enemyData.enemyName} dealt {daggerDamage} dagger damage!");
    }

    private void PerformShurikenThrow()
    {
        if (Time.time < lastShurikenTime + shurikenCooldown) return;
        
        StartCoroutine(ShurikenThrowSequence());
        lastShurikenTime = Time.time;
    }

    private IEnumerator ShurikenThrowSequence()
    {
        Debug.Log($"⭐ {enemyData.enemyName} throws shuriken!");
        
        // Calculate throw direction
        Vector2 throwDirection = (playerTransform.position - transform.position).normalized;
        
        // Create shuriken
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

    protected override void UpdateAI()
    {
        if (isAttackingFromBehind || isInStealth) return; // Don't update AI during special attacks
        
        base.UpdateAI();
    }

    protected override void MoveTowardsPlayer()
    {
        if (isAttackingFromBehind || isInStealth) return; // Don't move during special attacks
        
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
        
        // Dual Dagger special ability - Smoke Bomb Stealth
        if (Time.time >= lastSmokeBombTime + smokeBombCooldown)
        {
            StartCoroutine(SmokeBombStealth());
            lastSmokeBombTime = Time.time;
        }
    }

    private IEnumerator SmokeBombStealth()
    {
        Debug.Log($"💨 {enemyData.enemyName} used Smoke Bomb Stealth!");
        
        // Create smoke bomb effect
        if (smokeBombPrefab != null)
        {
            GameObject smoke = Instantiate(smokeBombPrefab, transform.position, Quaternion.identity);
            Destroy(smoke, smokeBombDuration);
        }
        
        // Enter stealth mode
        isInStealth = true;
        SetElfVisibility(0.2f);
        
        // Stop movement briefly
        StopMoving();
        
        // Wait a moment in smoke
        yield return new WaitForSeconds(0.5f);
        
        // Teleport behind player
        Vector2 playerForward = playerTransform.right;
        Vector2 behindPosition = (Vector2)playerTransform.position - playerForward * 2f;
        transform.position = behindPosition;
        
        // Wait in stealth
        yield return new WaitForSeconds(stealthDuration);
        
        // Exit stealth
        isInStealth = false;
        SetElfVisibility(1f);
        
        // Perform back attack
        if (IsBehindPlayer())
        {
            PerformBackAttack();
        }
        
        Debug.Log($"💨 {enemyData.enemyName} exited stealth!");
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
        Debug.Log($"🗡️ Dual Dagger Decay Elf spawned! Kingdom: {enemyData.kingdomType}");
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
        
        // Draw stealth range
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, teleportDistance);
    }
}
