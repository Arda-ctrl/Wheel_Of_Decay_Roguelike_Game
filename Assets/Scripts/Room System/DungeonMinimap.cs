using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DungeonMinimap : MonoBehaviour
{
    [Header("Minimap Settings")]
    public Transform minimapContainer;
    public GameObject roomIconPrefab;
    public GameObject playerIconPrefab;
    public float cellSize = 20f;
    public Color unexploredColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color exploredColor = Color.white;
    public Color currentRoomColor = Color.yellow;
    
    [Header("Display Settings")]
    public bool revealAllRooms = false;
    public bool showUnexploredRooms = false;
    
    // Dictionary to store room positions and their UI representations
    private Dictionary<Vector2Int, RoomMapIcon> roomIcons = new Dictionary<Vector2Int, RoomMapIcon>();
    private Vector2Int currentPlayerPosition;
    private GameObject playerIcon;
    
    // Delegate for room entry
    public delegate void RoomEnteredHandler(Vector2Int roomPos);
    public static RoomEnteredHandler OnRoomEntered;
    
    private void Awake()
    {
        // Create container if not assigned
        if (minimapContainer == null)
        {
            GameObject container = new GameObject("Minimap Container");
            container.transform.SetParent(transform, false);
            minimapContainer = container.transform;
        }
        
        // Initialize roomIcons dictionary
        roomIcons = new Dictionary<Vector2Int, RoomMapIcon>();
    }
    
    private void OnEnable()
    {
        // Subscribe to room entered event
        OnRoomEntered += HandleRoomEntered;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from room entered event
        OnRoomEntered -= HandleRoomEntered;
    }
    
    // Call this method from RoomGenerator after the dungeon is generated
    public void InitializeMinimap(Dictionary<Vector2Int, GameObject> roomGrid, Vector2Int startRoomPos)
    {
        ClearMinimap();
        
        foreach (var roomEntry in roomGrid)
        {
            Vector2Int gridPos = roomEntry.Key;
            GameObject roomObj = roomEntry.Value;
            
            // Create map icon for this room
            CreateRoomIcon(gridPos, roomObj.GetComponent<RoomData>());
        }
        
        // Set the start room as current and explored
        if (roomIcons.TryGetValue(startRoomPos, out RoomMapIcon startIcon))
        {
            startIcon.SetExplored(true);
            SetCurrentRoom(startRoomPos);
        }
        
        // If we should reveal all rooms, do so
        if (revealAllRooms)
        {
            RevealAllRooms();
        }
    }
    
    private void CreateRoomIcon(Vector2Int gridPos, RoomData roomData)
    {
        if (roomIconPrefab == null) return;
        
        // Calculate position in UI space
        Vector2 uiPos = new Vector2(gridPos.x * cellSize, gridPos.y * cellSize);
        
        // Instantiate icon
        GameObject iconObj = Instantiate(roomIconPrefab, minimapContainer);
        iconObj.transform.localPosition = uiPos;
        
        // Get or add RoomMapIcon component
        RoomMapIcon roomIcon = iconObj.GetComponent<RoomMapIcon>();
        if (roomIcon == null)
        {
            roomIcon = iconObj.AddComponent<RoomMapIcon>();
        }
        
        // Initialize icon
        roomIcon.Initialize(gridPos, roomData);
        
        // Set initial state
        roomIcon.SetExplored(revealAllRooms);
        roomIcon.SetVisible(revealAllRooms || showUnexploredRooms);
        
        // Add to dictionary
        roomIcons[gridPos] = roomIcon;
    }
    
    public void HandleRoomEntered(Vector2Int roomPos)
    {
        Debug.Log($"Player entered room at {roomPos}");
        
        // Set this room as current
        SetCurrentRoom(roomPos);
        
        // Mark this room as explored
        if (roomIcons.TryGetValue(roomPos, out RoomMapIcon roomIcon))
        {
            roomIcon.SetExplored(true);
            roomIcon.SetVisible(true);
        }
        
        // Reveal neighboring rooms
        RevealNeighbors(roomPos);
    }
    
    private void SetCurrentRoom(Vector2Int roomPos)
    {
        // Reset previous current room
        if (roomIcons.TryGetValue(currentPlayerPosition, out RoomMapIcon prevRoom))
        {
            prevRoom.SetCurrent(false);
        }
        
        // Set new current room
        if (roomIcons.TryGetValue(roomPos, out RoomMapIcon newRoom))
        {
            newRoom.SetCurrent(true);
        }
        
        // Update player position
        currentPlayerPosition = roomPos;
        
        // Move player icon if it exists
        if (playerIcon != null)
        {
            Vector2 uiPos = new Vector2(roomPos.x * cellSize, roomPos.y * cellSize);
            playerIcon.transform.localPosition = uiPos;
        }
        else if (playerIconPrefab != null)
        {
            // Create player icon if it doesn't exist
            Vector2 uiPos = new Vector2(roomPos.x * cellSize, roomPos.y * cellSize);
            playerIcon = Instantiate(playerIconPrefab, minimapContainer);
            playerIcon.transform.localPosition = uiPos;
        }
    }
    
    private void RevealNeighbors(Vector2Int roomPos)
    {
        // Check all four directions
        Vector2Int[] neighbors = new Vector2Int[] 
        {
            new Vector2Int(roomPos.x, roomPos.y + 1), // North
            new Vector2Int(roomPos.x + 1, roomPos.y), // East
            new Vector2Int(roomPos.x, roomPos.y - 1), // South
            new Vector2Int(roomPos.x - 1, roomPos.y)  // West
        };
        
        foreach (Vector2Int neighborPos in neighbors)
        {
            if (roomIcons.TryGetValue(neighborPos, out RoomMapIcon neighborIcon))
            {
                // Just make the neighbor visible, don't mark as explored
                neighborIcon.SetVisible(true);
            }
        }
    }
    
    public void RevealAllRooms()
    {
        foreach (var icon in roomIcons.Values)
        {
            icon.SetVisible(true);
            icon.SetExplored(true);
        }
    }
    
    public void ClearMinimap()
    {
        // Destroy all icons
        foreach (var icon in roomIcons.Values)
        {
            if (icon != null && icon.gameObject != null)
            {
                Destroy(icon.gameObject);
            }
        }
        
        // Destroy player icon if it exists
        if (playerIcon != null)
        {
            Destroy(playerIcon);
            playerIcon = null;
        }
        
        // Clear dictionary
        roomIcons.Clear();
        
        // Reset current position
        currentPlayerPosition = Vector2Int.zero;
    }
}

// Helper class for room icons on the minimap
public class RoomMapIcon : MonoBehaviour
{
    private Vector2Int gridPosition;
    private RoomData roomData;
    private bool isExplored = false;
    private bool isCurrent = false;
    
    // References to components
    private Image iconImage;
    
    private void Awake()
    {
        // Get components
        iconImage = GetComponent<Image>();
        if (iconImage == null)
        {
            iconImage = gameObject.AddComponent<Image>();
        }
    }
    
    public void Initialize(Vector2Int gridPos, RoomData data)
    {
        gridPosition = gridPos;
        roomData = data;
        
        // Set up visuals
        if (data != null && data.biom != null && data.biom.minimapTile != null)
        {
            iconImage.sprite = data.biom.minimapTile;
        }
        
        // Set initial state
        SetExplored(false);
        SetCurrent(false);
    }
    
    public void SetExplored(bool explored)
    {
        isExplored = explored;
        UpdateVisuals();
    }
    
    public void SetCurrent(bool current)
    {
        isCurrent = current;
        UpdateVisuals();
    }
    
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    private void UpdateVisuals()
    {
        if (iconImage == null) return;
        
        // Set color based on state
        if (isCurrent)
        {
            iconImage.color = GetComponent<DungeonMinimap>().currentRoomColor;
        }
        else if (isExplored)
        {
            iconImage.color = GetComponent<DungeonMinimap>().exploredColor;
        }
        else
        {
            iconImage.color = GetComponent<DungeonMinimap>().unexploredColor;
        }
    }
} 