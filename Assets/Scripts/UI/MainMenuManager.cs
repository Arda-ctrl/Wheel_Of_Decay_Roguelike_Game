using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

        // Initialize dropdowns
        InitializeDropdowns();
        
        // Hide all panels initially
        HideAllPanels();
    }

    private void InitializeDropdowns()
    {
        // Language dropdown
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string> { "English", "Turkish", "German", "French", "Spanish" });
            languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
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
            particleEffectsDropdown.ClearOptions();
            particleEffectsDropdown.AddOptions(new List<string> { "Low", "Medium", "High", "Ultra" });
            particleEffectsDropdown.value = PlayerPrefs.GetInt("ParticleEffects", 1);
        }

        // Blur quality dropdown
        if (blurQualityDropdown != null)
        {
            blurQualityDropdown.ClearOptions();
            blurQualityDropdown.AddOptions(new List<string> { "Off", "Low", "Medium", "High" });
            blurQualityDropdown.value = PlayerPrefs.GetInt("BlurQuality", 1);
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
                saveInfoText.text = "No save data found";
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
            settingsManager.ResetAllSettings();
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

    public void SetBrightness(float brightness)
    {
        if (settingsManager != null)
            settingsManager.SetBrightness(brightness);
    }

    public void ResetVideoDefaults()
    {
        if (settingsManager != null)
            settingsManager.ResetVideoSettings();
    }

    // Controller Options
    public void ResetControllerDefaults()
    {
        // Implement controller reset
        Debug.Log("Resetting controller defaults");
    }

    // Keyboard Options
    public void ResetKeyboardDefaults()
    {
        // Implement keyboard reset
        Debug.Log("Resetting keyboard defaults");
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