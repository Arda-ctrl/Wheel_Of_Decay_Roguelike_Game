using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealthSystem : MonoBehaviour, IHealth, IStatusEffect
{
    public static PlayerHealthSystem Instance;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float healthRegenRate = 2f; // Health per second
    [SerializeField] private float healthRegenDelay = 5f; // Delay after taking damage
    [SerializeField] private bool canRegenerate = true;

    [Header("Shield Settings")]
    [SerializeField] private float maxShield = 0f;
    [SerializeField] private float currentShield = 0f;
    [SerializeField] private float shieldRegenRate = 5f;
    [SerializeField] private float shieldRegenDelay = 3f;

    [Header("Damage Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float damageReduction = 0f; // 0-1 range (0 = no reduction, 1 = immune)
    
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider healthBackgroundSlider; // For smooth delayed effect
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image shieldFillImage;
    [SerializeField] private GameObject lowHealthWarning;
    [SerializeField] private GameObject criticalHealthWarning;

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private GameObject healEffect;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AnimationCurve healthBarAnimCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip lowHealthSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Screen Effects")]
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float criticalHealthThreshold = 0.15f;
    [SerializeField] private Image screenOverlay;
    [SerializeField] private Color lowHealthOverlayColor = new Color(1f, 0f, 0f, 0.1f);
    [SerializeField] private Color criticalHealthOverlayColor = new Color(1f, 0f, 0f, 0.3f);

    // Private variables
    private bool isInvincible = false;
    private float lastDamageTime = 0f;
    private float lastShieldDamageTime = 0f;
    private bool isLowHealth = false;
    private bool isCriticalHealth = false;
    private Coroutine healthBarUpdateCoroutine;
    private Coroutine screenEffectCoroutine;

    // Status effects
    private bool isPoisoned = false;
    private bool isBurning = false;
    private bool isFrozen = false;
    private bool isChilled = false;
    private float poisonDamage = 0f;
    private float burnDamage = 0f;
    private float poisonDuration = 0f;
    private float burnDuration = 0f;
    private float freezeDuration = 0f;
    private float chillDuration = 0f;

    // Events
    public System.Action<float, float> OnHealthChanged; // current, max
    public System.Action<float, float> OnShieldChanged; // current, max
    public System.Action OnPlayerDeath;
    public System.Action OnLowHealth;
    public System.Action OnCriticalHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeHealth();
        SetupUI();
    }

    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
        
        if (playerSprite == null)
        {
            playerSprite = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void SetupUI()
    {
        // Setup health slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        if (healthBackgroundSlider != null)
        {
            healthBackgroundSlider.maxValue = 1f;
            healthBackgroundSlider.value = 1f;
        }

        // Setup shield slider
        if (shieldSlider != null)
        {
            shieldSlider.maxValue = 1f;
            shieldSlider.value = maxShield > 0 ? 1f : 0f;
            shieldSlider.gameObject.SetActive(maxShield > 0);
        }

        UpdateHealthUI();
        UpdateHealthColors();
    }

    private void Update()
    {
        HandleStatusEffects();
        HandleRegeneration();
        UpdateHealthColors();
        UpdateScreenEffects();
    }

    private void HandleStatusEffects()
    {
        // Handle poison
        if (isPoisoned && poisonDuration > 0f)
        {
            poisonDuration -= Time.deltaTime;
            TakeDamage(poisonDamage * Time.deltaTime, DamageType.Poison, false);
            
            if (poisonDuration <= 0f)
            {
                RemoveStatus(StatusEffectType.Poisoned);
            }
        }

        // Handle burning
        if (isBurning && burnDuration > 0f)
        {
            burnDuration -= Time.deltaTime;
            TakeDamage(burnDamage * Time.deltaTime, DamageType.Fire, false);
            
            if (burnDuration <= 0f)
            {
                RemoveStatus(StatusEffectType.Burning);
            }
        }

        // Handle freeze
        if (isFrozen && freezeDuration > 0f)
        {
            freezeDuration -= Time.deltaTime;
            if (freezeDuration <= 0f)
            {
                RemoveStatus(StatusEffectType.Frozen);
            }
        }

        // Handle chill
        if (isChilled && chillDuration > 0f)
        {
            chillDuration -= Time.deltaTime;
            if (chillDuration <= 0f)
            {
                RemoveStatus(StatusEffectType.Chilled);
            }
        }
    }

    private void HandleRegeneration()
    {
        // Health regeneration
        if (canRegenerate && currentHealth < maxHealth && 
            Time.time >= lastDamageTime + healthRegenDelay && !isPoisoned && !isBurning)
        {
            float regenAmount = healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
            UpdateHealthUI();
        }

        // Shield regeneration
        if (maxShield > 0 && currentShield < maxShield && 
            Time.time >= lastShieldDamageTime + shieldRegenDelay)
        {
            float shieldRegenAmount = shieldRegenRate * Time.deltaTime;
            currentShield = Mathf.Min(currentShield + shieldRegenAmount, maxShield);
            UpdateShieldUI();
        }
    }

    public enum DamageType
    {
        Normal,
        Poison,
        Fire,
        Ice,
        Explosion
    }

    public void TakeDamage(float amount, DamageType damageType = DamageType.Normal, bool canBeBlocked = true)
    {
        if (isInvincible && canBeBlocked) return;
        if (currentHealth <= 0) return;

        // Apply damage reduction
        float finalDamage = amount * (1f - damageReduction);

        // Apply damage to shield first
        if (currentShield > 0)
        {
            float shieldDamage = Mathf.Min(finalDamage, currentShield);
            currentShield -= shieldDamage;
            finalDamage -= shieldDamage;
            lastShieldDamageTime = Time.time;
            UpdateShieldUI();
        }

        // Apply remaining damage to health
        if (finalDamage > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - finalDamage);
            lastDamageTime = Time.time;

            // Visual and audio feedback
            ShowDamageEffect(damageType);
            PlayDamageSound();
            
            if (canBeBlocked)
            {
                StartInvincibility();
            }

            UpdateHealthUI();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check for death
            if (currentHealth <= 0)
            {
                HandleDeath();
                return;
            }

            // Check for low/critical health
            CheckHealthThresholds();
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, DamageType.Normal, true);
    }

    public void Heal(float amount, bool showEffect = true)
    {
        if (currentHealth >= maxHealth) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        if (showEffect)
        {
            ShowHealEffect();
            PlayHealSound();
        }

        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddShield(float amount)
    {
        maxShield += amount;
        currentShield = maxShield;
        
        if (shieldSlider != null)
        {
            shieldSlider.gameObject.SetActive(maxShield > 0);
        }
        
        UpdateShieldUI();
        OnShieldChanged?.Invoke(currentShield, maxShield);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 1f;
        maxHealth = newMaxHealth;
        currentHealth = maxHealth * healthRatio;
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void StartInvincibility()
    {
        if (!isInvincible)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        
        // Visual feedback for invincibility
        if (playerSprite != null)
        {
            float flashDuration = invincibilityDuration;
            float elapsed = 0f;
            
            while (elapsed < flashDuration)
            {
                float alpha = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(elapsed * 8f, 1f));
                SetPlayerAlpha(alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            SetPlayerAlpha(1f);
        }
        
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private void UpdateHealthUI()
    {
        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        if (healthSlider != null)
        {
            if (healthBarUpdateCoroutine != null)
            {
                StopCoroutine(healthBarUpdateCoroutine);
            }
            healthBarUpdateCoroutine = StartCoroutine(UpdateHealthBarSmooth(healthRatio));
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
        }
    }

    private IEnumerator UpdateHealthBarSmooth(float targetValue)
    {
        if (healthSlider == null) yield break;

        float startValue = healthSlider.value;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = healthBarAnimCurve.Evaluate(t);
            
            healthSlider.value = Mathf.Lerp(startValue, targetValue, smoothT);
            yield return null;
        }

        healthSlider.value = targetValue;

        // Update background slider with delay
        if (healthBackgroundSlider != null)
        {
            yield return new WaitForSeconds(0.2f);
            
            float bgStartValue = healthBackgroundSlider.value;
            elapsed = 0f;
            duration = 0.8f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                healthBackgroundSlider.value = Mathf.Lerp(bgStartValue, targetValue, t);
                yield return null;
            }

            healthBackgroundSlider.value = targetValue;
        }
    }

    private void UpdateShieldUI()
    {
        if (shieldSlider != null && maxShield > 0)
        {
            float shieldRatio = currentShield / maxShield;
            shieldSlider.value = shieldRatio;
        }
        
        OnShieldChanged?.Invoke(currentShield, maxShield);
    }

    private void UpdateHealthColors()
    {
        if (healthFillImage == null) return;

        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        Color healthColor;
        if (healthRatio > 0.6f)
        {
            healthColor = Color.Lerp(Color.yellow, Color.green, (healthRatio - 0.6f) / 0.4f);
        }
        else if (healthRatio > 0.3f)
        {
            healthColor = Color.Lerp(Color.red, Color.yellow, (healthRatio - 0.3f) / 0.3f);
        }
        else
        {
            healthColor = Color.red;
        }

        healthFillImage.color = healthColor;
    }

    private void UpdateScreenEffects()
    {
        if (screenOverlay == null) return;

        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        Color targetColor = Color.clear;

        if (healthRatio <= criticalHealthThreshold)
        {
            targetColor = criticalHealthOverlayColor;
        }
        else if (healthRatio <= lowHealthThreshold)
        {
            float t = (lowHealthThreshold - healthRatio) / (lowHealthThreshold - criticalHealthThreshold);
            targetColor = Color.Lerp(Color.clear, lowHealthOverlayColor, t);
        }

        screenOverlay.color = Color.Lerp(screenOverlay.color, targetColor, Time.deltaTime * 2f);
    }

    private void CheckHealthThresholds()
    {
        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 0f;

        // Critical health check
        if (healthRatio <= criticalHealthThreshold && !isCriticalHealth)
        {
            isCriticalHealth = true;
            isLowHealth = true;
            OnCriticalHealth?.Invoke();
            
            if (criticalHealthWarning != null)
            {
                criticalHealthWarning.SetActive(true);
            }
        }
        else if (healthRatio > criticalHealthThreshold && isCriticalHealth)
        {
            isCriticalHealth = false;
            
            if (criticalHealthWarning != null)
            {
                criticalHealthWarning.SetActive(false);
            }
        }

        // Low health check
        if (healthRatio <= lowHealthThreshold && !isLowHealth)
        {
            isLowHealth = true;
            OnLowHealth?.Invoke();
            
            if (lowHealthWarning != null)
            {
                lowHealthWarning.SetActive(true);
            }
            
            if (lowHealthSound != null)
            {
                AudioSource.PlayClipAtPoint(lowHealthSound, transform.position);
            }
        }
        else if (healthRatio > lowHealthThreshold && isLowHealth && !isCriticalHealth)
        {
            isLowHealth = false;
            
            if (lowHealthWarning != null)
            {
                lowHealthWarning.SetActive(false);
            }
        }
    }

    private void ShowDamageEffect(DamageType damageType)
    {
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
            
            // Customize effect based on damage type
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                switch (damageType)
                {
                    case DamageType.Fire:
                        main.startColor = Color.red;
                        break;
                    case DamageType.Ice:
                        main.startColor = Color.cyan;
                        break;
                    case DamageType.Poison:
                        main.startColor = Color.green;
                        break;
                    default:
                        main.startColor = Color.white;
                        break;
                }
            }
        }
    }

    private void ShowHealEffect()
    {
        if (healEffect != null)
        {
            Instantiate(healEffect, transform.position, Quaternion.identity);
        }
    }

    private void PlayDamageSound()
    {
        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
        }
    }

    private void PlayHealSound()
    {
        if (healSound != null)
        {
            AudioSource.PlayClipAtPoint(healSound, transform.position);
        }
    }

    private void SetPlayerAlpha(float alpha)
    {
        if (playerSprite != null)
        {
            Color color = playerSprite.color;
            color.a = alpha;
            playerSprite.color = color;
        }
    }

    private void HandleDeath()
    {
        OnPlayerDeath?.Invoke();
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Disable player
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.gameObject.SetActive(false);
        }

        // Show death screen
        if (UI_Controller.Instance != null && UI_Controller.Instance.deathScreen != null)
        {
            UI_Controller.Instance.deathScreen.SetActive(true);
        }
    }

    // IHealth Implementation
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    // IStatusEffect Implementation
    public void ApplyStatus(StatusEffectType statusType, float duration)
    {
        switch (statusType)
        {
            case StatusEffectType.Poisoned:
                isPoisoned = true;
                poisonDuration = duration;
                poisonDamage = 5f; // Default poison damage
                break;
                
            case StatusEffectType.Burning:
                isBurning = true;
                burnDuration = duration;
                burnDamage = 8f; // Default burn damage
                break;
                
            case StatusEffectType.Frozen:
                isFrozen = true;
                freezeDuration = duration;
                // Apply movement restriction in PlayerController
                break;
                
            case StatusEffectType.Chilled:
                isChilled = true;
                chillDuration = duration;
                // Apply speed reduction in PlayerController
                break;
        }
    }

    public void RemoveStatus(StatusEffectType statusType)
    {
        switch (statusType)
        {
            case StatusEffectType.Poisoned:
                isPoisoned = false;
                poisonDuration = 0f;
                break;
                
            case StatusEffectType.Burning:
                isBurning = false;
                burnDuration = 0f;
                break;
                
            case StatusEffectType.Frozen:
                isFrozen = false;
                freezeDuration = 0f;
                break;
                
            case StatusEffectType.Chilled:
                isChilled = false;
                chillDuration = 0f;
                break;
        }
    }

    public bool HasStatus(StatusEffectType statusType)
    {
        return statusType switch
        {
            StatusEffectType.Poisoned => isPoisoned,
            StatusEffectType.Burning => isBurning,
            StatusEffectType.Frozen => isFrozen,
            StatusEffectType.Chilled => isChilled,
            _ => false
        };
    }

    // Public getters for status effects
    public bool IsInvincible => isInvincible;
    public bool IsLowHealth => isLowHealth;
    public bool IsCriticalHealth => isCriticalHealth;
    public float HealthRatio => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public float ShieldRatio => maxShield > 0 ? currentShield / maxShield : 0f;
}
