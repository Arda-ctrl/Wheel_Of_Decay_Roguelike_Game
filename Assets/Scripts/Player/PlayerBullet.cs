using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("Basic Settings")]
    public float speed = 7.5f;
    public float lifeTime = 2f;
    public float baseDamage = 10f;

    [Header("Effect Settings")]
    private AbilityEffectType effectType = AbilityEffectType.Normal;
    private float damageMultiplier = 1f;
    private float speedMultiplier = 1f;
    
    [Header("Elemental Settings")]
    private AbilityData currentAbilityData;
    private bool isElementalBuffActive = false;
    private ElementalAbilityManager elementalAbilityManager;
    [SerializeField] private int stackAmount = 1; // Bu mermi kaç stack ekleyecek

    [Header("Elemental Data")]
    public ElementData elementData; // Set by WeaponController when spawned

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
            Debug.Log("🔥 PlayerBullet hit an enemy!");

            // 1. Önce karakterdeki strike'ları uygula (stack ekle)
            if (elementalAbilityManager != null)
            {
                elementalAbilityManager.UseStrike(other.gameObject);
                Debug.Log($"⚔️ Elemental strike(s) applied to {other.gameObject.name} on hit!");
            }

            // 2. Efekt uygula (VFX/SFX)
            if (elementData != null)
            {
                ApplyElementEffect(other.gameObject);
            }

            // 3. Hasar uygula
            var health = other.GetComponent<IHealth>();
            if (health != null)
            {
                float finalDamage = CalculateFinalDamage(other.gameObject);
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
            case AbilityEffectType.Ice:
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

    private void ApplyElementEffect(GameObject target)
    {
        if (elementData is FireElementData fire)
        {
            // Basit bir burn effect örneği (ileride daha gelişmiş olabilir)
            var burn = target.AddComponent<TempBurnEffect>();
            burn.duration = fire.burnDuration;
            burn.damagePerTick = fire.burnDamagePerTick;
            burn.tickRate = fire.burnTickRate;
        }
        else if (elementData is IceElementData ice)
        {
            var slow = target.AddComponent<TempSlowEffect>();
            slow.slowPercent = ice.slowPercent;
            slow.duration = ice.slowDuration;
        }
        else if (elementData is PoisonElementData poison)
        {
            var poisonEff = target.AddComponent<TempPoisonEffect>();
            poisonEff.duration = poison.poisonDuration;
            poisonEff.damagePerTick = poison.poisonDamagePerTick;
            poisonEff.tickRate = poison.poisonTickRate;
            poisonEff.slowPercent = poison.poisonSlowPercent;
        }
    }

    /// <summary>
    /// Elemental stack'i düşmana uygular
    /// </summary>
    /// <param name="target">Hedef düşman</param>
    private void ApplyElementalStack(GameObject target)
    {
        // Elemental stack sistemi artık ElementalAbilityManager tarafından yönetiliyor
        // Bu method artık kullanılmıyor
        Debug.Log("🔧 Elemental stacks are now managed by ElementalAbilityManager");
    }
    
    /// <summary>
    /// Final hasarı hesaplar
    /// </summary>
    /// <param name="target">Hedef düşman</param>
    /// <returns>Hesaplanmış final hasar</returns>
    private float CalculateFinalDamage(GameObject target)
    {
        float finalDamage = baseDamage * damageMultiplier;
        // Eğer elementalAbilityManager varsa, buff etkisini uygula
        if (elementalAbilityManager != null)
        {
            // Varsayılan olarak Fire elementini kullanıyoruz, istersen burayı dinamik yapabilirsin
            finalDamage = elementalAbilityManager.CalculateBuffDamage(finalDamage, target, ElementType.Fire);
        }
        return finalDamage;
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
    /// Elemental buff'unu aktifleştir/deaktifleştir
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetElementalBuff(bool active)
    {
        isElementalBuffActive = active;
        Debug.Log($"⚡ Elemental buff {(active ? "activated" : "deactivated")}");
    }
    
    /// <summary>
    /// ElementalAbilityManager referansını ayarlar
    /// </summary>
    /// <param name="manager">ElementalAbilityManager referansı</param>
    public void SetElementalAbilityManager(ElementalAbilityManager manager)
    {
        elementalAbilityManager = manager;
    }
    
    /// <summary>
    /// Stack miktarını ayarlar
    /// </summary>
    /// <param name="amount">Stack miktarı</param>
    public void SetStackAmount(int amount)
    {
        stackAmount = amount;
        Debug.Log($"📊 Stack amount set to: {stackAmount}");
    }
    
    /// <summary>
    /// Strike için stack miktarını ayarlar
    /// </summary>
    /// <param name="amount">Stack miktarı</param>
    private void SetStackAmountForStrike(int amount)
    {
        if (elementalAbilityManager != null)
        {
            // ElementalStrike ability'sini bul ve stack miktarını ayarla
            var strikeAbility = elementalAbilityManager.GetAbility(AbilityType.ElementalStrike) as ElementalStrike;
            if (strikeAbility != null)
            {
                strikeAbility.SetStackAmount(amount);
                Debug.Log($"⚔️ Strike stack amount set to: {amount}");
            }
        }
    }
}
