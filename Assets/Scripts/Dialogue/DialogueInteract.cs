using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class DialogueInteract : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private GameObject interactionPrompt;

    

    private bool isInRange;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        // Initialize input actions
        playerInputActions = new PlayerInputActions();
        
        // Ensure trigger collider
        GetComponent<Collider2D>().isTrigger = true;
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Interact.performed += OnInteract;
        playerInputActions.Player.DialogueAdvance.performed += OnDialogueAdvance;
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        playerInputActions.Player.Interact.performed -= OnInteract;
        playerInputActions.Player.DialogueAdvance.performed -= OnDialogueAdvance;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from input events to prevent memory leaks
        if (playerInputActions != null)
        {
            playerInputActions.Player.Interact.performed -= OnInteract;
            playerInputActions.Player.DialogueAdvance.performed -= OnDialogueAdvance;
            playerInputActions.Dispose();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isInRange)
        {
            if (DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.ForceEndDialogue();
            }
            else
            {
                DialogueManager.Instance.StartDialogue(dialogueData);
            }
        }
    }

    private void OnDialogueAdvance(InputAction.CallbackContext context)
    {
        if (DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.DisplayNextLine();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
} 