using UnityEngine;

public abstract class FireEnemy : BaseEnemy
{
    [Header("Fire Kingdom Specific")]
    [SerializeField] protected float fireResistance = 0.5f;
    [SerializeField] protected float fireDamageBonus = 1.5f;
    [SerializeField] protected bool canBurnPlayer = true;
    [SerializeField] protected float burnChance = 0.3f;
    [SerializeField] protected float burnDuration = 3f;

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();
        
        // Fire Kingdom specific initialization
        if (enemyData.kingdomType != KingdomType.FireKingdom)
        {
            Debug.LogWarning($"FireEnemy {gameObject.name} has wrong kingdom type: {enemyData.kingdomType}");
        }
    }

    protected override void OnEnemyDamaged(float damage)
    {
        base.OnEnemyDamaged(damage);
        
        // Fire enemies take less damage from fire attacks
        // This will be handled by the damage system
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Fire Kingdom specific attack logic
        if (canBurnPlayer && Random.value < burnChance)
        {
            ApplyBurnToPlayer();
        }
    }

    protected virtual void ApplyBurnToPlayer()
    {
        if (PlayerController.Instance != null)
        {
            // Apply burn status effect to player
            var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
            if (playerStatus != null)
            {
                playerStatus.ApplyStatus(StatusEffectType.Burning, burnDuration);
                Debug.Log($"🔥 {enemyData.enemyName} burned the player!");
            }
        }
    }

    protected override void UseSpecialAbility()
    {
        base.UseSpecialAbility();
        
        // Fire Kingdom special abilities
        PerformFireSpecialAbility();
    }

    protected virtual void PerformFireSpecialAbility()
    {
        // Override in specific fire enemy types
        Debug.Log($"🔥 {enemyData.enemyName} used fire special ability!");
    }

    protected override void OnStatusEffectApplied(StatusEffectType statusType, float duration)
    {
        base.OnStatusEffectApplied(statusType, duration);
        
        // Fire enemies are resistant to fire effects
        if (statusType == StatusEffectType.Burning)
        {
            // Reduce burn duration for fire enemies
            RemoveStatus(StatusEffectType.Burning);
            ApplyStatus(StatusEffectType.Burning, duration * fireResistance);
        }
    }
} 