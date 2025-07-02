using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("Basic Settings")]
    public float speed = 7.5f;
    public float lifeTime = 2f;
    public float baseDamage = 10f;

    [Header("Effect Settings")]
    private AbilityEffectType effectType = AbilityEffectType.None;
    private float damageMultiplier = 1f;
    private float speedMultiplier = 1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Set initial velocity
        rb.linearVelocity = transform.right * speed * speedMultiplier;
        
        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Apply damage with any modifiers
            float finalDamage = baseDamage * damageMultiplier;

            // Apply ability effect if any
            if (effectType != AbilityEffectType.None)
            {
                ApplyAbilityEffect(other.gameObject);
            }

            // Apply damage
            var health = other.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(finalDamage);
            }

            // Destroy bullet
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void ApplyAbilityEffect(GameObject target)
    {
        // Create temporary ability data for the effect
        var tempAbilityData = ScriptableObject.CreateInstance<AbilityData>();
        tempAbilityData.effectType = effectType;
        tempAbilityData.damage = baseDamage * damageMultiplier * 0.5f; // Effect damage is half of bullet damage
        tempAbilityData.effectDuration = 3f; // Default duration

        // Create event data
        var eventData = new AbilityEventData(
            gameObject,
            target,
            tempAbilityData,
            target.transform.position
        );

        // Create and initialize effect
        GameObject effectObj = new GameObject($"BulletEffect_{effectType}");
        AbilityEffect effect = null;

        switch (effectType)
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

        // Destroy the temporary ability data
        Destroy(tempAbilityData);
    }

    // Setters for bullet modifications
    public void SetEffectType(AbilityEffectType type)
    {
        effectType = type;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed * speedMultiplier;
        }
    }
}
