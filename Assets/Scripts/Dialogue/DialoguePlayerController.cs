using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class DialoguePlayerController : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
        // Subscribe to dialogue events
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.onDialogueStart.AddListener(DisablePlayerControl);
            DialogueManager.Instance.onDialogueEnd.AddListener(EnablePlayerControl);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from dialogue events
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.onDialogueStart.RemoveListener(DisablePlayerControl);
            DialogueManager.Instance.onDialogueEnd.RemoveListener(EnablePlayerControl);
        }
    }

    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    private void EnablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
} 