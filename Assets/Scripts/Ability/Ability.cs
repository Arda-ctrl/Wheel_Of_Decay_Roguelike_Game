using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour
{
    [SerializeField] private AbilityData abilityData;
    
    private bool isOnCooldown;
    private float cooldownTimeRemaining;

    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimeRemaining -= Time.deltaTime;
            if (cooldownTimeRemaining <= 0)
            {
                isOnCooldown = false;
            }
        }
    }

    public bool CanUseAbility()
    {
        return !isOnCooldown && abilityData != null;
    }

    public void UseAbility(GameObject target)
    {
        if (!CanUseAbility()) return;

        // Create event data
        var eventData = new AbilityEventData(
            gameObject,
            target,
            abilityData,
            target.transform.position
        );

        // Trigger ability used event
        EventManager.Instance.TriggerAbilityUsed(eventData);

        // Create and initialize effect
        GameObject effectObj = new GameObject($"{abilityData.abilityName}_Effect");
        AbilityEffect effect = null;

        switch (abilityData.effectType)
        {
            case AbilityEffectType.Fire:
                effect = effectObj.AddComponent<FireEffect>();
                break;
            case AbilityEffectType.Poison:
                effect = effectObj.AddComponent<PoisonEffect>();
                break;
            case AbilityEffectType.Freeze:
                effect = effectObj.AddComponent<FreezeEffect>();
                break;
        }

        if (effect != null)
        {
            effect.Initialize(eventData);
            EventManager.Instance.TriggerAbilityStarted(eventData);
        }
        else
        {
            Destroy(effectObj);
        }

        // Start cooldown
        StartCooldown();
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimeRemaining = abilityData.cooldownDuration;
    }

    public float GetCooldownProgress()
    {
        if (!isOnCooldown) return 0f;
        return cooldownTimeRemaining / abilityData.cooldownDuration;
    }

    public AbilityData GetAbilityData()
    {
        return abilityData;
    }
} 