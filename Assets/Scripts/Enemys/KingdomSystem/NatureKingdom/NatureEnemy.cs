using UnityEngine;

public abstract class NatureEnemy : BaseEnemy
{
    [Header("Nature Kingdom Specific")]
    [SerializeField] protected float natureResistance = 0.5f;
    [SerializeField] protected float natureDamageBonus = 1.5f;
    [SerializeField] protected bool canPoisonPlayer = true;
    [SerializeField] protected float poisonChance = 0.4f;
    [SerializeField] protected float poisonDuration = 5f;
    [SerializeField] protected float healAmount = 10f;
    [SerializeField] protected float healCooldown = 10f;
    protected float lastHealTime = 0f;

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();
        
        // Nature Kingdom specific initialization
        if (enemyData.kingdomType != KingdomType.NatureKingdom)
        {
            Debug.LogWarning($"NatureEnemy {gameObject.name} has wrong kingdom type: {enemyData.kingdomType}");
        }
    }

    protected override void OnEnemyDamaged(float damage)
    {
        base.OnEnemyDamaged(damage);
        
        // Nature enemies can heal themselves when damaged
        TryHeal();
    }

    protected virtual void TryHeal()
    {
        if (Time.time >= lastHealTime + healCooldown && currentHealth < enemyData.maxHealth * 0.5f)
        {
            HealSelf();
        }
    }

    protected virtual void HealSelf()
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, enemyData.maxHealth);
        lastHealTime = Time.time;
        
        // Play heal effect
        if (animator != null)
        {
            animator.SetTrigger("Heal");
        }
        
        Debug.Log($"🌿 {enemyData.enemyName} healed itself!");
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Nature Kingdom specific attack logic
        if (canPoisonPlayer && Random.value < poisonChance)
        {
            ApplyPoisonToPlayer();
        }
    }

    protected virtual void ApplyPoisonToPlayer()
    {
        if (PlayerController.Instance != null)
        {
            // Apply poison status effect to player
            var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
            if (playerStatus != null)
            {
                playerStatus.ApplyStatus(StatusEffectType.Poisoned, poisonDuration);
                Debug.Log($"🌿 {enemyData.enemyName} poisoned the player!");
            }
        }
    }

    protected override void UseSpecialAbility()
    {
        base.UseSpecialAbility();
        
        // Nature Kingdom special abilities
        PerformNatureSpecialAbility();
    }

    protected virtual void PerformNatureSpecialAbility()
    {
        // Override in specific nature enemy types
        Debug.Log($"🌿 {enemyData.enemyName} used nature special ability!");
    }

    protected override void OnStatusEffectApplied(StatusEffectType statusType, float duration)
    {
        base.OnStatusEffectApplied(statusType, duration);
        
        // Nature enemies are resistant to poison effects
        if (statusType == StatusEffectType.Poisoned)
        {
            // Reduce poison duration for nature enemies
            RemoveStatus(StatusEffectType.Poisoned);
            ApplyStatus(StatusEffectType.Poisoned, duration * natureResistance);
        }
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();
        
        // Nature enemies might heal when not in combat
        if (!isPlayerInRange && Time.time >= lastHealTime + healCooldown)
        {
            TryHeal();
        }
    }
} 