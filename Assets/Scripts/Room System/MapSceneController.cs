using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the map scene UI and interactions.
/// </summary>
public class MapSceneController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mapCanvas;
    public Button continueButton;
    public Button newRunButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    
    [Header("Map References")]
    public MapManager mapManager;
    
    [Header("Map Data")]
    public MapBranchData[] availableMaps;
    public int currentMapIndex = 0;
    
    private void Start()
    {
        // Set up UI
        SetupUI();
        
        // Load map
        LoadCurrentMap();
    }
    
    private void SetupUI()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }
        
        if (newRunButton != null)
        {
            newRunButton.onClick.AddListener(StartNewRun);
        }
        
        // Update map info
        UpdateMapInfo();
    }
    
    private void UpdateMapInfo()
    {
        if (availableMaps == null || availableMaps.Length == 0 || currentMapIndex >= availableMaps.Length)
        {
            return;
        }
        
        MapBranchData currentMap = availableMaps[currentMapIndex];
        
        if (titleText != null)
        {
            titleText.text = currentMap.branchName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = currentMap.branchDescription;
        }
    }
    
    private void LoadCurrentMap()
    {
        if (mapManager == null)
        {
            mapManager = FindFirstObjectByType<MapManager>();
            
            if (mapManager == null)
            {
                Debug.LogError("MapManager not found!");
                return;
            }
        }
        
        if (availableMaps == null || availableMaps.Length == 0 || currentMapIndex >= availableMaps.Length)
        {
            // No maps available, generate procedural map
            mapManager.useSerializedMap = false;
            mapManager.GenerateMap();
            return;
        }
        
        // Load serialized map
        mapManager.useSerializedMap = true;
        mapManager.serializedMapData = availableMaps[currentMapIndex];
        mapManager.GenerateMap();
    }
    
    public void ContinueGame()
    {
        // Continue from last save point
        // This would need to be expanded with a proper save system
        Debug.Log("Continuing game from last save point...");
        
        // For now, just load the current map
        LoadCurrentMap();
    }
    
    public void StartNewRun()
    {
        // Start a new run with the current map
        Debug.Log("Starting new run...");
        
        // Reset map state
        if (mapManager != null)
        {
            mapManager.GenerateMap();
        }
    }
    
    public void NextMap()
    {
        if (availableMaps == null || availableMaps.Length <= 1)
        {
            return;
        }
        
        currentMapIndex = (currentMapIndex + 1) % availableMaps.Length;
        UpdateMapInfo();
        LoadCurrentMap();
    }
    
    public void PreviousMap()
    {
        if (availableMaps == null || availableMaps.Length <= 1)
        {
            return;
        }
        
        currentMapIndex = (currentMapIndex - 1 + availableMaps.Length) % availableMaps.Length;
        UpdateMapInfo();
        LoadCurrentMap();
    }
} 