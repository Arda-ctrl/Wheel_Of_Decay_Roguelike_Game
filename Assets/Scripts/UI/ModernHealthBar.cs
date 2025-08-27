using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ModernHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Slider mainHealthSlider;
    [SerializeField] private Slider backgroundHealthSlider;
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image backgroundFillImage;
    [SerializeField] private Image shieldFillImage;
    [SerializeField] private TMP_Text healthText;
    
    [Header("Visual Settings")]
    [SerializeField] private Gradient healthColorGradient;
    [SerializeField] private Color shieldColor = Color.cyan;
    [SerializeField] private Color backgroundHealthColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    
    [Header("Animation Settings")]
    [SerializeField] private float healthBarSpeed = 2f;
    [SerializeField] private float backgroundBarSpeed = 1f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseIntensity = 0.2f;
    
    [Header("Low Health Effects")]
    [SerializeField] private GameObject lowHealthIndicator;
    [SerializeField] private GameObject criticalHealthIndicator;
    [SerializeField] private Image healthBarGlow;
    [SerializeField] private Color lowHealthGlowColor = Color.red;
    [SerializeField] private float glowPulseSpeed = 4f;
    
    private PlayerHealthSystem playerHealth;
    private Coroutine healthUpdateCoroutine;
    private Coroutine backgroundUpdateCoroutine;
    private Coroutine pulseCoroutine;
    private bool isLowHealth = false;
    private bool isCriticalHealth = false;

    private void Start()
    {
        SetupHealthBar();
        SubscribeToHealthEvents();
    }

    private void SetupHealthBar()
    {
        playerHealth = PlayerHealthSystem.Instance;
        
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthSystem not found! Make sure it exists in the scene.");
            return;
        }

        // Initialize sliders
        if (mainHealthSlider != null)
        {
            mainHealthSlider.minValue = 0f;
            mainHealthSlider.maxValue = 1f;
            mainHealthSlider.value = 1f;
        }

        if (backgroundHealthSlider != null)
        {
            backgroundHealthSlider.minValue = 0f;
            backgroundHealthSlider.maxValue = 1f;
            backgroundHealthSlider.value = 1f;
        }

        if (shieldSlider != null)
        {
            shieldSlider.minValue = 0f;
            shieldSlider.maxValue = 1f;
            shieldSlider.value = 0f;
            shieldSlider.gameObject.SetActive(false);
        }

        // Set initial colors
        UpdateHealthBarColor(1f);
        
        if (backgroundFillImage != null)
        {
            backgroundFillImage.color = backgroundHealthColor;
        }
        
        if (shieldFillImage != null)
        {
            shieldFillImage.color = shieldColor;
        }

        // Initialize text
        UpdateHealthText();
    }

    private void SubscribeToHealthEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
            playerHealth.OnShieldChanged += OnShieldChanged;
            playerHealth.OnLowHealth += OnLowHealth;
            playerHealth.OnCriticalHealth += OnCriticalHealth;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
            playerHealth.OnShieldChanged -= OnShieldChanged;
            playerHealth.OnLowHealth -= OnLowHealth;
            playerHealth.OnCriticalHealth -= OnCriticalHealth;
        }
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        // Update main health bar with smooth animation
        if (healthUpdateCoroutine != null)
        {
            StopCoroutine(healthUpdateCoroutine);
        }
        healthUpdateCoroutine = StartCoroutine(UpdateHealthBarSmooth(healthRatio));
        
        // Update background bar with delay
        if (backgroundUpdateCoroutine != null)
        {
            StopCoroutine(backgroundUpdateCoroutine);
        }
        backgroundUpdateCoroutine = StartCoroutine(UpdateBackgroundBarWithDelay(healthRatio));
        
        // Update color
        UpdateHealthBarColor(healthRatio);
        
        // Update text
        UpdateHealthText();
        
        // Check for health thresholds
        CheckHealthThresholds(healthRatio);
    }

    private void OnShieldChanged(float currentShield, float maxShield)
    {
        if (shieldSlider != null)
        {
            bool hasShield = maxShield > 0;
            shieldSlider.gameObject.SetActive(hasShield);
            
            if (hasShield)
            {
                float shieldRatio = currentShield / maxShield;
                StartCoroutine(UpdateShieldBarSmooth(shieldRatio));
            }
        }
    }

    private void OnLowHealth()
    {
        isLowHealth = true;
        
        if (lowHealthIndicator != null)
        {
            lowHealthIndicator.SetActive(true);
        }
        
        StartLowHealthEffects();
    }

    private void OnCriticalHealth()
    {
        isCriticalHealth = true;
        
        if (criticalHealthIndicator != null)
        {
            criticalHealthIndicator.SetActive(true);
        }
        
        StartCriticalHealthEffects();
    }

    private IEnumerator UpdateHealthBarSmooth(float targetValue)
    {
        if (mainHealthSlider == null) yield break;

        float startValue = mainHealthSlider.value;
        float elapsed = 0f;
        float duration = 1f / healthBarSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Use smooth curve for animation
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            mainHealthSlider.value = Mathf.Lerp(startValue, targetValue, smoothT);
            
            yield return null;
        }

        mainHealthSlider.value = targetValue;
    }

    private IEnumerator UpdateBackgroundBarWithDelay(float targetValue)
    {
        yield return new WaitForSeconds(0.3f); // Delay for background bar
        
        if (backgroundHealthSlider == null) yield break;

        float startValue = backgroundHealthSlider.value;
        float elapsed = 0f;
        float duration = 1f / backgroundBarSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            backgroundHealthSlider.value = Mathf.Lerp(startValue, targetValue, smoothT);
            
            yield return null;
        }

        backgroundHealthSlider.value = targetValue;
    }

    private IEnumerator UpdateShieldBarSmooth(float targetValue)
    {
        if (shieldSlider == null) yield break;

        float startValue = shieldSlider.value;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            shieldSlider.value = Mathf.Lerp(startValue, targetValue, smoothT);
            
            yield return null;
        }

        shieldSlider.value = targetValue;
    }

    private void UpdateHealthBarColor(float healthRatio)
    {
        if (healthFillImage != null && healthColorGradient != null)
        {
            Color healthColor = healthColorGradient.Evaluate(healthRatio);
            healthFillImage.color = healthColor;
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null && playerHealth != null)
        {
            int currentHP = Mathf.RoundToInt(playerHealth.GetCurrentHealth());
            int maxHP = Mathf.RoundToInt(playerHealth.GetMaxHealth());
            healthText.text = $"{currentHP} / {maxHP}";
        }
    }

    private void CheckHealthThresholds(float healthRatio)
    {
        // Check if exiting low health
        if (healthRatio > 0.3f && isLowHealth)
        {
            isLowHealth = false;
            isCriticalHealth = false;
            
            if (lowHealthIndicator != null)
            {
                lowHealthIndicator.SetActive(false);
            }
            
            if (criticalHealthIndicator != null)
            {
                criticalHealthIndicator.SetActive(false);
            }
            
            StopLowHealthEffects();
        }
        else if (healthRatio > 0.15f && isCriticalHealth)
        {
            isCriticalHealth = false;
            
            if (criticalHealthIndicator != null)
            {
                criticalHealthIndicator.SetActive(false);
            }
        }
    }

    private void StartLowHealthEffects()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        
        pulseCoroutine = StartCoroutine(PulseHealthBar());
    }

    private void StartCriticalHealthEffects()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        
        pulseCoroutine = StartCoroutine(PulseCriticalHealthBar());
    }

    private void StopLowHealthEffects()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Reset glow
        if (healthBarGlow != null)
        {
            Color glowColor = healthBarGlow.color;
            glowColor.a = 0f;
            healthBarGlow.color = glowColor;
        }
    }

    private IEnumerator PulseHealthBar()
    {
        while (isLowHealth && !isCriticalHealth)
        {
            if (healthBarGlow != null)
            {
                float alpha = Mathf.PingPong(Time.time * pulseSpeed, pulseIntensity);
                Color glowColor = lowHealthGlowColor;
                glowColor.a = alpha;
                healthBarGlow.color = glowColor;
            }
            
            yield return null;
        }
    }

    private IEnumerator PulseCriticalHealthBar()
    {
        while (isCriticalHealth)
        {
            if (healthBarGlow != null)
            {
                float alpha = Mathf.PingPong(Time.time * glowPulseSpeed, pulseIntensity * 2f);
                Color glowColor = lowHealthGlowColor;
                glowColor.a = alpha;
                healthBarGlow.color = glowColor;
            }
            
            // Pulse the health bar itself for critical health
            if (healthFillImage != null)
            {
                float scale = 1f + Mathf.Sin(Time.time * glowPulseSpeed) * 0.1f;
                healthFillImage.transform.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        // Reset scale
        if (healthFillImage != null)
        {
            healthFillImage.transform.localScale = Vector3.one;
        }
    }

    // Public methods for external control
    public void SetHealthBarVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void FlashHealthBar(Color flashColor, float duration = 0.3f)
    {
        StartCoroutine(FlashHealthBarCoroutine(flashColor, duration));
    }

    private IEnumerator FlashHealthBarCoroutine(Color flashColor, float duration)
    {
        if (healthFillImage == null) yield break;

        Color originalColor = healthFillImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 4f, 1f);
            healthFillImage.color = Color.Lerp(originalColor, flashColor, t);
            yield return null;
        }

        healthFillImage.color = originalColor;
    }
}
