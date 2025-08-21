using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// Wheel UI'ının genel yönetimini sağlayan ana manager
/// WheelUIAnimator ile birlikte çalışır
/// </summary>
public class WheelUIManager : MonoBehaviour
{
    public static WheelUIManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private WheelManager wheelManager;
    [SerializeField] private StatsUI statsUI;
    
    [Header("Background")]
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.5f);
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip uiShowSound;
    [SerializeField] private AudioClip uiHideSound;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    private WheelUIAnimator animator;
    private bool isInitialized = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        animator = WheelUIAnimator.Instance;
        
        if (animator == null)
        {
            Debug.LogError("WheelUIAnimator not found! Make sure it's in the scene.");
            return;
        }
        
        // Background overlay ayarla
        if (backgroundOverlay != null)
        {
            backgroundOverlay.color = Color.clear;
            backgroundOverlay.gameObject.SetActive(false);
            backgroundOverlay.raycastTarget = true; // Background tıklamaları engellesin
        }
        
        // Audio source ayarla
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        isInitialized = true;
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // ESC tuşu ile de kapatabilsin
        if (Input.GetKeyDown(KeyCode.Escape) && animator.IsWheelUIVisible && !animator.IsAnimating)
        {
            HideWheelUI();
        }
    }
    
    public void ShowWheelUI()
    {
        if (!isInitialized || animator.IsAnimating) return;
        
        // Ses efekti çal
        PlaySound(uiShowSound);
        
        // Background overlay'i göster
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
            backgroundOverlay.DOColor(overlayColor, fadeDuration);
        }
        
        // Animator'ı çağır
        animator.ShowWheelUI();
        
        // Oyun durdurmak istersen (opsiyonel)
        // Time.timeScale = 0f;
    }
    
    public void HideWheelUI()
    {
        if (!isInitialized || animator.IsAnimating) return;
        
        // Ses efekti çal
        PlaySound(uiHideSound);
        
        // Background overlay'i gizle
        if (backgroundOverlay != null)
        {
            backgroundOverlay.DOColor(Color.clear, fadeDuration)
                .OnComplete(() => backgroundOverlay.gameObject.SetActive(false));
        }
        
        // Animator'ı çağır
        animator.HideWheelUI();
        
        // Oyunu devam ettir (eğer durdurmuşsan)
        // Time.timeScale = 1f;
    }
    
    public void ToggleWheelUI()
    {
        if (!isInitialized) return;
        
        if (animator.IsWheelUIVisible)
            HideWheelUI();
        else
            ShowWheelUI();
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Background tıklaması ile kapatma
    public void OnBackgroundClicked()
    {
        if (animator.IsWheelUIVisible && !animator.IsAnimating)
        {
            HideWheelUI();
        }
    }
    
    // Public properties
    public bool IsWheelUIVisible => animator?.IsWheelUIVisible ?? false;
    public bool IsAnimating => animator?.IsAnimating ?? false;
    
    // Segment seçildiğinde çağrılacak
    public void OnSegmentSelected()
    {
        // Segment seçildikten sonra UI'ı kapatmak istersen
        // HideWheelUI();
    }
    
    // Wheel spin başladığında çağrılacak
    public void OnWheelSpinStarted()
    {
        // Spin sırasında UI'ı kapatmak istersen
        if (IsWheelUIVisible)
        {
            HideWheelUI();
        }
    }
    
    // Debug için
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugToggleUI()
    {
        ToggleWheelUI();
    }
}
