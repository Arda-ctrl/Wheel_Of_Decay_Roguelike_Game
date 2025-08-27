using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController Instance;
    
    [Header("Legacy Compatibility")]
    [Tooltip("Use new PlayerHealthSystem instead. This is kept for backward compatibility.")]
    [SerializeField] private bool useLegacySystem = false;
    public int currentHealth;
    public int maxHealth;
    public float damageInvisibleLenght = 1f;
    private float invisCount;

    [SerializeField] private SpriteRenderer playerSprite;

    private void Awake()
    {
        Instance = this;
    }
    
    private PlayerHealthSystem newHealthSystem;
    private PlayerController_HealthIntegration healthIntegration;

    void Start()
    {
        // Try to find new health system components
        newHealthSystem = GetComponent<PlayerHealthSystem>();
        healthIntegration = GetComponent<PlayerController_HealthIntegration>();
        
        if (!useLegacySystem && newHealthSystem != null)
        {
            Debug.Log("Using new PlayerHealthSystem");
            return; // New system will handle initialization
        }
        
        // Legacy system initialization
        if (playerSprite == null)
        {
            playerSprite = GetComponentInChildren<SpriteRenderer>();
        }

        currentHealth = maxHealth;

        if (UI_Controller.Instance != null)
        {
            if (UI_Controller.Instance.healthSlider != null)
            {
                UI_Controller.Instance.healthSlider.maxValue = maxHealth;
                UI_Controller.Instance.healthSlider.value = currentHealth;
            }
            if (UI_Controller.Instance.healthText != null)
            {
                UI_Controller.Instance.healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
            }
        }
    }

    void Update()
    {
        // Skip legacy update if using new system
        if (!useLegacySystem && newHealthSystem != null)
        {
            return;
        }
        
        // Legacy invincibility system
        if (invisCount > 0)
        {
            invisCount -= Time.deltaTime;

            if (invisCount <= 0 && playerSprite != null)
            {
                SetPlayerAlpha(1f);
            }
        }
    }

    public void DamagePlayer()
    {
        // Use new health system if available
        if (!useLegacySystem && healthIntegration != null)
        {
            healthIntegration.DamagePlayer();
            return;
        }
        
        // Legacy system
        if (invisCount <= 0)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(11);
            }
            
            currentHealth--;

            invisCount = damageInvisibleLenght;

            SetPlayerAlpha(0.5f);

            if (currentHealth <= 0)
            {
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.gameObject.SetActive(false);
                }
                
                if (UI_Controller.Instance != null && UI_Controller.Instance.deathScreen != null)
                {
                    UI_Controller.Instance.deathScreen.SetActive(true);
                }
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayGameOver();
                    AudioManager.Instance.PlaySFX(8);
                }
            }

            if (UI_Controller.Instance != null)
            {
                if (UI_Controller.Instance.healthSlider != null)
                {
                    UI_Controller.Instance.healthSlider.value = currentHealth;
                }
                if (UI_Controller.Instance.healthText != null)
                {
                    UI_Controller.Instance.healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
                }
            }
        }
    }

    public void MakeInvincible(float lenght)
    {
        invisCount = lenght;
        SetPlayerAlpha(0.5f);
    }

    public void HealPlayer(int healAmount)
    {
        // Use new health system if available
        if (!useLegacySystem && healthIntegration != null)
        {
            healthIntegration.HealPlayer(healAmount);
            return;
        }
        
        // Legacy system
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (UI_Controller.Instance != null)
        {
            if (UI_Controller.Instance.healthSlider != null)
            {
                UI_Controller.Instance.healthSlider.value = currentHealth;
            }
            if (UI_Controller.Instance.healthText != null)
            {
                UI_Controller.Instance.healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
            }
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
}
