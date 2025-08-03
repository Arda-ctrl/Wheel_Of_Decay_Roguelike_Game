using UnityEngine;

public abstract class VerdantEnemy : BaseEnemy
{
    [Header("Verdant Hollow Specific")]
    [SerializeField] protected float verdantResistance = 0.5f;
    [SerializeField] protected float verdantDamageBonus = 1.5f;
    [SerializeField] protected bool canPoisonPlayer = true;
    [SerializeField] protected float poisonChance = 0.4f;
    [SerializeField] protected float poisonDuration = 5f;
    [SerializeField] protected bool canRootPlayer = true;
    [SerializeField] protected float rootChance = 0.2f;
    [SerializeField] protected float rootDuration = 3f;

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();
        
        // Verdant Hollow specific initialization
        if (enemyData.kingdomType != KingdomType.NatureKingdom)
        {
            Debug.LogWarning($"VerdantEnemy {gameObject.name} has wrong kingdom type: {enemyData.kingdomType}");
        }
    }

    protected override void OnEnemyDamaged(float damage)
    {
        base.OnEnemyDamaged(damage);
        
        // Verdant enemies might have special reactions to damage
        OnVerdantDamaged(damage);
    }

    protected virtual void OnVerdantDamaged(float damage)
    {
        // Override in specific enemy types
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Verdant Hollow specific attack logic
        if (canPoisonPlayer && Random.value < poisonChance)
        {
            ApplyPoisonToPlayer();
        }
        
        if (canRootPlayer && Random.value < rootChance)
        {
            ApplyRootToPlayer();
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

    protected virtual void ApplyRootToPlayer()
    {
        if (PlayerController.Instance != null)
        {
            // Apply root effect to player (slow movement)
            var playerMoveable = PlayerController.Instance.GetComponent<IMoveable>();
            if (playerMoveable != null)
            {
                playerMoveable.SetSpeedMultiplier(0.3f); // 70% slow
                StartCoroutine(RemoveRootEffect(playerMoveable, rootDuration));
            }
            Debug.Log($"🌿 {enemyData.enemyName} rooted the player!");
        }
    }

    private System.Collections.IEnumerator RemoveRootEffect(IMoveable playerMoveable, float duration)
    {
        yield return new WaitForSeconds(duration);
        playerMoveable.SetSpeedMultiplier(1f);
    }

    protected override void UseSpecialAbility()
    {
        base.UseSpecialAbility();
        
        // Verdant Hollow special abilities
        PerformVerdantSpecialAbility();
    }

    protected virtual void PerformVerdantSpecialAbility()
    {
        // Override in specific verdant enemy types
        Debug.Log($"🌿 {enemyData.enemyName} used verdant special ability!");
    }

    protected override void OnStatusEffectApplied(StatusEffectType statusType, float duration)
    {
        base.OnStatusEffectApplied(statusType, duration);
        
        // Verdant enemies are resistant to poison effects
        if (statusType == StatusEffectType.Poisoned)
        {
            // Reduce poison duration for verdant enemies
            RemoveStatus(StatusEffectType.Poisoned);
            ApplyStatus(StatusEffectType.Poisoned, duration * verdantResistance);
        }
    }
} 