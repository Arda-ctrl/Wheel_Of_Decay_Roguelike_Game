using UnityEngine;

/// <summary>
/// PlayerManaController - Player'ın mana sistemini yönetir
/// </summary>
public class PlayerManaController : MonoBehaviour, IMana
{
    public static PlayerManaController Instance;
    
    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float currentMana;
    [SerializeField] private float manaRegenRate = 5f; // Saniye başına mana yenileme
    [SerializeField] private float manaRegenDelay = 3f; // Mana tüketiminden sonra yenileme gecikmesi
    
    private float lastManaUsageTime;
    private bool isRegenerating = false;

    // Events
    public System.Action<float, float> OnManaChanged; // current, max
    public System.Action OnManaEmpty;
    public System.Action OnManaFull;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Mana'yı full olarak başlat
        currentMana = maxMana;
        
        Debug.Log($"💧 PlayerManaController initialized: {currentMana}/{maxMana}");
        Debug.Log($"💧 PlayerManaController Instance: {(Instance != null ? "SET" : "NULL")}");
        
        // UI'yi güncelle
        UpdateManaUI();
    }

    private void Update()
    {
        HandleManaRegeneration();
    }

    /// <summary>
    /// Mana yenileme sistemini yönetir
    /// </summary>
    private void HandleManaRegeneration()
    {
        // Eğer mana full değilse ve yeterli süre geçtiyse yenileme başlat
        if (currentMana < maxMana)
        {
            if (Time.time - lastManaUsageTime >= manaRegenDelay)
            {
                if (!isRegenerating)
                {
                    isRegenerating = true;
                    Debug.Log("💧 Mana regeneration started");
                }
                
                // Mana yenile
                RestoreMana(manaRegenRate * Time.deltaTime);
            }
        }
        else if (isRegenerating)
        {
            isRegenerating = false;
            Debug.Log("💧 Mana regeneration stopped - mana full");
        }
    }

    /// <summary>
    /// Mana tüketir
    /// </summary>
    /// <param name="amount">Tüketilecek mana miktarı</param>
    /// <returns>Mana tüketilebildi mi?</returns>
    public bool ConsumeMana(float amount)
    {
        Debug.Log($"💧 [ConsumeMana] Attempting to consume {amount} mana. Current: {currentMana}/{maxMana}");
        
        if (!HasEnoughMana(amount))
        {
            Debug.Log($"❌ Not enough mana! Required: {amount}, Current: {currentMana}");
            return false;
        }

        float oldMana = currentMana;
        currentMana -= amount;
        currentMana = Mathf.Max(0, currentMana);
        
        lastManaUsageTime = Time.time;
        isRegenerating = false;
        
        Debug.Log($"💧 Mana consumed: {amount}, Changed: {oldMana} → {currentMana}/{maxMana}");
        
        UpdateManaUI();
        OnManaChanged?.Invoke(currentMana, maxMana);
        
        if (currentMana <= 0)
        {
            OnManaEmpty?.Invoke();
        }
        
        return true;
    }

    /// <summary>
    /// Mana kazandırır
    /// </summary>
    /// <param name="amount">Kazanılacak mana miktarı</param>
    public void RestoreMana(float amount)
    {
        float oldMana = currentMana;
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        
        if (oldMana != currentMana)
        {
            UpdateManaUI();
            OnManaChanged?.Invoke(currentMana, maxMana);
            
            if (currentMana >= maxMana && oldMana < maxMana)
            {
                OnManaFull?.Invoke();
            }
        }
    }

    /// <summary>
    /// Mevcut mana miktarını döndürür
    /// </summary>
    /// <returns>Mevcut mana</returns>
    public float GetCurrentMana()
    {
        return currentMana;
    }

    /// <summary>
    /// Maksimum mana miktarını döndürür
    /// </summary>
    /// <returns>Maksimum mana</returns>
    public float GetMaxMana()
    {
        return maxMana;
    }

    /// <summary>
    /// Yeterli mana var mı kontrol eder
    /// </summary>
    /// <param name="amount">Kontrol edilecek mana miktarı</param>
    /// <returns>Yeterli mana var mı?</returns>
    public bool HasEnoughMana(float amount)
    {
        return currentMana >= amount;
    }

    /// <summary>
    /// Mana yüzdesini döndürür (0-1 arası)
    /// </summary>
    /// <returns>Mana yüzdesi</returns>
    public float GetManaPercentage()
    {
        return maxMana > 0 ? currentMana / maxMana : 0f;
    }

    /// <summary>
    /// Maksimum mana'yı artırır
    /// </summary>
    /// <param name="amount">Artırılacak miktar</param>
    public void IncreaseMana(float amount)
    {
        maxMana += amount;
        currentMana += amount; // Mevcut mana'yı da artır
        
        Debug.Log($"💧 Max mana increased by {amount}. New max: {maxMana}");
        
        UpdateManaUI();
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    /// <summary>
    /// Mana'yı instantly full yapar
    /// </summary>
    public void FillManaInstantly()
    {
        currentMana = maxMana;
        UpdateManaUI();
        OnManaChanged?.Invoke(currentMana, maxMana);
        OnManaFull?.Invoke();
        
        Debug.Log("💧 Mana instantly filled");
    }

    /// <summary>
    /// UI'yi günceller
    /// </summary>
    private void UpdateManaUI()
    {
        if (UI_Controller.Instance != null)
        {
            // Mana slider'ını güncelle
            if (UI_Controller.Instance.manaSlider != null)
            {
                UI_Controller.Instance.manaSlider.maxValue = maxMana;
                UI_Controller.Instance.manaSlider.value = currentMana;
                Debug.Log($"💧 [UI] Mana slider updated: {currentMana}/{maxMana}");
            }
            else
            {
                Debug.LogWarning("⚠️ [UI] Mana slider is NULL! Please assign mana slider in UI_Controller.");
            }
            
            if (UI_Controller.Instance.manaText != null)
            {
                UI_Controller.Instance.manaText.text = $"{Mathf.Ceil(currentMana)} / {maxMana}";
                Debug.Log($"💧 [UI] Mana text updated: {Mathf.Ceil(currentMana)} / {maxMana}");
            }
            else
            {
                Debug.LogWarning("⚠️ [UI] Mana text is NULL! Please assign mana text in UI_Controller.");
            }
        }
        else
        {
            Debug.LogError("❌ [UI] UI_Controller.Instance is NULL!");
        }
    }

    /// <summary>
    /// Debug için mana bilgilerini gösterir
    /// </summary>
    private void OnGUI()
    {
        // Debug bilgisi
        string manaInfo = $"💧 MANA: {currentMana:F1}/{maxMana}\n";
        manaInfo += $"Regen: {(isRegenerating ? "ON" : "OFF")}\n";
        manaInfo += $"Rate: {manaRegenRate}/sec\n";
        manaInfo += $"Delay: {manaRegenDelay}sec";
        
        GUI.color = Color.cyan;
        GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(10, 520, 200, 80), "");
        GUI.Label(new Rect(15, 525, 190, 70), manaInfo);
    }
} 