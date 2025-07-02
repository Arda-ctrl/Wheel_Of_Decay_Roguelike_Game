using UnityEngine;
using System.Collections.Generic;

public class PoisonEffect : AbilityEffect
{
    private float lastDamageTime;
    private IMoveable moveable;
    private int currentStacks = 1;
    private static Dictionary<GameObject, PoisonEffect> activeEffects = new Dictionary<GameObject, PoisonEffect>();

    public override void Initialize(AbilityEventData data)
    {
        // Check if target already has poison effect
        if (activeEffects.TryGetValue(data.Target, out PoisonEffect existingEffect))
        {
            // Stack the poison
            existingEffect.AddStack();
            Destroy(gameObject); // Destroy this new instance
            return;
        }

        base.Initialize(data);
        lastDamageTime = 0f;
        
        // Register this effect
        activeEffects[data.Target] = this;

        // Apply movement slow
        moveable = data.Target.GetComponent<IMoveable>();
        if (moveable != null)
        {
            float slowMultiplier = 1f - (eventData.AbilityData.poisonSlowAmount * currentStacks);
            moveable.SetSpeedMultiplier(slowMultiplier);
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

    public void AddStack()
    {
        if (currentStacks < eventData.AbilityData.maxPoisonStacks)
        {
            currentStacks++;
            // Update slow effect
            if (moveable != null)
            {
                float slowMultiplier = 1f - (eventData.AbilityData.poisonSlowAmount * currentStacks);
                moveable.SetSpeedMultiplier(slowMultiplier);
            }
            // Reset duration
            elapsedTime = 0f;
        }
    }

    protected override void OnEffectUpdate()
    {
        if (Time.time - lastDamageTime >= eventData.AbilityData.poisonTickRate)
        {
            float stackMultiplier = 1f + (eventData.AbilityData.stackDamageMultiplier * (currentStacks - 1));
            float tickDamage = eventData.AbilityData.damage * stackMultiplier * 
                             (eventData.AbilityData.poisonTickRate / eventData.AbilityData.effectDuration);
            
            ApplyDamage(eventData.Target, tickDamage);
            lastDamageTime = Time.time;
        }
    }

    protected override void EndEffect()
    {
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(1f);
        }
        
        // Remove from active effects
        if (activeEffects.ContainsKey(eventData.Target))
        {
            activeEffects.Remove(eventData.Target);
        }
        
        base.EndEffect();
    }
} 