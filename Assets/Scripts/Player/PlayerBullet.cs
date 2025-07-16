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
    
    [Header("Strike Settings")]
    private AbilityData currentAbilityData;
    private bool isStrikeBuffActive = false;

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
            // Apply strike stack if current ability has strike ability
            if (currentAbilityData != null && currentAbilityData.hasStrikeAbility)
            {
                ApplyStrikeStack(other.gameObject);
            }

            // Calculate final damage with strike system
            float finalDamage = CalculateFinalDamage(other.gameObject);

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

    /// <summary>
    /// Strike stack'i düşmana uygular
    /// </summary>
    /// <param name="target">Hedef düşman</param>
    private void ApplyStrikeStack(GameObject target)
    {
        var strikeStack = target.GetComponent<StrikeStack>();
        if (strikeStack == null)
        {
            // StrikeStack component'i yoksa ekle
            strikeStack = target.AddComponent<StrikeStack>();
        }
        
        // Ability data'yı StrikeStack'e geç
        if (currentAbilityData != null)
        {
            strikeStack.SetAbilityData(currentAbilityData);
        }
        
        // Strike stack ekle
        strikeStack.AddStrikeStack(1);
    }
    
    /// <summary>
    /// Strike sistemi ile final hasarı hesaplar
    /// </summary>
    /// <param name="target">Hedef düşman</param>
    /// <returns>Hesaplanmış final hasar</returns>
    private float CalculateFinalDamage(GameObject target)
    {
        float baseFinalDamage = baseDamage * damageMultiplier;
        
        // Strike sistemi kontrolü
        var strikeStack = target.GetComponent<StrikeStack>();
        if (strikeStack != null && strikeStack.HasStrikeStacks() && currentAbilityData != null)
        {
            // Strike buff aktifse daha fazla hasar
            if (isStrikeBuffActive && currentAbilityData.hasStrikeBuff)
            {
                baseFinalDamage = strikeStack.CalculateStrikeDamage(baseFinalDamage);
                Debug.Log($"⚡ Strike buff active! Damage: {baseFinalDamage}");
            }
            else if (currentAbilityData.hasStrikeAbility)
            {
                // Normal strike hasarı (SO'dan alınan değerler)
                int stacks = strikeStack.GetStrikeStacks();
                if (stacks == 1)
                {
                    baseFinalDamage = currentAbilityData.normalStrikeDamage1Stack;
                }
                else
                {
                    baseFinalDamage = currentAbilityData.normalStrikeDamage2PlusStacks + 
                                    (stacks - 1) * currentAbilityData.normalStrikeDamagePerAdditionalStack;
                }
                Debug.Log($"⚡ Normal strike damage: {baseFinalDamage} (stacks: {stacks})");
            }
        }
        
        return baseFinalDamage;
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
    
    /// <summary>
    /// Ability data'sını ayarlar
    /// </summary>
    /// <param name="abilityData">Ability data</param>
    public void SetAbilityData(AbilityData abilityData)
    {
        currentAbilityData = abilityData;
        Debug.Log($"⚡ Ability data set: {(abilityData != null ? abilityData.abilityName : "None")}");
    }
    
    /// <summary>
    /// Strike buff'unu aktifleştir/deaktifleştir
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetStrikeBuff(bool active)
    {
        isStrikeBuffActive = active;
        Debug.Log($"⚡ Strike buff {(active ? "activated" : "deactivated")}");
    }
}
