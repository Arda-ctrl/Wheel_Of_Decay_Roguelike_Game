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
    [SerializeField] private Animator animator;

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
    
    // Animation parameters
    private readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private readonly int MovementSpeedHash = Animator.StringToHash("movementSpeed");
    
    // Hareket dinamiği için değişkenler
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;
    private float velocitySmoothTime = 0.05f; // Daha hızlı tepki için düşürüldü
    private float velocityStopThreshold = 0.02f; // Bu eşiğin altındaki hızlar sıfırlanacak

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
        
        // Get animator if not already assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("No Animator component found on player!");
            }
        }
        
        // OrbStackManager'ı initialize et
        InitializeOrbStackManager();
        
        Debug.Log("🎮 PlayerController initialized with Elemental Ability System");
    }
    
    /// <summary>
    /// OrbStackManager'ı initialize eder
    /// </summary>
    private void InitializeOrbStackManager()
    {
        // OrbStackManager yoksa oluştur
        if (OrbStackManager.Instance == null)
        {
            GameObject orbManagerGO = new GameObject("OrbStackManager");
            orbManagerGO.AddComponent<OrbStackManager>();
            Debug.Log("🔮 OrbStackManager created and initialized");
        }
        else
        {
            Debug.Log("🔮 OrbStackManager already exists");
        }
    }

    void Update()
    {
        if (!canControl)
        {
            // Stop all movement and actions when control is disabled
            theRB.linearVelocity = Vector2.zero;
            UpdateAnimationState(Vector2.zero);
            return;
        }

        // Cult of the Lamb tarzı dinamik hareket
        // Hedef hızı hesapla
        targetVelocity = moveInput * (activeMoveSpeed * speedMultiplier);
        
                 // Giriş olmadığında (durma durumu) - daha hassas hareket algılama
         if (moveInput.sqrMagnitude < 0.005f) // Daha düşük eşik değeri
         {
             // Hızı daha çabuk azalt
             currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, Time.deltaTime * 12f); // Biraz daha yavaş durdurma
            
            // Eğer hız çok düşükse tamamen durdur
            if (currentVelocity.sqrMagnitude < velocityStopThreshold)
            {
                currentVelocity = Vector2.zero;
            }
        }
        else
        {
            // Mevcut hızı hedefe doğru yumuşak bir şekilde değiştir
            Vector2 refVel = Vector2.zero; // SmoothDamp için referans
            currentVelocity = Vector2.SmoothDamp(
                currentVelocity,
                targetVelocity,
                ref refVel,
                dashCounter > 0 ? velocitySmoothTime * 0.5f : velocitySmoothTime
            );
        }
        
        // Hesaplanan hızı uygula
        theRB.linearVelocity = currentVelocity;
        
        // Update animation state based on current velocity
        UpdateAnimationState(currentVelocity);

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
        
        // Orb sistem test için O tuşu kontrolü
        if (Input.GetKeyDown(KeyCode.O))
        {
            TestOrbSystem();
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
             
             // Dash sırasında animasyon hedefini belirle (direkt atama yapma)
             // İlerideki Update döngüsünde MovementSpeed smooth bir şekilde hedefe ilerleyecek
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
    
    private void TestOrbSystem()
    {
        var orbManager = OrbStackManager.Instance;
        if (orbManager != null)
        {
            Debug.Log($"🔮 Active Orbs: {orbManager.GetActiveOrbCount()}/4");
            Debug.Log($"🔮 Fire Stacks: {orbManager.GetCollectedStacks(ElementType.Fire)}");
            Debug.Log($"🔮 Ice Stacks: {orbManager.GetCollectedStacks(ElementType.Ice)}");
            Debug.Log($"🔮 Poison Stacks: {orbManager.GetCollectedStacks(ElementType.Poison)}");
        }
        else
        {
            Debug.Log("❌ OrbStackManager not found");
        }
    }
    
    /// <summary>
    /// Updates the animation state based on player velocity
    /// </summary>
    /// <param name="velocity">Current movement velocity</param>
    private void UpdateAnimationState(Vector2 velocity)
    {
        if (animator == null) return;
        
                 // Determine if moving (daha hassas eşik değeri - hafif dokunuşları algıla)
         bool isMoving = velocity.sqrMagnitude > 0.005f; // Daha düşük eşik değeri
         animator.SetBool(IsMovingHash, isMoving);
        
        if (isMoving)
        {
            // Cult of the Lamb tarzı hareket için animasyon kontrolü
            float velocityMagnitude = velocity.magnitude;
            
            // Yönü belirle (hareket yönü veya bakış yönü)
            float direction = 0f;
            if (Mathf.Abs(velocity.x) > 0.1f) // X yönünde anlamlı bir hareket varsa
            {
                direction = Mathf.Sign(velocity.x);
            }
            else // Yoksa karakterin bakış yönünü kullan
            {
                direction = transform.localScale.x;
            }
            
            // Normalize hız: 0 = durma, 1 = maximum hız
            float normalizedSpeed = Mathf.Clamp01(velocityMagnitude / (moveSpeed * speedMultiplier));
            
                         // Hız değeri için farklı bir eşik kullanıyoruz
             float runThreshold = 0.8f; // %80 ve üstü hızda koşma - daha geniş yürüme aralığı
            
                         // Hedef animasyon değerini belirle, smooth geçiş sağla
             float animValue;
             if (dashCounter > 0) // Dash sırasında
             {
                 // Koşma animasyonuna hızlı geçiş - yöne göre 2 veya -2
                 animValue = direction > 0 ? 2f : -2f;
             }
             else if (normalizedSpeed > runThreshold) // Normal koşma
             {
                 // Koşma animasyonu - aradaki değerlerden geçerek hedefe ulaşacak
                 float targetValue = direction > 0 ? 2f : -2f;
                 
                 // Koşma durumundaki hedef değere göre kademeli olarak arttır
                 if (direction > 0) // Sağa koşma
                 {
                     // 0'dan 2'ye doğru kademeli geçiş
                     animValue = Mathf.Lerp(0f, targetValue, normalizedSpeed);
                 }
                 else // Sola koşma
                 {
                     // 0'dan -2'ye doğru kademeli geçiş
                     animValue = Mathf.Lerp(0f, targetValue, normalizedSpeed);
                 }
             }
             else 
             {
                 // Yürüme animasyonu (hedef hep 0)
                 animValue = 0f;
             }
             
             // Hareket varsa animasyon tetiklensin
             if (isMoving && normalizedSpeed > 0.05f)
             {
                 // Yürüme animasyonunun aktifleştiğinden emin ol
                 animator.SetBool(IsMovingHash, true);
             }
            
                         // Çok küçük değerleri yuvarlayıp temizle (floating point hassasiyeti problemlerini önle)
             if (Mathf.Abs(animValue) < 0.01f)
                 animValue = 0f;
                 
             // Smooth animation değişimi için lerp kullan
             float currentAnimValue = animator.GetFloat(MovementSpeedHash);
             // Daha yavaş ve smooth geçiş için lerp hızını azalttık
             float smoothAnimValue = Mathf.Lerp(currentAnimValue, animValue, Time.deltaTime * 5f); // Daha yavaş, smooth geçiş
             
             // Çok küçük değerleri temizle (sadece çok yakın olanlar)
             if (Mathf.Abs(smoothAnimValue) < 0.01f) 
             {
                 smoothAnimValue = 0f;
             }
                
            animator.SetFloat(MovementSpeedHash, smoothAnimValue);
        }
        else
        {
            // Hareket yoksa animasyon değerini hızla sıfıra getir
            animator.SetFloat(MovementSpeedHash, 0f);
        }
    }
}
