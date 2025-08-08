using UnityEngine;
using System.Collections.Generic;

public class FireEffect : AbilityEffect
{
    private float lastDamageTime;
    private List<GameObject> affectedTargets = new List<GameObject>();

    public override void Initialize(AbilityEventData data)
    {
        base.Initialize(data);
        lastDamageTime = 0f;

        // Apply initial burst damage and find AOE targets
        ApplyInitialDamage();

        if (data.AbilityData.vfxPrefab != null)
        {
            foreach (var target in affectedTargets)
            {
                Instantiate(data.AbilityData.vfxPrefab, target.transform.position, Quaternion.identity, target.transform);
            }
        }

        if (data.AbilityData.sfxClip != null)
        {
            AudioSource.PlayClipAtPoint(data.AbilityData.sfxClip, data.Target.transform.position);
        }
    }

    private void ApplyInitialDamage()
    {
        // Apply burst damage to main target
        ApplyDamage(eventData.Target, eventData.AbilityData.initialBurstDamage);
        affectedTargets.Add(eventData.Target);

        // Check for AOE
        if (eventData.AbilityData.hasAreaEffect)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
                eventData.Target.transform.position, 
                eventData.AbilityData.areaEffectRadius
            );

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject != eventData.Target && hitCollider.gameObject != eventData.Caster)
                {
                    // Apply reduced burst damage to nearby targets
                    ApplyDamage(hitCollider.gameObject, eventData.AbilityData.initialBurstDamage * 0.5f);
                    affectedTargets.Add(hitCollider.gameObject);
                }
            }
        }
    }

    protected override void OnEffectUpdate()
    {
        if (Time.time - lastDamageTime >= eventData.AbilityData.igniteTickRate)
        {
            foreach (var target in affectedTargets)
            {
                if (target != null) // Check if target still exists
                {
                    float tickDamage = eventData.AbilityData.damage * 
                                     (eventData.AbilityData.igniteTickRate / eventData.AbilityData.igniteDuration);
                    
                    // AOE targets take reduced DOT damage
                    if (target != eventData.Target)
                    {
                        tickDamage *= 0.5f;
                    }
                    
                    ApplyDamage(target, tickDamage);
                }
            }
            lastDamageTime = Time.time;
        }
    }

    public override void EndEffect()
    {
        affectedTargets.Clear();
        base.EndEffect();
    }

    private void OnDrawGizmos()
    {
        // Draw AOE radius in editor for debugging
        if (eventData?.AbilityData.hasAreaEffect == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, eventData.AbilityData.areaEffectRadius);
        }
    }
} 