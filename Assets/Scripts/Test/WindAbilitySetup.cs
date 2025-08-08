using UnityEngine;

/// <summary>
/// Wind Ability Setup - Wind ability'lerini ElementalAbilityManager'a otomatik olarak ekler
/// </summary>
public class WindAbilitySetup : MonoBehaviour
{
    [Header("Wind Ability Assets")]
    [SerializeField] private ElementalAbilityData windStrikeAbility;
    [SerializeField] private ElementalAbilityData windProjectileAbility;
    [SerializeField] private ElementalAbilityData windBuffAbility;
    
    private void Start()
    {
        SetupWindAbilities();
    }
    
    /// <summary>
    /// Wind ability'lerini ElementalAbilityManager'a ekler
    /// </summary>
    private void SetupWindAbilities()
    {
        var player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogError("PlayerController.Instance is null!");
            return;
        }
        
        var elementalManager = player.GetComponent<ElementalAbilityManager>();
        if (elementalManager == null)
        {
            Debug.LogError("ElementalAbilityManager not found on player!");
            return;
        }
        
        // Load wind abilities from Resources if not assigned
        if (windStrikeAbility == null)
        {
            windStrikeAbility = Resources.Load<ElementalAbilityData>("SO/Ability/Ability/Wind/(Wind)Strike");
        }
        
        if (windProjectileAbility == null)
        {
            windProjectileAbility = Resources.Load<ElementalAbilityData>("SO/Ability/Ability/Wind/(Projectile)Projectile");
        }
        
        if (windBuffAbility == null)
        {
            windBuffAbility = Resources.Load<ElementalAbilityData>("SO/Ability/Ability/Wind/(Fire)Buff");
        }
        
        // Add wind abilities to the manager
        if (windStrikeAbility != null)
        {
            Debug.Log($"✅ Loaded Wind Strike ability: {windStrikeAbility.abilityName}");
        }
        else
        {
            Debug.LogError("❌ Failed to load Wind Strike ability!");
        }
        
        if (windProjectileAbility != null)
        {
            Debug.Log($"✅ Loaded Wind Projectile ability: {windProjectileAbility.abilityName}");
        }
        else
        {
            Debug.LogError("❌ Failed to load Wind Projectile ability!");
        }
        
        if (windBuffAbility != null)
        {
            Debug.Log($"✅ Loaded Wind Buff ability: {windBuffAbility.abilityName}");
        }
        else
        {
            Debug.LogError("❌ Failed to load Wind Buff ability!");
        }
    }
    
    /// <summary>
    /// Test wind ability functionality
    /// </summary>
    [ContextMenu("Test Wind Ability")]
    public void TestWindAbility()
    {
        var player = PlayerController.Instance;
        if (player == null) return;
        
        var elementalManager = player.GetComponent<ElementalAbilityManager>();
        if (elementalManager == null) return;
        
        // Find an enemy to test with
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            var testTarget = enemies[0];
            Debug.Log($"🧪 Testing wind ability on {testTarget.name}");
            
            // Use wind strike
            elementalManager.UseStrike(testTarget);
        }
        else
        {
            Debug.LogWarning("No enemies found to test with!");
        }
    }
} 