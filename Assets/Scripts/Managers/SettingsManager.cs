using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    [Header("Brightness Control")]
    [SerializeField] private Image brightnessOverlay;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TextMeshProUGUI brightnessValueText;
    
    [Header("Audio Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    
    [Header("Video Controls")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private TMP_Dropdown particleEffectsDropdown;
    [SerializeField] private TMP_Dropdown blurQualityDropdown;
    
    [Header("Game Controls")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private SettingsData settingsData;
    private string settingsFilePath;
    private bool isTransitioning = false;
    
    public static SettingsManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadSettings();
        SetupUIListeners();
        ApplySettings();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    #region Initialization
    private void InitializeSettings()
    {
        settingsData = new SettingsData();
        settingsFilePath = Path.Combine(Application.persistentDataPath, "settings.json");
        
        // Create brightness overlay if not assigned
        if (brightnessOverlay == null)
        {
            CreateBrightnessOverlay();
        }
    }
    
    private void CreateBrightnessOverlay()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Settings Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Ensure it's on top
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create brightness overlay
        GameObject overlayGO = new GameObject("Brightness Overlay");
        overlayGO.transform.SetParent(canvas.transform, false);
        
        brightnessOverlay = overlayGO.AddComponent<Image>();
        brightnessOverlay.color = new Color(0, 0, 0, 0); // Transparent black
        brightnessOverlay.raycastTarget = false; // Don't block UI interactions
        
        // Set RectTransform to fill screen
        RectTransform rectTransform = brightnessOverlay.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
    #endregion
    
    #region Settings Management
    public void SaveSettings()
    {
        try
        {
            // Update settings data from UI
            UpdateSettingsFromUI();
            
            // Serialize to JSON
            string json = JsonUtility.ToJson(settingsData, true);
            File.WriteAllText(settingsFilePath, json);
            
            Debug.Log($"Settings saved to: {settingsFilePath}");
            Debug.Log($"Current settings: Brightness={settingsData.brightness}, Master={settingsData.masterVolume}, Sound={settingsData.soundVolume}, Music={settingsData.musicVolume}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save settings: {e.Message}");
        }
    }
    
    public void LoadSettings()
    {
        try
        {
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                settingsData = JsonUtility.FromJson<SettingsData>(json);
                Debug.Log($"Settings loaded from: {settingsFilePath}");
                Debug.Log($"Loaded settings: Brightness={settingsData.brightness}, Master={settingsData.masterVolume}, Sound={settingsData.soundVolume}, Music={settingsData.musicVolume}");
            }
            else
            {
                settingsData = new SettingsData();
                SaveSettings(); // Save default settings
                Debug.Log("No settings file found, created default settings");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load settings: {e.Message}");
            settingsData = new SettingsData();
        }
    }
    
    private void UpdateSettingsFromUI()
    {
        // Always update from UI elements if they exist
        if (brightnessSlider != null)
            settingsData.brightness = brightnessSlider.value;
        
        if (masterVolumeSlider != null)
            settingsData.masterVolume = masterVolumeSlider.value;
        
        if (soundVolumeSlider != null)
            settingsData.soundVolume = soundVolumeSlider.value;
        
        if (musicVolumeSlider != null)
            settingsData.musicVolume = musicVolumeSlider.value;
        
        if (resolutionDropdown != null)
            settingsData.resolutionIndex = resolutionDropdown.value;
        
        if (fullscreenToggle != null)
            settingsData.fullscreen = fullscreenToggle.isOn;
        
        if (vSyncToggle != null)
            settingsData.vSync = vSyncToggle.isOn;
        
        if (particleEffectsDropdown != null)
            settingsData.particleEffectsQuality = particleEffectsDropdown.value;
        
        if (blurQualityDropdown != null)
            settingsData.blurQuality = blurQualityDropdown.value;
        
        if (languageDropdown != null)
            settingsData.languageIndex = languageDropdown.value;
    }
    
    private void ApplySettings()
    {
        // Update UI elements first (without saving)
        UpdateUIFromSettings();
        
        // Apply brightness
        SetBrightness(settingsData.brightness);
        
        // Apply audio settings
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(settingsData.masterVolume);
            AudioManager.Instance.SetSFXVolume(settingsData.soundVolume);
            AudioManager.Instance.SetMusicVolume(settingsData.musicVolume);
        }
        
        // Apply video settings
        SetResolution(settingsData.resolutionIndex);
        SetFullscreen(settingsData.fullscreen);
        SetVSync(settingsData.vSync);
    }
    
    private void UpdateUIFromSettings()
    {
        // Temporarily remove listeners to prevent conflicts
        RemoveUIListeners();
        
        if (brightnessSlider != null)
            brightnessSlider.value = settingsData.brightness;
        
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = settingsData.masterVolume;
        
        if (soundVolumeSlider != null)
            soundVolumeSlider.value = settingsData.soundVolume;
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = settingsData.musicVolume;
        
        if (resolutionDropdown != null)
            resolutionDropdown.value = settingsData.resolutionIndex;
        
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = settingsData.fullscreen;
        
        if (vSyncToggle != null)
            vSyncToggle.isOn = settingsData.vSync;
        
        if (particleEffectsDropdown != null)
            particleEffectsDropdown.value = settingsData.particleEffectsQuality;
        
        if (blurQualityDropdown != null)
            blurQualityDropdown.value = settingsData.blurQuality;
        
        if (languageDropdown != null)
            languageDropdown.value = settingsData.languageIndex;
        
        // Re-add listeners
        SetupUIListeners();
    }
    #endregion
    
    #region Brightness Control
    public void SetBrightness(float brightness)
    {
        settingsData.brightness = Mathf.Clamp01(brightness);
        
        if (brightnessOverlay != null)
        {
            // Brightness: 0 = dark (alpha 1), 1 = bright (alpha 0)
            float alpha = settingsData.brightness;
            Color color = brightnessOverlay.color;
            color.a = alpha;
            brightnessOverlay.color = color;
        }
        
        // Update UI text
        if (brightnessValueText != null)
        {
            int percentage = Mathf.RoundToInt(settingsData.brightness * 100f);
            brightnessValueText.text = $"{percentage}%";
        }
    }
    
    public float GetBrightness()
    {
        return settingsData.brightness;
    }
    
    public float GetMasterVolume()
    {
        return settingsData.masterVolume;
    }
    
    public float GetSoundVolume()
    {
        return settingsData.soundVolume;
    }
    
    public float GetMusicVolume()
    {
        return settingsData.musicVolume;
    }
    
    public void ResetBrightness()
    {
        SetBrightness(0f);
    }
    #endregion
    
    #region Audio Settings
    public void SetMasterVolume(float volume)
    {
        settingsData.masterVolume = Mathf.Clamp01(volume);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(settingsData.masterVolume);
    }
    
    public void SetSoundVolume(float volume)
    {
        settingsData.soundVolume = Mathf.Clamp01(volume);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(settingsData.soundVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        settingsData.musicVolume = Mathf.Clamp01(volume);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(settingsData.musicVolume);
    }
    #endregion
    
    #region Video Settings
    public void SetResolution(int resolutionIndex)
    {
        settingsData.resolutionIndex = resolutionIndex;
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        settingsData.fullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
    }
    
    public void SetVSync(bool enableVSync)
    {
        settingsData.vSync = enableVSync;
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
    }
    
    public void SetParticleEffects(int qualityIndex)
    {
        settingsData.particleEffectsQuality = qualityIndex;
    }
    
    public void SetBlurQuality(int qualityIndex)
    {
        settingsData.blurQuality = qualityIndex;
    }
    #endregion
    
    #region Game Settings
    public void SetLanguage(int languageIndex)
    {
        settingsData.languageIndex = languageIndex;
    }
    #endregion
    
    #region Fade Effects
    public void PlayFadeIn()
    {
        if (!isTransitioning && brightnessOverlay != null)
        {
            StartCoroutine(FadeInCoroutine());
        }
    }
    
    private System.Collections.IEnumerator FadeInCoroutine()
    {
        isTransitioning = true;
        
        Color startColor = brightnessOverlay.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;
            float curveValue = fadeCurve.Evaluate(t);
            
            brightnessOverlay.color = Color.Lerp(startColor, targetColor, curveValue);
            
            yield return null;
        }
        
        brightnessOverlay.color = targetColor;
        isTransitioning = false;
    }
    
    public void PlayFadeOut()
    {
        if (!isTransitioning && brightnessOverlay != null)
        {
            StartCoroutine(FadeOutCoroutine());
        }
    }
    
    private System.Collections.IEnumerator FadeOutCoroutine()
    {
        isTransitioning = true;
        
        Color startColor = brightnessOverlay.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, settingsData.brightness);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;
            float curveValue = fadeCurve.Evaluate(t);
            
            brightnessOverlay.color = Color.Lerp(startColor, targetColor, curveValue);
            
            yield return null;
        }
        
        brightnessOverlay.color = targetColor;
        isTransitioning = false;
    }
    #endregion
    
    #region UI Setup
    private void SetupUIListeners()
    {
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        
        if (soundVolumeSlider != null)
            soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        
        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.AddListener(SetVSync);
        
        if (particleEffectsDropdown != null)
            particleEffectsDropdown.onValueChanged.AddListener(SetParticleEffects);
        
        if (blurQualityDropdown != null)
            blurQualityDropdown.onValueChanged.AddListener(SetBlurQuality);
        
        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(SetLanguage);
    }
    
    private void RemoveUIListeners()
    {
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveAllListeners();
        
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
        
        if (soundVolumeSlider != null)
            soundVolumeSlider.onValueChanged.RemoveAllListeners();
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
        
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveAllListeners();
        
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveAllListeners();
        
        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.RemoveAllListeners();
        
        if (particleEffectsDropdown != null)
            particleEffectsDropdown.onValueChanged.RemoveAllListeners();
        
        if (blurQualityDropdown != null)
            blurQualityDropdown.onValueChanged.RemoveAllListeners();
        
        if (languageDropdown != null)
            languageDropdown.onValueChanged.RemoveAllListeners();
    }
    #endregion
    
    #region Reset Functions
    public void ResetAllSettings()
    {
        settingsData = new SettingsData();
        ApplySettings();
        SaveSettings();
    }
    
    public void ResetAudioSettings()
    {
        settingsData.masterVolume = 1f;
        settingsData.soundVolume = 1f;
        settingsData.musicVolume = 1f;
        ApplySettings();
        SaveSettings();
    }
    
    public void ResetVideoSettings()
    {
        settingsData.resolutionIndex = 0;
        settingsData.fullscreen = true;
        settingsData.vSync = false;
        settingsData.particleEffectsQuality = 1;
        settingsData.blurQuality = 1;
        settingsData.brightness = 0f;
        ApplySettings();
        SaveSettings();
    }
    #endregion
    
    #region Save File Management
    private void HandleInput()
    {
        // P tuşuna basıldığında save dosyalarını temizle
        if (Input.GetKeyDown(KeyCode.P))
        {
            ClearAllSaveFiles();
        }
        
        // O tuşuna basıldığında tüm ayarları kaydet
        if (Input.GetKeyDown(KeyCode.O))
        {
            SaveAllSettings();
        }
    }
    
    public void ClearAllSaveFiles()
    {
        try
        {
            // Settings dosyasını sil
            if (File.Exists(settingsFilePath))
            {
                File.Delete(settingsFilePath);
                Debug.Log("Settings file deleted successfully");
            }
            
            // Diğer save dosyalarını da sil (SaveManager'dan)
            string saveDirectory = Path.GetDirectoryName(settingsFilePath);
            string[] saveFiles = Directory.GetFiles(saveDirectory, "*.json");
            
            foreach (string file in saveFiles)
            {
                if (file != settingsFilePath) // Settings dosyasını tekrar silme
                {
                    File.Delete(file);
                    Debug.Log($"Deleted save file: {file}");
                }
            }
            
            // Yeni default settings oluştur
            settingsData = new SettingsData();
            SaveSettings();
            ApplySettings();
            
            Debug.Log("All save files cleared and default settings created");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to clear save files: {e.Message}");
        }
    }
    
    public void SaveAllSettings()
    {
        try
        {
            // UI'dan tüm ayarları al
            UpdateSettingsFromUI();
            
            // Settings dosyasını kaydet
            SaveSettings();
            
            // SaveManager'dan da kaydet (eğer varsa)
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
                Debug.Log("Game save data also saved");
            }
            
            // AudioManager'dan da kaydet (eğer varsa)
            if (AudioManager.Instance != null)
            {
                // AudioManager'ın kendi save sistemi varsa burada çağırılabilir
                Debug.Log("Audio settings applied");
            }
            
            Debug.Log("🎮 ALL SETTINGS SAVED! 🎮");
            Debug.Log($"✅ Brightness: {settingsData.brightness * 100:F0}%");
            Debug.Log($"✅ Master Volume: {settingsData.masterVolume * 100:F0}%");
            Debug.Log($"✅ Sound Volume: {settingsData.soundVolume * 100:F0}%");
            Debug.Log($"✅ Music Volume: {settingsData.musicVolume * 100:F0}%");
            Debug.Log($"✅ Resolution: {settingsData.resolutionIndex}");
            Debug.Log($"✅ Fullscreen: {settingsData.fullscreen}");
            Debug.Log($"✅ V-Sync: {settingsData.vSync}");
            Debug.Log($"✅ Particle Effects: {settingsData.particleEffectsQuality}");
            Debug.Log($"✅ Blur Quality: {settingsData.blurQuality}");
            Debug.Log($"✅ Language: {settingsData.languageIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save all settings: {e.Message}");
        }
    }
    #endregion
} 