using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ImprovedDungeonGenerator : MonoBehaviour
{
    // Singleton instance
    public static ImprovedDungeonGenerator instance;
    
    [Header("Room Settings")]
    public RoomData[] roomPool;
    public int roomCount = 10;
    public float xOffset = 18f;
    public float yOffset = 10f;
    public LayerMask whatIsRoom;
    
    [Header("Generation Settings")]
    public int maxAttempts = 50;
    public float roomCheckRadius = 0.2f;
    
    [Header("Connection Closure")]
    public GameObject doorCapPrefab; // Kapanmamış kapılar için kullanılacak prefab
    public bool allowBossDeadEnd = true; // Boss odası tek girişli çıkmaz sokak olabilir
    
    [Header("Debug Settings")]
    [Tooltip("Show detailed logs")]
    public bool detailedLogging = false;
    
    // Changed from private to public to allow access from GridConnectionValidator
    public List<GameObject> spawnedRooms = new List<GameObject>();
    private Dictionary<Vector2Int, GameObject> roomGrid = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int startRoomPos = Vector2Int.zero;
    private GameObject startRoom;
    private GameObject endRoom;
    
    // Spawn sayılarını takip etmek için sayaçlar
    private int startRoomSpawnCount = 0;
    private int endRoomSpawnCount = 0;
    private int invalidRoomConnectionsDetected = 0;
    
    // Direction enum for room connections
    public enum Direction
    {
        North, East, South, West
    }
    
    // Room placement tracking
    private class RoomPlacement
    {
        public Vector2Int position;
        public GameObject roomObject;
        public RoomData roomData;
        public List<Direction> availableDirections = new List<Direction>();
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Multiple ImprovedDungeonGenerator instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Sayaçları sıfırla
        startRoomSpawnCount = 0;
        endRoomSpawnCount = 0;
        invalidRoomConnectionsDetected = 0;
    }
    
    private void Start()
    {
        GenerateDungeon(roomCount);
    }
    
    public void GenerateDungeon(int numberOfRooms)
    {
        Debug.Log($"Starting dungeon generation with {numberOfRooms} rooms");
        
        // Clear any existing rooms
        ClearExistingRooms();
        
        // Track generation success
        bool generationSuccessful = false;
        int generationAttempts = 0;
        
        // Sayaçları sıfırla
        startRoomSpawnCount = 0;
        endRoomSpawnCount = 0;
        invalidRoomConnectionsDetected = 0;
        
        // Try to generate dungeon multiple times if needed
        while (!generationSuccessful && generationAttempts < maxAttempts)
        {
            generationAttempts++;
            Debug.Log($"Dungeon generation attempt {generationAttempts}");
            
            // Clear previous attempt
            ClearExistingRooms();
            
            // Try to generate a valid dungeon
            generationSuccessful = GenerateDungeonWithBacktracking(numberOfRooms);
            
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
            Debug.Log($"DEBUG STATS: Start Rooms: {startRoomSpawnCount}, End Rooms: {endRoomSpawnCount}, Invalid Connections: {invalidRoomConnectionsDetected}");
            
            // Açık bağlantıları kapat
            CloseOpenConnections();
            
            // Oda bağlantılarını kontrol et ve hatalı olanları düzelt
            ValidateRoomConnections();
            
            VisualizeConnections();
        }
    }
    
    private void ClearExistingRooms()
    {
        foreach (var room in spawnedRooms)
        {
            if (room != null)
            {
                Destroy(room);
            }
        }
        spawnedRooms.Clear();
        roomGrid.Clear();
        
        // Sayaçları sıfırla
        startRoomSpawnCount = 0;
        endRoomSpawnCount = 0;
    }
    
    // Odanın belirtilen konumda çakışma olup olmadığını kontrol eden metod
    private bool IsRoomColliding(Vector3 position)
    {
        // Debug ekle
        if (detailedLogging) Debug.Log($"<color=blue>COLLISION CHECK:</color> Checking for collisions at position {position}");
        
        // Önce grid pozisyonunda kayıtlı bir oda var mı kontrol et
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(position.x / xOffset), Mathf.RoundToInt(position.y / yOffset));
        if (roomGrid.ContainsKey(gridPos))
        {
            if (detailedLogging) Debug.LogWarning($"<color=red>GRID COLLISION:</color> Room already exists at grid position {gridPos} - {(roomGrid[gridPos] != null ? roomGrid[gridPos].name : "NULL")}");
            
            // Grid ama oda null kontrolü
            if (roomGrid[gridPos] == null)
            {
                Debug.LogError($"<color=red>INCONSISTENCY:</color> Grid position {gridPos} is registered but has NULL reference!");
            }
            
            return true;
        }
        else
        {
            if (detailedLogging) Debug.Log($"<color=green>GRID CHECK OK:</color> No room in grid at position {gridPos}");
        }
        
        // Fiziksel çakışma kontrolü
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, roomCheckRadius, whatIsRoom);
        if (colliders != null && colliders.Length > 0)
        {
            if (detailedLogging) Debug.LogWarning($"<color=orange>PHYSICS CHECK:</color> Found {colliders.Length} potential collisions at {position}");
            
            foreach (var collider in colliders)
            {
                // Kendi collider'ı değilse çakışma var demektir
                if (collider.gameObject != gameObject)
                {
                    if (detailedLogging) Debug.LogWarning($"<color=red>PHYSICAL COLLISION:</color> Room collision detected at {position} with {collider.gameObject.name}");
                    
                    // Grid ve fiziksel oda tutarsızlığını tespit et
                    Vector2Int colliderGridPos = new Vector2Int(
                        Mathf.RoundToInt(collider.transform.position.x / xOffset),
                        Mathf.RoundToInt(collider.transform.position.y / yOffset));
                    
                    if (!roomGrid.ContainsKey(colliderGridPos))
                    {
                        Debug.LogError($"<color=red>GRID INCONSISTENCY:</color> Physical room at {colliderGridPos} but no entry in grid dictionary!");
                    }
                    else if (roomGrid[colliderGridPos] != collider.gameObject && roomGrid[colliderGridPos] != collider.transform.parent.gameObject)
                    {
                        Debug.LogError($"<color=red>REFERENCE MISMATCH:</color> Grid has different object than detected collider at {colliderGridPos}!");
                        Debug.LogError($"<color=red>GRID OBJECT:</color> {roomGrid[colliderGridPos]?.name ?? "NULL"}");
                        Debug.LogError($"<color=red>COLLIDER OBJECT:</color> {collider.gameObject.name} (parent: {collider.transform.parent?.name ?? "NULL"})");
                    }
                    
                    return true;
                }
            }
        }
        else
        {
            if (detailedLogging) Debug.Log($"<color=green>PHYSICS CHECK OK:</color> No collisions detected at {position}");
        }
        
        // Çakışma yok
        if (detailedLogging) Debug.Log($"<color=green>ALL CHECKS PASSED:</color> Position {position} is clear for room placement");
        return false;
    }
    
    private bool GenerateDungeonWithBacktracking(int numberOfRooms)
    {
        // Start with finding a start room
        RoomData startRoomData = roomPool.FirstOrDefault(r => r.isStartRoom);
        if (startRoomData == null)
        {
            // If no specific start room, use a room with single exit
            startRoomData = FindRoomWithConnectionType(RoomData.RoomConnectionType.SingleUp);
            if (startRoomData == null)
            {
                Debug.LogError("No suitable start room found!");
                return false;
            }
        }
        
        // Place start room at origin
        Vector3 startRoomPos3D = Vector3.zero;
        
        // Başlangıç odası için çakışma kontrolü
        if (IsRoomColliding(startRoomPos3D))
        {
            Debug.LogError("Start room position is already occupied!");
            return false;
        }
        
        startRoom = SpawnRoom(startRoomData, startRoomPos3D);
        if (startRoom == null)
        {
            Debug.LogError("Failed to spawn start room!");
            return false;
        }
        
        startRoomSpawnCount++; // Start room sayacını artır
        
        spawnedRooms.Add(startRoom);
        roomGrid.Add(startRoomPos, startRoom);
        
        Debug.Log($"Start room placed at {startRoomPos}");
        
        // Create a stack for backtracking
        Stack<RoomPlacement> placementStack = new Stack<RoomPlacement>();
        
        // Add start room to stack
        RoomPlacement startPlacement = new RoomPlacement
        {
            position = startRoomPos,
            roomObject = startRoom,
            roomData = startRoomData,
            availableDirections = GetAvailableDirections(startRoomData.connectionType)
        };
        
        placementStack.Push(startPlacement);
        
        // Track number of rooms placed
        int roomsPlaced = 1;
        
        // Önceden end odası yerleştirildi mi kontrolü
        bool endRoomPlaced = false;
        
        // Continue until we've placed all rooms or backtracked completely
        while (placementStack.Count > 0 && roomsPlaced < numberOfRooms)
        {
            // Get the current room from the stack
            RoomPlacement current = placementStack.Peek();
            
            // If no more available directions from this room, backtrack
            if (current.availableDirections.Count == 0)
            {
                placementStack.Pop();
                continue;
            }
            
            // Get a random direction from available directions
            int dirIndex = Random.Range(0, current.availableDirections.Count);
            Direction direction = current.availableDirections[dirIndex];
            
            // Remove this direction so we don't try it again
            current.availableDirections.RemoveAt(dirIndex);
            
            // Calculate new position
            Vector2Int newPos = GetPositionInDirection(current.position, direction);
            
            // Skip if position already occupied
            if (roomGrid.ContainsKey(newPos))
            {
                continue;
            }
            
            // Calculate world position
            Vector3 worldPos = new Vector3(newPos.x * xOffset, newPos.y * yOffset, 0);
            
            // Check for collisions using our improved method
            if (IsRoomColliding(worldPos))
            {
                Debug.LogWarning($"Room collision detected at {worldPos}. Cannot place room.");
                continue;
            }
            
            // Determine required door types
            RoomData.RoomConnectionType requiredConnectionType = GetRequiredConnectionType(direction, true);
            
            // Eğer kaynak oda tek girişli ise, sadece bu girişten çıkış yapılabilir
            if (IsSingleConnectionRoom(current.roomData.connectionType))
            {
                // Bağlantı yönünü kontrol et
                if (!IsValidConnectionDirectionForSingleRoom(current.roomData.connectionType, direction))
                {
                    if (detailedLogging) Debug.LogWarning($"Invalid connection from single connection room {current.roomObject.name} to direction {direction}");
                    invalidRoomConnectionsDetected++;
                    continue; // Bu yöne bağlantı yapılamaz
                }
            }
            
            // For non-end rooms, ensure they have at least one exit
            bool isLastRoom = (roomsPlaced == numberOfRooms - 1);
            RoomData targetRoomData;
            
            if (isLastRoom && !endRoomPlaced)
            {
                // For the last room, prefer an end room or a room with only one connection
                targetRoomData = roomPool.FirstOrDefault(r => r.isEndRoom);
                if (targetRoomData == null)
                {
                    // If no end room found, use a room with only one connection
                    targetRoomData = FindRoomWithSingleConnection(GetOppositeDirection(direction));
                }
                
                // Son oda yerleştirildi olarak işaretle
                endRoomPlaced = true;
            }
            else
            {
                // For non-end rooms, find a room with the required connection type that has at least one exit
                // Son oda zaten yerleştirilmişse, son oda olarak işaretlenen odaları kullanma
                targetRoomData = FindRoomWithConnectionAndExit(requiredConnectionType, endRoomPlaced);
            }
            
            if (targetRoomData == null)
            {
                Debug.LogWarning($"No suitable room found for direction {direction}");
                continue;
            }
            
            // Spawn the room
            GameObject targetRoomObj = SpawnRoom(targetRoomData, worldPos);
            if (targetRoomObj == null)
            {
                Debug.LogError("Failed to spawn target room!");
                continue;
            }
            
            // End room sayacını artır
            if (targetRoomData.isEndRoom)
            {
                endRoomSpawnCount++;
            }
            
            // Bağlantı uyumluluğunu kontrol et
            bool connectionValid = IsConnectionValid(current.roomData.connectionType, direction, targetRoomData.connectionType);
            if (!connectionValid)
            {
                Debug.LogWarning($"Invalid connection: {current.roomData.connectionType} in direction {direction} with {targetRoomData.connectionType}");
                invalidRoomConnectionsDetected++;
                Destroy(targetRoomObj); // Hatalı odayı yok et
                continue;
            }
            
            // Save the room
            spawnedRooms.Add(targetRoomObj);
            roomGrid[newPos] = targetRoomObj;
            roomsPlaced++;
            
            // Connect the rooms
            ConnectRooms(current.roomObject, targetRoomObj, direction);
            
            Debug.Log($"Room {roomsPlaced} placed at {newPos} with connection type {targetRoomData.connectionType}");
            
            // If this is the last room, mark it as the end room
            if (targetRoomData.isEndRoom)
            {
                endRoom = targetRoomObj;
                Debug.Log("End room placed");
            }
            else
            {
                // Add this room to the stack for further expansion
                RoomPlacement newPlacement = new RoomPlacement
                {
                    position = newPos,
                    roomObject = targetRoomObj,
                    roomData = targetRoomData,
                    availableDirections = GetAvailableDirections(targetRoomData.connectionType)
                };
                
                placementStack.Push(newPlacement);
            }
        }
        
        // Check if we placed all rooms
        if (roomsPlaced < numberOfRooms)
        {
            Debug.LogWarning($"Could only place {roomsPlaced} out of {numberOfRooms} rooms");
            return false;
        }
        
        return true;
    }
    
    private List<Direction> GetAvailableDirections(RoomData.RoomConnectionType connectionType)
    {
        List<Direction> directions = new List<Direction>();
        
        // Check which directions are available based on the connection type
        switch (connectionType)
        {
            case RoomData.RoomConnectionType.SingleUp:
                directions.Add(Direction.North);
                break;
            case RoomData.RoomConnectionType.SingleDown:
                directions.Add(Direction.South);
                break;
            case RoomData.RoomConnectionType.SingleRight:
                directions.Add(Direction.East);
                break;
            case RoomData.RoomConnectionType.SingleLeft:
                directions.Add(Direction.West);
                break;
            case RoomData.RoomConnectionType.DoubleUpDown:
                directions.Add(Direction.North);
                directions.Add(Direction.South);
                break;
            case RoomData.RoomConnectionType.DoubleLeftRight:
                directions.Add(Direction.East);
                directions.Add(Direction.West);
                break;
            case RoomData.RoomConnectionType.DoubleUpLeft:
                directions.Add(Direction.North);
                directions.Add(Direction.West);
                break;
            case RoomData.RoomConnectionType.DoubleUpRight:
                directions.Add(Direction.North);
                directions.Add(Direction.East);
                break;
            case RoomData.RoomConnectionType.DoubleDownLeft:
                directions.Add(Direction.South);
                directions.Add(Direction.West);
                break;
            case RoomData.RoomConnectionType.DoubleDownRight:
                directions.Add(Direction.South);
                directions.Add(Direction.East);
                break;
            case RoomData.RoomConnectionType.TripleUpRightDown:
                directions.Add(Direction.North);
                directions.Add(Direction.East);
                directions.Add(Direction.South);
                break;
            case RoomData.RoomConnectionType.TripleUpLeftDown:
                directions.Add(Direction.North);
                directions.Add(Direction.West);
                directions.Add(Direction.South);
                break;
            case RoomData.RoomConnectionType.TripleDownRightLeft:
                directions.Add(Direction.South);
                directions.Add(Direction.East);
                directions.Add(Direction.West);
                break;
            case RoomData.RoomConnectionType.TripleUpLeftRight:
                directions.Add(Direction.North);
                directions.Add(Direction.West);
                directions.Add(Direction.East);
                break;
            case RoomData.RoomConnectionType.Fourway:
                directions.Add(Direction.North);
                directions.Add(Direction.East);
                directions.Add(Direction.South);
                directions.Add(Direction.West);
                break;
        }
        
        // Shuffle the directions for randomness
        ShuffleList(directions);
        return directions;
    }
    
    private RoomData FindRoomWithConnectionType(RoomData.RoomConnectionType connectionType)
    {
        var matchingRooms = roomPool.Where(r => r.connectionType == connectionType).ToList();
        if (matchingRooms.Count == 0)
            return null;
            
        return matchingRooms[Random.Range(0, matchingRooms.Count)];
    }
    
    private RoomData FindRoomWithSingleConnection(Direction requiredDirection)
    {
        RoomData.RoomConnectionType requiredType = DirectionToSingleConnectionType(requiredDirection);
        return FindRoomWithConnectionType(requiredType);
    }
    
    private RoomData FindRoomWithConnectionAndExit(RoomData.RoomConnectionType requiredType, bool endRoomPlaced)
    {
        // Find rooms that have the required connection type and at least one exit
        var matchingRooms = roomPool.Where(r => 
            HasRequiredConnectionType(r.connectionType, requiredType) && 
            GetAvailableDirections(r.connectionType).Count > 1 &&
            (!endRoomPlaced || !r.isEndRoom)).ToList(); // Son oda zaten yerleştirilmişse, end room'ları hariç tut
            
        if (matchingRooms.Count == 0)
        {
            Debug.LogWarning($"No rooms found with connection type {requiredType} and at least one exit. Trying any compatible room.");
            
            // Eğer uygun oda bulunamazsa, sadece bağlantı tipine göre ara
            matchingRooms = roomPool.Where(r => 
                HasRequiredConnectionType(r.connectionType, requiredType) && 
                (!endRoomPlaced || !r.isEndRoom)).ToList();
                
            if (matchingRooms.Count == 0)
                return null;
        }
            
        return matchingRooms[Random.Range(0, matchingRooms.Count)];
    }
    
    private bool HasRequiredConnectionType(RoomData.RoomConnectionType actualType, RoomData.RoomConnectionType requiredType)
    {
        // Check if the actual connection type includes the required connection
        switch (requiredType)
        {
            case RoomData.RoomConnectionType.SingleUp:
                return HasNorthConnection(actualType);
            case RoomData.RoomConnectionType.SingleDown:
                return HasSouthConnection(actualType);
            case RoomData.RoomConnectionType.SingleRight:
                return HasEastConnection(actualType);
            case RoomData.RoomConnectionType.SingleLeft:
                return HasWestConnection(actualType);
            default:
                return actualType == requiredType;
        }
    }
    
    private bool HasNorthConnection(RoomData.RoomConnectionType type)
    {
        return type == RoomData.RoomConnectionType.SingleUp ||
               type == RoomData.RoomConnectionType.DoubleUpDown ||
               type == RoomData.RoomConnectionType.DoubleUpLeft ||
               type == RoomData.RoomConnectionType.DoubleUpRight ||
               type == RoomData.RoomConnectionType.TripleUpRightDown ||
               type == RoomData.RoomConnectionType.TripleUpLeftDown ||
               type == RoomData.RoomConnectionType.TripleUpLeftRight ||
               type == RoomData.RoomConnectionType.Fourway;
    }
    
    private bool HasSouthConnection(RoomData.RoomConnectionType type)
    {
        return type == RoomData.RoomConnectionType.SingleDown ||
               type == RoomData.RoomConnectionType.DoubleUpDown ||
               type == RoomData.RoomConnectionType.DoubleDownLeft ||
               type == RoomData.RoomConnectionType.DoubleDownRight ||
               type == RoomData.RoomConnectionType.TripleUpRightDown ||
               type == RoomData.RoomConnectionType.TripleUpLeftDown ||
               type == RoomData.RoomConnectionType.TripleDownRightLeft ||
               type == RoomData.RoomConnectionType.Fourway;
    }
    
    private bool HasEastConnection(RoomData.RoomConnectionType type)
    {
        return type == RoomData.RoomConnectionType.SingleRight ||
               type == RoomData.RoomConnectionType.DoubleLeftRight ||
               type == RoomData.RoomConnectionType.DoubleUpRight ||
               type == RoomData.RoomConnectionType.DoubleDownRight ||
               type == RoomData.RoomConnectionType.TripleUpRightDown ||
               type == RoomData.RoomConnectionType.TripleDownRightLeft ||
               type == RoomData.RoomConnectionType.TripleUpLeftRight ||
               type == RoomData.RoomConnectionType.Fourway;
    }
    
    private bool HasWestConnection(RoomData.RoomConnectionType type)
    {
        return type == RoomData.RoomConnectionType.SingleLeft ||
               type == RoomData.RoomConnectionType.DoubleLeftRight ||
               type == RoomData.RoomConnectionType.DoubleUpLeft ||
               type == RoomData.RoomConnectionType.DoubleDownLeft ||
               type == RoomData.RoomConnectionType.TripleUpLeftDown ||
               type == RoomData.RoomConnectionType.TripleDownRightLeft ||
               type == RoomData.RoomConnectionType.TripleUpLeftRight ||
               type == RoomData.RoomConnectionType.Fourway;
    }
    
    private RoomData.RoomConnectionType DirectionToSingleConnectionType(Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return RoomData.RoomConnectionType.SingleUp;
            case Direction.East: return RoomData.RoomConnectionType.SingleRight;
            case Direction.South: return RoomData.RoomConnectionType.SingleDown;
            case Direction.West: return RoomData.RoomConnectionType.SingleLeft;
            default: return RoomData.RoomConnectionType.SingleUp;
        }
    }
    
    private RoomData.RoomConnectionType GetRequiredConnectionType(Direction direction, bool needsExit)
    {
        // This is a simplified version - in a real implementation, you'd consider the needed exits too
        switch (direction)
        {
            case Direction.North: return RoomData.RoomConnectionType.SingleDown; // Opposite direction for connection
            case Direction.East: return RoomData.RoomConnectionType.SingleLeft;
            case Direction.South: return RoomData.RoomConnectionType.SingleUp;
            case Direction.West: return RoomData.RoomConnectionType.SingleRight;
            default: return RoomData.RoomConnectionType.SingleUp;
        }
    }
    
    private Direction GetOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return Direction.South;
            case Direction.East: return Direction.West;
            case Direction.South: return Direction.North;
            case Direction.West: return Direction.East;
            default: return Direction.South;
        }
    }
    
    private Vector2Int GetPositionInDirection(Vector2Int pos, Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return new Vector2Int(pos.x, pos.y + 1);
            case Direction.East: return new Vector2Int(pos.x + 1, pos.y);
            case Direction.South: return new Vector2Int(pos.x, pos.y - 1);
            case Direction.West: return new Vector2Int(pos.x - 1, pos.y);
            default: return pos;
        }
    }
    
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
    
    // Spawn a room at the given position with the given room data
    // Changed from private to public to allow access from GridConnectionValidator
    public GameObject SpawnRoom(RoomData roomData, Vector3 position)
    {
        if (roomData == null || roomData.roomPrefab == null)
        {
            Debug.LogError("<color=red>SPAWN FAILURE:</color> Room data or prefab is null!");
            return null;
        }
        
        if (detailedLogging) Debug.Log($"<color=yellow>SPAWNING ROOM:</color> Attempting to spawn room {roomData.roomID} at position {position}");
        
        // Instantiate room
        GameObject roomObj = Instantiate(roomData.roomPrefab, position, Quaternion.identity);
        
        // Set name
        roomObj.name = $"Room_{roomData.roomID}";
        
        if (detailedLogging) Debug.Log($"<color=green>ROOM SPAWNED:</color> {roomObj.name} at {position}");
        
        // Grid pozisyonunu hesapla ve kontrol et
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(position.x / xOffset), Mathf.RoundToInt(position.y / yOffset));
        if (roomGrid.ContainsKey(gridPos))
        {
            Debug.LogError($"<color=red>GRID INCONSISTENCY:</color> Trying to spawn room at {gridPos} but grid already contains {(roomGrid[gridPos] != null ? roomGrid[gridPos].name : "NULL")}");
        }
        
        // Initialize room with room data
        var initializer = roomObj.GetComponent<RoomInitializer>();
        if (initializer != null)
        {
            initializer.InitializeRoom(roomData);
            if (detailedLogging) Debug.Log($"<color=cyan>ROOM INITIALIZED:</color> {roomObj.name} with room data {roomData.roomID}");
        }
        else
        {
            Debug.LogWarning($"<color=orange>MISSING COMPONENT:</color> Room {roomObj.name} has no RoomInitializer component! Adding one...");
            initializer = roomObj.AddComponent<RoomInitializer>();
            initializer.InitializeRoom(roomData);
        }
        
        // RoomInteractive bileşenini kontrol et, yoksa ekle
        var interactive = roomObj.GetComponent<RoomInteractive>();
        if (interactive == null)
        {
            Debug.LogWarning($"<color=orange>MISSING COMPONENT:</color> Room {roomObj.name} has no RoomInteractive component! Adding one...");
            interactive = roomObj.AddComponent<RoomInteractive>();
            
            // Oda tipine göre temel ConnectionPoint'ler ekle
            CreateConnectionPoints(roomObj, roomData.connectionType);
        }
        else
        {
            if (detailedLogging) Debug.Log($"<color=cyan>INTERACTIVE FOUND:</color> {roomObj.name} has {interactive.connectionPoints?.Count ?? 0} connection points");
        }
        
        // Collider'ları kontrol et
        Collider2D[] colliders = roomObj.GetComponentsInChildren<Collider2D>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning($"<color=orange>NO COLLIDERS:</color> Room {roomObj.name} has no colliders!");
        }
        else if (detailedLogging)
        {
            Debug.Log($"<color=cyan>COLLIDERS:</color> Room {roomObj.name} has {colliders.Length} colliders");
        }
        
        return roomObj;
    }
    
    // Oda için bağlantı noktaları oluştur
    private void CreateConnectionPoints(GameObject roomObj, RoomData.RoomConnectionType connectionType)
    {
        RoomInteractive interactive = roomObj.GetComponent<RoomInteractive>();
        if (interactive == null) return;
        
        // Bağlantı noktalarını temizle
        interactive.connectionPoints.Clear();
        
        // Odanın boyutunu al veya varsayılan değeri kullan
        Vector2 roomSize = new Vector2(16f, 8f);
        SpriteRenderer spriteRenderer = roomObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            roomSize = spriteRenderer.size;
        }
        
        // Tipe göre bağlantı noktalarını oluştur
        if (HasNorthConnection(connectionType))
        {
            CreateConnectionPoint(roomObj, interactive, ConnectionDirection.North, new Vector3(0f, roomSize.y / 2f, 0f));
        }
        
        if (HasSouthConnection(connectionType))
        {
            CreateConnectionPoint(roomObj, interactive, ConnectionDirection.South, new Vector3(0f, -roomSize.y / 2f, 0f));
        }
        
        if (HasEastConnection(connectionType))
        {
            CreateConnectionPoint(roomObj, interactive, ConnectionDirection.East, new Vector3(roomSize.x / 2f, 0f, 0f));
        }
        
        if (HasWestConnection(connectionType))
        {
            CreateConnectionPoint(roomObj, interactive, ConnectionDirection.West, new Vector3(-roomSize.x / 2f, 0f, 0f));
        }
        
        Debug.Log($"Created {interactive.connectionPoints.Count} connection points for room {roomObj.name}");
    }
    
    // ConnectionPoint oluştur
    private RoomConnectionPoint CreateConnectionPoint(GameObject roomObj, RoomInteractive interactive, ConnectionDirection direction, Vector3 localPosition)
    {
        // Bağlantı noktası için yeni GameObject oluştur
        GameObject pointObj = new GameObject($"ConnectionPoint_{direction}");
        pointObj.transform.parent = roomObj.transform;
        pointObj.transform.localPosition = localPosition;
        
        // Yerel rotasyonu ayarla
        switch (direction)
        {
            case ConnectionDirection.North:
                pointObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case ConnectionDirection.East:
                pointObj.transform.localRotation = Quaternion.Euler(0, 0, 270);
                break;
            case ConnectionDirection.South:
                pointObj.transform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case ConnectionDirection.West:
                pointObj.transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
        }
        
        // ConnectionPoint bileşeni ekle
        RoomConnectionPoint connectionPoint = pointObj.AddComponent<RoomConnectionPoint>();
        connectionPoint.direction = direction;
        
        // Door trigger ekle
        DoorTrigger doorTrigger = pointObj.AddComponent<DoorTrigger>();
        doorTrigger.connectionPoint = connectionPoint;
        
        // Spawn noktası ekle
        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.parent = pointObj.transform;
        spawnPoint.transform.localPosition = new Vector3(0f, -1.5f, 0f);
        doorTrigger.spawnPoint = spawnPoint.transform;
        
        // Trigger collider ekle
        BoxCollider2D collider = pointObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(3f, 2f);
        collider.offset = new Vector2(0f, 0.5f);
        
        // Bağlantı noktasını listeye ekle
        interactive.connectionPoints.Add(connectionPoint);
        
        return connectionPoint;
    }
    
    private void ConnectRooms(GameObject sourceRoom, GameObject targetRoom, Direction direction)
    {
        if (sourceRoom == null || targetRoom == null)
        {
            Debug.LogError("Cannot connect null rooms!");
            return;
        }
        
        // Get connection points from both rooms
        var sourceRoomInteractive = sourceRoom.GetComponent<RoomInteractive>();
        var targetRoomInteractive = targetRoom.GetComponent<RoomInteractive>();
        
        // Eksik componentleri ekle
        if (sourceRoomInteractive == null)
        {
            Debug.LogWarning($"Source room {sourceRoom.name} has no RoomInteractive component! Adding one...");
            sourceRoomInteractive = sourceRoom.AddComponent<RoomInteractive>();
            
            // RoomData'ya göre bağlantı noktaları ekle
            RoomData sourceRoomData = GetRoomData(sourceRoom);
            if (sourceRoomData != null)
            {
                CreateConnectionPoints(sourceRoom, sourceRoomData.connectionType);
            }
        }
        
        if (targetRoomInteractive == null)
        {
            Debug.LogWarning($"Target room {targetRoom.name} has no RoomInteractive component! Adding one...");
            targetRoomInteractive = targetRoom.AddComponent<RoomInteractive>();
            
            // RoomData'ya göre bağlantı noktaları ekle
            RoomData targetRoomData = GetRoomData(targetRoom);
            if (targetRoomData != null)
            {
                CreateConnectionPoints(targetRoom, targetRoomData.connectionType);
            }
        }
        
        // Bağlantı noktalarının hala eksik olup olmadığını kontrol et
        if (sourceRoomInteractive.connectionPoints == null || sourceRoomInteractive.connectionPoints.Count == 0 ||
            targetRoomInteractive.connectionPoints == null || targetRoomInteractive.connectionPoints.Count == 0)
        {
            Debug.LogError("Rooms have RoomInteractive but missing connection points!");
            return;
        }
        
        // Find connection points in the right direction
        ConnectionDirection sourceDirection = DirectionToConnectionDirection(direction);
        ConnectionDirection targetDirection = DirectionToConnectionDirection(GetOppositeDirection(direction));
        
        RoomConnectionPoint sourcePoint = null;
        RoomConnectionPoint targetPoint = null;
        
        // Find available connection points
        foreach (var point in sourceRoomInteractive.connectionPoints)
        {
            if (point != null && !point.isOccupied && point.direction == sourceDirection)
            {
                sourcePoint = point;
                break;
            }
        }
        
        foreach (var point in targetRoomInteractive.connectionPoints)
        {
            if (point != null && !point.isOccupied && point.direction == targetDirection)
            {
                targetPoint = point;
                break;
            }
        }
        
        if (sourcePoint != null && targetPoint != null)
        {
            // Connect the doors
            sourcePoint.Connect(targetPoint);
            
            Debug.Log($"Connected {direction} door of source room to {GetOppositeDirection(direction)} door of target room");
            
            // Set up door triggers if they exist
            DoorTrigger sourceDoorTrigger = sourcePoint.GetComponent<DoorTrigger>();
            DoorTrigger targetDoorTrigger = targetPoint.GetComponent<DoorTrigger>();
            
            if (sourceDoorTrigger != null)
                sourceDoorTrigger.connectionPoint = sourcePoint;
            
            if (targetDoorTrigger != null)
                targetDoorTrigger.connectionPoint = targetPoint;
        }
        else
        {
            Debug.LogWarning($"Could not find connection points for direction {direction}. Source: {sourcePoint != null}, Target: {targetPoint != null}");
        }
    }
    
    private ConnectionDirection DirectionToConnectionDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return ConnectionDirection.North;
            case Direction.East: return ConnectionDirection.East;
            case Direction.South: return ConnectionDirection.South;
            case Direction.West: return ConnectionDirection.West;
            default: return ConnectionDirection.North;
        }
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
            RoomInteractive roomInteractive = room.GetComponent<RoomInteractive>();
            if (roomInteractive == null) continue;
            
            List<RoomConnectionPoint> openPoints = new List<RoomConnectionPoint>();
            
            // Find unoccupied connection points
            foreach (RoomConnectionPoint point in roomInteractive.connectionPoints)
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
            RoomInteractive bossRoomInteractive = endRoom.GetComponent<RoomInteractive>();
            if (bossRoomInteractive != null)
            {
                // Count how many connections are already occupied (should be 1 for entrance)
                int occupiedConnections = 0;
                foreach (RoomConnectionPoint point in bossRoomInteractive.connectionPoints)
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
        bool foundSourcePos = false;
        
        // Find grid position of source room
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value == sourceRoom)
            {
                sourcePos = kvp.Key;
                foundSourcePos = true;
                break;
            }
        }
        
        // Eğer kaynak odanın grid pozisyonu bulunamazsa, işlemi durdur
        if (!foundSourcePos)
        {
            Debug.LogWarning($"Could not find grid position for source room {sourceRoom.name}");
            return false;
        }
        
        // Calculate target position based on connection direction
        Direction direction = ConvertConnectionToDirection(point.direction);
        Vector2Int targetPos = GetPositionInDirection(sourcePos, direction);
        
        // If there's already a room at target position, try to connect to it
        if (roomGrid.TryGetValue(targetPos, out GameObject targetRoom))
        {
            if (targetRoom == null)
            {
                Debug.LogWarning($"Target room at position {targetPos} is null! Removing from grid.");
                roomGrid.Remove(targetPos);
                return false;
            }
            
            // Check if target room has RoomInteractive component
            RoomInteractive targetInteractive = targetRoom.GetComponent<RoomInteractive>();
            if (targetInteractive == null)
            {
                Debug.LogWarning($"Target room {targetRoom.name} has no RoomInteractive component! Adding one...");
                targetInteractive = targetRoom.AddComponent<RoomInteractive>();
                
                // Get RoomData to create connection points
                RoomData targetRoomData = GetRoomData(targetRoom);
                if (targetRoomData != null)
                {
                    // Create connection points based on room data
                    CreateConnectionPoints(targetRoom, targetRoomData.connectionType);
                }
            }
            
            // Try to connect existing rooms
            ConnectRooms(sourceRoom, targetRoom, direction);
            
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
            // Find a suitable room type with matching connection
            RoomData.RoomConnectionType requiredType = GetRequiredConnectionType(direction, false);
            RoomData newRoomData = FindRoomWithConnectionType(requiredType);
            
            if (newRoomData != null)
            {
                // Calculate world position
                Vector3 worldPos = new Vector3(targetPos.x * xOffset, targetPos.y * yOffset, 0);
                
                // Check if position is clear
                if (!IsRoomColliding(worldPos))
                {
                    // Spawn the room
                    GameObject newRoom = SpawnRoom(newRoomData, worldPos);
                    if (newRoom != null)
                    {
                        // Add to tracking
                        spawnedRooms.Add(newRoom);
                        roomGrid[targetPos] = newRoom;
                        
                        // Connect the rooms
                        ConnectRooms(sourceRoom, newRoom, direction);
                        
                        // Check if connection was successful
                        if (point.isOccupied)
                        {
                            Debug.Log($"Placed new room at {targetPos} to connect open door");
                            return true;
                        }
                    }
                }
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
            
            RoomInteractive roomInteractive = room.GetComponent<RoomInteractive>();
            if (roomInteractive == null) continue;
            
            foreach (RoomConnectionPoint point in roomInteractive.connectionPoints)
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
    
    private void VisualizeConnections()
    {
        // This can be used for debugging to visualize connections between rooms
        foreach (var room in spawnedRooms)
        {
            if (room == null) continue;
            
            var roomInteractive = room.GetComponent<RoomInteractive>();
            if (roomInteractive == null) continue;
            
            foreach (var point in roomInteractive.connectionPoints)
            {
                if (point == null) continue;
                
                if (point.connectedTo != null)
                {
                    Debug.DrawLine(
                        point.transform.position, 
                        point.connectedTo.transform.position, 
                        Color.green, 
                        100f
                    );
                }
                else if (!point.isOccupied)
                {
                    // Draw unconnected points for debugging
                    Debug.DrawRay(
                        point.transform.position, 
                        DirectionToVector(point.direction) * 2f, 
                        Color.red, 
                        100f
                    );
                }
            }
        }
    }
    
    private Vector3 DirectionToVector(ConnectionDirection direction)
    {
        switch (direction)
        {
            case ConnectionDirection.North: return Vector3.up;
            case ConnectionDirection.East: return Vector3.right;
            case ConnectionDirection.South: return Vector3.down;
            case ConnectionDirection.West: return Vector3.left;
            default: return Vector3.zero;
        }
    }

    // Tek girişli bir oda için geçerli bağlantı yönünü kontrol et
    private bool IsValidConnectionDirectionForSingleRoom(RoomData.RoomConnectionType roomType, Direction direction)
    {
        switch (roomType)
        {
            case RoomData.RoomConnectionType.SingleUp:
                return direction == Direction.North;
            case RoomData.RoomConnectionType.SingleDown:
                return direction == Direction.South;
            case RoomData.RoomConnectionType.SingleRight:
                return direction == Direction.East;
            case RoomData.RoomConnectionType.SingleLeft:
                return direction == Direction.West;
            default:
                return true; // Tek girişli olmayan odalar için tüm yönler geçerli
        }
    }
    
    // Odanın sadece tek bağlantısı mı var
    private bool IsSingleConnectionRoom(RoomData.RoomConnectionType roomType)
    {
        return roomType == RoomData.RoomConnectionType.SingleUp || 
               roomType == RoomData.RoomConnectionType.SingleDown ||
               roomType == RoomData.RoomConnectionType.SingleRight ||
               roomType == RoomData.RoomConnectionType.SingleLeft;
    }
    
    // İki oda arasındaki bağlantı geçerli mi kontrol et
    private bool IsConnectionValid(RoomData.RoomConnectionType sourceType, Direction direction, RoomData.RoomConnectionType targetType)
    {
        // Kaynak odanın bu yönde bağlantısı olmalı
        bool sourceHasConnection = false;
        
        switch (direction)
        {
            case Direction.North:
                sourceHasConnection = HasNorthConnection(sourceType);
                break;
            case Direction.South:
                sourceHasConnection = HasSouthConnection(sourceType);
                break;
            case Direction.East:
                sourceHasConnection = HasEastConnection(sourceType);
                break;
            case Direction.West:
                sourceHasConnection = HasWestConnection(sourceType);
                break;
        }
        
        if (!sourceHasConnection)
        {
            return false;
        }
        
        // Hedef odanın ters yönde bağlantısı olmalı
        Direction oppositeDirection = GetOppositeDirection(direction);
        bool targetHasConnection = false;
        
        switch (oppositeDirection)
        {
            case Direction.North:
                targetHasConnection = HasNorthConnection(targetType);
                break;
            case Direction.South:
                targetHasConnection = HasSouthConnection(targetType);
                break;
            case Direction.East:
                targetHasConnection = HasEastConnection(targetType);
                break;
            case Direction.West:
                targetHasConnection = HasWestConnection(targetType);
                break;
        }
        
        return targetHasConnection;
    }
    
    // Oda bağlantılarını doğrula ve hatalı olanları düzelt
    private void ValidateRoomConnections()
    {
        Debug.Log("Validating room connections...");
        
        List<Vector2Int> invalidRoomPositions = new List<Vector2Int>();
        
        // Tüm odaları ve bağlantılarını kontrol et
        foreach (var kvp in roomGrid)
        {
            Vector2Int pos = kvp.Key;
            GameObject room = kvp.Value;
            
            if (room == null) continue;
            
            RoomInteractive roomInteractive = room.GetComponent<RoomInteractive>();
            if (roomInteractive == null) continue;
            
            RoomData roomData = GetRoomData(room);
            if (roomData == null) continue;
            
            // Odanın bağlantı noktalarını kontrol et
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                // Bu yönde bağlantı olup olmadığını kontrol et
                bool shouldHaveConnection = false;
                
                switch (dir)
                {
                    case Direction.North:
                        shouldHaveConnection = HasNorthConnection(roomData.connectionType);
                        break;
                    case Direction.South:
                        shouldHaveConnection = HasSouthConnection(roomData.connectionType);
                        break;
                    case Direction.East:
                        shouldHaveConnection = HasEastConnection(roomData.connectionType);
                        break;
                    case Direction.West:
                        shouldHaveConnection = HasWestConnection(roomData.connectionType);
                        break;
                }
                
                if (shouldHaveConnection)
                {
                    // Bu yönde bir bağlantı olmalı
                    Vector2Int neighborPos = GetPositionInDirection(pos, dir);
                    
                    // Komşu pozisyonda bir oda var mı?
                    if (roomGrid.TryGetValue(neighborPos, out GameObject neighborRoom))
                    {
                        // Komşu odanın bilgilerini al
                        RoomData neighborData = GetRoomData(neighborRoom);
                        if (neighborData == null) continue;
                        
                        // Bağlantı geçerli mi?
                        bool connectionValid = IsConnectionValid(roomData.connectionType, dir, neighborData.connectionType);
                        
                        if (!connectionValid)
                        {
                            Debug.LogWarning($"Invalid connection detected at {pos} in direction {dir}");
                            invalidRoomPositions.Add(neighborPos);
                        }
                    }
                }
            }
        }
        
        // Hatalı odaları kaldır
        foreach (Vector2Int invalidPos in invalidRoomPositions)
        {
            if (roomGrid.TryGetValue(invalidPos, out GameObject invalidRoom))
            {
                Debug.Log($"Removing invalid room at {invalidPos}");
                spawnedRooms.Remove(invalidRoom);
                roomGrid.Remove(invalidPos);
                Destroy(invalidRoom);
            }
        }
        
        Debug.Log($"Validation complete. Removed {invalidRoomPositions.Count} invalid rooms.");
    }
    
    // Bir GameObject'ten RoomData'yı almak için yardımcı metod
    private RoomData GetRoomData(GameObject roomObj)
    {
        var initializer = roomObj.GetComponent<RoomInitializer>();
        if (initializer != null)
        {
            // RoomInitializer sınıfında RoomData property'si kullanıyoruz
            return initializer.RoomData;
        }
        return null;
    }

    [SerializeField] private GridConnectionValidator gridValidator;

    private void ValidateAndFixConnections()
    {
        // Check if we have a validator
        if (gridValidator == null)
        {
            gridValidator = GetComponent<GridConnectionValidator>();
            if (gridValidator == null)
            {
                gridValidator = gameObject.AddComponent<GridConnectionValidator>();
            }
        }
        
        // Validate grid connections
        gridValidator.ValidateGridConnections(roomGrid);
    }

    private bool EnsureStartRoomPlacement()
    {
        // Clear any existing room at start position if needed
        if (roomGrid.ContainsKey(startRoomPos))
        {
            Destroy(roomGrid[startRoomPos]);
            roomGrid.Remove(startRoomPos);
        }
        
        // Find a start room
        RoomData startRoomData = roomPool.FirstOrDefault(r => r.isStartRoom);
        if (startRoomData == null)
        {
            // If no specific start room, use a room with single exit
            startRoomData = FindRoomWithConnectionType(RoomData.RoomConnectionType.SingleUp);
            if (startRoomData == null)
            {
                Debug.LogError("No suitable start room found!");
                return false;
            }
        }
        
        // Place start room at origin
        Vector3 startRoomPos3D = Vector3.zero;
        
        // Retry if there's a collision, but with more attempts
        int attempts = 0;
        while (IsRoomColliding(startRoomPos3D) && attempts < 10)
        {
            // Try slightly different positions
            startRoomPos3D += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
            attempts++;
        }
        
        if (attempts >= 10)
        {
            Debug.LogError("Failed to place start room after multiple attempts!");
            return false;
        }
        
        startRoom = SpawnRoom(startRoomData, startRoomPos3D);
        if (startRoom == null)
        {
            Debug.LogError("Failed to spawn start room!");
            return false;
        }
        
        startRoomSpawnCount++;
        spawnedRooms.Add(startRoom);
        roomGrid.Add(startRoomPos, startRoom);
        
        Debug.Log($"Start room placed at {startRoomPos}");
        return true;
    }

    private bool EnsureEndRoomPlacement()
    {
        // Find the furthest position from start for end room
        Vector2Int furthestPos = FindFurthestRoomPosition();
        
        // Find an end room
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
                // Replace in collections
                if (index >= 0)
                {
                    spawnedRooms[index] = endRoom;
                }
                else
                {
                    spawnedRooms.Add(endRoom);
                }
                
                roomGrid[furthestPos] = endRoom;
                endRoomSpawnCount++;
                Debug.Log($"End room replaced existing room at {furthestPos}");
                return true;
            }
        }
        else
        {
            // No room at furthest position, place end room there
            Vector3 worldPos = new Vector3(furthestPos.x * xOffset, furthestPos.y * yOffset, 0);
            endRoom = SpawnRoom(endRoomData, worldPos);
            
            if (endRoom != null)
            {
                spawnedRooms.Add(endRoom);
                roomGrid[furthestPos] = endRoom;
                endRoomSpawnCount++;
                Debug.Log($"End room placed at {furthestPos}");
                return true;
            }
        }
        
        Debug.LogError("Failed to place end room!");
        return false;
    }

    private Vector2Int FindFurthestRoomPosition()
    {
        Vector2Int furthestPos = startRoomPos;
        float maxDistance = 0f;
        
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

    private bool ShouldKeepConnectionOpen(Vector2Int sourcePos, Vector2Int targetPos, Direction direction)
    {
        // If the target position already has a room, the connection should be handled elsewhere
        if (roomGrid.ContainsKey(targetPos) && roomGrid[targetPos] != null)
            return false;
            
        // Check for other rooms that need a connection to this position
        foreach (var kvp in roomGrid)
        {
            if (kvp.Key == sourcePos || kvp.Value == null) continue;
            
            RoomConnectionPoint[] points = kvp.Value.GetComponentsInChildren<RoomConnectionPoint>();
            foreach (var point in points)
            {
                if (point.isOccupied) continue;
                
                Direction pointDirection = ConvertConnectionToDirection(point.direction);
                Vector2Int pointTargetPos = GetPositionInDirection(kvp.Key, pointDirection);
                
                // If another room also wants to connect to this position, keep it open
                if (pointTargetPos == targetPos)
                    return true;
            }
        }
        
        return false;
    }

    private Vector2Int GetRoomGridPosition(GameObject room)
    {
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value == room)
                return kvp.Key;
        }
        
        // If not found, estimate from world position
        Vector3 pos = room.transform.position;
        return new Vector2Int(
            Mathf.RoundToInt(pos.x / xOffset),
            Mathf.RoundToInt(pos.y / yOffset)
        );
    }

    /// <summary>
    /// Grid ve fiziksel odalar arasında tutarsızlık olup olmadığını kontrol eden debug metodu
    /// </summary>
    [ContextMenu("Debug - Check Grid Consistency")]
    public void DebugCheckGridConsistency()
    {
        Debug.Log("<color=yellow>********** STARTING GRID CONSISTENCY CHECK **********</color>");
        
        // Sahne üzerindeki tüm oda objelerini bul
        var allRoomObjects = FindObjectsByType<RoomInteractive>(FindObjectsSortMode.None).Select(ri => ri.gameObject).ToList();
        List<GameObject> missingInGrid = new List<GameObject>();
        List<Vector2Int> emptyGridEntries = new List<Vector2Int>();
        List<Vector2Int> mismatchedEntries = new List<Vector2Int>();
        
        Debug.Log($"<color=cyan>FOUND:</color> {allRoomObjects.Count} physical rooms in scene, {roomGrid.Count} entries in room grid");
        
        // 1. Grid'de var olan her şeyin doğru fiziksel karşılığı var mı?
        foreach (var kvp in roomGrid)
        {
            Vector2Int gridPos = kvp.Key;
            GameObject gridRoomObj = kvp.Value;
            Vector3 expectedWorldPos = new Vector3(gridPos.x * xOffset, gridPos.y * yOffset, 0);
            
            if (gridRoomObj == null)
            {
                Debug.LogError($"<color=red>NULL ENTRY:</color> Grid position {gridPos} has NULL room reference");
                emptyGridEntries.Add(gridPos);
                continue;
            }
            
            // Odanın fiziksel pozisyonu ile grid pozisyonu uyumlu mu?
            float positionError = Vector3.Distance(gridRoomObj.transform.position, expectedWorldPos);
            if (positionError > 0.1f) // Küçük bir tolerans payı
            {
                Debug.LogError($"<color=red>POSITION MISMATCH:</color> Grid position {gridPos} room is actually at {gridRoomObj.transform.position}, expected {expectedWorldPos}, error: {positionError}");
                mismatchedEntries.Add(gridPos);
            }
            
            // Bu grid pozisyonunda başka bir oda var mı?
            Collider2D[] colliders = Physics2D.OverlapCircleAll(expectedWorldPos, roomCheckRadius, whatIsRoom);
            foreach (var collider in colliders)
            {
                GameObject colObj = collider.gameObject;
                if (colObj != gridRoomObj && colObj != gameObject && colObj.transform.parent != gridRoomObj.transform)
                {
                    // Başka bir oda bu grid pozisyonunda
                    Debug.LogError($"<color=red>OVERLAP:</color> Grid position {gridPos} has {gridRoomObj.name} but also detected {colObj.name} at same position");
                }
            }
        }
        
        // 2. Sahnedeki her odanın grid'de karşılığı var mı?
        foreach (var roomObj in allRoomObjects)
        {
            Vector2Int gridPos = GetRoomGridPosition(roomObj);
            
            if (!roomGrid.ContainsKey(gridPos))
            {
                Debug.LogError($"<color=red>MISSING FROM GRID:</color> Room {roomObj.name} at position {roomObj.transform.position} (grid: {gridPos}) is not in the grid dictionary");
                missingInGrid.Add(roomObj);
            }
            else if (roomGrid[gridPos] != roomObj)
            {
                Debug.LogError($"<color=red>MISMATCH:</color> Room {roomObj.name} is at grid position {gridPos} but grid contains {roomGrid[gridPos]?.name ?? "NULL"}");
            }
        }
        
        // 3. Fiziksel çakışmalar var mı?
        for (int i = 0; i < allRoomObjects.Count; i++)
        {
            for (int j = i + 1; j < allRoomObjects.Count; j++)
            {
                float distance = Vector3.Distance(allRoomObjects[i].transform.position, allRoomObjects[j].transform.position);
                if (distance < 1f) // Birbirine çok yakın odalar
                {
                    Debug.LogError($"<color=red>ROOM OVERLAP:</color> Room {allRoomObjects[i].name} and {allRoomObjects[j].name} are too close: {distance} units apart");
                }
            }
        }
        
        // 4. Özet bilgi
        Debug.Log("<color=yellow>********** GRID CONSISTENCY CHECK COMPLETED **********</color>");
        Debug.Log($"<color=magenta>SUMMARY:</color> Found {missingInGrid.Count} rooms missing from grid, {emptyGridEntries.Count} empty grid entries, {mismatchedEntries.Count} position mismatches");
        
        if (missingInGrid.Count > 0 || emptyGridEntries.Count > 0 || mismatchedEntries.Count > 0)
        {
            Debug.LogError("<color=red>GRID INCONSISTENCIES DETECTED!</color> Use the detailed logs above to fix issues.");
        }
        else
        {
            Debug.Log("<color=green>GRID IS CONSISTENT!</color> No issues detected between physical rooms and grid dictionary.");
        }
    }
} 