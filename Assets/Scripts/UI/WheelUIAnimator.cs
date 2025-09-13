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
    [SerializeField] private float showAnimationDuration = 0.5f; // Paneller için gelme hızı
    [SerializeField] private float hideAnimationDuration = 0.3f; // Paneller için gitme hızı
    [SerializeField] private Ease slideEase = Ease.OutQuart; // Paneller için easing
    
    [Header("Wheel Animation Settings")]
    [SerializeField] private float wheelShowDuration = 2.0f; // Çark için gelme hızı - çok daha yavaş
    [SerializeField] private float wheelHideDuration = 1.8f; // Çark için gitme hızı - çok daha yavaş
    [SerializeField] private Ease wheelEase = Ease.OutBack; // Çark için daha doğal easing (hafif geri tepme ile)
    [SerializeField] private float wheelOffsetY = 300f; // Yukarıdan gelecek - pozitif değer yukarı, negatif değer aşağı
    [SerializeField] private float rightPanelOffsetX = 1000f; // Sağ taraftan gelecek
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    
    [Header("Prize Wheel Integration")]
    [SerializeField] private GameObject wheelPrizeObject; // Direkt WheelPrize GameObject'i
    [SerializeField] private PrizeWheelManager prizeWheelManager;
    
    // Panel pozisyonları
    private Vector3 wheelOriginalPos;
    private Vector3 wheelHiddenPos;
    private Vector2 statsOriginalPos;
    private Vector2 statsHiddenPos;
    private Vector2 buttonsOriginalPos;
    private Vector2 buttonsHiddenPos;
    
    // Prize wheel pozisyonu artık kullanılmıyor, direkt GameObject referansı kullanıyoruz
    
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
        
        // Başlangıçta prize wheel'i aktif et
        if (wheelPrizeObject != null)
        {
            wheelPrizeObject.SetActive(true);
            Debug.Log("🎰 WheelPrize GameObject başlangıçta aktif edildi");
        }
        else
        {
            Debug.LogError("❌ WheelPrize GameObject atanmamış! Lütfen Inspector'dan atayın.");
        }
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
            // Debug.Log("Original position: " + wheelOriginalPos + ", Offset: " + wheelOffsetY);
            wheelHiddenPos = new Vector3(wheelOriginalPos.x, wheelOriginalPos.y - wheelOffsetY, wheelOriginalPos.z); // Yukarıdan gelecek - offset negatif olmalı
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
        
        // WheelPrize GameObject'i varsa deaktif et
        if (wheelPrizeObject != null)
        {
            // Direkt GameObject'i deaktif et
            wheelPrizeObject.SetActive(false);
            Debug.Log("🎰 WheelPrize GameObject deaktif edildi - Ana çark açılıyor");
        }
        else
        {
            // WheelPrize GameObject'i bulunamadıysa, hierarchy'den tekrar bul
            GameObject wheelPrizeInScene = GameObject.Find("WheelPrize");
            if (wheelPrizeInScene != null)
            {
                wheelPrizeObject = wheelPrizeInScene;
                wheelPrizeObject.SetActive(false);
                Debug.Log("🎰 WheelPrize GameObject runtime'da bulundu ve deaktif edildi");
            }
        }
        
        isAnimating = true;
        isWheelUIVisible = true;
        
        // Sequence oluştur - panellerin sırayla gelmesi için
        Sequence showSequence = DOTween.Sequence();
        
        // Önce wheel manager'ı yukarıdan gelsin
        if (wheelManagerObject != null)
        {
            wheelManagerObject.gameObject.SetActive(true);
            showSequence.Append(wheelManagerObject.DOMove(wheelOriginalPos, wheelShowDuration)
                .SetEase(wheelEase));
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
            
            // Ana çarkın tooltip sistemini düzelt
            WheelManager wheelManager = FindFirstObjectByType<WheelManager>();
            if (wheelManager != null)
            {
                wheelManager.ResetTooltipSystem();
            }
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
            hideSequence.Join(wheelManagerObject.DOMove(wheelHiddenPos, wheelHideDuration)
                .SetEase(wheelEase)); // Gecikme olmadan direkt geri dönüş
        }
        
        hideSequence.OnComplete(() => {
            isAnimating = false;
            OnWheelUIHidden();
            
            // Ana çark kapatıldıktan sonra WheelPrize GameObject'i aktif et ve yeni çark oluştur
            if (wheelPrizeObject != null && prizeWheelManager != null)
            {
                // WheelPrize GameObject'i aktif et
                wheelPrizeObject.SetActive(true);
                
                // Tüm alt GameObject'leri aktif et
                foreach (Transform child in wheelPrizeObject.transform)
                {
                    child.gameObject.SetActive(true);
                    Debug.Log($"🎰 Alt GameObject aktif edildi: {child.name}");
                }
                
                
                // Yeni çark oluştur
                prizeWheelManager.CreateNewWheel();
                Debug.Log("🎰 WheelPrize GameObject aktif edildi ve yeni çark oluşturuldu - Ana çark kapatıldı");
            }
            else
            {
                // WheelPrize GameObject'i bulunamadıysa, hierarchy'den tekrar bul
                GameObject wheelPrizeInScene = GameObject.Find("WheelPrize");
                if (wheelPrizeInScene != null)
                {
                    wheelPrizeObject = wheelPrizeInScene;
                    wheelPrizeObject.SetActive(true);
                    
                    // Tüm alt GameObject'leri aktif et
                    foreach (Transform child in wheelPrizeObject.transform)
                    {
                        child.gameObject.SetActive(true);
                        Debug.Log($"🎰 Alt GameObject aktif edildi: {child.name}");
                    }
                    
                    
                    // PrizeWheelManager bileşenini bul
                    prizeWheelManager = wheelPrizeInScene.GetComponent<PrizeWheelManager>();
                    if (prizeWheelManager != null)
                    {
                        prizeWheelManager.CreateNewWheel();
                        Debug.Log("🎰 WheelPrize GameObject runtime'da bulundu, aktif edildi ve yeni çark oluşturuldu");
                    }
                    else
                    {
                        Debug.LogError("❌ WheelPrize GameObject'inde PrizeWheelManager bileşeni bulunamadı!");
                    }
                }
                else
                {
                    Debug.LogError("❌ WheelPrize GameObject bulunamadı! Lütfen Inspector'dan atayın.");
                }
            }
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
    
    // PrizeWheelManager tarafından çağrılır - referansları günceller
    public void SetWheelPrizeReference(GameObject wheelPrizeObj)
    {
        if (wheelPrizeObj != null)
        {
            wheelPrizeObject = wheelPrizeObj;
            prizeWheelManager = wheelPrizeObj.GetComponentInChildren<PrizeWheelManager>();
            Debug.Log($"🎰 WheelPrize referansı güncellendi: {wheelPrizeObj.name}");
        }
    }
    
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
