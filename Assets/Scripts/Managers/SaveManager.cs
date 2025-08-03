using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public float currentHealth;
    public float maxHealth;
    public float currentMana;
    public float maxMana;
    public int level;
    public int experience;
    public int score;
    public Vector3 playerPosition;
    public string currentElement; // Fire, Ice, Earth, etc.
    public List<string> unlockedAbilities;
    public List<string> inventoryItems;
    public float gameTime;
}

[System.Serializable]
public class LevelData
{
    public int currentLevel;
    public int currentRoom;
    public string currentBiome;
    public Vector3 playerSpawnPosition;
    public List<SaveRoomData> visitedRooms;
    public List<EnemyData> activeEnemies;
    public List<PickupData> activePickups;
    public bool isBossRoom;
    public bool isCompleted;
}

[System.Serializable]
public class SaveRoomData
{
    public int roomId;
    public Vector3 position;
    public bool isCleared;
    public bool isVisited;
    public List<string> enemiesDefeated;
    public List<string> pickupsCollected;
}

[System.Serializable]
public class EnemyData
{
    public string enemyType;
    public Vector3 position;
    public float currentHealth;
    public bool isActive;
}

[System.Serializable]
public class PickupData
{
    public string pickupType;
    public Vector3 position;
    public bool isCollected;
}

[System.Serializable]
public class SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool isFullscreen;
    public int qualityLevel;
    public string language;
    public bool showDamageNumbers;
    public bool showFPS;
}

[System.Serializable]
public class GameSaveData
{
    public PlayerData playerData;
    public LevelData levelData;
    public SettingsData settingsData;
    public DateTime saveTime;
    public string saveVersion;
    public int totalPlayTime; // in seconds
    public int totalDeaths;
    public int totalKills;
    public int totalRoomsCleared;
}

public class SaveManager : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "gamesave.json";
    [SerializeField] private string backupFileName = "gamesave_backup.json";
    [SerializeField] private int maxSaveSlots = 3;
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 60f; // seconds

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    private string BackupPath => Path.Combine(Application.persistentDataPath, backupFileName);
    
    private GameSaveData currentSaveData;
    private UI_Manager uiManager;
    private float lastAutoSaveTime;
    private bool isInitialized = false;

    public static SaveManager Instance { get; private set; }
    public bool HasSaveData => File.Exists(SavePath);
    public GameSaveData CurrentSaveData => currentSaveData;

    void Awake()
    {
        // Singleton pattern
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
    }

    void Start()
    {
        InitializeSaveManager();
    }

    void Update()
    {
        if (autoSaveEnabled && isInitialized)
        {
            CheckAutoSave();
        }
    }

    #region Initialization
    private void InitializeSaveManager()
    {
        uiManager = FindObjectOfType<UI_Manager>();
        lastAutoSaveTime = Time.time;
        
        // Create save directory if it doesn't exist
        string directory = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        isInitialized = true;
        
        if (debugMode)
            Debug.Log("SaveManager initialized successfully");
    }
    #endregion

    #region Save Operations
    public void SaveGame()
    {
        try
        {
            // Create backup of existing save
            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, BackupPath, true);
            }

            // Create new save data
            currentSaveData = CreateSaveData();
            
            // Serialize to JSON
            string jsonData = JsonUtility.ToJson(currentSaveData, true);
            
            // Write to file
            File.WriteAllText(SavePath, jsonData);
            
            if (debugMode)
                Debug.Log($"Game saved successfully to: {SavePath}");
            
            // Notify UI
            if (uiManager != null)
                uiManager.OnGameSaved();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            
            // Try to restore from backup
            if (File.Exists(BackupPath))
            {
                File.Copy(BackupPath, SavePath, true);
                Debug.Log("Restored from backup save");
            }
        }
    }

    public void LoadGame()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("No save file found");
                return;
            }

            // Read JSON data
            string jsonData = File.ReadAllText(SavePath);
            
            // Deserialize
            currentSaveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            if (debugMode)
                Debug.Log($"Game loaded successfully from: {SavePath}");
            
            // Apply loaded data
            ApplyLoadedData();
            
            // Notify UI
            if (uiManager != null)
                uiManager.OnGameLoaded();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            
            // Try to load from backup
            if (File.Exists(BackupPath))
            {
                try
                {
                    string backupData = File.ReadAllText(BackupPath);
                    currentSaveData = JsonUtility.FromJson<GameSaveData>(backupData);
                    ApplyLoadedData();
                    Debug.Log("Loaded from backup save");
                }
                catch (Exception backupError)
                {
                    Debug.LogError($"Failed to load from backup: {backupError.Message}");
                }
            }
        }
    }

    public void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted");
            }
            
            if (File.Exists(BackupPath))
            {
                File.Delete(BackupPath);
                Debug.Log("Backup file deleted");
            }
            
            currentSaveData = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
        }
    }

    public void CreateNewGame()
    {
        try
        {
            currentSaveData = CreateNewSaveData();
            
            // Save immediately
            SaveGame();
            
            if (debugMode)
                Debug.Log("New game created and saved");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create new game: {e.Message}");
        }
    }
    #endregion

    #region Data Creation
    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // Get current game state from managers
        saveData.playerData = GetCurrentPlayerData();
        saveData.levelData = GetCurrentLevelData();
        saveData.settingsData = GetCurrentSettingsData();
        
        // Metadata
        saveData.saveTime = DateTime.Now;
        saveData.saveVersion = Application.version;
        
        // Statistics
        if (currentSaveData != null)
        {
            saveData.totalPlayTime = currentSaveData.totalPlayTime + Mathf.RoundToInt(Time.time);
            saveData.totalDeaths = currentSaveData.totalDeaths;
            saveData.totalKills = currentSaveData.totalKills;
            saveData.totalRoomsCleared = currentSaveData.totalRoomsCleared;
        }
        
        return saveData;
    }

    private GameSaveData CreateNewSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // Initialize player data
        saveData.playerData = new PlayerData
        {
            currentHealth = 100f,
            maxHealth = 100f,
            currentMana = 50f,
            maxMana = 50f,
            level = 1,
            experience = 0,
            score = 0,
            playerPosition = Vector3.zero,
            currentElement = "Fire",
            unlockedAbilities = new List<string> { "BasicAttack" },
            inventoryItems = new List<string>(),
            gameTime = 0f
        };
        
        // Initialize level data
        saveData.levelData = new LevelData
        {
            currentLevel = 1,
            currentRoom = 0,
            currentBiome = "Forest",
            playerSpawnPosition = Vector3.zero,
            visitedRooms = new List<SaveRoomData>(),
            activeEnemies = new List<EnemyData>(),
            activePickups = new List<PickupData>(),
            isBossRoom = false,
            isCompleted = false
        };
        
        // Initialize settings data
        saveData.settingsData = GetCurrentSettingsData();
        
        // Metadata
        saveData.saveTime = DateTime.Now;
        saveData.saveVersion = Application.version;
        saveData.totalPlayTime = 0;
        saveData.totalDeaths = 0;
        saveData.totalKills = 0;
        saveData.totalRoomsCleared = 0;
        
        return saveData;
    }
    #endregion

    #region Data Retrieval
    private PlayerData GetCurrentPlayerData()
    {
        PlayerData playerData = new PlayerData();
        
        // Get player data from Player component or GameManager
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Example - adjust based on your player component
            // Player playerComponent = player.GetComponent<Player>();
            // if (playerComponent != null)
            // {
            //     playerData.currentHealth = playerComponent.CurrentHealth;
            //     playerData.maxHealth = playerComponent.MaxHealth;
            //     playerData.currentMana = playerComponent.CurrentMana;
            //     playerData.maxMana = playerComponent.MaxMana;
            //     playerData.level = playerComponent.Level;
            //     playerData.experience = playerComponent.Experience;
            //     playerData.score = playerComponent.Score;
            //     playerData.playerPosition = player.transform.position;
            //     playerData.currentElement = playerComponent.CurrentElement;
            //     playerData.unlockedAbilities = playerComponent.UnlockedAbilities;
            //     playerData.inventoryItems = playerComponent.InventoryItems;
            // }
            
            // For now, use default values or current save data
            if (currentSaveData != null)
            {
                playerData = currentSaveData.playerData;
                playerData.playerPosition = player.transform.position;
                playerData.gameTime = uiManager != null ? uiManager.GetGameTime() : 0f;
            }
        }
        
        return playerData;
    }

    private LevelData GetCurrentLevelData()
    {
        LevelData levelData = new LevelData();
        
        // Get level data from LevelManager or GameManager
        // Example - adjust based on your level system
        // LevelManager levelManager = FindObjectOfType<LevelManager>();
        // if (levelManager != null)
        // {
        //     levelData.currentLevel = levelManager.CurrentLevel;
        //     levelData.currentRoom = levelManager.CurrentRoom;
        //     levelData.currentBiome = levelManager.CurrentBiome;
        //     levelData.visitedRooms = levelManager.VisitedRooms;
        //     levelData.activeEnemies = levelManager.ActiveEnemies;
        //     levelData.activePickups = levelManager.ActivePickups;
        // }
        
        // For now, use current save data or defaults
        if (currentSaveData != null)
        {
            levelData = currentSaveData.levelData;
        }
        
        return levelData;
    }

    private SettingsData GetCurrentSettingsData()
    {
        SettingsData settingsData = new SettingsData();
        
        // Get settings from PlayerPrefs
        settingsData.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        settingsData.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        settingsData.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        settingsData.isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        settingsData.qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
        settingsData.language = PlayerPrefs.GetString("Language", "English");
        settingsData.showDamageNumbers = PlayerPrefs.GetInt("ShowDamageNumbers", 1) == 1;
        settingsData.showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        
        return settingsData;
    }
    #endregion

    #region Data Application
    private void ApplyLoadedData()
    {
        if (currentSaveData == null) return;
        
        // Apply player data
        ApplyPlayerData(currentSaveData.playerData);
        
        // Apply level data
        ApplyLevelData(currentSaveData.levelData);
        
        // Apply settings data
        ApplySettingsData(currentSaveData.settingsData);
        
        if (debugMode)
            Debug.Log("Loaded data applied successfully");
    }

    private void ApplyPlayerData(PlayerData playerData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Apply player position
            player.transform.position = playerData.playerPosition;
            
            // Apply player stats to Player component
            // Player playerComponent = player.GetComponent<Player>();
            // if (playerComponent != null)
            // {
            //     playerComponent.SetHealth(playerData.currentHealth, playerData.maxHealth);
            //     playerComponent.SetMana(playerData.currentMana, playerData.maxMana);
            //     playerComponent.SetLevel(playerData.level);
            //     playerComponent.SetExperience(playerData.experience);
            //     playerComponent.SetScore(playerData.score);
            //     playerComponent.SetElement(playerData.currentElement);
            //     playerComponent.SetUnlockedAbilities(playerData.unlockedAbilities);
            //     playerComponent.SetInventoryItems(playerData.inventoryItems);
            // }
            
            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateHealthUI(playerData.currentHealth, playerData.maxHealth);
                uiManager.UpdateManaUI(playerData.currentMana, playerData.maxMana);
                uiManager.UpdateScore(playerData.score);
                uiManager.UpdateLevel(playerData.level);
            }
        }
    }

    private void ApplyLevelData(LevelData levelData)
    {
        // Apply level data to LevelManager or GameManager
        // LevelManager levelManager = FindObjectOfType<LevelManager>();
        // if (levelManager != null)
        // {
        //     levelManager.SetCurrentLevel(levelData.currentLevel);
        //     levelManager.SetCurrentRoom(levelData.currentRoom);
        //     levelManager.SetCurrentBiome(levelData.currentBiome);
        //     levelManager.SetVisitedRooms(levelData.visitedRooms);
        //     levelManager.SetActiveEnemies(levelData.activeEnemies);
        //     levelManager.SetActivePickups(levelData.activePickups);
        // }
    }

    private void ApplySettingsData(SettingsData settingsData)
    {
        // Apply settings
        PlayerPrefs.SetFloat("MasterVolume", settingsData.masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settingsData.musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", settingsData.sfxVolume);
        PlayerPrefs.SetInt("Fullscreen", settingsData.isFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", settingsData.qualityLevel);
        PlayerPrefs.SetString("Language", settingsData.language);
        PlayerPrefs.SetInt("ShowDamageNumbers", settingsData.showDamageNumbers ? 1 : 0);
        PlayerPrefs.SetInt("ShowFPS", settingsData.showFPS ? 1 : 0);
        PlayerPrefs.Save();
        
        // Apply to current system
        Screen.fullScreen = settingsData.isFullscreen;
        QualitySettings.SetQualityLevel(settingsData.qualityLevel);
    }
    #endregion

    #region Auto Save
    private void CheckAutoSave()
    {
        if (Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            SaveGame();
            lastAutoSaveTime = Time.time;
        }
    }

    public void SetAutoSaveEnabled(bool enabled)
    {
        autoSaveEnabled = enabled;
        PlayerPrefs.SetInt("AutoSaveEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    #endregion

    #region Public Interface
    public void SaveGameToSlot(int slot)
    {
        // For multiple save slots
        string slotPath = Path.Combine(Application.persistentDataPath, $"gamesave_slot_{slot}.json");
        // Implementation for multiple save slots
    }

    public void LoadGameFromSlot(int slot)
    {
        // For multiple save slots
        string slotPath = Path.Combine(Application.persistentDataPath, $"gamesave_slot_{slot}.json");
        // Implementation for multiple save slots
    }

    public bool HasSaveDataInSlot(int slot)
    {
        string slotPath = Path.Combine(Application.persistentDataPath, $"gamesave_slot_{slot}.json");
        return File.Exists(slotPath);
    }

    public DateTime GetSaveTime()
    {
        return currentSaveData?.saveTime ?? DateTime.MinValue;
    }

    public string GetSaveInfo()
    {
        if (currentSaveData == null) return "No save data";
        
        return $"Level {currentSaveData.playerData.level} - " +
               $"Health: {currentSaveData.playerData.currentHealth}/{currentSaveData.playerData.maxHealth} - " +
               $"Score: {currentSaveData.playerData.score} - " +
               $"Time: {currentSaveData.saveTime:MM/dd/yyyy HH:mm}";
    }
    #endregion
} 