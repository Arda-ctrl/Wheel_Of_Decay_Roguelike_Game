using UnityEngine;

public abstract class IceEnemy : BaseEnemy
{
    [Header("Ice Kingdom Specific")]
    [SerializeField] protected float iceResistance = 0.5f;
    [SerializeField] protected float iceDamageBonus = 1.5f;
    [SerializeField] protected bool canFreezePlayer = true;
    [SerializeField] protected float freezeChance = 0.2f;
    [SerializeField] protected float freezeDuration = 2f;
    [SerializeField] protected float chillChance = 0.5f;
    [SerializeField] protected float chillDuration = 4f;

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();
        
        // Ice Kingdom specific initialization
        if (enemyData.kingdomType != KingdomType.IceKingdom)
        {
            Debug.LogWarning($"IceEnemy {gameObject.name} has wrong kingdom type: {enemyData.kingdomType}");
        }
    }

    protected override void OnEnemyDamaged(float damage)
    {
        base.OnEnemyDamaged(damage);
        
        // Ice enemies take less damage from ice attacks
        // This will be handled by the damage system
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Ice Kingdom specific attack logic
        if (canFreezePlayer)
        {
            if (Random.value < freezeChance)
            {
                ApplyFreezeToPlayer();
            }
            else if (Random.value < chillChance)
            {
                ApplyChillToPlayer();
            }
        }
    }

    protected virtual void ApplyFreezeToPlayer()
    {
        if (PlayerController.Instance != null)
        {
            // Apply freeze status effect to player
            var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
            if (playerStatus != null)
            {
                playerStatus.ApplyStatus(StatusEffectType.Frozen, freezeDuration);
                Debug.Log($"❄️ {enemyData.enemyName} froze the player!");
            }
        }
    }

    protected virtual void ApplyChillToPlayer()
    {
        if (PlayerController.Instance != null)
        {
            // Apply chill status effect to player
            var playerStatus = PlayerController.Instance.GetComponent<IStatusEffect>();
            if (playerStatus != null)
            {
                playerStatus.ApplyStatus(StatusEffectType.Chilled, chillDuration);
                Debug.Log($"❄️ {enemyData.enemyName} chilled the player!");
            }
        }
    }

    protected override void UseSpecialAbility()
    {
        base.UseSpecialAbility();
        
        // Ice Kingdom special abilities
        PerformIceSpecialAbility();
    }

    protected virtual void PerformIceSpecialAbility()
    {
        // Override in specific ice enemy types
        Debug.Log($"❄️ {enemyData.enemyName} used ice special ability!");
    }

    protected override void OnStatusEffectApplied(StatusEffectType statusType, float duration)
    {
        base.OnStatusEffectApplied(statusType, duration);
        
        // Ice enemies are resistant to ice effects
        if (statusType == StatusEffectType.Frozen || statusType == StatusEffectType.Chilled)
        {
            // Reduce freeze/chill duration for ice enemies
            RemoveStatus(statusType);
            ApplyStatus(statusType, duration * iceResistance);
        }
    }
} 