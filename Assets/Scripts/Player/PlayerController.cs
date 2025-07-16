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

        // Handle movement
        theRB.linearVelocity = moveInput * activeMoveSpeed;

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
        if (!canControl || weaponController == null) return;
        weaponController.StartFiring();
    }

    private void OnFireEnd(InputAction.CallbackContext context)
    {
        if (weaponController != null)
        {
            weaponController.StopFiring();
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
}
