using UnityEngine;

public class FreezeEffect : AbilityEffect
{
    private IMoveable moveable;
    private bool isFrozen;
    private float freezeEndTime;
    private float chillEndTime;

    public override void Initialize(AbilityEventData data)
    {
        base.Initialize(data);
        
        moveable = data.Target.GetComponent<IMoveable>();
        if (moveable != null)
        {
            // Apply initial damage
            ApplyDamage(data.Target, data.AbilityData.damage);

            // Check for freeze chance
            if (Random.value < data.AbilityData.freezeChance)
            {
                // Apply freeze
                ApplyFreeze();
            }
            else
            {
                // Apply chill
                ApplyChill();
            }
        }

        if (data.AbilityData.vfxPrefab != null)
        {
            Instantiate(data.AbilityData.vfxPrefab, data.Target.transform.position, Quaternion.identity, data.Target.transform);
        }

        if (data.AbilityData.sfxClip != null)
        {
            AudioSource.PlayClipAtPoint(data.AbilityData.sfxClip, data.Target.transform.position);
        }
    }

    private void ApplyFreeze()
    {
        isFrozen = true;
        freezeEndTime = Time.time + eventData.AbilityData.freezeDuration;
        
        // Complete stop when frozen
        moveable.SetSpeedMultiplier(0f);

        // Notify any status effect handlers
        var statusHandler = eventData.Target.GetComponent<IStatusEffect>();
        statusHandler?.ApplyStatus(StatusEffectType.Frozen, eventData.AbilityData.freezeDuration);
    }

    private void ApplyChill()
    {
        chillEndTime = Time.time + eventData.AbilityData.chillDuration;
        
        // Apply slow effect
        moveable.SetSpeedMultiplier(1f - eventData.AbilityData.chillSlowAmount);

        // Notify any status effect handlers
        var statusHandler = eventData.Target.GetComponent<IStatusEffect>();
        statusHandler?.ApplyStatus(StatusEffectType.Chilled, eventData.AbilityData.chillDuration);
    }

    protected override void OnEffectUpdate()
    {
        if (moveable == null) return;

        if (isFrozen && Time.time >= freezeEndTime)
        {
            // Transition from freeze to chill
            isFrozen = false;
            ApplyChill();
        }
        else if (!isFrozen && Time.time >= chillEndTime)
        {
            // End the effect when chill expires
            EndEffect();
        }
    }

    protected override void EndEffect()
    {
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(1f);
            
            // Clear status effects
            var statusHandler = eventData.Target.GetComponent<IStatusEffect>();
            if (statusHandler != null)
            {
                if (isFrozen)
                    statusHandler.RemoveStatus(StatusEffectType.Frozen);
                statusHandler.RemoveStatus(StatusEffectType.Chilled);
            }
        }
        base.EndEffect();
    }
}

public class AbilityManager : MonoBehaviour
{
    private void OnEnable()
    {
        // Event'lere abone ol
        EventManager.Instance.OnAbilityUsed += HandleAbilityUsed;
        EventManager.Instance.OnAbilityStarted += HandleAbilityStarted;
        EventManager.Instance.OnAbilityEnded += HandleAbilityEnded;
    }

    private void OnDisable()
    {
        // Event'lerden çık
        EventManager.Instance.OnAbilityUsed -= HandleAbilityUsed;
        EventManager.Instance.OnAbilityStarted -= HandleAbilityStarted;
        EventManager.Instance.OnAbilityEnded -= HandleAbilityEnded;
    }

    private void HandleAbilityUsed(AbilityEventData data)
    {
        Debug.Log($"{data.AbilityData.abilityName} used by {data.Caster.name} on {data.Target.name}");
    }

    private void HandleAbilityStarted(AbilityEventData data)
    {
        Debug.Log($"{data.AbilityData.abilityName} effect started");
    }

    private void HandleAbilityEnded(AbilityEventData data)
    {
        Debug.Log($"{data.AbilityData.abilityName} effect ended");
    }
} 