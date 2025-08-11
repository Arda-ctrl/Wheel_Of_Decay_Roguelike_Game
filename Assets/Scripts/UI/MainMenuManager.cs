using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Localization;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject continuePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject extrasPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("Options Sub-Panels")]
    [SerializeField] private GameObject gameOptionsPanel;
    [SerializeField] private GameObject audioOptionsPanel;
    [SerializeField] private GameObject videoOptionsPanel;
    [SerializeField] private GameObject brightnessPanel;
    [SerializeField] private GameObject controllerOptionsPanel;
    [SerializeField] private GameObject keyboardOptionsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button extrasButton;
    [SerializeField] private Button quitButton;

    [Header("Continue Panel")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button clearSaveButton;
    [SerializeField] private Button backToMainButton;
    [SerializeField] private TextMeshProUGUI saveInfoText;

    [Header("Options Buttons")]
    [SerializeField] private Button gameOptionsButton;
    [SerializeField] private Button audioOptionsButton;
    [SerializeField] private Button videoOptionsButton;
    [SerializeField] private Button controllerOptionsButton;
    [SerializeField] private Button keyboardOptionsButton;
    [SerializeField] private Button backToOptionsButton;

    [Header("Game Options")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button backerCreditsButton;
    [SerializeField] private Button showAchievementsButton;
    [SerializeField] private Button resetGameDefaultsButton;
    [SerializeField] private Button backToGameOptionsButton;

    [Header("Audio Options")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button resetAudioDefaultsButton;
    [SerializeField] private Button backToAudioOptionsButton;

    [Header("Video Options")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private TMP_Dropdown particleEffectsDropdown;
    [SerializeField] private TMP_Dropdown blurQualityDropdown;
    [SerializeField] private Button brightnessButton;
    [SerializeField] private Button resetVideoDefaultsButton;
    [SerializeField] private Button backToVideoOptionsButton;

    [Header("Controller Options")]
    [SerializeField] private Button resetControllerDefaultsButton;
    [SerializeField] private Button backToControllerOptionsButton;

    [Header("Brightness Panel")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Button backToBrightnessButton;
    [SerializeField] private TextMeshProUGUI brightnessValueText;
    [SerializeField] private TextMeshProUGUI brightnessInfoText; // Yeni eklenen info text
    
    [Header("Keyboard Options")]
    [SerializeField] private Button resetKeyboardDefaultsButton;
    [SerializeField] private Button backToKeyboardOptionsButton;

    [Header("Extras")]
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button backToExtrasButton;

    [Header("Quit Confirm")]
    [SerializeField] private Button quitYesButton;
    [SerializeField] private Button quitNoButton;

    [Header("UI References")]
    [SerializeField] private UI_Manager uiManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private LocalizationManager localizationManager;

    [Header("Menu Navigation")]
    [SerializeField] private GameObject menuSelectionIndicator;
    [SerializeField] private float menuTransitionDelay = 0.1f;

    private GameObject currentPanel;
    private bool isTransitioning = false;

    void Start()
    {
        InitializeMainMenu();
        SetupButtonListeners();
        LoadSettings();
        ShowMainMenu();
    }

    void Update()
    {
        HandleInput();
    }

    #region Initialization
    private void InitializeMainMenu()
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UI_Manager>();
        
        if (saveManager == null)
            saveManager = FindFirstObjectByType<SaveManager>();
        
        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();
        
        if (settingsManager == null)
            settingsManager = FindFirstObjectByType<SettingsManager>();
        
        if (localizationManager == null)
            localizationManager = FindFirstObjectByType<LocalizationManager>();

        // Initialize localization first
        InitializeLocalization();
        
        // Wait for localization to be ready, then initialize dropdowns
        StartCoroutine(InitializeAfterLocalization());
        
        // Hide all panels initially
        HideAllPanels();
    }

    private IEnumerator InitializeAfterLocalization()
    {
        // LocalizationManager'ın hazır olmasını bekle
        if (localizationManager != null)
        {
            // Birkaç frame bekle
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            Debug.Log("🔄 Localization hazır, dropdown'lar başlatılıyor...");
            InitializeDropdowns();
        }
        else
        {
            Debug.LogError("❌ LocalizationManager bulunamadı!");
        }
    }

    private void InitializeLocalization()
    {
        if (localizationManager == null) return;
        
        // UI text'lerini localization sistemine ekle
        SetupLocalizedTexts();
        
        // Mevcut dili ayarla
        int currentLanguage = PlayerPrefs.GetInt("Language", 0);
        localizationManager.SetLanguage(currentLanguage);
    }
    
    private void SetupLocalizedTexts()
    {
        if (localizationManager == null) return;
        
        // Button text'lerini localization'a ekle
        if (newGameButton != null)
        {
            var buttonText = newGameButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("StartGame", newGameButton, "Start Game");
            }
        }
        
        if (continueButton != null)
        {
            var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Continue", continueButton, "Continue");
            }
        }
        
        if (optionsButton != null)
        {
            var buttonText = optionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Options", optionsButton, "Options");
            }
        }
        
        if (extrasButton != null)
        {
            var buttonText = extrasButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Extras", extrasButton, "Extras");
            }
        }
        
        if (quitButton != null)
        {
            var buttonText = quitButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Quit", quitButton, "Quit");
            }
        }
        
        if (loadGameButton != null)
        {
            var buttonText = loadGameButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("LoadGame", loadGameButton, "Load Game");
            }
        }
        
        if (clearSaveButton != null)
        {
            var buttonText = clearSaveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("ClearSave", clearSaveButton, "Clear Save");
            }
        }
        
        if (backToMainButton != null)
        {
            var buttonText = backToMainButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToMainButton, "Back");
            }
        }
        
        if (gameOptionsButton != null)
        {
            var buttonText = gameOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("GameOptions", gameOptionsButton, "Game Options");
            }
        }
        
        if (audioOptionsButton != null)
        {
            var buttonText = audioOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("AudioOptions", audioOptionsButton, "Audio Options");
            }
        }
        
        if (videoOptionsButton != null)
        {
            var buttonText = videoOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("VideoOptions", videoOptionsButton, "Video Options");
            }
        }
        
        if (controllerOptionsButton != null)
        {
            var buttonText = controllerOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("ControllerOptions", controllerOptionsButton, "Controller Options");
            }
        }
        
        if (keyboardOptionsButton != null)
        {
            var buttonText = keyboardOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("KeyboardOptions", keyboardOptionsButton, "Keyboard Options");
            }
        }
        
        if (creditsButton != null)
        {
            var buttonText = creditsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Credits", creditsButton, "Credits");
            }
        }
        
        if (quitYesButton != null)
        {
            var buttonText = quitYesButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Yes", quitYesButton, "Yes");
            }
        }
        
        if (quitNoButton != null)
        {
            var buttonText = quitNoButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("No", quitNoButton, "No");
            }
        }
        
        // ===== EKSİK UI ELEMENTLERİ =====
        
        // Back button'ları
        if (backToOptionsButton != null)
        {
            var buttonText = backToOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToOptionsButton, "Back");
            }
        }
        
        if (backToGameOptionsButton != null)
        {
            var buttonText = backToGameOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToGameOptionsButton, "Back");
            }
        }
        
        if (backToAudioOptionsButton != null)
        {
            var buttonText = backToAudioOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToAudioOptionsButton, "Back");
            }
        }
        
        if (backToVideoOptionsButton != null)
        {
            var buttonText = backToVideoOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToVideoOptionsButton, "Back");
            }
        }
        
        if (backToControllerOptionsButton != null)
        {
            var buttonText = backToControllerOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToControllerOptionsButton, "Back");
            }
        }
        
        if (backToKeyboardOptionsButton != null)
        {
            var buttonText = backToKeyboardOptionsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToKeyboardOptionsButton, "Back");
            }
        }
        
        if (backToExtrasButton != null)
        {
            var buttonText = backToExtrasButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToExtrasButton, "Back");
            }
        }
        
        if (backToBrightnessButton != null)
        {
            var buttonText = backToBrightnessButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Back", backToBrightnessButton, "Back");
            }
        }
        
        // Reset Defaults button'ları
        if (resetGameDefaultsButton != null)
        {
            var buttonText = resetGameDefaultsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("ResetDefaults", resetGameDefaultsButton, "Reset to Defaults");
            }
        }
        
        if (resetAudioDefaultsButton != null)
        {
            var buttonText = resetAudioDefaultsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("ResetDefaults", resetAudioDefaultsButton, "Reset to Defaults");
            }
        }
        
        if (resetVideoDefaultsButton != null)
        {
            var buttonText = resetVideoDefaultsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("ResetDefaults", resetVideoDefaultsButton, "Reset to Defaults");
            }
        }
        
        if (resetControllerDefaultsButton != null)
        {
            var buttonText = resetControllerDefaultsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("ResetDefaults", resetControllerDefaultsButton, "Reset to Defaults");
            }
        }
        
        if (resetKeyboardDefaultsButton != null)
        {
            var buttonText = resetKeyboardDefaultsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("ResetDefaults", resetKeyboardDefaultsButton, "Reset to Defaults");
            }
        }
        
        // Diğer button'lar
        if (backerCreditsButton != null)
        {
            var buttonText = backerCreditsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("BackerCredits", backerCreditsButton, "Backer Credits");
            }
        }
        
        if (showAchievementsButton != null)
        {
            var buttonText = showAchievementsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Achievements", showAchievementsButton, "Achievements");
            }
        }
        
        if (brightnessButton != null)
        {
            var buttonText = brightnessButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                localizationManager.AddLocalizedButton("Brightness", brightnessButton, "Brightness");
            }
        }
        
        // ===== PANEL BAŞLIKLARI VE LABEL'LAR =====
        
        // Panel başlıkları için TextMeshPro component'leri bul ve ekle
        AddPanelTitleLocalization("Options", "Options");
        AddPanelTitleLocalization("GameOptions", "Game Options");
        AddPanelTitleLocalization("AudioOptions", "Audio Options");
        AddPanelTitleLocalization("VideoOptions", "Video Options");
        AddPanelTitleLocalization("ControllerOptions", "Controller Options");
        AddPanelTitleLocalization("KeyboardOptions", "Keyboard Options");
        AddPanelTitleLocalization("Extras", "Extras");
        AddPanelTitleLocalization("Brightness", "Brightness");
        
        // Volume label'ları
        AddVolumeLabelLocalization("MasterVolume", "Master Volume");
        AddVolumeLabelLocalization("SoundVolume", "Sound Volume");
        AddVolumeLabelLocalization("MusicVolume", "Music Volume");
        
        // Video ayarları label'ları
        AddVideoLabelLocalization("Resolution", "Resolution");
        AddVideoLabelLocalization("Fullscreen", "Fullscreen");
        AddVideoLabelLocalization("VSync", "V-Sync");
        AddVideoLabelLocalization("ParticleEffects", "Particle Effects");
        AddVideoLabelLocalization("BlurQuality", "Blur Quality");
        
        // Brightness value text - sadece slider değerini göster, localization'a ekleme
        if (brightnessValueText != null)
        {
            // Brightness value text'i otomatik güncelle
            UpdateBrightnessValueText();
        }
        
        // ===== BRIGHTNESS PANEL INFORMATION TEXT =====
        AddBrightnessPanelLocalization();
        
        Debug.Log("✅ Tüm UI elementleri localization sistemine eklendi!");
    }
    
    // Panel başlıkları için helper method
    private void AddPanelTitleLocalization(string key, string fallbackText)
    {
        // Tüm panel'lerde başlık arama
        GameObject[] allPanels = { optionsPanel, gameOptionsPanel, audioOptionsPanel, videoOptionsPanel, 
                                   controllerOptionsPanel, keyboardOptionsPanel, extrasPanel, brightnessPanel };
        
        foreach (var panel in allPanels)
        {
            if (panel != null)
            {
                var titleTexts = panel.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var titleText in titleTexts)
                {
                    // Başlık olarak kabul edilecek text'leri bul
                    if (titleText != null && 
                        (titleText.text.Contains(fallbackText) || 
                         titleText.text.ToUpper() == fallbackText.ToUpper() ||
                         titleText.text.ToUpper().Contains(fallbackText.ToUpper()) ||
                         titleText.text.ToUpper().Contains("AUDIO") && key == "AudioOptions" ||
                         titleText.text.ToUpper().Contains("VIDEO") && key == "VideoOptions" ||
                         titleText.text.ToUpper().Contains("GAME") && key == "GameOptions" ||
                         titleText.text.ToUpper().Contains("OPTIONS") && key == "Options"))
                    {
                        localizationManager.AddLocalizedText(key, titleText, fallbackText);
                        Debug.Log($"✅ Panel başlığı bulundu: {key} -> {titleText.name} (Text: {titleText.text})");
                        return;
                    }
                }
            }
        }
        
        Debug.LogWarning($"⚠️ Panel başlığı bulunamadı: {key}");
    }
    
    // Volume label'ları için helper method
    private void AddVolumeLabelLocalization(string key, string fallbackText)
    {
        if (audioOptionsPanel != null)
        {
            var labels = audioOptionsPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var label in labels)
            {
                // Daha esnek arama (büyük/küçük harf duyarsız)
                if (label.text.Contains(fallbackText) || 
                    label.text.ToUpper().Contains(fallbackText.ToUpper()) ||
                    label.text.ToUpper().Contains("SFX") && key == "SoundVolume" || // SFX için özel kontrol
                    label.text.ToUpper().Contains("MUSIC") && key == "MusicVolume" || // Music için özel kontrol
                    label.text.ToUpper().Contains("ANA") && key == "MasterVolume") // Ana için özel kontrol
                {
                    localizationManager.AddLocalizedText(key, label, fallbackText);
                    Debug.Log($"✅ Volume label bulundu: {key} -> {label.name} (Text: {label.text})");
                    return;
                }
            }
        }
        
        Debug.LogWarning($"⚠️ Volume label bulunamadı: {key}");
    }
    
    // Video label'ları için helper method
    private void AddVideoLabelLocalization(string key, string fallbackText)
    {
        if (videoOptionsPanel != null)
        {
            var labels = videoOptionsPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var label in labels)
            {
                // Debug için tüm label'ları yazdır
                Debug.Log($"🔍 Video panel'de label bulundu: {label.name} -> '{label.text}'");
                
                // Daha esnek arama (büyük/küçük harf duyarsız)
                if (label.text.Contains(fallbackText) || 
                    label.text.ToUpper().Contains(fallbackText.ToUpper()) ||
                    label.text.ToUpper().Contains("ÇÖZÜNÜRLÜK") && key == "Resolution" ||
                    label.text.ToUpper().Contains("TAM EKRAN") && key == "Fullscreen" ||
                    label.text.ToUpper().Contains("VSYNC") && key == "VSync" ||
                    label.text.ToUpper().Contains("PARÇACIK") && key == "ParticleEffects" ||
                    label.text.ToUpper().Contains("BULANTI") && key == "BlurQuality" ||
                    label.text.ToUpper().Contains("RESOLUTION") && key == "Resolution" ||
                    label.text.ToUpper().Contains("FULLSCREEN") && key == "Fullscreen" ||
                    label.text.ToUpper().Contains("FULL SCREEN") && key == "Fullscreen" ||
                    label.text.ToUpper().Contains("V-SYNC") && key == "VSync" ||
                    label.text.ToUpper().Contains("PARTICLE") && key == "ParticleEffects" ||
                    label.text.ToUpper().Contains("BLUR") && key == "BlurQuality")
                {
                    localizationManager.AddLocalizedText(key, label, fallbackText);
                    Debug.Log($"✅ Video label bulundu: {key} -> {label.name} (Text: {label.text})");
                    return;
                }
            }
        }
        
        Debug.LogWarning($"⚠️ Video label bulunamadı: {key}");
    }
    
    // Brightness panel information text'i için helper method
    private void AddBrightnessPanelLocalization()
    {
        // Yeni eklenen info text'i localization'a ekle
        if (brightnessInfoText != null)
        {
            localizationManager.AddLocalizedText("BrightnessInfo", brightnessInfoText, "Information");
            Debug.Log($"✅ Brightness info text localization'a eklendi: BrightnessInfo -> {brightnessInfoText.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Brightness info text null! Inspector'da atanması gerekiyor.");
        }
    }
    
    private List<string> GetLocalizedDropdownOptions(string key, List<string> fallbackOptions)
    {
        if (localizationManager == null) 
        {
            Debug.LogWarning($"⚠️ LocalizationManager null! Fallback options kullanılıyor: {key}");
            return fallbackOptions;
        }
        
        List<string> localizedOptions = new List<string>();
        
        Debug.Log($"🔍 Dropdown options için key: {key}");
        Debug.Log($"🔍 Fallback options: {string.Join(", ", fallbackOptions)}");
        Debug.Log($"🌍 Mevcut dil index: {localizationManager.GetCurrentLanguageIndex()}");
        
        foreach (var option in fallbackOptions)
        {
            // ✅ DOĞRU: Shared Data'daki anahtar formatını kullan
            // "Low" -> "ParticleEffects_Low" veya "BlurQuality_Low"
            string localizationKey = $"{key}_{option}";
            string localizedOption = localizationManager.GetLocalizedString(localizationKey, option);
            
            // Eğer localization bulunamadıysa fallback kullan
            if (localizedOption == option)
            {
                Debug.LogWarning($"⚠️ Localization key bulunamadı: {localizationKey}, fallback kullanılıyor: {option}");
                localizedOption = option;
            }
            
            localizedOptions.Add(localizedOption);
            Debug.Log($"✅ Dropdown option localized: {option} -> {localizedOption} (key: {localizationKey})");
        }
        
        Debug.Log($"🎯 Final localized options: {string.Join(", ", localizedOptions)}");
        return localizedOptions;
    }

    private void InitializeDropdowns()
    {
        // Language dropdown
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            
            // LocalizationManager'dan mevcut dilleri al
            if (localizationManager != null)
            {
                var languageNames = localizationManager.GetAvailableLanguageNames();
                languageDropdown.AddOptions(languageNames);
                
                // PlayerPrefs'den kaydedilmiş dili al ve dropdown'ı ayarla
                int savedLanguage = PlayerPrefs.GetInt("Language", 0);
                languageDropdown.value = savedLanguage;
                
                Debug.Log($"🌍 Language dropdown başlatıldı. Saved language: {savedLanguage}, Dropdown value: {languageDropdown.value}");
                Debug.Log($"🌍 Available languages: {string.Join(", ", languageNames)}");
            }
            else
            {
                // Fallback: Sabit dil listesi
                languageDropdown.AddOptions(new List<string> { "English", "Turkish", "German" });
                int savedLanguage = PlayerPrefs.GetInt("Language", 0);
                languageDropdown.value = savedLanguage;
                Debug.Log($"⚠️ LocalizationManager null! Fallback dil listesi kullanılıyor. Saved language: {savedLanguage}");
            }
        }

        // Resolution dropdown
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            Resolution[] resolutions = Screen.resolutions;
            List<string> resolutionOptions = new List<string>();
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = $"{resolutions[i].width} x {resolutions[i].height}";
                resolutionOptions.Add(option);
            }
            
            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", 0);
        }

        // Particle effects dropdown
        if (particleEffectsDropdown != null)
        {
            Debug.Log("🔍 Particle Effects dropdown başlatılıyor...");
            particleEffectsDropdown.ClearOptions();
            var particleOptions = GetLocalizedDropdownOptions("ParticleEffects", new List<string> { "Low", "Medium", "High", "Ultra" });
            particleEffectsDropdown.AddOptions(particleOptions);
            particleEffectsDropdown.value = PlayerPrefs.GetInt("ParticleEffects", 1);
            
            // Debug: Dropdown'ın içeriğini kontrol et
            Debug.Log($"✅ Particle Effects dropdown başlatıldı. Options count: {particleEffectsDropdown.options.Count}");
            for (int i = 0; i < particleEffectsDropdown.options.Count; i++)
            {
                Debug.Log($"   Option {i}: {particleEffectsDropdown.options[i].text}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Particle Effects dropdown null!");
        }

        // Blur quality dropdown
        if (blurQualityDropdown != null)
        {
            Debug.Log("🔍 Blur Quality dropdown başlatılıyor...");
            blurQualityDropdown.ClearOptions();
            var blurOptions = GetLocalizedDropdownOptions("BlurQuality", new List<string> { "Off", "Low", "Medium", "High" });
            blurQualityDropdown.AddOptions(blurOptions);
            blurQualityDropdown.value = PlayerPrefs.GetInt("BlurQuality", 1);
            
            // Debug: Dropdown'ın içeriğini kontrol et
            Debug.Log($"✅ Blur Quality dropdown başlatıldı. Options count: {blurQualityDropdown.options.Count}");
            for (int i = 0; i < blurQualityDropdown.options.Count; i++)
            {
                Debug.Log($"   Option {i}: {blurQualityDropdown.options[i].text}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Blur Quality dropdown null!");
        }
    }

    private void SetupButtonListeners()
    {
        // Main menu buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(ShowContinuePanel);
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);
        if (optionsButton != null)
            optionsButton.onClick.AddListener(ShowOptionsPanel);
        if (extrasButton != null)
            extrasButton.onClick.AddListener(ShowExtrasPanel);
        if (quitButton != null)
            quitButton.onClick.AddListener(ShowQuitConfirm);

        // Continue panel buttons
        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);
        if (clearSaveButton != null)
            clearSaveButton.onClick.AddListener(ClearSave);
        if (backToMainButton != null)
            backToMainButton.onClick.AddListener(BackToMainMenu);

        // Options buttons
        if (gameOptionsButton != null)
            gameOptionsButton.onClick.AddListener(() => ShowSubPanel(gameOptionsPanel));
        if (audioOptionsButton != null)
            audioOptionsButton.onClick.AddListener(() => ShowSubPanel(audioOptionsPanel));
        if (videoOptionsButton != null)
            videoOptionsButton.onClick.AddListener(() => ShowSubPanel(videoOptionsPanel));
        if (controllerOptionsButton != null)
            controllerOptionsButton.onClick.AddListener(() => ShowSubPanel(controllerOptionsPanel));
        if (keyboardOptionsButton != null)
            keyboardOptionsButton.onClick.AddListener(() => ShowSubPanel(keyboardOptionsPanel));
        if (backToOptionsButton != null)
            backToOptionsButton.onClick.AddListener(BackToOptions);

        // Game options buttons
        if (backerCreditsButton != null)
            backerCreditsButton.onClick.AddListener(ShowBackerCredits);
        if (showAchievementsButton != null)
            showAchievementsButton.onClick.AddListener(ShowAchievements);
        if (resetGameDefaultsButton != null)
            resetGameDefaultsButton.onClick.AddListener(ResetGameDefaults);
        if (backToGameOptionsButton != null)
            backToGameOptionsButton.onClick.AddListener(BackToGameOptions);

        // Audio options
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (soundVolumeSlider != null)
            soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (resetAudioDefaultsButton != null)
            resetAudioDefaultsButton.onClick.AddListener(ResetAudioDefaults);
        if (backToAudioOptionsButton != null)
            backToAudioOptionsButton.onClick.AddListener(BackToAudioOptions);

        // Video options
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
        if (brightnessButton != null)
            brightnessButton.onClick.AddListener(ShowBrightnessPanel);
        if (resetVideoDefaultsButton != null)
            resetVideoDefaultsButton.onClick.AddListener(ResetVideoDefaults);
        if (backToVideoOptionsButton != null)
            backToVideoOptionsButton.onClick.AddListener(BackToVideoOptions);

        // Controller options
        if (resetControllerDefaultsButton != null)
            resetControllerDefaultsButton.onClick.AddListener(ResetControllerDefaults);
        if (backToControllerOptionsButton != null)
            backToControllerOptionsButton.onClick.AddListener(BackToControllerOptions);

        // Brightness panel
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        if (backToBrightnessButton != null)
            backToBrightnessButton.onClick.AddListener(BackToVideoOptions);
            
        // Keyboard options
        if (resetKeyboardDefaultsButton != null)
            resetKeyboardDefaultsButton.onClick.AddListener(ResetKeyboardDefaults);
        if (backToKeyboardOptionsButton != null)
            backToKeyboardOptionsButton.onClick.AddListener(BackToKeyboardOptions);

        // Extras
        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
        if (backToExtrasButton != null)
            backToExtrasButton.onClick.AddListener(BackToExtras);

        // Quit Confirm
        if (quitYesButton != null)
            quitYesButton.onClick.AddListener(QuitGame);
        if (quitNoButton != null)
            quitNoButton.onClick.AddListener(BackToMainMenu);

        // Dropdowns
        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(SetLanguage);
    }

    private void LoadSettings()
    {
        // Settings are now handled by SettingsManager
        // The SettingsManager will automatically load and apply settings
        if (settingsManager != null)
        {
            // SettingsManager handles all loading automatically
        }
    }
    #endregion

    #region Menu Navigation
    public void ShowMainMenu()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(mainMenuPanel));
    }

    public void ShowContinuePanel()
    {
        if (isTransitioning) return;
        
        UpdateSaveInfo();
        StartCoroutine(TransitionToPanel(continuePanel));
    }

    public void ShowOptionsPanel()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(optionsPanel));
    }

    public void ShowExtrasPanel()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(extrasPanel));
    }

    public void ShowCredits()
    {
        // Credits sahnesini yükle
        UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
    }

    public void ShowQuitConfirm()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(quitConfirmPanel));
    }
    
    public void ShowBrightnessPanel()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(brightnessPanel));
    }

    private void ShowSubPanel(GameObject subPanel)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(subPanel));
    }

    private void BackToMainMenu()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(mainMenuPanel));
    }

    private void BackToOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(mainMenuPanel));
    }

    private void BackToExtras()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(mainMenuPanel));
    }

    private void BackToVideoOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(optionsPanel));
    }

    private void BackToGameOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(optionsPanel));
    }

    private void BackToAudioOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(optionsPanel));
    }

    private void BackToControllerOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(optionsPanel));
    }

    private void BackToKeyboardOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToPanel(optionsPanel));
    }

    private IEnumerator TransitionToPanel(GameObject targetPanel)
    {
        isTransitioning = true;

        // Hide current panel
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        // Wait for transition delay
        yield return new WaitForSeconds(menuTransitionDelay);

        // Show target panel
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            currentPanel = targetPanel;
        }

        isTransitioning = false;
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (continuePanel != null) continuePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (extrasPanel != null) extrasPanel.SetActive(false);

        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
        if (gameOptionsPanel != null) gameOptionsPanel.SetActive(false);
        if (audioOptionsPanel != null) audioOptionsPanel.SetActive(false);
        if (videoOptionsPanel != null) videoOptionsPanel.SetActive(false);
        if (brightnessPanel != null) brightnessPanel.SetActive(false);
        if (controllerOptionsPanel != null) controllerOptionsPanel.SetActive(false);
        if (keyboardOptionsPanel != null) keyboardOptionsPanel.SetActive(false);
    }
    #endregion

    #region Game Actions
    public void StartNewGame()
    {
        if (saveManager != null)
        {
            saveManager.CreateNewGame();
        }
        
        if (uiManager != null)
        {
            uiManager.StartGame();
        }
    }

    public void LoadGame()
    {
        if (saveManager != null)
        {
            saveManager.LoadGame();
        }
        
        if (uiManager != null)
        {
            uiManager.StartGame();
        }
    }

    public void ClearSave()
    {
        if (saveManager != null)
        {
            saveManager.DeleteSave();
            UpdateSaveInfo();
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void UpdateSaveInfo()
    {
        if (saveInfoText != null && saveManager != null)
        {
            if (saveManager.HasSaveData)
            {
                saveInfoText.text = saveManager.GetSaveInfo();
                if (loadGameButton != null)
                    loadGameButton.interactable = true;
            }
            else
            {
                // Localization kullan
                if (localizationManager != null)
                {
                    saveInfoText.text = localizationManager.GetLocalizedString("NoSaveData", "No save data found");
                }
                else
                {
                    saveInfoText.text = "No save data found";
                }
                if (loadGameButton != null)
                    loadGameButton.interactable = false;
            }
        }
    }
    #endregion

    #region Settings
    // Game Options
    public void SetLanguage(int languageIndex)
    {
        if (settingsManager != null)
            settingsManager.SetLanguage(languageIndex);
        
        if (localizationManager != null)
            localizationManager.SetLanguage(languageIndex);
        
        // Language dropdown'ı güncelle
        if (languageDropdown != null)
        {
            languageDropdown.value = languageIndex;
            Debug.Log($"🌍 Dil değiştirildi: {languageIndex}, Dropdown güncellendi");
        }
        
        // Dil ayarını PlayerPrefs'e kaydet
        PlayerPrefs.SetInt("Language", languageIndex);
        PlayerPrefs.Save();
        Debug.Log($"💾 Dil ayarı kaydedildi: {languageIndex}");
        
        // Dropdown'ları yeniden başlat (localization için)
        StartCoroutine(RefreshDropdownsAfterLanguageChange());
    }
    
    private IEnumerator RefreshDropdownsAfterLanguageChange()
    {
        // Localization'ın güncellenmesi için daha uzun bekleme
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f); // Ekstra bekleme ekle
        
        Debug.Log("🔄 Dil değişimi sonrası dropdown'lar yenileniyor...");
        
        // Dropdown'ları yeniden başlat
        if (particleEffectsDropdown != null)
        {
            int currentValue = particleEffectsDropdown.value;
            Debug.Log($"🔍 Particle Effects dropdown yenileniyor. Mevcut değer: {currentValue}");
            
            // Dropdown'ı tamamen temizle ve yeniden oluştur
            particleEffectsDropdown.ClearOptions();
            
            // Yeni localized options'ları al
            var particleOptions = GetLocalizedDropdownOptions("ParticleEffects", new List<string> { "Low", "Medium", "High", "Ultra" });
            
            // Options'ları ekle
            particleEffectsDropdown.AddOptions(particleOptions);
            
            // Mevcut değeri koru (eğer geçerliyse)
            if (currentValue >= 0 && currentValue < particleOptions.Count)
            {
                particleEffectsDropdown.value = currentValue;
                Debug.Log($"✅ Particle Effects dropdown yenilendi. Yeni değer: {currentValue} -> {particleOptions[currentValue]}");
            }
            else
            {
                particleEffectsDropdown.value = 0; // Default değer
                Debug.LogWarning($"⚠️ Particle Effects dropdown değeri geçersiz, default kullanılıyor: 0");
            }
            
            // UI'ı zorla güncelle - BU ÇOK ÖNEMLİ!
            particleEffectsDropdown.RefreshShownValue();
            
            // Dropdown'ı zorla yeniden çiz
            Canvas.ForceUpdateCanvases();
            
            // Ekstra güvenlik için dropdown'ı yeniden aktifleştir
            particleEffectsDropdown.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            particleEffectsDropdown.gameObject.SetActive(true);
            
            Debug.Log($"✅ Particle Effects dropdown tamamen yenilendi. Options: {string.Join(", ", particleOptions)}");
        }
        
        if (blurQualityDropdown != null)
        {
            int currentValue = blurQualityDropdown.value;
            Debug.Log($"🔍 Blur Quality dropdown yenileniyor. Mevcut değer: {currentValue}");
            
            // Dropdown'ı tamamen temizle ve yeniden oluştur
            blurQualityDropdown.ClearOptions();
            
            // Yeni localized options'ları al
            var blurOptions = GetLocalizedDropdownOptions("BlurQuality", new List<string> { "Off", "Low", "Medium", "High" });
            
            // Options'ları ekle
            blurQualityDropdown.AddOptions(blurOptions);
            
            // Mevcut değeri koru (eğer geçerliyse)
            if (currentValue >= 0 && currentValue < blurOptions.Count)
            {
                blurQualityDropdown.value = currentValue;
                Debug.Log($"✅ Blur Quality dropdown yenilendi. Yeni değer: {currentValue} -> {blurOptions[currentValue]}");
            }
            else
            {
                blurQualityDropdown.value = 0; // Default değer
                Debug.LogWarning($"⚠️ Blur Quality dropdown değeri geçersiz, default kullanılıyor: 0");
            }
            
            // UI'ı zorla güncelle - BU ÇOK ÖNEMLİ!
            blurQualityDropdown.RefreshShownValue();
            
            // Dropdown'ı zorla yeniden çiz
            Canvas.ForceUpdateCanvases();
            
            // Ekstra güvenlik için dropdown'ı yeniden aktifleştir
            blurQualityDropdown.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            blurQualityDropdown.gameObject.SetActive(true);
            
            Debug.Log($"✅ Blur Quality dropdown tamamen yenilendi. Options: {string.Join(", ", blurOptions)}");
        }
        
        Debug.Log("✅ Tüm dropdown'lar dil değişimi sonrası yenilendi!");
    }

    public void ShowBackerCredits()
    {
        // Implement backer credits display
        Debug.Log("Showing backer credits");
    }

    public void ShowAchievements()
    {
        // Implement achievements system
        Debug.Log("Showing achievements");
    }

    public void ResetGameDefaults()
    {
        if (settingsManager != null)
            settingsManager.ResetGameSettings();
    }

    // Audio Options
    public void SetMasterVolume(float volume)
    {
        if (settingsManager != null)
            settingsManager.SetMasterVolume(volume);
    }

    public void SetSoundVolume(float volume)
    {
        if (settingsManager != null)
            settingsManager.SetSoundVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (settingsManager != null)
            settingsManager.SetMusicVolume(volume);
    }

    public void ResetAudioDefaults()
    {
        if (settingsManager != null)
            settingsManager.ResetAudioSettings();
    }

    // Video Options
    public void SetResolution(int resolutionIndex)
    {
        if (settingsManager != null)
            settingsManager.SetResolution(resolutionIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        if (settingsManager != null)
            settingsManager.SetFullscreen(isFullscreen);
    }

    public void SetVSync(bool enableVSync)
    {
        if (settingsManager != null)
            settingsManager.SetVSync(enableVSync);
    }

    public void SetParticleEffects(int qualityIndex)
    {
        if (settingsManager != null)
            settingsManager.SetParticleEffects(qualityIndex);
    }

    public void SetBlurQuality(int qualityIndex)
    {
        if (settingsManager != null)
            settingsManager.SetBlurQuality(qualityIndex);
    }



    public void ResetVideoDefaults()
    {
        if (settingsManager != null)
            settingsManager.ResetVideoSettings();
    }

    // Controller Options
    public void ResetControllerDefaults()
    {
        Debug.Log("🎮 Resetting controller defaults...");
        // Controller ayarları için özel reset logic eklenebilir
        // Şimdilik sadece log
    }

    // Keyboard Options
    public void ResetKeyboardDefaults()
    {
        Debug.Log("⌨️ Resetting keyboard defaults...");
        // Keyboard ayarları için özel reset logic eklenebilir
        // Şimdilik sadece log
    }
    
    // Brightness value text güncelleme
    private void UpdateBrightnessValueText()
    {
        if (brightnessValueText != null && brightnessSlider != null)
        {
            float brightnessValue = brightnessSlider.value;
            string localizedBrightness = localizationManager != null ? 
                localizationManager.GetLocalizedString("Brightness", "Brightness") : "Brightness";
            
            // Sadece yüzde değerini göster, duplicate text'i önle
            brightnessValueText.text = $"Slider %{Mathf.RoundToInt(brightnessValue * 100)}";
            Debug.Log($"✅ Brightness value text güncellendi: %{Mathf.RoundToInt(brightnessValue * 100)}");
        }
    }
    
    // Brightness slider değiştiğinde text'i güncelle
    public void SetBrightness(float brightness)
    {
        if (settingsManager != null)
            settingsManager.SetBrightness(brightness);
        
        // Brightness value text'i güncelle
        UpdateBrightnessValueText();
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        // Handle back button (Escape key)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }

    private void HandleBackButton()
    {
        if (isTransitioning) return;

        if (currentPanel == mainMenuPanel)
        {
            ShowQuitConfirm();
        }
        else if (currentPanel == continuePanel || currentPanel == optionsPanel || currentPanel == extrasPanel)
        {
            BackToMainMenu();
        }
        else if (currentPanel == quitConfirmPanel)
        {
            BackToMainMenu();
        }
        else
        {
            // Default back to main menu
            BackToMainMenu();
        }
    }
    #endregion
} 