using UnityEngine;

/// <summary>
/// PlayerElementalIntegration - Mevcut PlayerController'a elemental sistem entegrasyonu
/// Bu sınıf mevcut PlayerController'ınıza eklenebilir veya ayrı bir component olarak kullanılabilir
/// </summary>
public class PlayerElementalIntegration : MonoBehaviour
{
    [Header("Elemental Integration Settings")]
    [SerializeField] private ElementalAbilityManager elementalAbilityManager;
    [SerializeField] private bool enableElementalStrike = true;
    [SerializeField] private bool enableElementalBuff = true;
    [SerializeField] private bool enableElementalProjectile = true;
    
    // Mevcut PlayerController referansı
    private PlayerController playerController;
    
    private void Start()
    {
        Debug.Log("🚀 PlayerElementalIntegration starting...");
        
        // PlayerController'ı bul
        playerController = GetComponent<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogError("❌ PlayerController not found on this GameObject!");
            return;
        }
        else
        {
            Debug.Log("✅ PlayerController found successfully");
        }
        
        // ElementalAbilityManager'ı bul veya oluştur
        if (elementalAbilityManager == null)
        {
            elementalAbilityManager = GetComponent<ElementalAbilityManager>();
            if (elementalAbilityManager == null)
            {
                Debug.Log("🔧 Creating ElementalAbilityManager component...");
                elementalAbilityManager = gameObject.AddComponent<ElementalAbilityManager>();
            }
            else
            {
                Debug.Log("✅ ElementalAbilityManager found on GameObject");
            }
        }
        else
        {
            Debug.Log("✅ ElementalAbilityManager assigned in inspector");
        }
        
        // Elemental sistemi initialize et
        InitializeElementalSystem();
        
        Debug.Log("🎉 PlayerElementalIntegration initialization complete!");
    }
    
    /// <summary>
    /// Elemental sistemi initialize eder
    /// </summary>
    private void InitializeElementalSystem()
    {
        // Ability'leri aktif/pasif yap
        if (elementalAbilityManager != null)
        {
            elementalAbilityManager.SetAbilityActive(AbilityType.Strike, enableElementalStrike);
            elementalAbilityManager.SetAbilityActive(AbilityType.Buff, enableElementalBuff);
            elementalAbilityManager.SetAbilityActive(AbilityType.Projectile, enableElementalProjectile);
        }
        
        Debug.Log("Player Elemental Integration initialized");
    }
    
    /// <summary>
    /// Normal saldırı yapıldığında çağrılır
    /// Mevcut PlayerController'ınızın attack method'unda bu method'u çağırın
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    public void OnPlayerAttack(GameObject target)
    {
        if (elementalAbilityManager != null)
        {
            // Elemental strike uygula
            elementalAbilityManager.UseStrike(target);
            
            // Projectile sayacını artır
            elementalAbilityManager.OnAttack();
        }
    }
    
    /// <summary>
    /// Hasar hesaplaması yapıldığında çağrılır
    /// Mevcut damage calculation'ınızda bu method'u kullanın
    /// </summary>
    /// <param name="baseDamage">Temel hasar</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="elementType">Hasarın element türü</param>
    /// <returns>Buff'lanmış hasar</returns>
    public float CalculateElementalDamage(float baseDamage, GameObject target, ElementType elementType)
    {
        if (elementalAbilityManager != null)
        {
            return elementalAbilityManager.CalculateBuffDamage(baseDamage, target, elementType);
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// Elementi değiştirir
    /// </summary>
    /// <param name="elementType">Yeni element türü</param>
    public void ChangeElement(ElementType elementType)
    {
        if (elementalAbilityManager != null)
        {
            elementalAbilityManager.SetElement(elementType);
        }
    }
    
    /// <summary>
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <param name="active">Aktif mi?</param>
    public void SetAbilityActive(AbilityType abilityType, bool active)
    {
        if (elementalAbilityManager != null)
        {
            elementalAbilityManager.SetAbilityActive(abilityType, active);
        }
    }
    
    /// <summary>
    /// Mevcut element türünü döndürür
    /// </summary>
    /// <returns>Mevcut element türü</returns>
    public ElementType GetCurrentElementType()
    {
        if (elementalAbilityManager != null)
        {
            return elementalAbilityManager.GetCurrentElementType();
        }
        
        return ElementType.Fire; // Default element
    }
    
    /// <summary>
    /// Ability'nin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Ability aktif mi?</returns>
    public bool IsAbilityActive(AbilityType abilityType)
    {
        if (elementalAbilityManager != null)
        {
            return elementalAbilityManager.IsAbilityActive(abilityType);
        }
        
        return false;
    }
    
    /// <summary>
    /// Ability'nin cooldown progress'ini döndürür
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>0-1 arası progress değeri</returns>
    public float GetAbilityCooldownProgress(AbilityType abilityType)
    {
        if (elementalAbilityManager != null)
        {
            return elementalAbilityManager.GetAbilityCooldownProgress(abilityType);
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Tüm ability'leri sıfırlar
    /// </summary>
    public void ResetAllAbilities()
    {
        if (elementalAbilityManager != null)
        {
            elementalAbilityManager.ResetAllAbilities();
        }
    }
    
    // Input handling örnekleri
    private void Update()
    {
        HandleElementalInputs();
    }
    
    /// <summary>
    /// Elemental input'ları handle eder
    /// </summary>
    private void HandleElementalInputs()
    {
        // Element değiştirme input'ları
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("🔥 Key 1 pressed - Changing to FIRE element");
            ChangeElement(ElementType.Fire);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("❄️ Key 2 pressed - Changing to ICE element");
            ChangeElement(ElementType.Ice);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("☠️ Key 3 pressed - Changing to POISON element");
            ChangeElement(ElementType.Poison);
        }
        
        // Ability toggle input'ları
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("🛡️ Key Q pressed - Toggling BUFF ability");
            ToggleAbility(AbilityType.Buff);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("🎯 Key E pressed - Toggling PROJECTILE ability");
            ToggleAbility(AbilityType.Projectile);
        }
    }
    
    /// <summary>
    /// Ability'yi toggle eder
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    private void ToggleAbility(AbilityType abilityType)
    {
        bool currentState = IsAbilityActive(abilityType);
        SetAbilityActive(abilityType, !currentState);
        
        Debug.Log($"{abilityType} ability {(currentState ? "deactivated" : "activated")}");
    }
}

/// <summary>
/// Mevcut PlayerController'ınıza entegrasyon için örnek kod:
/// 
/// 1. PlayerController'ınıza ElementalAbilityManager ekleyin:
/// [SerializeField] private ElementalAbilityManager elementalAbilityManager;
/// 
/// 2. Attack method'unuzda elemental strike'ı çağırın:
/// public void Attack(GameObject target)
/// {
///     // Mevcut attack logic'iniz...
///     
///     // Elemental strike uygula
///     if (elementalAbilityManager != null)
///     {
///         elementalAbilityManager.UseStrike(target);
///         elementalAbilityManager.OnAttack(); // Projectile için
///     }
/// }
/// 
/// 3. Damage calculation'ınızda elemental buff'ı kullanın:
/// public float CalculateDamage(float baseDamage, GameObject target, ElementType elementType)
/// {
///     if (elementalAbilityManager != null)
///     {
///         return elementalAbilityManager.CalculateBuffDamage(baseDamage, target, elementType);
///     }
///     return baseDamage;
/// }
/// 
/// 4. Düşman prefab'larına ElementStack component'i ekleyin
/// </summary> 