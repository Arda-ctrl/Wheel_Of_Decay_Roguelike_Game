using UnityEngine;

/// <summary>
/// PlayerController extension for health system integration
/// This component handles the integration between PlayerController and PlayerHealthSystem
/// </summary>
public class PlayerController_HealthIntegration : MonoBehaviour
{
    [Header("Health Integration Settings")]
    [SerializeField] private float movementSpeedReduction = 0.5f; // When chilled or frozen
    [SerializeField] private bool disableMovementWhenFrozen = true;
    [SerializeField] private float poisonMovementPenalty = 0.8f; // 20% speed reduction when poisoned
    
    private PlayerController playerController;
    private PlayerHealthSystem healthSystem;
    private float originalMoveSpeed;
    private bool wasControlEnabled = true;

    private void Start()
    {
        InitializeComponents();
        SubscribeToHealthEvents();
    }

    private void InitializeComponents()
    {
        playerController = GetComponent<PlayerController>();
        healthSystem = GetComponent<PlayerHealthSystem>();
        
        if (playerController != null)
        {
            originalMoveSpeed = playerController.moveSpeed;
        }
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found! This component requires PlayerController.");
        }
        
        if (healthSystem == null)
        {
            Debug.LogError("PlayerHealthSystem not found! This component requires PlayerHealthSystem.");
        }
    }

    private void SubscribeToHealthEvents()
    {
        if (healthSystem != null)
        {
            healthSystem.OnPlayerDeath += OnPlayerDeath;
            healthSystem.OnLowHealth += OnLowHealth;
            healthSystem.OnCriticalHealth += OnCriticalHealth;
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnPlayerDeath -= OnPlayerDeath;
            healthSystem.OnLowHealth -= OnLowHealth;
            healthSystem.OnCriticalHealth -= OnCriticalHealth;
        }
    }

    private void Update()
    {
        if (healthSystem == null || playerController == null) return;
        
        HandleStatusEffectMovement();
    }

    private void HandleStatusEffectMovement()
    {
        float speedMultiplier = 1f;
        bool canMove = true;

        // Handle frozen status
        if (healthSystem.HasStatus(StatusEffectType.Frozen))
        {
            if (disableMovementWhenFrozen)
            {
                canMove = false;
            }
            else
            {
                speedMultiplier *= movementSpeedReduction;
            }
        }
        // Handle chilled status
        else if (healthSystem.HasStatus(StatusEffectType.Chilled))
        {
            speedMultiplier *= movementSpeedReduction;
        }

        // Handle poison status
        if (healthSystem.HasStatus(StatusEffectType.Poisoned))
        {
            speedMultiplier *= poisonMovementPenalty;
        }

        // Apply movement changes
        if (!canMove && wasControlEnabled)
        {
            // Disable movement completely
            DisablePlayerMovement();
            wasControlEnabled = false;
        }
        else if (canMove && !wasControlEnabled)
        {
            // Re-enable movement
            EnablePlayerMovement();
            wasControlEnabled = true;
        }
        else if (canMove)
        {
            // Update movement speed
            UpdateMovementSpeed(speedMultiplier);
        }
    }

    private void DisablePlayerMovement()
    {
        if (playerController != null)
        {
            // You might need to add a public method to PlayerController to disable movement
            // For now, we'll set speed to 0
            playerController.moveSpeed = 0f;
        }
    }

    private void EnablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.moveSpeed = originalMoveSpeed;
        }
    }

    private void UpdateMovementSpeed(float multiplier)
    {
        if (playerController != null)
        {
            playerController.moveSpeed = originalMoveSpeed * multiplier;
        }
    }

    private void OnPlayerDeath()
    {
        // Handle player death
        Debug.Log("Player died!");
        
        // Disable player controls
        DisablePlayerMovement();
        
        // You can add additional death handling here
        // like playing death animation, disabling other components, etc.
    }

    private void OnLowHealth()
    {
        Debug.Log("Player health is low!");
        
        // You can add low health effects here
        // like screen effects, sound warnings, etc.
    }

    private void OnCriticalHealth()
    {
        Debug.Log("Player health is critical!");
        
        // You can add critical health effects here
        // like intense screen effects, heartbeat sounds, etc.
    }

    // Public methods for external damage dealing
    public void DealDamageToPlayer(float amount, PlayerHealthSystem.DamageType damageType = PlayerHealthSystem.DamageType.Normal)
    {
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(amount, damageType);
        }
    }

    public void HealPlayer(float amount)
    {
        if (healthSystem != null)
        {
            healthSystem.Heal(amount);
        }
    }

    public void ApplyStatusEffect(StatusEffectType statusType, float duration)
    {
        if (healthSystem != null)
        {
            healthSystem.ApplyStatus(statusType, duration);
        }
    }

    // Compatibility methods with old system
    public void DamagePlayer()
    {
        DealDamageToPlayer(1f); // Old system used 1 point of damage
    }

    public void MakeInvincible(float duration)
    {
        // This would require adding a public method to PlayerHealthSystem
        // or handling invincibility differently
        Debug.Log($"Making player invincible for {duration} seconds");
    }

    // Getters for external access
    public float GetCurrentHealth()
    {
        return healthSystem != null ? healthSystem.GetCurrentHealth() : 0f;
    }

    public float GetMaxHealth()
    {
        return healthSystem != null ? healthSystem.GetMaxHealth() : 0f;
    }

    public float GetHealthRatio()
    {
        return healthSystem != null ? healthSystem.HealthRatio : 0f;
    }

    public bool IsPlayerDead()
    {
        return GetCurrentHealth() <= 0f;
    }

    public bool IsInvincible()
    {
        return healthSystem != null ? healthSystem.IsInvincible : false;
    }
}
