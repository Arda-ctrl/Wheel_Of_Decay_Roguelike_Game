using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class WheelUIAnimator : MonoBehaviour
{
    public static WheelUIAnimator Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private Transform wheelManagerObject; // WheelManager GameObject'i
    [SerializeField] private RectTransform statsPanel;
    [SerializeField] private RectTransform buttonsPanel;
    
    [Header("Animation Settings")]
    [SerializeField] private float showAnimationDuration = 0.5f; // Gelme hızı
    [SerializeField] private float hideAnimationDuration = 0.3f; // Gitme hızı
    [SerializeField] private Ease slideEase = Ease.OutQuart;
    [SerializeField] private float wheelOffsetX = -1000f; // Sol taraftan gelecek
    [SerializeField] private float rightPanelOffsetX = 1000f; // Sağ taraftan gelecek
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    
    // Panel pozisyonları
    private Vector3 wheelOriginalPos;
    private Vector3 wheelHiddenPos;
    private Vector2 statsOriginalPos;
    private Vector2 statsHiddenPos;
    private Vector2 buttonsOriginalPos;
    private Vector2 buttonsHiddenPos;
    
    private bool isWheelUIVisible = false;
    private bool isAnimating = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Start()
    {
        InitializePanelPositions();
        // Başlangıçta UI'ları gizli konuma al
        HideUIInstantly();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !isAnimating)
        {
            ToggleWheelUI();
        }
    }
    
    private void InitializePanelPositions()
    {
        // Orijinal pozisyonları kaydet
        if (wheelManagerObject != null)
        {
            wheelOriginalPos = wheelManagerObject.position;
            wheelHiddenPos = new Vector3(wheelOriginalPos.x + (wheelOffsetX * 0.01f), wheelOriginalPos.y, wheelOriginalPos.z); // World space için küçük değer
        }
        
        if (statsPanel != null)
        {
            statsOriginalPos = statsPanel.anchoredPosition;
            statsHiddenPos = new Vector2(statsOriginalPos.x + rightPanelOffsetX, statsOriginalPos.y);
        }
        
        if (buttonsPanel != null)
        {
            buttonsOriginalPos = buttonsPanel.anchoredPosition;
            buttonsHiddenPos = new Vector2(buttonsOriginalPos.x + rightPanelOffsetX, buttonsOriginalPos.y);
        }
    }
    
    public void ToggleWheelUI()
    {
        if (isAnimating) return;
        
        if (isWheelUIVisible)
            HideWheelUI();
        else
            ShowWheelUI();
    }
    
    public void ShowWheelUI()
    {
        if (isAnimating || isWheelUIVisible) return;
        
        isAnimating = true;
        isWheelUIVisible = true;
        
        // Sequence oluştur - panellerin sırayla gelmesi için
        Sequence showSequence = DOTween.Sequence();
        
        // Önce wheel manager'ı soldan gelsin
        if (wheelManagerObject != null)
        {
            wheelManagerObject.gameObject.SetActive(true);
            showSequence.Append(wheelManagerObject.DOMove(wheelOriginalPos, showAnimationDuration)
                .SetEase(slideEase));
        }
        
        // Sonra sağ taraftaki paneller gelsin (paralel olarak)
        if (statsPanel != null)
        {
            statsPanel.gameObject.SetActive(true);
            showSequence.Join(statsPanel.DOAnchorPos(statsOriginalPos, showAnimationDuration)
                .SetEase(slideEase)
                .SetDelay(0.1f)); // Küçük bir gecikme
        }
        
        if (buttonsPanel != null)
        {
            buttonsPanel.gameObject.SetActive(true);
            showSequence.Join(buttonsPanel.DOAnchorPos(buttonsOriginalPos, showAnimationDuration)
                .SetEase(slideEase)
                .SetDelay(0.15f)); // Biraz daha gecikme
        }
        
        showSequence.OnComplete(() => {
            isAnimating = false;
            OnWheelUIShown();
        });
    }
    
    public void HideWheelUI()
    {
        if (isAnimating || !isWheelUIVisible) return;
        
        isAnimating = true;
        isWheelUIVisible = false;
        
        // Sequence oluştur - tersine animasyon
        Sequence hideSequence = DOTween.Sequence();
        
        // Önce sağ taraftaki paneller gitsin
        if (buttonsPanel != null)
        {
            hideSequence.Append(buttonsPanel.DOAnchorPos(buttonsHiddenPos, hideAnimationDuration)
                .SetEase(slideEase));
        }
        
        if (statsPanel != null)
        {
            hideSequence.Join(statsPanel.DOAnchorPos(statsHiddenPos, hideAnimationDuration)
                .SetEase(slideEase)
                .SetDelay(0.05f));
        }
        
        // Son olarak wheel manager gitsin
        if (wheelManagerObject != null)
        {
            hideSequence.Join(wheelManagerObject.DOMove(wheelHiddenPos, hideAnimationDuration)
                .SetEase(slideEase)
                .SetDelay(0.1f));
        }
        
        hideSequence.OnComplete(() => {
            isAnimating = false;
            OnWheelUIHidden();
        });
    }
    
    private void HideUIInstantly()
    {
        if (wheelManagerObject != null)
        {
            wheelManagerObject.position = wheelHiddenPos;
            // WheelManager'ı deactive etmiyoruz - player stats'ları için çalışmaya devam etmeli
        }
        
        if (statsPanel != null)
        {
            statsPanel.anchoredPosition = statsHiddenPos;
            statsPanel.gameObject.SetActive(false);
        }
        
        if (buttonsPanel != null)
        {
            buttonsPanel.anchoredPosition = buttonsHiddenPos;
            buttonsPanel.gameObject.SetActive(false);
        }
        
        isWheelUIVisible = false;
    }
    
    private void OnWheelUIShown()
    {
        Debug.Log("Wheel UI is now visible!");
        // Burada wheel UI açıldığında yapılacak işlemler
    }
    
    private void OnWheelUIHidden()
    {
        Debug.Log("Wheel UI is now hidden!");
        
        // SADECE UI panellerini deactive et, WheelManager'ı değil!
        // WheelManager çalışmaya devam etmeli çünkü player stats'larını etkiliyor
        if (statsPanel != null) statsPanel.gameObject.SetActive(false);
        if (buttonsPanel != null) buttonsPanel.gameObject.SetActive(false);
        
        // WheelManager'ı deactive etmiyoruz - çalışmaya devam etsin
        // Sadece görsel olarak gizleniyor, mantık çalışıyor
    }
    
    // Public properties
    public bool IsWheelUIVisible => isWheelUIVisible;
    public bool IsAnimating => isAnimating;
    
    // Manuel kontrol için
    public void SetToggleKey(KeyCode newKey)
    {
        toggleKey = newKey;
    }
    
    // Animasyon ayarları
    public void SetShowAnimationDuration(float duration)
    {
        showAnimationDuration = Mathf.Max(0.1f, duration);
    }
    
    public void SetHideAnimationDuration(float duration)
    {
        hideAnimationDuration = Mathf.Max(0.1f, duration);
    }
    
    public void SetSlideEase(Ease ease)
    {
        slideEase = ease;
    }
}
