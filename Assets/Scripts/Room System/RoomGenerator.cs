using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Direction enum for room connections
public enum Direction
{
    North, East, South, West
}

// ConnectionDirection enum'u RoomConnectionPoint.cs'de zaten tanımlandığı için burada tekrar tanımlamıyoruz

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator instance;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false; // Debug log'ları kapatmak için false yapın
    
    [Header("Generation Settings")]
    public int numberOfRooms = 10;
    public RoomData[] roomPool;
    public float xOffset = 18f;
    public float yOffset = 10f;
    public LayerMask whatIsRoom;
    public float roomCheckRadius = 0.2f; // Oda çakışma kontrolü için yarıçap
    
    [Header("Connection Closure")]
    public GameObject doorCapPrefab; // Prefab to use for closing open doorways
    public bool allowBossDeadEnd = true; // Allow boss room to be a dead-end with only one entrance
    
    private List<GameObject> spawnedRooms = new List<GameObject>();
    private Transform generatorPoint;
    private GameObject startRoom;
    private GameObject endRoom;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            if (enableDebugLogs) Debug.LogWarning("Multiple RoomGenerator instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        if (enableDebugLogs) Debug.Log("RoomGenerator initialized");
    }

    private void Start()
    {
        generatorPoint = new GameObject("Generator Point").transform;
        generatorPoint.position = Vector3.zero;
        GenerateLevel();
        
        // Initialize minimap after generation
        InitializeMinimap();
    }
    
    // Room grid for tracking placed rooms
    private Dictionary<Vector2Int, GameObject> roomGrid = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int startRoomPos = Vector2Int.zero;

    void GenerateLevel()
    {
        if (enableDebugLogs) Debug.Log("RoomGenerator.GenerateLevel() called");
        
        // Check if room pool is valid
        if (roomPool == null || roomPool.Length == 0)
        {
            Debug.LogError("Room pool is empty! Cannot generate level.");
            return;
        }
        
        if (enableDebugLogs) Debug.Log($"Room pool contains {roomPool.Length} rooms");
        
        // Clear any previous data
        spawnedRooms.Clear();
        roomGrid.Clear();
        
        // Track generation attempts to prevent infinite loops
        int generationAttempts = 0;
        bool generationSuccessful = false;
        
        while (!generationSuccessful && generationAttempts < 3)
        {
            generationAttempts++;
            
            // Clear previous attempt
            foreach (var room in spawnedRooms)
            {
                if (room != null)
                {
                    Destroy(room);
                }
            }
            spawnedRooms.Clear();
            roomGrid.Clear();
            
            // Try to generate a valid dungeon
            generationSuccessful = GenerateDungeonLayout();
            
            if (!generationSuccessful)
            {
                Debug.LogWarning($"Dungeon generation attempt {generationAttempts} failed. Retrying...");
            }
        }
        
        if (!generationSuccessful)
        {
            Debug.LogError("Failed to generate a valid dungeon after multiple attempts!");
        }
        else
        {
            Debug.Log($"Dungeon generated successfully with {spawnedRooms.Count} rooms.");
            
            // Initialize minimap after generation
            InitializeMinimap();
        }
    }
    
    private bool GenerateDungeonLayout()
    {
        // Start with finding a start room
        RoomData startRoomData = roomPool.FirstOrDefault(r => r.isStartRoom);
        if (startRoomData == null)
        {
            // If no specific start room, use any room that has a south door closed
            startRoomData = FindRoomWithConnectionType(RoomData.RoomConnectionType.SingleUp);
            if (startRoomData == null)
            {
                Debug.LogError("No suitable start room found!");
                return false;
            }
        }
        
        // Place start room at origin
        Vector3 startRoomPos3D = Vector3.zero;
        startRoom = SpawnRoom(startRoomData, startRoomPos3D);
        if (startRoom == null)
        {
            Debug.LogError("Failed to spawn start room!");
            return false;
        }
        
        spawnedRooms.Add(startRoom);
        roomGrid.Add(startRoomPos, startRoom);
        
        Debug.Log($"Start room placed at {startRoomPos}");
        
        // Initialize open list with start room
        List<RoomOpenEntry> openList = new List<RoomOpenEntry>
        {
            new RoomOpenEntry { gridPos = startRoomPos, room = startRoom }
        };
        
        // Track number of rooms placed
        int roomsPlaced = 1;
        int attempts = 0;
        int maxAttemptsTotal = numberOfRooms * 10; // Increased attempts to ensure more rooms are placed
        
        Debug.Log($"Attempting to place {numberOfRooms} rooms");
        
        // Keep track of critical path positions
        HashSet<Vector2Int> criticalPathPositions = new HashSet<Vector2Int>();
        
        // Place rooms using door-aware placement
        while (openList.Count > 0 && roomsPlaced < numberOfRooms && attempts < maxAttemptsTotal)
        {
            attempts++;
            
            // Get a room from the open list (randomly with higher probability for latest rooms)
            int index = openList.Count > 3 ? Random.Range(openList.Count - 3, openList.Count) : Random.Range(0, openList.Count);
            RoomOpenEntry current = openList[index];
            openList.RemoveAt(index);
            
            // Try to expand in random directions
            List<Direction> directions = new List<Direction> { Direction.North, Direction.East, Direction.South, Direction.West };
            ShuffleList(directions);
            
            // First, check if there are any critical path positions around this room
            List<Vector2Int> criticalPositions = new List<Vector2Int>();
            foreach (Direction dir in directions)
            {
                Vector2Int newPos = GetPositionInDirection(current.gridPos, dir);
                
                // Skip if position already occupied
                if (roomGrid.ContainsKey(newPos)) continue;
                
                // Check if this is a critical path position
                if (IsPathBlockingRoom(newPos))
                {
                    criticalPositions.Add(newPos);
                    criticalPathPositions.Add(newPos);
                }
            }
            
            // First try to place rooms at critical path positions
            int successfulPlacements = 0;
            foreach (Vector2Int criticalPos in criticalPositions)
            {
                Direction dir = GetDirectionToPosition(current.gridPos, criticalPos);
                bool success = TryPlaceRoomWithDoor(current.gridPos, criticalPos, dir);
                
                if (success)
                {
                    roomsPlaced++;
                    successfulPlacements++;
                    Debug.Log($"Room {roomsPlaced} placed at critical path position {criticalPos}");
                    
                    // Always add to open list to ensure more connections
                    if (roomsPlaced < numberOfRooms)
                    {
                        openList.Add(new RoomOpenEntry { gridPos = criticalPos, room = roomGrid[criticalPos] });
                    }
                }
            }
            
            // Then try normal placements if we still have room
            if (successfulPlacements == 0)
            {
                foreach (Direction dir in directions)
                {
                    // Calculate new position
                    Vector2Int newPos = GetPositionInDirection(current.gridPos, dir);
                    
                    // Skip if position already occupied
                    if (roomGrid.ContainsKey(newPos))
                    {
                        continue;
                    }
                    
                    // Determine required door types
                    bool success = TryPlaceRoomWithDoor(current.gridPos, newPos, dir);
                    
                    if (success)
                    {
                        roomsPlaced++;
                        successfulPlacements++;
                        Debug.Log($"Room {roomsPlaced} placed at {newPos}");
                        
                        // Always add to open list to ensure more connections
                        if (roomsPlaced < numberOfRooms)
                        {
                            openList.Add(new RoomOpenEntry { gridPos = newPos, room = roomGrid[newPos] });
                        }
                        
                        // Try to place more rooms from current node, but limit to avoid too many branches from one room
                        if (successfulPlacements >= 2 || Random.value < 0.3f)
                        {
                            break;
                        }
                    }
                }
            }
            
            // If we've tried too many times without progress, break to avoid infinite loop
            if (attempts > roomsPlaced * 10 + 20)
            {
                Debug.LogWarning("Too many attempts without progress, breaking generation loop");
                break;
            }
        }
        
        Debug.Log($"Placed {roomsPlaced} rooms out of {numberOfRooms} after {attempts} attempts");
        
        // Place end room
        if (!PlaceEndRoom())
        {
            Debug.LogWarning("Failed to place end room!");
            return false;
        }
        
        // Ensure all path-blocking positions have rooms
        EnsurePathConnectivity();
        
        // Handle open connections before verification
        CloseOpenConnections();
        
        // Verify dungeon is valid (all rooms reachable, end room accessible)
        return VerifyDungeonLayout();
    }
    
    private bool TryPlaceRoomWithDoor(Vector2Int sourcePos, Vector2Int targetPos, Direction direction)
    {
        // Mevcut odaların kapı düzenleri kontrol ediliyor
        GameObject sourceRoomObj = roomGrid[sourcePos];
        if (sourceRoomObj == null)
        {
            Debug.LogError($"Source room at position {sourcePos} is null!");
            return false;
        }
        
        // Kapı türlerini belirle
        RoomData.RoomConnectionType requiredSourceDoorType = GetRequiredDoorType(direction);
        RoomData.RoomConnectionType requiredTargetDoorType = GetRequiredDoorType(GetOppositeDirection(direction));
        
        // Uygun odaları havuzdan bul
        RoomData targetRoomData = FindRoomWithConnectionType(requiredTargetDoorType);
        
        if (targetRoomData == null)
        {
            Debug.LogWarning($"No suitable room found for direction {direction} (connection type {requiredTargetDoorType})");
            return false;
        }
        
        // Dünya konumunu hesapla - xOffset ve yOffset kullan
        Vector3 worldPos = new Vector3(targetPos.x * xOffset, targetPos.y * yOffset, 0);
        
        Debug.Log($"Placing room at world position: {worldPos}, grid position: {targetPos}, direction from source: {direction}");
        
        // Çakışma kontrolü yap
        Collider2D collision = Physics2D.OverlapCircle(worldPos, roomCheckRadius, whatIsRoom);
        if (collision != null)
        {
            Debug.LogWarning($"Room collision detected at {worldPos}. Cannot place room.");
            return false;
        }
        
        // Oda oluştur
        GameObject targetRoomObj = SpawnRoom(targetRoomData, worldPos);
        if (targetRoomObj == null)
        {
            Debug.LogError("Failed to spawn target room!");
            return false;
        }
        
        // Odayı kaydet
        spawnedRooms.Add(targetRoomObj);
        roomGrid[targetPos] = targetRoomObj;
        
        // Kapıları bağla
        ConnectRoomDoors(sourceRoomObj, targetRoomObj, direction);
        
        return true;
    }
    
    // Odalar arasındaki kapı bağlantılarını kur
    private void ConnectRoomDoors(GameObject sourceRoom, GameObject targetRoom, Direction direction)
    {
        if (sourceRoom == null || targetRoom == null)
        {
            Debug.LogError("ConnectRoomDoors: Source or target room is null!");
            return;
        }

        // Get connection points from both rooms
        var sourceRoomPoints = sourceRoom.GetComponentsInChildren<RoomConnectionPoint>();
        var targetRoomPoints = targetRoom.GetComponentsInChildren<RoomConnectionPoint>();
        
        if (sourceRoomPoints == null || sourceRoomPoints.Length == 0 || targetRoomPoints == null || targetRoomPoints.Length == 0)
        {
            Debug.LogWarning($"One or both rooms have no connection points! Source: {sourceRoom.name}, Target: {targetRoom.name}");
            return;
        }
        
        // Find connection points in the right direction
        ConnectionDirection sourceDirection = ConvertToConnectionDirection(direction);
        ConnectionDirection targetDirection = ConvertToConnectionDirection(GetOppositeDirection(direction));
        
        Debug.Log($"Looking for connection points - Source: {sourceDirection}, Target: {targetDirection}");
        
        var sourcePoint = System.Array.Find(sourceRoomPoints, p => p != null && p.direction == sourceDirection);
        var targetPoint = System.Array.Find(targetRoomPoints, p => p != null && p.direction == targetDirection);
        
        if (sourcePoint != null && targetPoint != null)
        {
            // Connect the doors using the new Connect method
            sourcePoint.Connect(targetPoint);
            
            Debug.Log($"Connected {direction} door of source room to {GetOppositeDirection(direction)} door of target room");
            
            // Get DoorTrigger components if they exist
            DoorTrigger sourceDoorTrigger = sourcePoint.GetComponent<DoorTrigger>();
            DoorTrigger targetDoorTrigger = targetPoint.GetComponent<DoorTrigger>();
            
            // Set up door triggers if they exist
            if (sourceDoorTrigger != null)
                sourceDoorTrigger.connectionPoint = sourcePoint;
            
            if (targetDoorTrigger != null)
                targetDoorTrigger.connectionPoint = targetPoint;
        }
        else
        {
            Debug.LogWarning($"Could not find connection points for direction {direction}. Source point: {sourcePoint != null}, Target point: {targetPoint != null}");
            Debug.LogWarning($"Source room: {sourceRoom.name}, Target room: {targetRoom.name}");
            
            // List all connection points for debugging
            Debug.Log("Source room connection points:");
            foreach (var point in sourceRoomPoints)
            {
                if (point != null)
                    Debug.Log($"  - Direction: {point.direction}, Occupied: {point.isOccupied}");
            }
            
            Debug.Log("Target room connection points:");
            foreach (var point in targetRoomPoints)
            {
                if (point != null)
                    Debug.Log($"  - Direction: {point.direction}, Occupied: {point.isOccupied}");
            }
        }
    }
    
    // Direction enum'u ile ConnectionDirection enum'u arasında dönüşüm yapan yardımcı fonksiyon
    private ConnectionDirection ConvertToConnectionDirection(Direction dir)
    {
        switch(dir)
        {
            case Direction.North: return ConnectionDirection.North;
            case Direction.East: return ConnectionDirection.East;
            case Direction.South: return ConnectionDirection.South;
            case Direction.West: return ConnectionDirection.West;
            default: return ConnectionDirection.North;
        }
    }
    
    private bool PlaceEndRoom()
    {
        // Find the room furthest from start
        Vector2Int furthestPos = FindFurthestRoomPosition();
        
        // Find an end room in the pool
        RoomData endRoomData = roomPool.FirstOrDefault(r => r.isEndRoom);
        if (endRoomData == null)
        {
            Debug.LogWarning("No end room found in room pool!");
            return false;
        }
        
        // If there's already a room at the furthest position, replace it
        if (roomGrid.TryGetValue(furthestPos, out GameObject existingRoom))
        {
            int index = spawnedRooms.IndexOf(existingRoom);
            Destroy(existingRoom);
            
            Vector3 worldPos = new Vector3(furthestPos.x * xOffset, furthestPos.y * yOffset, 0);
            endRoom = SpawnRoom(endRoomData, worldPos);
            
            if (endRoom != null)
            {
                spawnedRooms[index] = endRoom;
                roomGrid[furthestPos] = endRoom;
                return true;
            }
        }
        
        return false;
    }
    
    private Vector2Int FindFurthestRoomPosition()
    {
        Vector2Int furthestPos = startRoomPos;
        float maxDistance = 0;
        
        foreach (var kvp in roomGrid)
        {
            float distance = Vector2Int.Distance(startRoomPos, kvp.Key);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestPos = kvp.Key;
            }
        }
        
        return furthestPos;
    }
    
    private bool VerifyDungeonLayout()
    {
        // Perform DFS to check if all rooms are reachable from start
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        
        stack.Push(startRoomPos);
        
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            
            if (!visited.Contains(current))
            {
                visited.Add(current);
                
                // Check all four directions
                List<Direction> directions = new List<Direction> { Direction.North, Direction.East, Direction.South, Direction.West };
                foreach (Direction dir in directions)
                {
                    Vector2Int neighbor = GetPositionInDirection(current, dir);
                    if (roomGrid.ContainsKey(neighbor) && !visited.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }
        }
        
        // Verify all rooms were visited
        bool allRoomsReachable = visited.Count == roomGrid.Count;
        
        // If not all rooms are reachable, try to fix the dungeon
        if (!allRoomsReachable)
        {
            Debug.LogWarning($"Not all rooms are reachable: {visited.Count}/{roomGrid.Count}. Attempting to fix...");
            
            // Identify unreachable rooms
            List<Vector2Int> unreachableRooms = new List<Vector2Int>();
            foreach (var roomPos in roomGrid.Keys)
            {
                if (!visited.Contains(roomPos))
                {
                    unreachableRooms.Add(roomPos);
                }
            }
            
            // Try to connect each unreachable room to the main component
            foreach (var roomPos in unreachableRooms)
            {
                // Find the closest reachable room
                Vector2Int closestReachable = startRoomPos;
                float minDistance = float.MaxValue;
                
                foreach (var reachablePos in visited)
                {
                    float distance = Vector2.Distance(roomPos, reachablePos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestReachable = reachablePos;
                    }
                }
                
                // Try to connect this room to the closest reachable room
                if (TryConnectComponents(closestReachable, roomPos))
                {
                    Debug.Log($"Fixed connectivity for room at {roomPos}");
                }
                else
                {
                    Debug.LogWarning($"Failed to fix connectivity for room at {roomPos}");
                }
            }
            
            // Re-run the connectivity check
            visited.Clear();
            stack.Clear();
            stack.Push(startRoomPos);
            
            while (stack.Count > 0)
            {
                Vector2Int current = stack.Pop();
                
                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    
                    // Check all four directions
                    List<Direction> directions = new List<Direction> { Direction.North, Direction.East, Direction.South, Direction.West };
                    foreach (Direction dir in directions)
                    {
                        Vector2Int neighbor = GetPositionInDirection(current, dir);
                        if (roomGrid.ContainsKey(neighbor) && !visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
            }
            
            // Check again
            allRoomsReachable = visited.Count == roomGrid.Count;
        }
        
        // Verify end room is reachable
        bool endRoomReachable = false;
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value == endRoom && visited.Contains(kvp.Key))
            {
                endRoomReachable = true;
                break;
            }
        }
        
        // Check for any remaining open connections
        int openConnectionCount = CountOpenConnections();
        if (openConnectionCount > 0)
        {
            Debug.LogWarning($"Dungeon still has {openConnectionCount} open connections after validation");
            return false;
        }
        
        if (!allRoomsReachable)
        {
            Debug.LogError("Not all rooms are reachable even after fixing attempts");
            return false;
        }
        
        if (!endRoomReachable)
        {
            Debug.LogError("End room is not reachable");
            return false;
        }
        
        Debug.Log("Dungeon layout verified successfully - all rooms are connected and accessible");
        return allRoomsReachable && endRoomReachable;
    }
    
    // Finds and handles all open connections in the dungeon
    private void CloseOpenConnections()
    {
        Debug.Log("Analyzing dungeon for open connections...");
        
        // First pass - identify all open connections
        Dictionary<GameObject, List<RoomConnectionPoint>> openConnectionsMap = new Dictionary<GameObject, List<RoomConnectionPoint>>();
        
        foreach (GameObject room in spawnedRooms)
        {
            if (room == null) continue;
            
            // Find all connection points in this room
            RoomConnectionPoint[] connectionPoints = room.GetComponentsInChildren<RoomConnectionPoint>();
            List<RoomConnectionPoint> openPoints = new List<RoomConnectionPoint>();
            
            // Find unoccupied connection points
            foreach (RoomConnectionPoint point in connectionPoints)
            {
                if (point != null && !point.isOccupied)
                {
                    openPoints.Add(point);
                }
            }
            
            if (openPoints.Count > 0)
            {
                openConnectionsMap.Add(room, openPoints);
                Debug.Log($"Room {room.name} has {openPoints.Count} open connections");
            }
        }
        
        // Special case for end room - allow it to be a dead end with only one entrance if setting is enabled
        if (allowBossDeadEnd && endRoom != null)
        {
            // Find all connections in the boss room
            RoomConnectionPoint[] bossConnections = endRoom.GetComponentsInChildren<RoomConnectionPoint>();
            
            // Count how many connections are already occupied (should be 1 for entrance)
            int occupiedConnections = 0;
            foreach (RoomConnectionPoint point in bossConnections)
            {
                if (point != null && point.isOccupied)
                {
                    occupiedConnections++;
                }
            }
            
            // If only one connection is occupied (the entrance), remove the boss room from the open connections map
            if (occupiedConnections == 1 && openConnectionsMap.ContainsKey(endRoom))
            {
                Debug.Log("Allowing boss room to be a dead-end with single entrance");
                openConnectionsMap.Remove(endRoom);
            }
        }
        
        // Second pass - try to connect open doors to each other or create new rooms
        while (openConnectionsMap.Count > 0)
        {
            // Try to handle one room at a time
            GameObject currentRoom = openConnectionsMap.Keys.First();
            List<RoomConnectionPoint> openPoints = openConnectionsMap[currentRoom];
            
            // Process each open connection point
            List<RoomConnectionPoint> processedPoints = new List<RoomConnectionPoint>();
            
            foreach (RoomConnectionPoint point in openPoints)
            {
                bool handled = TryConnectOpenPoint(point);
                
                if (handled)
                {
                    processedPoints.Add(point);
                }
                else
                {
                    // Cap the door if we couldn't connect it
                    CapOpenConnection(point);
                    processedPoints.Add(point);
                }
            }
            
            // Remove processed points
            foreach (RoomConnectionPoint point in processedPoints)
            {
                openPoints.Remove(point);
            }
            
            // Remove room from map if all points processed
            if (openPoints.Count == 0)
            {
                openConnectionsMap.Remove(currentRoom);
            }
        }
        
        Debug.Log($"Finished closing open connections. Remaining open: {CountOpenConnections()}");
    }
    
    // Try to connect an open connection point to another room
    private bool TryConnectOpenPoint(RoomConnectionPoint point)
    {
        if (point == null || point.isOccupied)
            return true; // Already handled
            
        // Get the room this point belongs to
        GameObject sourceRoom = point.transform.parent.gameObject;
        Vector2Int sourcePos = Vector2Int.zero;
        
        // Find grid position of source room
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value == sourceRoom)
            {
                sourcePos = kvp.Key;
                break;
            }
        }
        
        // Calculate target position based on connection direction
        Direction direction = ConvertConnectionToDirection(point.direction);
        Vector2Int targetPos = GetPositionInDirection(sourcePos, direction);
        
        // If there's already a room at target position, try to connect to it
        if (roomGrid.TryGetValue(targetPos, out GameObject targetRoom))
        {
            // Try to connect existing rooms
            ConnectRoomDoors(sourceRoom, targetRoom, direction);
            
            // Check if connection was successful
            if (point.isOccupied)
            {
                Debug.Log($"Successfully connected open door at {direction} to existing room");
                return true;
            }
        }
        else
        {
            // Try to place a new room to connect to this open point
            bool success = TryPlaceRoomWithDoor(sourcePos, targetPos, direction);
            if (success)
            {
                Debug.Log($"Placed new room at {targetPos} to connect open door");
                return true;
            }
        }
        
        return false;
    }
    
    // Cap an open connection that couldn't be connected
    private void CapOpenConnection(RoomConnectionPoint point)
    {
        if (point == null || point.isOccupied)
            return;
            
        if (doorCapPrefab != null)
        {
            // Instantiate cap at the connection point
            GameObject cap = Instantiate(doorCapPrefab, point.transform.position, point.transform.rotation);
            cap.transform.parent = point.transform;
            cap.name = "Door Cap";
            
            // Mark connection as occupied
            point.isOccupied = true;
            Debug.Log($"Capped open connection at {point.direction}");
        }
        else
        {
            // No cap prefab available, just mark as occupied
            point.isOccupied = true;
            Debug.Log($"Marked open connection as occupied (no cap prefab)");
        }
    }
    
    // Helper method to count remaining open connections
    private int CountOpenConnections()
    {
        int count = 0;
        
        foreach (GameObject room in spawnedRooms)
        {
            if (room == null) continue;
            
            foreach (RoomConnectionPoint point in room.GetComponentsInChildren<RoomConnectionPoint>())
            {
                if (point != null && !point.isOccupied)
                {
                    count++;
                }
            }
        }
        
        return count;
    }
    
    // Convert ConnectionDirection to Direction
    private Direction ConvertConnectionToDirection(ConnectionDirection connectionDir)
    {
        switch (connectionDir)
        {
            case ConnectionDirection.North: return Direction.North;
            case ConnectionDirection.East: return Direction.East;
            case ConnectionDirection.South: return Direction.South;
            case ConnectionDirection.West: return Direction.West;
            default: return Direction.North;
        }
    }
    
    private void InitializeMinimap()
    {
        // Find or create dungeon minimap
        DungeonMinimap minimap = FindObjectOfType<DungeonMinimap>();
        if (minimap == null)
        {
            GameObject minimapObj = new GameObject("Dungeon Minimap");
            minimap = minimapObj.AddComponent<DungeonMinimap>();
        }
        
        // Initialize minimap with room data
        minimap.InitializeMinimap(roomGrid, startRoomPos);
    }
    
    // OnDrawGizmos metodunda kapı bağlantılarını görselleştir
    private void OnDrawGizmos()
    {
        // Sadece Editor modunda çalışır
        if (!Application.isPlaying)
            return;
            
        // Tüm odalar için bağlantı noktalarını göster
        foreach (var room in spawnedRooms)
        {
            if (room == null)
                continue;
                
            foreach (var point in room.GetComponentsInChildren<RoomConnectionPoint>())
            {
                if (point.connectedTo != null)
                {
                    // Bağlı kapıları yeşil çizgilerle göster
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(point.transform.position, point.connectedTo.transform.position);
                }
                else
                {
                    // Bağlı olmayan kapıları kırmızı kürelerle göster
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(point.transform.position, 0.3f);
                }
            }
        }
    }

    // Helper class for open list during generation
    private class RoomOpenEntry
    {
        public Vector2Int gridPos;
        public GameObject room;
    }
    
    // Find a room with the specified connection type
    private RoomData FindRoomWithConnectionType(RoomData.RoomConnectionType requestedType)
    {
        // Tam eşleşme önce deneyelim
        var matchingRooms = roomPool.Where(r => 
            r.connectionType == requestedType && 
            !r.isStartRoom && 
            !r.isEndRoom).ToArray();

        // Tam eşleşme bulunamazsa, bağlantıya uygun alternatif odalar arayalım
        if (matchingRooms.Length == 0)
        {
            // Alternatif uyumlu bağlantı tipleri bul
            var compatibleRooms = roomPool.Where(r =>
                IsCompatibleRoomType(r.connectionType, requestedType) &&
                !r.isStartRoom && 
                !r.isEndRoom).ToArray();
                
            if (compatibleRooms.Length > 0)
            {
                Debug.Log($"Found {compatibleRooms.Length} compatible rooms for {requestedType}");
                matchingRooms = compatibleRooms;
            }
            else
            {
                // Hala uygun oda bulunamadıysa, herhangi bir çok bağlantılı oda kullanabiliriz
                matchingRooms = roomPool.Where(r =>
                    r.connectionType == RoomData.RoomConnectionType.Fourway ||
                    r.connectionType.ToString().StartsWith("Triple") ||
                    r.connectionType.ToString().StartsWith("Double")).ToArray();
                    
                if (matchingRooms.Length == 0)
                {
                    Debug.LogWarning($"No room found with connection type {requestedType} or compatible alternatives");
                    
                    // Son çare olarak herhangi bir odayı döndür
                    if (roomPool.Length > 0)
                    {
                        Debug.LogWarning("Using any available room as fallback");
                        return roomPool[Random.Range(0, roomPool.Length)];
                    }
                    
                    return null;
                }
            }
        }
        
        return matchingRooms[Random.Range(0, matchingRooms.Length)];
    }
    
    // İki bağlantı tipinin uyumlu olup olmadığını kontrol et
    private bool IsCompatibleRoomType(RoomData.RoomConnectionType actualType, RoomData.RoomConnectionType requestedType)
    {
        // İstenen bağlantı tek yönlüyse (SingleUp, SingleDown, vb.)
        string requestedStr = requestedType.ToString();
        
        if (requestedStr.StartsWith("Single"))
        {
            // Tek yönlü bağlantı isteniyor
            string direction = requestedStr.Substring(6); // "Up", "Down", "Left", "Right"
            
            // actualType'ın bu yönde bağlantısı var mı kontrol et
            string actualStr = actualType.ToString();
            
            // Fourway her zaman uyumludur
            if (actualType == RoomData.RoomConnectionType.Fourway)
                return true;
                
            // Triple odalar için kontrol
            if (actualStr.StartsWith("Triple") && actualStr.Contains(direction))
                return true;
                
            // Double odalar için kontrol
            if (actualStr.StartsWith("Double") && actualStr.Contains(direction))
            return true;
        }

        return false;
    }
    
    // Get the required door type for a direction
    private RoomData.RoomConnectionType GetRequiredDoorType(Direction direction)
    {
        // Bu metodun kapsamını genişleteceğiz
        // Tek bir yön yerine, bağlantı türünü kontrol etmemiz gerekiyor
        switch (direction)
        {
            case Direction.North:
                return RoomData.RoomConnectionType.SingleUp;
            case Direction.East:
                return RoomData.RoomConnectionType.SingleRight;
            case Direction.South:
                return RoomData.RoomConnectionType.SingleDown;
            case Direction.West:
                return RoomData.RoomConnectionType.SingleLeft;
            default:
                return RoomData.RoomConnectionType.SingleUp;
        }
    }
    
    // Get the opposite direction
    private Direction GetOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return Direction.South;
            case Direction.East:
                return Direction.West;
            case Direction.South:
                return Direction.North;
            case Direction.West:
                return Direction.East;
            default:
                return Direction.South;
        }
    }
    
    // Get the position in a specific direction
    private Vector2Int GetPositionInDirection(Vector2Int pos, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector2Int(pos.x, pos.y + 1);
            case Direction.East:
                return new Vector2Int(pos.x + 1, pos.y);
            case Direction.South:
                return new Vector2Int(pos.x, pos.y - 1);
            case Direction.West:
                return new Vector2Int(pos.x - 1, pos.y);
            default:
                return pos;
        }
    }
    
    // Shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    RoomData.RoomConnectionType DetermineRoomType(bool up, bool down, bool left, bool right)
    {
        int connections = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        switch (connections)
        {
            case 1:
                if (up) return RoomData.RoomConnectionType.SingleUp;
                if (down) return RoomData.RoomConnectionType.SingleDown;
                if (left) return RoomData.RoomConnectionType.SingleLeft;
                return RoomData.RoomConnectionType.SingleRight;

            case 2:
                if (up && down) return RoomData.RoomConnectionType.DoubleUpDown;
                if (left && right) return RoomData.RoomConnectionType.DoubleLeftRight;
                if (up && left) return RoomData.RoomConnectionType.DoubleUpLeft;
                if (up && right) return RoomData.RoomConnectionType.DoubleUpRight;
                if (down && left) return RoomData.RoomConnectionType.DoubleDownLeft;
                return RoomData.RoomConnectionType.DoubleDownRight;

            case 3:
                if (up && right && down) return RoomData.RoomConnectionType.TripleUpRightDown;
                if (up && left && down) return RoomData.RoomConnectionType.TripleUpLeftDown;
                if (down && left && right) return RoomData.RoomConnectionType.TripleDownRightLeft;
                return RoomData.RoomConnectionType.TripleUpLeftRight;

            case 4:
                return RoomData.RoomConnectionType.Fourway;

            default:
                Debug.LogError("Geçersiz bağlantı sayısı!");
                return RoomData.RoomConnectionType.SingleUp;
        }
    }

    void MoveGeneratorPoint()
    {
        int direction = Random.Range(0, 4);
        switch (direction)
        {
            case 0: // Up
                generatorPoint.position += new Vector3(0, yOffset, 0);
                break;
            case 1: // Right
                generatorPoint.position += new Vector3(xOffset, 0, 0);
                break;
            case 2: // Down
                generatorPoint.position += new Vector3(0, -yOffset, 0);
                break;
            case 3: // Left
                generatorPoint.position += new Vector3(-xOffset, 0, 0);
                break;
        }
    }

    void ChangeGeneratorDirection()
    {
        MoveGeneratorPoint();
        while (Physics2D.OverlapCircle(generatorPoint.position, roomCheckRadius, whatIsRoom))
        {
            MoveGeneratorPoint();
        }
    }

    GameObject SpawnRoom(RoomData roomData, Vector3 position)
    {
        if (roomData == null)
        {
            Debug.LogError("Cannot spawn room: roomData is null");
            return null;
        }
        
        if (roomData.roomPrefab == null)
        {
            Debug.LogError($"Room {roomData.roomID} has no prefab assigned!");
            return null;
        }
        
        // Instantiate room
        GameObject roomObj = Instantiate(roomData.roomPrefab, position, Quaternion.identity);
        
        if (roomObj == null)
        {
            Debug.LogError($"Failed to instantiate room prefab for {roomData.roomID}");
            return null;
        }
        
        // Set name
        roomObj.name = $"Room_{roomData.roomID}";
        
        // Initialize room with room data
        var initializer = roomObj.GetComponent<RoomInitializer>();
        if (initializer != null)
        {
            initializer.InitializeRoom(roomData);
        }
        else
        {
            Debug.LogWarning($"Room {roomData.roomID} prefab has no RoomInitializer component");
            
            // Oda rengini ayarla
            var spriteRenderer = roomObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = roomData.roomColor;
            }
        }
        
        return roomObj;
    }

    // This method checks if a room at the given position would be a critical path connector
    // Returns true if the position is surrounded by 2+ rooms that would be disconnected without it
    private bool IsPathBlockingRoom(Vector2Int pos)
    {
        // Check for rooms in all four cardinal directions
        List<Vector2Int> adjacentPositions = new List<Vector2Int>
        {
            GetPositionInDirection(pos, Direction.North),
            GetPositionInDirection(pos, Direction.East),
            GetPositionInDirection(pos, Direction.South),
            GetPositionInDirection(pos, Direction.West)
        };
        
        // Count occupied adjacent positions
        int occupiedCount = 0;
        foreach (var adjPos in adjacentPositions)
        {
            if (roomGrid.ContainsKey(adjPos))
            {
                occupiedCount++;
            }
        }
        
        // If there are 2 or more adjacent rooms, this is a potential path blocker
        if (occupiedCount >= 2)
        {
            // Check if removing this position would disconnect the graph
            // We do this by temporarily removing it and checking connectivity
            
            // Get all adjacent occupied positions
            List<Vector2Int> occupiedAdjacentPositions = adjacentPositions.Where(p => roomGrid.ContainsKey(p)).ToList();
            
            // If less than 2 occupied, it's not a blocker
            if (occupiedAdjacentPositions.Count < 2) return false;
            
            // Check if all occupied adjacent positions can reach each other without going through this position
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            
            // Start BFS from first adjacent position
            queue.Enqueue(occupiedAdjacentPositions[0]);
            visited.Add(occupiedAdjacentPositions[0]);
            
            // BFS to find all connected rooms without going through pos
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                // Check all four directions from current
                foreach (Direction dir in new[] { Direction.North, Direction.East, Direction.South, Direction.West })
                {
                    Vector2Int next = GetPositionInDirection(current, dir);
                    
                    // Skip if next is the position we're checking or if already visited
                    if (next == pos || visited.Contains(next)) continue;
                    
                    // If there's a room, add to visited and queue
                    if (roomGrid.ContainsKey(next) && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
            
            // If not all adjacent positions were visited, this position is a path blocker
            foreach (var adjPos in occupiedAdjacentPositions)
            {
                if (!visited.Contains(adjPos))
                {
                    Debug.Log($"Position {pos} is a critical path connector between rooms");
                    return true;
                }
            }
        }
        
        return false;
    }

    // New method to get direction from one position to another
    private Direction GetDirectionToPosition(Vector2Int from, Vector2Int to)
    {
        if (to.x > from.x) return Direction.East;
        if (to.x < from.x) return Direction.West;
        if (to.y > from.y) return Direction.North;
        if (to.y < from.y) return Direction.South;
        return Direction.North; // Default fallback
    }

    // New method to ensure all required path positions have rooms
    private void EnsurePathConnectivity()
    {
        bool madeChanges = false;
        
        // First pass: perform BFS from start room to identify all connected components
        Dictionary<Vector2Int, int> componentMap = new Dictionary<Vector2Int, int>();
        int componentCount = 0;
        
        foreach (var roomPos in roomGrid.Keys)
        {
            if (!componentMap.ContainsKey(roomPos))
            {
                // New component found, assign an ID
                componentCount++;
                
                // Perform BFS to find all rooms in this component
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                queue.Enqueue(roomPos);
                componentMap[roomPos] = componentCount;
                
                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    
                    // Check all four directions
                    foreach (Direction dir in new[] { Direction.North, Direction.East, Direction.South, Direction.West })
                    {
                        Vector2Int neighbor = GetPositionInDirection(current, dir);
                        
                        // If neighbor exists and hasn't been assigned a component
                        if (roomGrid.ContainsKey(neighbor) && !componentMap.ContainsKey(neighbor))
                        {
                            componentMap[neighbor] = componentCount;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
        
        // If there's more than one component, try to connect them
        if (componentCount > 1)
        {
            Debug.Log($"Found {componentCount} disconnected components in dungeon, attempting to connect them");
            
            // Find potential connection points between components
            for (int i = 1; i <= componentCount; i++)
            {
                for (int j = i + 1; j <= componentCount; j++)
                {
                    // Find rooms in component i
                    var componentI = componentMap.Where(kvp => kvp.Value == i).Select(kvp => kvp.Key).ToList();
                    
                    // Find rooms in component j
                    var componentJ = componentMap.Where(kvp => kvp.Value == j).Select(kvp => kvp.Key).ToList();
                    
                    // Find closest pair between components
                    Vector2Int? bestI = null;
                    Vector2Int? bestJ = null;
                    float minDistance = float.MaxValue;
                    
                    foreach (var posI in componentI)
                    {
                        foreach (var posJ in componentJ)
                        {
                            float distance = Vector2.Distance(posI, posJ);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                bestI = posI;
                                bestJ = posJ;
                            }
                        }
                    }
                    
                    if (bestI.HasValue && bestJ.HasValue)
                    {
                        // Find shortest path between these positions
                        if (TryConnectComponents(bestI.Value, bestJ.Value))
                        {
                            madeChanges = true;
                            Debug.Log($"Connected components {i} and {j}");
                        }
                    }
                }
            }
        }
        
        // If we made changes, verify the dungeon again
        if (madeChanges)
        {
            // We don't need to call VerifyDungeonLayout here as it will be called after
            // this method in the generation flow
            Debug.Log("Made changes to ensure path connectivity");
        }
    }

    // New method to try connecting two components by placing rooms between them
    private bool TryConnectComponents(Vector2Int posA, Vector2Int posB)
    {
        // Determine direction to move from A to B
        Vector2Int delta = new Vector2Int(
            Mathf.Clamp(posB.x - posA.x, -1, 1),
            Mathf.Clamp(posB.y - posA.y, -1, 1)
        );
        
        // Start at position A
        Vector2Int current = posA;
        Vector2Int next;
        
        // Keep moving one step at a time towards B
        while (current != posB)
        {
            // Calculate next position (prioritize moving along x, then y)
            if (delta.x != 0)
            {
                next = new Vector2Int(current.x + delta.x, current.y);
                delta.x = Mathf.Clamp(posB.x - next.x, -1, 1);
            }
            else if (delta.y != 0)
            {
                next = new Vector2Int(current.x, current.y + delta.y);
                delta.y = Mathf.Clamp(posB.y - next.y, -1, 1);
            }
            else
            {
                // We've reached B
                break;
            }
            
            // If the next position doesn't have a room and it's not B, place one
            if (!roomGrid.ContainsKey(next) && next != posB)
            {
                Direction dir = GetDirectionToPosition(current, next);
                
                // Try to place a room from current to next
                bool success = TryPlaceRoomWithDoor(current, next, dir);
                
                if (!success)
                {
                    // Try the reverse direction
                    dir = GetOppositeDirection(dir);
                    success = TryPlaceRoomWithDoor(next, current, dir);
                }
                
                if (!success)
                {
                    Debug.LogWarning($"Failed to place connecting room at {next}");
                    return false;
                }
                else
                {
                    Debug.Log($"Placed connecting room at {next}");
                }
            }
            
            current = next;
        }
        
        return true;
    }
}
