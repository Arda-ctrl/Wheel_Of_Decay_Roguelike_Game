using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI npcNameText;

    [Header("Typewriter Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private AudioSource typingSoundSource;
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField, Range(0f, 1f)] private float typingSoundVolume = 0.1f;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private Coroutine typewriterCoroutine;
    private bool isDestroyed = false;
    //private float lastPlayTime;
    private const float MIN_SOUND_INTERVAL = 0.05f;

    [SerializeField]
    public UnityEvent onDialogueStart;
    [SerializeField]
    public UnityEvent onDialogueEnd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize events if they haven't been
        if (onDialogueStart == null)
            onDialogueStart = new UnityEvent();
        if (onDialogueEnd == null)
            onDialogueEnd = new UnityEvent();

        // Configure audio source
        if (typingSoundSource != null)
        {
            typingSoundSource.playOnAwake = false;
            typingSoundSource.loop = false;
            typingSoundSource.volume = typingSoundVolume;
        }
    }
    
    private void OnDestroy()
    {
        isDestroyed = true;
        StopTypingSound();
        
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (isDialogueActive || isDestroyed) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        //lastPlayTime = 0f;
        
        dialoguePanel.SetActive(true);
        npcNameText.text = dialogue.npcName;
        
        if (onDialogueStart != null)
        {
            Debug.Log($"onDialogueStart event exists with {onDialogueStart.GetPersistentEventCount()} listeners");
            onDialogueStart.Invoke();
            Debug.Log("onDialogueStart invoked successfully");
        }
        else
        {
            Debug.LogError("onDialogueStart event is null!");
        }
        
        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (isTyping)
        {
            CompleteCurrentLine();
            return;
        }

        if (currentLineIndex >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentDialogue.dialogueLines[currentLineIndex].text));
        currentLineIndex++;
    }

    private void CompleteCurrentLine()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            StopTypingSound();

            if (currentLineIndex < currentDialogue.dialogueLines.Length)
            {
                dialogueText.text = currentDialogue.dialogueLines[currentLineIndex - 1].text;
            }
            isTyping = false;
            typewriterCoroutine = null;
        }
    }

    private IEnumerator TypewriterEffect(string textToType)
    {
        if (isDestroyed) yield break;
        
        isTyping = true;
        dialogueText.text = "";
        StartTypingSound();

        foreach (char letter in textToType.ToCharArray())
        {
            if (isDestroyed) yield break;
            dialogueText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        if (!isDestroyed)
        {
            StopTypingSound();
            isTyping = false;
            typewriterCoroutine = null;
        }
    }

    private void StartTypingSound()
    {
        if (typingSoundSource != null && typewriterSound != null)
        {
            typingSoundSource.clip = typewriterSound;
            typingSoundSource.volume = typingSoundVolume;
            typingSoundSource.loop = true;
            typingSoundSource.Play();
        }
    }

    private void StopTypingSound()
    {
        if (typingSoundSource != null)
        {
            typingSoundSource.Stop();
            typingSoundSource.loop = false;
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        currentDialogue = null;
        StopTypingSound();
        onDialogueEnd?.Invoke();
    }

    public void ForceEndDialogue()
    {
        if (isDialogueActive)
        {
            EndDialogue();
        }
    }

    public bool IsDialogueActive() => isDialogueActive;

    private void OnDisable()
    {
        StopTypingSound();
    }
} 