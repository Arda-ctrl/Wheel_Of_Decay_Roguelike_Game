using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseBoss : BaseEnemy
{
    [Header("Boss Specific")]
    [SerializeField] protected BossData bossData;
    [SerializeField] protected float phaseTransitionHealth = 0.5f; // 50% health triggers phase 2
    [SerializeField] protected bool isInPhase2 = false;
    [SerializeField] protected float enrageHealth = 0.25f; // 25% health triggers enrage
    [SerializeField] protected bool isEnraged = false;
    
    [Header("Boss Abilities")]
    [SerializeField] protected List<BossAbility> bossAbilities = new List<BossAbility>();
    [SerializeField] protected float abilityCooldown = 3f;
    protected float lastAbilityTime = 0f;
    protected int currentAbilityIndex = 0;

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();
        
        // Boss specific initialization
        if (enemyData.enemyType != EnemyType.Boss)
        {
            Debug.LogWarning($"BaseBoss {gameObject.name} has wrong enemy type: {enemyData.enemyType}");
        }
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();
        
        // Check for phase transitions
        CheckPhaseTransitions();
        
        // Boss specific AI
        UpdateBossAI();
    }

    protected virtual void CheckPhaseTransitions()
    {
        float healthPercentage = currentHealth / enemyData.maxHealth;
        
        // Phase 2 transition
        if (!isInPhase2 && healthPercentage <= phaseTransitionHealth)
        {
            EnterPhase2();
        }
        
        // Enrage transition
        if (!isEnraged && healthPercentage <= enrageHealth)
        {
            EnterEnrage();
        }
    }

    protected virtual void EnterPhase2()
    {
        isInPhase2 = true;
        Debug.Log($"⚔️ {enemyData.enemyName} entered Phase 2!");
        
        // Phase 2 specific changes
        OnPhase2Entered();
    }

    protected virtual void EnterEnrage()
    {
        isEnraged = true;
        Debug.Log($"⚔️ {enemyData.enemyName} is ENRAGED!");
        
        // Enrage specific changes
        OnEnrageEntered();
    }

    protected virtual void UpdateBossAI()
    {
        if (!isPlayerInRange) return;

        // Use abilities on cooldown
        if (Time.time >= lastAbilityTime + abilityCooldown)
        {
            UseBossAbility();
        }
    }

    protected virtual void UseBossAbility()
    {
        if (bossAbilities.Count == 0) return;

        // Select ability based on current phase and state
        BossAbility selectedAbility = SelectBossAbility();
        
        if (selectedAbility != null)
        {
            ExecuteBossAbility(selectedAbility);
            lastAbilityTime = Time.time;
        }
    }

    protected virtual BossAbility SelectBossAbility()
    {
        // Override in derived classes for specific boss logic
        if (currentAbilityIndex < bossAbilities.Count)
        {
            BossAbility ability = bossAbilities[currentAbilityIndex];
            currentAbilityIndex = (currentAbilityIndex + 1) % bossAbilities.Count;
            return ability;
        }
        
        return null;
    }

    protected virtual void ExecuteBossAbility(BossAbility ability)
    {
        Debug.Log($"⚔️ {enemyData.enemyName} used {ability.abilityName}!");
        
        // Execute the ability
        StartCoroutine(PerformBossAbility(ability));
    }

    protected virtual IEnumerator PerformBossAbility(BossAbility ability)
    {
        // Override in derived classes
        yield return new WaitForSeconds(ability.castTime);
        
        // Default ability execution
        Debug.Log($"⚔️ {enemyData.enemyName} finished casting {ability.abilityName}!");
    }

    protected override void OnEnemyDamaged(float damage)
    {
        base.OnEnemyDamaged(damage);
        
        // Boss specific damage reactions
        OnBossDamaged(damage);
    }

    protected override void Die()
    {
        // Boss death is special
        OnBossDeath();
        base.Die();
    }

    protected override void GiveRewards()
    {
        // Boss rewards are special
        Debug.Log($"⚔️ {enemyData.enemyName} was defeated! Special rewards given!");
        
        // Give boss-specific rewards
        GiveBossRewards();
    }

    // Virtual methods for override
    protected virtual void OnPhase2Entered() { }
    protected virtual void OnEnrageEntered() { }
    protected virtual void OnBossDamaged(float damage) { }
    protected virtual void OnBossDeath() { }
    protected virtual void GiveBossRewards() { }

    protected override void OnGUI()
    {
        base.OnGUI();
        
        if (showDebugInfo && Application.isEditor)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            float xPos = screenPos.x - 50f;
            float yPos = Screen.height - screenPos.y - 50f;
            
            if (isInPhase2)
            {
                GUI.Label(new Rect(xPos, yPos + 80, 200, 20), "PHASE 2");
            }
            
            if (isEnraged)
            {
                GUI.Label(new Rect(xPos, yPos + 100, 200, 20), "ENRAGED");
            }
        }
    }
}

[System.Serializable]
public class BossAbility
{
    public string abilityName;
    public float castTime = 1f;
    public float cooldown = 5f;
    public float damage = 20f;
    public bool isPhase2Ability = false;
    public bool isEnrageAbility = false;
} 