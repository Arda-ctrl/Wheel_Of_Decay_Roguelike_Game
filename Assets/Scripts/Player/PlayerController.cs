using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public float moveSpeed;
    public Rigidbody2D theRB;
    public Transform gunArm;
    
    [Header("References")]
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private ElementalAbilityManager elementalAbilityManager;

    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Camera theCam;
    public SpriteRenderer bodySR;
    private float activeMoveSpeed;
    public float dashSpeed = 8f, dashLenght = .5f, dashCooldown = 1f, dashInvisiblity;
    [HideInInspector]
    public float dashCounter;
    private float dashCoolCounter;

    private PlayerInputActions playerInputActions;
    private bool canControl = true;
    
    // Element stack efektleri için değişkenler
    private float speedMultiplier = 1f;
    private float damageMultiplier = 1f;
    private float poisonDamage = 0f;

    void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();

        // Subscribe to input events
        playerInputActions.Player.Movement.performed += OnMovement;
        playerInputActions.Player.Movement.canceled += OnMovement;
        playerInputActions.Player.Look.performed += OnLook;
        playerInputActions.Player.Fire.started += OnFireStart;
        playerInputActions.Player.Fire.canceled += OnFireEnd;
        playerInputActions.Player.Dash.performed += OnDash;
        
        // Test için Overflow ability'si (T tuşu) - Input Action kullanmadan direkt key check yapacağız

        // Get WeaponController reference if not set
        if (weaponController == null)
        {
            weaponController = GetComponent<WeaponController>();
        }
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from input events to prevent memory leaks
        if (playerInputActions != null)
        {
            playerInputActions.Player.Movement.performed -= OnMovement;
            playerInputActions.Player.Movement.canceled -= OnMovement;
            playerInputActions.Player.Look.performed -= OnLook;
            playerInputActions.Player.Fire.started -= OnFireStart;
            playerInputActions.Player.Fire.canceled -= OnFireEnd;
            playerInputActions.Player.Dash.performed -= OnDash;
            
            playerInputActions.Dispose();
        }
    }

    void Start()
    {
        theCam = Camera.main;
        activeMoveSpeed = moveSpeed;
        
        // ElementalAbilityManager'ı bul
        if (elementalAbilityManager == null)
        {
            elementalAbilityManager = GetComponent<ElementalAbilityManager>();
        }
        
        Debug.Log("🎮 PlayerController initialized with Elemental Ability System");
    }

    void Update()
    {
        if (!canControl)
        {
            // Stop all movement and actions when control is disabled
            theRB.linearVelocity = Vector2.zero;
            return;
        }

        // Handle movement with speed multiplier
        theRB.linearVelocity = moveInput * (activeMoveSpeed * speedMultiplier);

        // Handle gun arm rotation
        Vector3 screenPoint = theCam.WorldToScreenPoint(transform.localPosition);
        
        if (mousePosition.x < screenPoint.x)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            gunArm.localScale = new Vector3(-1f, -1f, 1f);
        }
        else
        {
            transform.localScale = Vector3.one;
            gunArm.localScale = Vector3.one;
        }

        // Calculate gun rotation
        Vector2 offset = new Vector2(mousePosition.x - screenPoint.x, mousePosition.y - screenPoint.y);
        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        gunArm.rotation = Quaternion.Euler(0, 0, angle);

        // Handle dash cooldown
        if (dashCounter > 0)
        {
            dashCounter -= Time.deltaTime;
            if (dashCounter <= 0)
            {
                activeMoveSpeed = moveSpeed;
                dashCoolCounter = dashCooldown;
            }
        }
        if (dashCoolCounter > 0)
        {
            dashCoolCounter -= Time.deltaTime;
        }
        
        // Test için T tuşu kontrolü
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestElementalOverflow();
        }
        
        // Mana test için M tuşu kontrolü
        if (Input.GetKeyDown(KeyCode.M))
        {
            TestManaSystem();
        }
    }

    private void OnMovement(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        moveInput = context.ReadValue<Vector2>();
        moveInput.Normalize();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        mousePosition = context.ReadValue<Vector2>();
    }

    private void OnFireStart(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        if (weaponController != null)
        {
            weaponController.StartFiring();
        }
        // Elemental yeteneklerin saldırı bazlı efektlerini tetikle
        if (elementalAbilityManager != null)
        {
            elementalAbilityManager.OnAttack();
        }
    }

    private void OnFireEnd(InputAction.CallbackContext context)
    {
        if (weaponController != null)
        {
            weaponController.StopFiring();
        }
    }

    private void TestElementalOverflow()
    {
        if (!canControl) return;
        
        // Test için ElementalOverflow'u tetikle
        if (elementalAbilityManager != null)
        {
            var overflowAbility = elementalAbilityManager.GetAbility(ElementType.Fire, AbilityType.ElementalOverflow) as ElementalOverflow;
            if (overflowAbility != null)
            {
                Debug.Log("🧪 Testing ElementalOverflow with T key");
                overflowAbility.UseAbility(gameObject, null, new FireElement());
            }
            else
            {
                Debug.Log("❌ ElementalOverflow ability not found");
            }
        }
        else
        {
            Debug.Log("❌ ElementalAbilityManager not found");
        }
    }
    
    private void TestManaSystem()
    {
        var manaController = PlayerManaController.Instance;
        if (manaController != null)
        {
            Debug.Log($"💧 Current Mana: {manaController.GetCurrentMana()}/{manaController.GetMaxMana()} ({manaController.GetManaPercentage() * 100:F1}%)");
            Debug.Log($"💧 PlayerManaController found and working!");
            
            // Test mana consumption
            Debug.Log("🧪 Testing 25 mana consumption...");
            bool success = manaController.ConsumeMana(25f);
            Debug.Log($"🧪 Mana consumption result: {success}");
        }
        else
        {
            Debug.LogError("❌ PlayerManaController not found! Please add PlayerManaController component to Player GameObject!");
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        if (dashCoolCounter <= 0 && dashCounter <= 0)
        {
            activeMoveSpeed = dashSpeed;
            dashCounter = dashLenght;
            PlayerHealthController.Instance.MakeInvincible(dashInvisiblity);
            AudioManager.Instance.PlaySFX(8);
        }
    }

    public void DisableControl()
    {
        canControl = false;
        if (weaponController != null)
        {
            weaponController.StopFiring();
        }
        moveInput = Vector2.zero;
        
        if (theRB != null)
        {
            theRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }
    }

    public void EnableControl()
    {
        if (theRB != null)
        {
            theRB.constraints = RigidbodyConstraints2D.None;
        }
        canControl = true;
    }
    
    // Element stack efektleri için metodlar
    
    /// <summary>
    /// Hız çarpanını ayarlar (Ice stack efekti için)
    /// </summary>
    /// <param name="multiplier">Hız çarpanı (0.8f = %20 yavaşlatma)</param>
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        Debug.Log($"🎮 Player speed multiplier set to: {multiplier}");
    }
    
    /// <summary>
    /// Hasar çarpanını ayarlar (Fire stack efekti için)
    /// </summary>
    /// <param name="multiplier">Hasar çarpanı (1.5f = %50 artış)</param>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
        Debug.Log($"🎮 Player damage multiplier set to: {multiplier}");
    }
    
    /// <summary>
    /// Poison hasarını ayarlar (Poison stack efekti için)
    /// </summary>
    /// <param name="damage">Poison hasarı</param>
    public void SetPoisonDamage(float damage)
    {
        poisonDamage = damage;
        Debug.Log($"🎮 Player poison damage set to: {damage}");
    }
    
    /// <summary>
    /// Mevcut hasar çarpanını döndürür
    /// </summary>
    /// <returns>Hasar çarpanı</returns>
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }
    
    /// <summary>
    /// Mevcut poison hasarını döndürür
    /// </summary>
    /// <returns>Poison hasarı</returns>
    public float GetPoisonDamage()
    {
        return poisonDamage;
    }
}
