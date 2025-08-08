using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_Manager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    
    [Header("Main Menu Integration")]
    [SerializeField] private MainMenuManager mainMenuManager;

    [Header("Game UI Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Menu UI Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings UI Elements")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    private bool isPaused = false;
    private bool isGameOver = false;
    private float gameTime = 0f;

    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
    }

    void Update()
    {
        if (!isPaused && !isGameOver)
        {
            UpdateGameTime();
        }

        // Pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    #region Initialization
    private void InitializeUI()
    {
        // Set initial UI state
        ShowMainMenu();
        
        // Initialize sliders
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 100f;
            healthBar.value = 100f;
        }

        if (manaBar != null)
        {
            manaBar.minValue = 0f;
            manaBar.maxValue = 100f;
            manaBar.value = 100f;
        }

        // Initialize settings
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (qualityDropdown != null)
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 2);
    }

    private void SetupButtonListeners()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ShowMainMenu);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Settings listeners
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
    }
    #endregion

    #region Menu Navigation
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        HideAllPanels();
        if (gameUIPanel != null)
            gameUIPanel.SetActive(true);
        
        isPaused = false;
        isGameOver = false;
        gameTime = 0f;
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }
    #endregion

    #region Pause System
    public void PauseGame()
    {
        if (!isGameOver)
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    #endregion

    #region Game State
    public void GameOver()
    {
        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void Victory()
    {
        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;
        
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void RestartGame()
    {
        isGameOver = false;
        isPaused = false;
        gameTime = 0f;
        Time.timeScale = 1f;
        
        // Burada GameManager'a restart sinyali gönderilebilir
        // GameManager.Instance.RestartGame();
    }
    #endregion

    #region UI Updates (Called by other systems)
    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
    }

    public void UpdateManaUI(float currentMana, float maxMana)
    {
        if (manaBar != null)
        {
            manaBar.maxValue = maxMana;
            manaBar.value = currentMana;
        }

        if (manaText != null)
        {
            manaText.text = $"{Mathf.RoundToInt(currentMana)}/{Mathf.RoundToInt(maxMana)}";
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }

    private void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
        
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    #endregion

    #region Settings
    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
        // AudioManager.Instance.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        // AudioManager.Instance.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
        // AudioManager.Instance.SetSFXVolume(volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetQualityLevel(int qualityLevel)
    {
        QualitySettings.SetQualityLevel(qualityLevel);
        PlayerPrefs.SetInt("QualityLevel", qualityLevel);
        PlayerPrefs.Save();
    }
    #endregion

    #region Utility Methods
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public float GetGameTime()
    {
        return gameTime;
    }
    #endregion

    #region Save/Load Integration (Called by SaveManager)
    public void OnGameSaved()
    {
        // Save başarılı olduğunda UI'da gösterilecek mesaj
        Debug.Log("Game saved successfully!");
        // Burada save başarılı mesajı gösterilebilir
        ShowSaveNotification("Game Saved!");
    }

    public void OnGameLoaded()
    {
        // Load başarılı olduğunda UI'da gösterilecek mesaj
        Debug.Log("Game loaded successfully!");
        // Burada load başarılı mesajı gösterilebilir
        ShowLoadNotification("Game Loaded!");
    }

    private void ShowSaveNotification(string message)
    {
        // Burada save notification UI'ı gösterilebilir
        // Örnek: NotificationPanel.SetActive(true);
        Debug.Log(message);
    }

    private void ShowLoadNotification(string message)
    {
        // Burada load notification UI'ı gösterilebilir
        // Örnek: NotificationPanel.SetActive(true);
        Debug.Log(message);
    }
    #endregion

    #region Save/Load Button Methods
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    public void LoadGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }

    public void NewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CreateNewGame();
            StartGame();
        }
    }

    public void DeleteSave()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
    }
    #endregion

    #region Main Menu Integration
    public void SetMainMenuManager(MainMenuManager manager)
    {
        mainMenuManager = manager;
    }

    public void ReturnToMainMenu()
    {
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
        else
        {
            ShowMainMenu();
        }
    }
    #endregion
}
