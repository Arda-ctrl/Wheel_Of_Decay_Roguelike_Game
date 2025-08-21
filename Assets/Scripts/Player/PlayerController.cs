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
    private float velocitySmoothTime = 0.05f; // Normal tepki hızı
    private float velocityStopThreshold = 0.01f; // Bu eşiğin altındaki hızlar sıfırlanacak
    
    // Hareket hızı ve yön hafızası için değişkenler
    private float lastSignificantSpeed = 0f;
    private bool wasMovingSignificantly = false;
    private float speedRetentionTime = 0.05f;  // Çok kısa hız korunma süresi (S-A geçişi sorununu çözecek)
    private float lastMoveTime = 0f;          // Son hareket zamanı
    private float accelerationMultiplier = 1f; // Hızlanma çarpanı (daha hızlı hareket için)
    private Vector2 previousMoveInput = Vector2.zero; // Önceki hareket girişi (tuş değişimlerini takip için)

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

                // Cult of the Lamb tarzı dinamik hareket - ama hız tam moveSpeed'e göre
        // Hedef hızı hesapla - tam olarak moveSpeed değerini kullan
        float effectiveSpeed = activeMoveSpeed * speedMultiplier; // Acceleration kullanma
        targetVelocity = moveInput * effectiveSpeed;
        
        // Hareket hızını yönet - SADECE TUŞ YOKKEN DURDUR, tuş değişimlerinde HIZLA DEVAM ET
        if (moveInput.sqrMagnitude < 0.005f) // Hiç tuş basılı değil (tam durma)
        {
            // Hiç tuş basılmadığında hızla dur
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, Time.deltaTime * 15f);
            
            // Eğer hız çok düşükse tamamen durdur
            if (currentVelocity.sqrMagnitude < velocityStopThreshold)
            {
                currentVelocity = Vector2.zero;
                wasMovingSignificantly = false; // Hareket hafızasını sıfırla
            }
        }
        else // TUŞ BASILIYSA - BASİT VE DİREKT HAREKET
        {
            // PROBLEM ÇÖZÜMÜ: Karmaşık kontrolleri kaldır, basit ve hızlı hareket
            Vector2 refVel = Vector2.zero;
            
            // Tuş basılıysa HIZLA hedef hıza ulaş (S-A geçişi gibi durumlar için)
            float smoothTime = velocitySmoothTime * 0.3f; // Çok hızlı tepki
            
            // Eğer zaten hareket ediyorsa ve yön değiştiriyorsa
            if (currentVelocity.sqrMagnitude > 0.3f)
            {
                // MEVCUT HIZI KORU - sadece yönü değiştir (S->A geçişi için kritik)
                float currentSpeed = Mathf.Max(currentVelocity.magnitude, effectiveSpeed * 0.8f);
                targetVelocity = moveInput.normalized * currentSpeed;
                smoothTime = velocitySmoothTime * 0.2f; // Çok hızlı yön değişimi
            }
            else
            {
                // Hareket başlangıcı
                targetVelocity = moveInput * effectiveSpeed;
                smoothTime = velocitySmoothTime * 0.4f;
            }
            
            // Hızlı ve direkt hareket
            currentVelocity = Vector2.SmoothDamp(
                currentVelocity,
                targetVelocity,
                ref refVel,
                smoothTime
            );
            
            // Hareket durumunu güncelle
            lastSignificantSpeed = currentVelocity.magnitude;
            wasMovingSignificantly = true;
            lastMoveTime = Time.time;
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
        
        // Önceki değeri sakla
        previousMoveInput = moveInput;
        
        // Yeni hareket değerini al ve normalize et
        moveInput = context.ReadValue<Vector2>();
        
        // Basit kontrol: Eğer hareket varsa normalize et
        if (moveInput.sqrMagnitude > 0.1f) 
        {
            moveInput = moveInput.normalized;
            
            // Hareket durumunu güncelle
            wasMovingSignificantly = true;
            lastMoveTime = Time.time;
        }
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
        
                 // Fiziksel hareket ve animasyon senkronizasyonu için DAHA SIKI kontrol
         // İdle animasyonu oynarken hareket etme sorununu çöz
         bool isMoving = velocity.sqrMagnitude > 0.05f; // Eşiği yükselttik
         
         // Idle ve hareket arasında kesin çizgi için
         bool isDefinitelyMoving = velocity.sqrMagnitude > 0.2f;
         
         // Mevcut animasyon durumunu kontrol et
         bool wasMovingAnim = animator.GetBool(IsMovingHash);
         
         // Animasyon ve fiziksel hareket arasındaki uyumsuzluğu gidermek için kararlı geçişler
         if (isDefinitelyMoving)
         {
             // Kesinlikle hareket ediyorsa animasyon da öyle olmalı
             animator.SetBool(IsMovingHash, true);
         }
         else if (!isMoving && wasMovingAnim) 
         {
             // Hareket kesildiğinde animasyonu da durdur
             animator.SetBool(IsMovingHash, false);
         }
         else if (isMoving && !wasMovingAnim)
         {
             // Hareket başladığında animasyon da başlasın
             animator.SetBool(IsMovingHash, true);
         }
        
        if (isMoving)
        {
            // Cult of the Lamb tarzı hareket için animasyon kontrolü
            float velocityMagnitude = velocity.magnitude;
            
                         // Yönü belirle (hareket yönü veya bakış yönü)
             float direction = 0f;
             
             // Öncelikle herhangi bir anlamlı hareket olup olmadığını kontrol et (x VEYA y yönünde)
             bool hasSignificantMovement = velocity.sqrMagnitude > 0.05f;
             
             // Mevcut yönü sakla (yön değişimlerini takip etmek için)
             float prevDirection = animator.GetFloat(MovementSpeedHash);
             prevDirection = Mathf.Sign(prevDirection); // İşareti al (+ veya -)
             if (Mathf.Approximately(prevDirection, 0)) // Eğer 0 ise
                 prevDirection = transform.localScale.x; // Karakter bakış yönünü kullan
             
             // X yönünde anlamlı bir hareket varsa o yönü kullan
             if (Mathf.Abs(velocity.x) > 0.1f) 
             {
                 direction = Mathf.Sign(velocity.x);
             }
             // Yoksa ama y yönünde hareket varsa veya çapraz hareket varsa mevcut yönü koru
             else if (hasSignificantMovement)
             {
                 // Yön değişimini önlemek için mevcut animasyon yönünü koru
                 direction = prevDirection != 0 ? prevDirection : transform.localScale.x;
             }
             // Hiç hareket yoksa karakter bakış yönünü kullan
             else
             {
                 direction = transform.localScale.x;
             }
            
                         // Normalize hız: 0 = durma, 1 = maximum hız
            float normalizedSpeed = Mathf.Clamp01(velocityMagnitude / (moveSpeed * speedMultiplier));
            
            // BASIT HIZ KONTROLÜ - karmaşık korumaları kaldır
            // Eğer fiziksel olarak hareket ediyorsa, animasyon da hareket olsun
            if (velocityMagnitude > 0.3f)
            {
                normalizedSpeed = Mathf.Clamp01(velocityMagnitude / (moveSpeed * speedMultiplier));
            }
            
                         // Hız değerleri için eşikler
             float jogThreshold = 0.08f; // %8 hız = jogging başlangıcı (daha kolay jog)
             float runThreshold = 0.8f; // %80 hız = tam koşu
            
                         // Hedef animasyon değerini belirle, smooth geçiş sağla
             float animValue;
             
             if (dashCounter > 0) // Dash sırasında
             {
                 // Koşma animasyonuna hızlı geçiş - yöne göre 2 veya -2
                 animValue = direction > 0 ? 2f : -2f;
             }
             else if (normalizedSpeed > runThreshold) // Tam koşma (hızlı)
             {
                 // Run animasyonu - hedef 2 veya -2
                 float targetValue = direction > 0 ? 2f : -2f;
                 
                 // Hız bazlı kademeli geçiş
                 float speedRatio = Mathf.InverseLerp(runThreshold, 1f, normalizedSpeed);
                 animValue = Mathf.Lerp(1f, targetValue, speedRatio);
                 if (direction < 0) animValue = -animValue;
             }
             else if (normalizedSpeed > jogThreshold) // Jogging (yürümeden hızlı)
             {
                 // Jog animasyonu - yöne göre 1 veya -1
                 float jogValue = direction > 0 ? 1f : -1f;
                 
                 // Jog ile run arası geçiş
                 float speedRatio = Mathf.InverseLerp(jogThreshold, runThreshold, normalizedSpeed);
                 animValue = jogValue;
             }
             else if (normalizedSpeed > 0.01f) // Hafif hareket - yürüme
             {
                 // Yürüme animasyonu (hedef 0)
                 animValue = 0f;
             }
             else // Çok düşük hız veya hareket yok
             {
                 animValue = 0f;
             }
             
             // Çapraz hareket sırasında da animasyonun doğru çalışması için
             if (isMoving && velocity.sqrMagnitude > 0.01f)
             {
                 // Çaprazlama sırasında jog'da kalmayı sağla
                 if (Mathf.Abs(animValue) < 0.8f && normalizedSpeed >= jogThreshold)
                 {
                     // Eğer animasyon değeri jog seviyesinin altındaysa ve yeterince hareket varsa
                     // jogda kalması için değeri yön bazlı bir değere zorla
                     float minJogValue = direction > 0 ? 1f : -1f;
                     
                     // Mevcut değer çok düşükse zorla
                     if (Mathf.Abs(animValue) < 0.9f)
                     {
                         animValue = minJogValue;
                     }
                 }
                 
                 // Kesinlikle hareket olduğundan emin ol
                 animator.SetBool(IsMovingHash, true);
             }
            
                         // Çok küçük değerleri yuvarlayıp temizle (floating point hassasiyeti problemlerini önle)
             if (Mathf.Abs(animValue) < 0.01f)
                 animValue = 0f;
                 
             // Smooth animation değişimi için lerp kullan - YÖN DEĞİŞİMİNDE ÖZEL İŞLEM
             float currentAnimValue = animator.GetFloat(MovementSpeedHash);
             float smoothAnimValue;
             
             // Yön değişimi kontrolü (pozitiften negatife veya negatiften pozitife geçiş)
             bool isChangingDirection = (currentAnimValue > 0 && animValue < 0) || (currentAnimValue < 0 && animValue > 0);
             
             // Cult of the Lamb tarzı - daha hızlı ve daha kararlı animasyon değişimi
             if (isChangingDirection)
             {
                 // Yön değişiminde mutlak değeri koru, işareti değiştir (0'dan geçmeden)
                 float currentAbsValue = Mathf.Abs(currentAnimValue);
                 float targetAbsValue = Mathf.Abs(animValue);
                 
                 // Cult of the Lamb tarzı çok hızlı yön değişimi (animasyon durmaları yok)
                 float changeDirectionSpeed = Time.deltaTime * 25f; // Çok daha hızlı yön değişimi
                 float absResult = Mathf.Lerp(currentAbsValue, targetAbsValue, changeDirectionSpeed);
                 
                 // İşaret kontrolü - hedef değerin işaretini kullan (animasyon sıfırlanmadan)
                 smoothAnimValue = absResult * Mathf.Sign(animValue);
                 
                 // Koşu animasyonları arasında anlık değişim - Cult of the Lamb tarzı
                 if (Mathf.Abs(currentAnimValue) > 0.9f && Mathf.Abs(animValue) > 0.9f) 
                 {
                     // Koşu durumlarında hızlı, neredeyse anlık geçiş
                     smoothAnimValue = animValue;
                 }
             }
             else
             {
                 // Normal geçişlerde de hızlı tepki - Cult of the Lamb tarzı
                 float lerpSpeed;
                 
                 // Hareket durumuna göre geçiş hızını ayarla
                 if (Mathf.Abs(animValue) > 1.5f) // Koşma (2/-2)
                 {
                     // Koşmaya geçiş çok hızlı olsun
                     lerpSpeed = Time.deltaTime * 20f;
                 }
                 else if (Mathf.Abs(animValue) > 0.5f) // Jogging (1/-1)
                 {
                     // Jog'a geçiş de hızlı ama o kadar değil
                     lerpSpeed = Time.deltaTime * 15f;
                 }
                 else // Yürüme veya durma (0)
                 {
                     // Daha yumuşak geçiş
                     lerpSpeed = Time.deltaTime * 10f;
                 }
                 
                 // Mevcut değere göre ekstra ayarlama (daha hızlı değişim için)
                 if (Mathf.Abs(currentAnimValue - animValue) > 1.0f)
                 {
                     // Büyük değişimlerde daha hızlı tepki
                     lerpSpeed *= 1.5f;
                 }
                 
                 smoothAnimValue = Mathf.Lerp(currentAnimValue, animValue, lerpSpeed);
             }
             
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
