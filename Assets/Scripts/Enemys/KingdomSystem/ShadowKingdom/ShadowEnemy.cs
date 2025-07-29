using UnityEngine;

public abstract class ShadowEnemy : BaseEnemy
{
    [Header("Shadow Kingdom Specific")]
    [SerializeField] protected float shadowResistance = 0.5f;
    [SerializeField] protected float shadowDamageBonus = 1.5f;
    [SerializeField] protected bool canStealth = true;
    [SerializeField] protected float stealthDuration = 3f;
    [SerializeField] protected float stealthCooldown = 8f;
    [SerializeField] protected float stealthDamageMultiplier = 2f;
    protected float lastStealthTime = 0f;
    protected bool isStealthed = false;
    protected SpriteRenderer originalRenderer;

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();
        
        // Shadow Kingdom specific initialization
        if (enemyData.kingdomType != KingdomType.ShadowKingdom)
        {
            Debug.LogWarning($"ShadowEnemy {gameObject.name} has wrong kingdom type: {enemyData.kingdomType}");
        }

        originalRenderer = spriteRenderer;
    }

    protected override void OnEnemyDamaged(float damage)
    {
        base.OnEnemyDamaged(damage);
        
        // Shadow enemies might go into stealth when damaged
        if (canStealth && !isStealthed && Random.value < 0.3f)
        {
            TryStealth();
        }
    }

    protected virtual void TryStealth()
    {
        if (Time.time >= lastStealthTime + stealthCooldown)
        {
            EnterStealth();
        }
    }

    protected virtual void EnterStealth()
    {
        isStealthed = true;
        lastStealthTime = Time.time;
        
        // Make enemy semi-transparent
        if (spriteRenderer != null)
        {
            Color stealthColor = spriteRenderer.color;
            stealthColor.a = 0.3f;
            spriteRenderer.color = stealthColor;
        }
        
        // Increase damage while stealthed
        currentDamage *= stealthDamageMultiplier;
        
        Debug.Log($"👤 {enemyData.enemyName} entered stealth!");
        
        // Exit stealth after duration
        Invoke(nameof(ExitStealth), stealthDuration);
    }

    protected virtual void ExitStealth()
    {
        isStealthed = false;
        
        // Restore visibility
        if (spriteRenderer != null)
        {
            Color normalColor = spriteRenderer.color;
            normalColor.a = 1f;
            spriteRenderer.color = normalColor;
        }
        
        // Restore normal damage
        currentDamage = enemyData.baseDamage * enemyData.damageMultiplier;
        
        Debug.Log($"👤 {enemyData.enemyName} exited stealth!");
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Shadow Kingdom specific attack logic
        if (isStealthed)
        {
            // Stealth attacks are more powerful
            Debug.Log($"👤 {enemyData.enemyName} performed a stealth attack!");
        }
    }

    protected override void UseSpecialAbility()
    {
        base.UseSpecialAbility();
        
        // Shadow Kingdom special abilities
        PerformShadowSpecialAbility();
    }

    protected virtual void PerformShadowSpecialAbility()
    {
        // Override in specific shadow enemy types
        Debug.Log($"👤 {enemyData.enemyName} used shadow special ability!");
    }

    protected override void OnStatusEffectApplied(StatusEffectType statusType, float duration)
    {
        base.OnStatusEffectApplied(statusType, duration);
        
        // Shadow enemies are resistant to most status effects while stealthed
        if (isStealthed)
        {
            // Reduce all status effect durations while stealthed
            RemoveStatus(statusType);
            ApplyStatus(statusType, duration * shadowResistance);
        }
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();
        
        // Shadow enemies might use stealth strategically
        if (canStealth && !isStealthed && isPlayerInRange && 
            Time.time >= lastStealthTime + stealthCooldown && Random.value < 0.1f)
        {
            TryStealth();
        }
    }

    protected override void OnGUI()
    {
        base.OnGUI();
        
        if (showDebugInfo && Application.isEditor)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            float xPos = screenPos.x - 50f;
            float yPos = Screen.height - screenPos.y - 50f;
            
            if (isStealthed)
            {
                GUI.Label(new Rect(xPos, yPos + 60, 200, 20), "STEALTHED");
            }
        }
    }
} 