using UnityEngine;

public class SaveSystemTest : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode saveKey = KeyCode.F5;
    [SerializeField] private KeyCode loadKey = KeyCode.F9;
    [SerializeField] private KeyCode newGameKey = KeyCode.F1;
    [SerializeField] private KeyCode deleteSaveKey = KeyCode.Delete;

    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private string lastAction = "None";

    private SaveManager saveManager;
    private UI_Manager uiManager;

    void Start()
    {
        saveManager = SaveManager.Instance;
        uiManager = FindFirstObjectByType<UI_Manager>();
        
        if (saveManager == null)
        {
            Debug.LogError("SaveManager not found! Make sure SaveManager is in the scene.");
        }
    }

    void Update()
    {
        // Test controls
        if (Input.GetKeyDown(saveKey))
        {
            TestSave();
        }
        
        if (Input.GetKeyDown(loadKey))
        {
            TestLoad();
        }
        
        if (Input.GetKeyDown(newGameKey))
        {
            TestNewGame();
        }
        
        if (Input.GetKeyDown(deleteSaveKey))
        {
            TestDeleteSave();
        }
    }

    void TestSave()
    {
        if (saveManager != null)
        {
            saveManager.SaveGame();
            lastAction = "Save Game";
            Debug.Log("Test: Save Game triggered");
        }
    }

    void TestLoad()
    {
        if (saveManager != null)
        {
            saveManager.LoadGame();
            lastAction = "Load Game";
            Debug.Log("Test: Load Game triggered");
        }
    }

    void TestNewGame()
    {
        if (saveManager != null)
        {
            saveManager.CreateNewGame();
            lastAction = "New Game";
            Debug.Log("Test: New Game triggered");
        }
    }

    void TestDeleteSave()
    {
        if (saveManager != null)
        {
            saveManager.DeleteSave();
            lastAction = "Delete Save";
            Debug.Log("Test: Delete Save triggered");
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Save System Test", GUI.skin.box);
        
        GUILayout.Label($"Last Action: {lastAction}");
        GUILayout.Label($"Has Save Data: {saveManager?.HasSaveData ?? false}");
        
        if (saveManager?.CurrentSaveData != null)
        {
            var saveData = saveManager.CurrentSaveData;
            GUILayout.Label($"Player Level: {saveData.playerData.level}");
            GUILayout.Label($"Player Health: {saveData.playerData.currentHealth}/{saveData.playerData.maxHealth}");
            GUILayout.Label($"Player Mana: {saveData.playerData.currentMana}/{saveData.playerData.maxMana}");
            GUILayout.Label($"Score: {saveData.playerData.score}");
            GUILayout.Label($"Save Time: {saveData.saveTime:MM/dd/yyyy HH:mm}");
        }
        
        GUILayout.Label("Controls:");
        GUILayout.Label($"Save: {saveKey}");
        GUILayout.Label($"Load: {loadKey}");
        GUILayout.Label($"New Game: {newGameKey}");
        GUILayout.Label($"Delete Save: {deleteSaveKey}");
        
        GUILayout.EndArea();
    }
} 