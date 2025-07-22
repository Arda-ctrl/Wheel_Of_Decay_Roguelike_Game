using UnityEngine;
using System.Collections;

public abstract class AbilityEffect : MonoBehaviour
{
    protected AbilityEventData eventData;
    protected bool isActive;
    protected float elapsedTime;

    public virtual void Initialize(AbilityEventData data)
    {
        eventData = data;
        elapsedTime = 0f;
        isActive = true;
    }

    protected virtual void Update()
    {
        if (!isActive) return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= eventData.AbilityData.effectDuration)
        {
            EndEffect();
        }
        else
        {
            OnEffectUpdate();
        }
    }

    protected abstract void OnEffectUpdate();
    
    public virtual void EndEffect()
    {
        isActive = false;
        EventManager.Instance.TriggerAbilityEnded(eventData);
        Destroy(gameObject);
    }

    protected virtual void ApplyDamage(GameObject target, float damage)
    {
        // Get health component and apply damage
        var health = target.GetComponent<IHealth>();
        health?.TakeDamage(damage);
    }
} 