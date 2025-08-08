using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// GridConnectionValidator handles the validation and correction of room connections in the dungeon grid.
/// It ensures rooms are properly connected and prevents caps from being placed where rooms should be.
/// </summary>
public class GridConnectionValidator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ImprovedDungeonGenerator dungeonGenerator;
    
    [Header("Settings")]
    [SerializeField] private bool detailedDebugLogging = true;  // Detaylı debug logları için yeni değişken
    
    private Dictionary<Vector2Int, List<ConnectionRequirement>> requiredConnectionsMap = new Dictionary<Vector2Int, List<ConnectionRequirement>>();
    private Dictionary<Vector2Int, bool> roomOccupancyDebugMap = new Dictionary<Vector2Int, bool>();  // Debug için odaların işgal durumu
    
    // Data structure to track connection requirements
    private class ConnectionRequirement
    {
        public Vector2Int sourcePosition;
        public Direction requiredDirection;
        public GameObject sourceRoom;
        public RoomConnectionPoint connectionPoint;
    }
    
    /// <summary>
    /// Validates all grid connections after dungeon generation
    /// </summary>
    public void ValidateGridConnections(Dictionary<Vector2Int, GameObject> roomGrid)
    {
        Debug.Log("<color=yellow>********** GRID CONNECTION VALIDATION STARTING **********</color>");
        
        // 0. ADIM: MEVCUT BAĞLANTILARI KONTROL ET VE DÜZELT
        Debug.Log("<color=yellow>STEP 0: VALIDATING EXISTING CONNECTIONS</color>");
        ValidateAllExistingConnections(roomGrid);
        
        // Clear previous data
        requiredConnectionsMap.Clear();
        roomOccupancyDebugMap.Clear();
        
        // Grid durumunu kaydet
        foreach (var kvp in roomGrid)
        {
            roomOccupancyDebugMap[kvp.Key] = kvp.Value != null;
            if (detailedDebugLogging)
            {
                Debug.Log($"<color=cyan>GRID DEBUG:</color> Position {kvp.Key} is {(kvp.Value != null ? "OCCUPIED" : "EMPTY")} in roomGrid");
            }
        }
        
        // 1. ADIM: TÜM AÇIK BAĞLANTILARI TOPLA
        Debug.Log("<color=yellow>STEP 1: COLLECTING ALL OPEN CONNECTIONS</color>");
        Dictionary<Vector2Int, List<ConnectionRequirement>> allConnectionsMap = CollectAllOpenConnections(roomGrid);
        
        // 2. ADIM: GEREKLİ TÜM ODALARI OLUŞTUR (MEVCUT ODALARA DOKUNMA)
        Debug.Log("<color=yellow>STEP 2: CREATING ALL MISSING ROOMS</color>");
        CreateAllMissingRooms(roomGrid, allConnectionsMap);
        
        // 3. ADIM: MEVCUT ODALARA BAĞLANTILARI YAP
        Debug.Log("<color=yellow>STEP 3: CONNECTING EXISTING ROOMS</color>");
        ConnectAllExistingRooms(roomGrid, allConnectionsMap);
        
        // 4. ADIM: ODALAR ARASINDAKİ BOŞLUKLARI DOLDUR
        Debug.Log("<color=yellow>STEP 4: FILLING GAPS BETWEEN ROOMS</color>");
        FillGapsBetweenRooms(roomGrid);
        
        // YENİ: 5. ADIM: KOPUK ODA GRUPLARINI TESPİT ET VE BAĞLA
        Debug.Log("<color=yellow>STEP 5: CONNECTING ISOLATED ROOM GROUPS</color>");
        ConnectIsolatedRoomGroups(roomGrid);
        
        // 6. ADIM: SON OLARAK TÜM BAĞLANTILARI TEKRAR DOĞRULA
        Debug.Log("<color=yellow>STEP 6: FINAL CONNECTION VALIDATION</color>");
        ValidateAllExistingConnections(roomGrid);
        
        Debug.Log("<color=yellow>********** GRID CONNECTION VALIDATION COMPLETED **********</color>");
        
        // Final grid durumunu logla
        if (detailedDebugLogging)
        {
            Debug.Log("<color=magenta>FINAL GRID STATE:</color>");
            foreach (var kvp in roomGrid)
            {
                string initialState = roomOccupancyDebugMap.ContainsKey(kvp.Key) ? (roomOccupancyDebugMap[kvp.Key] ? "OCCUPIED" : "EMPTY") : "UNKNOWN";
                string finalState = kvp.Value != null ? "OCCUPIED" : "EMPTY";
                
                if (initialState != finalState)
                {
                    Debug.Log($"<color=yellow>GRID CHANGED:</color> Position {kvp.Key} changed from {initialState} to {finalState}");
                }
                else
                {
                    Debug.Log($"<color=cyan>GRID UNCHANGED:</color> Position {kvp.Key} remained {finalState}");
                }
            }
        }
    }
    
    /// <summary>
    /// Tüm mevcut bağlantı noktalarını doğrular ve sorunları düzeltir
    /// </summary>
    private void ValidateAllExistingConnections(Dictionary<Vector2Int, GameObject> roomGrid)
    {
        int validatedPoints = 0;
        int fixedConnections = 0;
        int invalidConnections = 0;
        
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value == null) continue;
            
            RoomConnectionPoint[] connectionPoints = kvp.Value.GetComponentsInChildren<RoomConnectionPoint>();
            foreach (var point in connectionPoints)
            {
                validatedPoints++;
                
                // İşaretli ama bağlantısı yoksa düzelt
                if (point.isOccupied && point.connectedTo == null)
                {
                    Debug.LogWarning($"<color=orange>INVALID CONNECTION:</color> Connection point at {kvp.Key} marked as occupied but has no target");
                    point.isOccupied = false;
                    fixedConnections++;
                }
                // Bağlantısı var ama karşılıklı değilse düzelt
                else if (point.isOccupied && point.connectedTo != null && point.connectedTo.connectedTo != point)
                {
                    Debug.LogWarning($"<color=orange>ONE-WAY CONNECTION:</color> Connection from {point.gameObject.name} to {point.connectedTo.gameObject.name} is not bidirectional");
                    point.connectedTo.isOccupied = true;
                    point.connectedTo.connectedTo = point;
                    fixedConnections++;
                }
                // Bağlantı hedefi zaten başka bir noktaya bağlıysa hata ver
                else if (point.isOccupied && point.connectedTo != null && point.connectedTo.isOccupied && point.connectedTo.connectedTo != point)
                {
                    Debug.LogError($"<color=red>CONNECTION CONFLICT:</color> Target point is connected to another point");
                    invalidConnections++;
                }
            }
        }
        
        Debug.Log($"<color=magenta>CONNECTION VALIDATION:</color> Validated {validatedPoints} connection points, fixed {fixedConnections} issues, found {invalidConnections} invalid connections");
    }
    
    /// <summary>
    /// Tüm açık bağlantıları toplayıp bir haritada organize eder
    /// </summary>
    private Dictionary<Vector2Int, List<ConnectionRequirement>> CollectAllOpenConnections(Dictionary<Vector2Int, GameObject> roomGrid)
    {
        Dictionary<Vector2Int, List<ConnectionRequirement>> connectionsMap = new Dictionary<Vector2Int, List<ConnectionRequirement>>();
        
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value == null)
            {
                if (detailedDebugLogging)
                    Debug.Log($"<color=red>NULL ROOM:</color> Found null room in grid at {kvp.Key}");
                continue;
            }
            
            // Get all connection points in this room
            RoomConnectionPoint[] connectionPoints = kvp.Value.GetComponentsInChildren<RoomConnectionPoint>();
            
            if (detailedDebugLogging)
                Debug.Log($"<color=cyan>ROOM CONNECTIONS:</color> Room at {kvp.Key} has {connectionPoints.Length} connection points");
            
            foreach (var point in connectionPoints)
            {
                if (point.isOccupied) continue;  // Skip already connected points
                
                // Determine which grid position this connection would connect to
                Direction direction = ConvertConnectionToDirection(point.direction);
                Vector2Int targetPos = GetPositionInDirection(kvp.Key, direction);
                
                if (detailedDebugLogging)
                    Debug.Log($"<color=orange>OPEN CONNECTION:</color> Room at {kvp.Key} has open connection in direction {direction} targeting position {targetPos}");
                
                // Hedef pozisyonda zaten bir oda var mı kontrol et
                if (roomGrid.ContainsKey(targetPos) && roomGrid[targetPos] != null)
                {
                    GameObject targetRoom = roomGrid[targetPos];
                    Debug.Log($"<color=yellow>EXISTING ROOM:</color> Position {targetPos} already has room {targetRoom.name}");
                }
                
                // Bu bağlantıyı gereken bağlantılar listesine ekle
                if (!connectionsMap.ContainsKey(targetPos))
                {
                    connectionsMap[targetPos] = new List<ConnectionRequirement>();
                }
                
                connectionsMap[targetPos].Add(new ConnectionRequirement
                {
                    sourcePosition = kvp.Key,
                    requiredDirection = direction,
                    sourceRoom = kvp.Value,
                    connectionPoint = point
                });
            }
        }
        
        // Log required connections
        Debug.Log($"<color=yellow>REQUIRED CONNECTIONS:</color> Found {connectionsMap.Count} positions requiring connections");
        foreach (var kvp in connectionsMap)
        {
            Debug.Log($"<color=cyan>POSITION {kvp.Key}</color> requires {kvp.Value.Count} connections from:");
            foreach (var req in kvp.Value)
            {
                Debug.Log($"  - From {req.sourcePosition} in direction {req.requiredDirection}");
            }
        }
        
        return connectionsMap;
    }
    
    /// <summary>
    /// Eksik olan tüm odaları oluşturur
    /// </summary>
    private void CreateAllMissingRooms(Dictionary<Vector2Int, GameObject> roomGrid, Dictionary<Vector2Int, List<ConnectionRequirement>> connectionsMap)
    {
        // Önce tüm eksik odaları tespit et
        List<Vector2Int> missingRoomPositions = new List<Vector2Int>();
        
        foreach (var kvp in connectionsMap)
        {
            Vector2Int pos = kvp.Key;
            
            // Pozisyonda oda yoksa veya null referans varsa listeye ekle
            if (!roomGrid.ContainsKey(pos) || roomGrid[pos] == null)
            {
                missingRoomPositions.Add(pos);
                Debug.Log($"<color=orange>MISSING ROOM:</color> Position {pos} needs a room with {kvp.Value.Count} connections");
            }
            else
            {
                // Oda var ama eksik bağlantılar olabilir, kontrol et
                GameObject existingRoom = roomGrid[pos];
                var existingPoints = existingRoom.GetComponentsInChildren<RoomConnectionPoint>();
                
                // Her bağlantı gereksinimi için uygun bir bağlantı var mı kontrol et
                foreach (var req in kvp.Value)
                {
                    Direction oppositeDir = GetOppositeDirection(req.requiredDirection);
                    ConnectionDirection neededDirection = DirectionToConnectionDirection(oppositeDir);
                    
                    // Bu yönde açık bir bağlantı var mı?
                    bool hasMatchingPoint = false;
                    foreach (var point in existingPoints)
                    {
                        if (!point.isOccupied && point.direction == neededDirection)
                        {
                            hasMatchingPoint = true;
                            break;
                        }
                    }
                    
                    if (!hasMatchingPoint)
                    {
                        Debug.LogWarning($"<color=orange>MISSING CONNECTION:</color> Room at {pos} has no available {neededDirection} connection required by room at {req.sourcePosition}");
                    }
                }
            }
        }
        
        // Şimdi tüm eksik odaları oluştur
        int createdRooms = 0;
        int failedRooms = 0;
        
        foreach (var pos in missingRoomPositions)
        {
            if (!connectionsMap.ContainsKey(pos)) continue;
            
            List<ConnectionRequirement> requirements = connectionsMap[pos];
            Vector3 worldPos = new Vector3(pos.x * dungeonGenerator.xOffset, pos.y * dungeonGenerator.yOffset, 0);
            
            Debug.Log($"<color=yellow>CREATING ROOM:</color> Position {pos} with {requirements.Count} connections");
            
            // Gerekli tüm bağlantı yönlerini tespit et
            bool needsNorth = false, needsSouth = false, needsEast = false, needsWest = false;
            
            foreach (var req in requirements)
            {
                Direction oppositeDir = GetOppositeDirection(req.requiredDirection);
                
                switch (oppositeDir)
                {
                    case Direction.North: needsNorth = true; break;
                    case Direction.South: needsSouth = true; break;
                    case Direction.East: needsEast = true; break;
                    case Direction.West: needsWest = true; break;
                }
            }
            
            // Log hangi bağlantıların gerektiğini
            Debug.Log($"<color=cyan>REQUIRED DIRECTIONS:</color> North: {needsNorth}, South: {needsSouth}, East: {needsEast}, West: {needsWest}");
            
            // Determine what connection type we need
            RoomData.RoomConnectionType requiredType = DetermineRoomType(needsNorth, needsSouth, needsWest, needsEast);
            
            // Fiziksel bir çakışma olup olmadığını kontrol et
            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, dungeonGenerator.roomCheckRadius, dungeonGenerator.whatIsRoom);
            if (colliders != null && colliders.Length > 0)
            {
                Debug.LogError($"<color=red>PHYSICAL COLLISION:</color> Cannot place room at {pos} due to physical collisions");
                
                foreach (var collider in colliders)
                {
                    Debug.LogError($"<color=red>COLLIDING WITH:</color> {collider.gameObject.name}");
                }
                
                failedRooms++;
                continue;
            }
            
            // Find matching room
            RoomData roomData = FindRoomWithRequiredConnections(requiredType);
            
            if (roomData != null)
            {
                // Spawn the room
                GameObject roomObj = dungeonGenerator.SpawnRoom(roomData, worldPos);
                
                if (roomObj != null)
                {
                    Debug.Log($"<color=green>SUCCESS:</color> Placed room at {pos} with connection type {requiredType}");
                    
                    // Add to room tracking
                    roomGrid[pos] = roomObj;
                    dungeonGenerator.spawnedRooms.Add(roomObj);
                    createdRooms++;
                }
                else
                {
                    Debug.LogError($"<color=red>SPAWN FAILED:</color> Could not spawn room at {pos}");
                    failedRooms++;
                }
            }
            else
            {
                Debug.LogError($"<color=red>NO SUITABLE ROOM:</color> Could not find room with connection type {requiredType} for position {pos}");
                failedRooms++;
            }
        }
        
        Debug.Log($"<color=magenta>ROOM CREATION:</color> Created {createdRooms} rooms, failed to create {failedRooms} rooms");
    }
    
    /// <summary>
    /// Mevcut tüm odalar arasında bağlantıları yapar
    /// </summary>
    private void ConnectAllExistingRooms(Dictionary<Vector2Int, GameObject> roomGrid, Dictionary<Vector2Int, List<ConnectionRequirement>> connectionsMap)
    {
        int connectionsMade = 0;
        int connectionsFailed = 0;
        
        // Her bağlantıyı işle
        foreach (var kvp in connectionsMap)
        {
            Vector2Int targetPos = kvp.Key;
            
            // Hedef pozisyonda oda var mı?
            if (!roomGrid.ContainsKey(targetPos) || roomGrid[targetPos] == null)
            {
                Debug.LogWarning($"<color=orange>MISSING ROOM:</color> Position {targetPos} has no room to connect to");
                connectionsFailed += kvp.Value.Count;
                continue;
            }
            
            GameObject targetRoom = roomGrid[targetPos];
            
            // Her bağlantı gereksinimi için
            foreach (var req in kvp.Value)
            {
                Debug.Log($"<color=cyan>CONNECTING:</color> Room at {req.sourcePosition} to room at {targetPos} in direction {req.requiredDirection}");
                
                // Kaynak oda var mı?
                if (req.sourceRoom == null)
                {
                    Debug.LogError($"<color=red>CONNECTION ERROR:</color> Source room is null");
                    connectionsFailed++;
                    continue;
                }
                
                // İki odayı bağla
                if (ConnectRoomsIfPossible(req.sourceRoom, targetRoom, req.requiredDirection))
                {
                    connectionsMade++;
                }
                else
                {
                    connectionsFailed++;
                }
            }
        }
        
        Debug.Log($"<color=magenta>CONNECTION RESULTS:</color> Made {connectionsMade} connections, failed {connectionsFailed}");
    }
    
    /// <summary>
    /// İki odayı belirtilen yönde bağlamaya çalışır
    /// </summary>
    private bool ConnectRoomsIfPossible(GameObject sourceRoom, GameObject targetRoom, Direction direction)
    {
        if (sourceRoom == null || targetRoom == null)
        {
            Debug.LogError("<color=red>CONNECTION ERROR:</color> Source or target room is null");
            return false;
        }
        
        // Get connection points from both rooms
        var sourcePoints = sourceRoom.GetComponentsInChildren<RoomConnectionPoint>();
        var targetPoints = targetRoom.GetComponentsInChildren<RoomConnectionPoint>();
        
        // Karşı bağlantı yönünü hesapla
        Direction oppositeDirection = GetOppositeDirection(direction);
        
        // Bağlantı yönlerini dönüştür
        ConnectionDirection sourceDirection = DirectionToConnectionDirection(direction);
        ConnectionDirection targetDirection = DirectionToConnectionDirection(oppositeDirection);
        
        // Uygun bağlantı noktaları bul
        var sourcePoint = System.Array.Find(sourcePoints, p => p.direction == sourceDirection && !p.isOccupied);
        var targetPoint = System.Array.Find(targetPoints, p => p.direction == targetDirection && !p.isOccupied);
        
        // Her iki tarafta da uygun bağlantı noktası var mı?
        if (sourcePoint != null && targetPoint != null)
        {
            // Bağlantıyı yap
            sourcePoint.Connect(targetPoint);
            Debug.Log($"<color=green>CONNECTED:</color> {sourceRoom.name} to {targetRoom.name} via {direction} direction");
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=orange>CONNECTION FAILED:</color> Could not find matching connection points between {sourceRoom.name} and {targetRoom.name}");
            if (sourcePoint == null)
                Debug.LogWarning($"<color=orange>SOURCE MISSING:</color> No available {sourceDirection} connection on {sourceRoom.name}");
            if (targetPoint == null)
                Debug.LogWarning($"<color=orange>TARGET MISSING:</color> No available {targetDirection} connection on {targetRoom.name}");
            return false;
        }
    }
    
    /// <summary>
    /// Tüm odaların bağlantılarını kontrol eder ve tutarsızlıkları tespit eder
    /// </summary>
    [ContextMenu("Debug - Validate All Connections")]
    public void DebugValidateAllConnections()
    {
        Debug.Log("<color=yellow>********** STARTING CONNECTION VALIDATION **********</color>");
        
        if (dungeonGenerator == null)
        {
            Debug.LogError("<color=red>ERROR:</color> Dungeon Generator reference is missing!");
            return;
        }
        
        int totalConnectionPoints = 0;
        int openConnections = 0;
        int occupiedConnections = 0;
        int invalidConnections = 0;
        int mismatchedConnections = 0;
        
        // Tüm odaları bul
        var allRooms = dungeonGenerator.spawnedRooms;
        Dictionary<Vector2Int, GameObject> roomGrid = new Dictionary<Vector2Int, GameObject>();
        
        // Önce tüm odaları grid haritasına ekle
        foreach (var room in allRooms)
        {
            if (room == null) continue;
            
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(room.transform.position.x / dungeonGenerator.xOffset), 
                Mathf.RoundToInt(room.transform.position.y / dungeonGenerator.yOffset));
            
            roomGrid[gridPos] = room;
            
            // Bağlantı noktalarını say
            var points = room.GetComponentsInChildren<RoomConnectionPoint>();
            totalConnectionPoints += points.Length;
            
            foreach (var point in points)
            {
                if (point.isOccupied)
                    occupiedConnections++;
                else
                    openConnections++;
            }
        }
        
        Debug.Log($"<color=cyan>ROOMS:</color> Found {allRooms.Count} rooms with {totalConnectionPoints} connection points ({occupiedConnections} occupied, {openConnections} open)");
        
        // Her bağlantıyı doğrula
        foreach (var room in allRooms)
        {
            if (room == null) continue;
            
            Vector2Int roomGridPos = new Vector2Int(
                Mathf.RoundToInt(room.transform.position.x / dungeonGenerator.xOffset), 
                Mathf.RoundToInt(room.transform.position.y / dungeonGenerator.yOffset));
                
            var points = room.GetComponentsInChildren<RoomConnectionPoint>();
            
            foreach (var point in points)
            {
                if (point.isOccupied)
                {
                                         // Bağlantı karşılıklı mı?
                     if (point.connectedTo == null)
                     {
                         Debug.LogError($"<color=red>INVALID CONNECTION:</color> Room {room.name} at {roomGridPos} has occupied connection {point.direction} but connectedTo is null!");
                         invalidConnections++;
                         continue;
                     }
                     
                     // Karşı tarafta da bağlantı var mı?
                     var targetPoint = point.connectedTo;
                    
                                         if (!targetPoint.isOccupied || targetPoint.connectedTo != point)
                    {
                        Debug.LogError($"<color=red>MISMATCHED CONNECTION:</color> Room {room.name} connects to {targetPoint.transform.parent.name} but target doesn't connect back properly");
                        mismatchedConnections++;
                        continue;
                    }
                    
                    // Doğru yöne mi bağlanıyor?
                    ConnectionDirection expectedDirection = GetOppositeConnectionDirection(point.direction);
                    if (targetPoint.direction != expectedDirection)
                    {
                        Debug.LogError($"<color=red>DIRECTION MISMATCH:</color> Room {room.name} connects {point.direction} to {targetPoint.transform.parent.name}'s {targetPoint.direction} (should be {expectedDirection})");
                        mismatchedConnections++;
                    }
                    
                    // Fiziksel olarak doğru pozisyonda mı?
                    Vector2Int expectedTargetGridPos = GetPositionInDirection(roomGridPos, ConvertConnectionToDirection(point.direction));
                    Vector2Int actualTargetGridPos = new Vector2Int(
                        Mathf.RoundToInt(targetPoint.transform.parent.position.x / dungeonGenerator.xOffset), 
                        Mathf.RoundToInt(targetPoint.transform.parent.position.y / dungeonGenerator.yOffset));
                    
                    if (expectedTargetGridPos != actualTargetGridPos)
                    {
                        Debug.LogError($"<color=red>POSITION MISMATCH:</color> Connection from {room.name} {point.direction} should go to grid {expectedTargetGridPos} but target is at {actualTargetGridPos}");
                        mismatchedConnections++;
                    }
                }
            }
        }
        
        // Özet
        Debug.Log("<color=yellow>********** CONNECTION VALIDATION COMPLETED **********</color>");
        Debug.Log($"<color=magenta>SUMMARY:</color> Found {invalidConnections} invalid connections, {mismatchedConnections} mismatched connections");
        
        if (invalidConnections > 0 || mismatchedConnections > 0)
        {
            Debug.LogError("<color=red>CONNECTION ISSUES DETECTED!</color> Use the detailed logs above to fix issues.");
        }
        else
        {
            Debug.Log("<color=green>ALL CONNECTIONS VALID!</color> No issues detected.");
        }
    }
    
    /// <summary>
    /// Determines the required room type based on all connection requirements
    /// </summary>
    private RoomData.RoomConnectionType DetermineRequiredRoomType(List<ConnectionRequirement> requirements)
    {
        bool needsNorth = false;
        bool needsSouth = false;
        bool needsEast = false;
        bool needsWest = false;
        
        foreach (var req in requirements)
        {
            Direction oppositeDirection = GetOppositeDirection(req.requiredDirection);
            
            switch (oppositeDirection)
            {
                case Direction.North:
                    needsNorth = true;
                    break;
                case Direction.South:
                    needsSouth = true;
                    break;
                case Direction.East:
                    needsEast = true;
                    break;
                case Direction.West:
                    needsWest = true;
                    break;
            }
        }
        
        return DetermineRoomType(needsNorth, needsSouth, needsWest, needsEast);
    }
    
    /// <summary>
    /// Finds a room with the required connection type
    /// </summary>
    private RoomData FindRoomWithRequiredConnections(RoomData.RoomConnectionType requiredType)
    {
        // First try to find exact match
        RoomData exactMatch = dungeonGenerator.roomPool.FirstOrDefault(r => 
            r.connectionType == requiredType && 
            !r.isStartRoom && 
            !r.isEndRoom);
        
        if (exactMatch != null)
            return exactMatch;
        
        // Try compatible room types
        return dungeonGenerator.roomPool.FirstOrDefault(r => 
            IsCompatibleRoomType(r.connectionType, requiredType) && 
            !r.isStartRoom && 
            !r.isEndRoom);
    }
    
    /// <summary>
    /// Checks if a room type is compatible with the required type
    /// </summary>
    private bool IsCompatibleRoomType(RoomData.RoomConnectionType actualType, RoomData.RoomConnectionType requiredType)
    {
        // Fourway is compatible with everything
        if (actualType == RoomData.RoomConnectionType.Fourway)
            return true;
            
        // Check if actual type has all the connections required by requiredType
        bool requiredNorth = HasNorthConnection(requiredType);
        bool requiredSouth = HasSouthConnection(requiredType);
        bool requiredEast = HasEastConnection(requiredType);
        bool requiredWest = HasWestConnection(requiredType);
        
        bool actualNorth = HasNorthConnection(actualType);
        bool actualSouth = HasSouthConnection(actualType);
        bool actualEast = HasEastConnection(actualType);
        bool actualWest = HasWestConnection(actualType);
        
        // The actual room must have AT LEAST all the connections required
        return (!requiredNorth || actualNorth) &&
               (!requiredSouth || actualSouth) &&
               (!requiredEast || actualEast) &&
               (!requiredWest || actualWest);
    }
    
    #region Helper Methods
    
    private RoomData.RoomConnectionType DetermineRoomType(bool up, bool down, bool left, bool right)
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
                Debug.LogError("Invalid connection count!");
                return RoomData.RoomConnectionType.SingleUp;
        }
    }
    
    private bool HasNorthConnection(RoomData.RoomConnectionType type)
    {
        return type == RoomData.RoomConnectionType.SingleUp ||
               type == RoomData.RoomConnectionType.DoubleUpDown ||
               type == RoomData.RoomConnectionType.DoubleUpLeft ||
               type == RoomData.RoomConnectionType.DoubleUpRight ||
               type == RoomData.RoomConnectionType.TripleUpLeftDown ||
               type == RoomData.RoomConnectionType.TripleUpLeftRight ||
               type == RoomData.RoomConnectionType.TripleUpRightDown ||
               type == RoomData.RoomConnectionType.Fourway;
    }
    
    private bool HasSouthConnection(RoomData.RoomConnectionType type)
    {
        return type == RoomData.RoomConnectionType.SingleDown ||
               type == RoomData.RoomConnectionType.DoubleUpDown ||
               type == RoomData.RoomConnectionType.DoubleDownLeft ||
               type == RoomData.RoomConnectionType.DoubleDownRight ||
               type == RoomData.RoomConnectionType.TripleUpLeftDown ||
               type == RoomData.RoomConnectionType.TripleUpRightDown ||
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
               type == RoomData.RoomConnectionType.TripleUpLeftRight ||
               type == RoomData.RoomConnectionType.TripleDownRightLeft ||
               type == RoomData.RoomConnectionType.Fourway;
    }
    
    private bool HasWestConnection(RoomData.RoomConnectionType type)
    {
        return type == RoomData.RoomConnectionType.SingleLeft ||
               type == RoomData.RoomConnectionType.DoubleLeftRight ||
               type == RoomData.RoomConnectionType.DoubleUpLeft ||
               type == RoomData.RoomConnectionType.DoubleDownLeft ||
               type == RoomData.RoomConnectionType.TripleUpLeftDown ||
               type == RoomData.RoomConnectionType.TripleUpLeftRight ||
               type == RoomData.RoomConnectionType.TripleDownRightLeft ||
               type == RoomData.RoomConnectionType.Fourway;
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

    private ConnectionDirection GetOppositeConnectionDirection(ConnectionDirection direction)
    {
        switch (direction)
        {
            case ConnectionDirection.North: return ConnectionDirection.South;
            case ConnectionDirection.South: return ConnectionDirection.North;
            case ConnectionDirection.East: return ConnectionDirection.West;
            case ConnectionDirection.West: return ConnectionDirection.East;
            default: return ConnectionDirection.North;
        }
    }
    
    /// <summary>
    /// Odalar arasındaki boşlukları tespit edip doldurur
    /// </summary>
    private void FillGapsBetweenRooms(Dictionary<Vector2Int, GameObject> roomGrid)
    {
        // Tüm grid pozisyonlarını toplama
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value != null)
            {
                occupiedPositions.Add(kvp.Key);
            }
        }
        
        // Boşlukları tespit et
        List<Vector2Int> gapsToFill = new List<Vector2Int>();
        HashSet<Vector2Int> checkedGaps = new HashSet<Vector2Int>();
        
        // 1. ENHANCED: İlk olarak grid üzerindeki potansiyel kritik noktaları tara
        ScanGridForCriticalGaps(roomGrid, occupiedPositions, gapsToFill, checkedGaps);
        
        // 2. Her bir odanın etrafını kontrol et (mevcut yöntem)
        foreach (var pos in occupiedPositions)
        {
            // Dört yöne bak
            CheckDirectionForGaps(pos, Direction.North, roomGrid, occupiedPositions, gapsToFill, checkedGaps);
            CheckDirectionForGaps(pos, Direction.South, roomGrid, occupiedPositions, gapsToFill, checkedGaps);
            CheckDirectionForGaps(pos, Direction.East, roomGrid, occupiedPositions, gapsToFill, checkedGaps);
            CheckDirectionForGaps(pos, Direction.West, roomGrid, occupiedPositions, gapsToFill, checkedGaps);
        }
        
        // Tespit edilen boşlukları doldur
        int filledGaps = 0;
        foreach (var gapPos in gapsToFill)
        {
            // Bu pozisyonda herhangi bir oda var mı kontrol et
            if (roomGrid.ContainsKey(gapPos) && roomGrid[gapPos] != null)
            {
                continue; // Zaten dolu
            }
            
            // Fiziksel bir çakışma var mı kontrol et
            Vector3 worldPos = new Vector3(gapPos.x * dungeonGenerator.xOffset, gapPos.y * dungeonGenerator.yOffset, 0);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, dungeonGenerator.roomCheckRadius, dungeonGenerator.whatIsRoom);
            if (colliders != null && colliders.Length > 0)
            {
                Debug.LogWarning($"<color=orange>GAP COLLISION:</color> Cannot fill gap at {gapPos} due to physical collisions");
                continue;
            }
            
            // Komşu odaları kontrol et ve bağlantı gereksinimlerini belirle
            bool needsNorth = false, needsSouth = false, needsEast = false, needsWest = false;
            
            // Kuzey komşu
            Vector2Int northPos = GetPositionInDirection(gapPos, Direction.North);
            if (occupiedPositions.Contains(northPos))
            {
                needsSouth = true; // Kuzeydeki odaya güney bağlantısı gerekiyor
            }
            
            // Güney komşu
            Vector2Int southPos = GetPositionInDirection(gapPos, Direction.South);
            if (occupiedPositions.Contains(southPos))
            {
                needsNorth = true; // Güneydeki odaya kuzey bağlantısı gerekiyor
            }
            
            // Doğu komşu
            Vector2Int eastPos = GetPositionInDirection(gapPos, Direction.East);
            if (occupiedPositions.Contains(eastPos))
            {
                needsWest = true; // Doğudaki odaya batı bağlantısı gerekiyor
            }
            
            // Batı komşu
            Vector2Int westPos = GetPositionInDirection(gapPos, Direction.West);
            if (occupiedPositions.Contains(westPos))
            {
                needsEast = true; // Batıdaki odaya doğu bağlantısı gerekiyor
            }
            
            Debug.Log($"<color=cyan>FILLING GAP:</color> Position {gapPos} needs N:{needsNorth}, S:{needsSouth}, E:{needsEast}, W:{needsWest}");
            
            // Uygun bir oda tipi belirle
            RoomData.RoomConnectionType requiredType = DetermineRoomType(needsNorth, needsSouth, needsWest, needsEast);
            
            // Odayı oluştur
            RoomData roomData = FindRoomWithRequiredConnections(requiredType);
            if (roomData != null)
            {
                GameObject roomObj = dungeonGenerator.SpawnRoom(roomData, worldPos);
                if (roomObj != null)
                {
                    roomGrid[gapPos] = roomObj;
                    dungeonGenerator.spawnedRooms.Add(roomObj);
                    filledGaps++;
                    Debug.Log($"<color=green>GAP FILLED:</color> Created room at {gapPos} with connection type {requiredType}");
                    
                    // Add this position to the occupied list for subsequent gap checks
                    occupiedPositions.Add(gapPos);
                }
            }
            else
            {
                Debug.LogError($"<color=red>GAP FILL FAILED:</color> No suitable room type found for connections N:{needsNorth}, S:{needsSouth}, E:{needsEast}, W:{needsWest}");
            }
        }
        
        // 3. ENHANCED: Son bir kontrol olarak, gruplar arasında hala kopukluk var mı diye bak ve düzelt
        if (filledGaps > 0)
        {
            // After filling gaps, re-check for isolated groups
            ConnectIsolatedRoomGroups(roomGrid);
        }
        
        Debug.Log($"<color=magenta>GAP FILLING:</color> Filled {filledGaps} gaps out of {gapsToFill.Count} detected");
    }
    
    /// <summary>
    /// Scans the entire grid to find all critical connection points that need rooms
    /// </summary>
    private void ScanGridForCriticalGaps(Dictionary<Vector2Int, GameObject> roomGrid, 
                                        HashSet<Vector2Int> occupiedPositions, 
                                        List<Vector2Int> gapsToFill, 
                                        HashSet<Vector2Int> checkedGaps)
    {
        // Find min and max grid coordinates to determine scan range
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        
        if (occupiedPositions.Count == 0)
            return;
            
        foreach (var pos in occupiedPositions)
        {
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
        }
        
        // Add a buffer around the grid for edge detection
        minX -= 1; maxX += 1;
        minY -= 1; maxY += 1;
        
        Debug.Log($"<color=yellow>GRID SCAN:</color> Scanning area from ({minX},{minY}) to ({maxX},{maxY})");
        
        // Check every possible position in the grid
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                
                // Skip positions that are already occupied or checked
                if (occupiedPositions.Contains(pos) || checkedGaps.Contains(pos))
                    continue;
                
                checkedGaps.Add(pos);
                
                // Check if this is a critical path position using our enhanced algorithm
                if (IsCriticalPathPosition(pos, roomGrid, occupiedPositions))
                {
                    gapsToFill.Add(pos);
                    Debug.Log($"<color=red>CRITICAL GAP DETECTED:</color> Grid scan found critical path position at {pos}");
                }
                
                // Check for isolated rooms nearby
                bool hasAdjacentRoom = false;
                foreach (Direction dir in new[] { Direction.North, Direction.East, Direction.South, Direction.West })
                {
                    Vector2Int adjPos = GetPositionInDirection(pos, dir);
                    if (occupiedPositions.Contains(adjPos))
                    {
                        hasAdjacentRoom = true;
                        break;
                    }
                }
                
                // Check for cross-diagonal pattern (a common pattern that can lead to disconnected paths)
                // X . X
                // . o .  (where X are occupied, o is current position, and . are empty)
                // X . X
                if (!hasAdjacentRoom)
                {
                    bool northEastOccupied = occupiedPositions.Contains(new Vector2Int(x+1, y+1));
                    bool northWestOccupied = occupiedPositions.Contains(new Vector2Int(x-1, y+1));
                    bool southEastOccupied = occupiedPositions.Contains(new Vector2Int(x+1, y-1));
                    bool southWestOccupied = occupiedPositions.Contains(new Vector2Int(x-1, y-1));
                    
                    int diagOccupiedCount = 0;
                    if (northEastOccupied) diagOccupiedCount++;
                    if (northWestOccupied) diagOccupiedCount++;
                    if (southEastOccupied) diagOccupiedCount++;
                    if (southWestOccupied) diagOccupiedCount++;
                    
                    // Diagonal pattern that should be filled
                    if (diagOccupiedCount >= 2)
                    {
                        bool crossDiagonal = (northEastOccupied && southWestOccupied) || (northWestOccupied && southEastOccupied);
                        if (crossDiagonal)
                        {
                            gapsToFill.Add(pos);
                            Debug.Log($"<color=orange>DIAGONAL GAP DETECTED:</color> Cross-diagonal pattern at {pos}");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Belirli bir yöndeki boşlukları kontrol eder
    /// </summary>
    private void CheckDirectionForGaps(
        Vector2Int startPos, 
        Direction direction, 
        Dictionary<Vector2Int, GameObject> roomGrid,
        HashSet<Vector2Int> occupiedPositions,
        List<Vector2Int> gapsToFill,
        HashSet<Vector2Int> checkedGaps)
    {
        // ENHANCED GAP DETECTION: Direct path gaps, diagonal gaps, and isolated corners
        
        // 1. Direct line gaps (existing)
        Vector2Int neighborPos = GetPositionInDirection(startPos, direction);
        if (!occupiedPositions.Contains(neighborPos) && !checkedGaps.Contains(neighborPos))
        {
            checkedGaps.Add(neighborPos);
            
            // Check if this gap connects two rooms
            Vector2Int nextPos = GetPositionInDirection(neighborPos, direction);
            if (occupiedPositions.Contains(nextPos))
            {
                gapsToFill.Add(neighborPos);
                Debug.Log($"<color=yellow>GAP DETECTED:</color> Linear gap between {startPos} and {nextPos} at position {neighborPos}");
            }
            
            // 2. NEW: Check for diagonal gaps (L-shape)
            // Left turn
            Direction leftTurn = RotateDirectionLeft(direction);
            Vector2Int leftDiagPos = GetPositionInDirection(neighborPos, leftTurn);
            
            if (occupiedPositions.Contains(leftDiagPos))
            {
                gapsToFill.Add(neighborPos);
                Debug.Log($"<color=yellow>GAP DETECTED:</color> L-shape gap between {startPos} and {leftDiagPos} at position {neighborPos}");
            }
            
            // Right turn
            Direction rightTurn = RotateDirectionRight(direction);
            Vector2Int rightDiagPos = GetPositionInDirection(neighborPos, rightTurn);
            
            if (occupiedPositions.Contains(rightDiagPos))
            {
                gapsToFill.Add(neighborPos);
                Debug.Log($"<color=yellow>GAP DETECTED:</color> L-shape gap between {startPos} and {rightDiagPos} at position {neighborPos}");
            }
            
            // 3. NEW: Check for corner spaces
            Vector2Int cornerLeftPos = GetPositionInDirection(startPos, leftTurn);
            Vector2Int cornerRightPos = GetPositionInDirection(startPos, rightTurn);
            
            if (occupiedPositions.Contains(cornerLeftPos) && !occupiedPositions.Contains(neighborPos))
            {
                Vector2Int cornerGapPos = GetPositionInDirection(cornerLeftPos, direction);
                if (!occupiedPositions.Contains(cornerGapPos) && !checkedGaps.Contains(cornerGapPos))
                {
                    gapsToFill.Add(cornerGapPos);
                    checkedGaps.Add(cornerGapPos);
                    Debug.Log($"<color=yellow>GAP DETECTED:</color> Corner gap between {startPos}, {neighborPos} and {cornerLeftPos} at position {cornerGapPos}");
                }
            }
            
            if (occupiedPositions.Contains(cornerRightPos) && !occupiedPositions.Contains(neighborPos))
            {
                Vector2Int cornerGapPos = GetPositionInDirection(cornerRightPos, direction);
                if (!occupiedPositions.Contains(cornerGapPos) && !checkedGaps.Contains(cornerGapPos))
                {
                    gapsToFill.Add(cornerGapPos);
                    checkedGaps.Add(cornerGapPos);
                    Debug.Log($"<color=yellow>GAP DETECTED:</color> Corner gap between {startPos}, {neighborPos} and {cornerRightPos} at position {cornerGapPos}");
                }
            }
            
            // 4. NEW: Check for critical path positions
            bool isCriticalPathPosition = IsCriticalPathPosition(neighborPos, roomGrid, occupiedPositions);
            if (isCriticalPathPosition)
            {
                gapsToFill.Add(neighborPos);
                Debug.Log($"<color=red>CRITICAL GAP DETECTED:</color> Position {neighborPos} is a critical path connector");
            }
        }
    }
    
    /// <summary>
    /// Checks if a position would block paths between rooms if left empty
    /// </summary>
    private bool IsCriticalPathPosition(Vector2Int pos, Dictionary<Vector2Int, GameObject> roomGrid, HashSet<Vector2Int> occupiedPositions)
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
            if (occupiedPositions.Contains(adjPos))
            {
                occupiedCount++;
            }
        }
        
        // If there are 2 or more adjacent rooms, this is a potential path blocker
        if (occupiedCount >= 2)
        {
            // Get all adjacent occupied positions
            List<Vector2Int> occupiedAdjacentPositions = adjacentPositions.Where(p => occupiedPositions.Contains(p)).ToList();
            
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
                    if (occupiedPositions.Contains(next) && !visited.Contains(next))
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
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Rotates a direction 90 degrees left
    /// </summary>
    private Direction RotateDirectionLeft(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.West;
            case Direction.East: return Direction.North;
            case Direction.South: return Direction.East;
            case Direction.West: return Direction.South;
            default: return dir;
        }
    }
    
    /// <summary>
    /// Rotates a direction 90 degrees right
    /// </summary>
    private Direction RotateDirectionRight(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.East;
            case Direction.East: return Direction.South;
            case Direction.South: return Direction.West;
            case Direction.West: return Direction.North;
            default: return dir;
        }
    }
    
    /// <summary>
    /// Kopuk oda gruplarını tespit eder ve ana haritaya bağlar
    /// </summary>
    private void ConnectIsolatedRoomGroups(Dictionary<Vector2Int, GameObject> roomGrid)
    {
        // Geçerli odaları topla
        List<Vector2Int> validRoomPositions = new List<Vector2Int>();
        foreach (var kvp in roomGrid)
        {
            if (kvp.Value != null)
            {
                validRoomPositions.Add(kvp.Key);
            }
        }
        
        if (validRoomPositions.Count <= 1)
        {
            Debug.Log("<color=yellow>NO ISOLATED GROUPS:</color> Not enough rooms to have isolated groups");
            return;
        }
        
        // Oda gruplarını tespit et (Flood-fill algoritması)
        List<List<Vector2Int>> roomGroups = FindRoomGroups(validRoomPositions, roomGrid);
        
        Debug.Log($"<color=cyan>ROOM GROUPS:</color> Found {roomGroups.Count} separate room groups");
        
        // Tek bir grup varsa, tüm odalar zaten bağlı demektir
        if (roomGroups.Count <= 1)
        {
            Debug.Log("<color=green>NO ISOLATED GROUPS:</color> All rooms are already connected");
            return;
        }
        
        // Birden fazla grup varsa, bunları birbirine bağlamamız gerekiyor
        // En büyük grubu ana grup olarak seç
        int largestGroupIndex = 0;
        int largestGroupSize = 0;
        
        for (int i = 0; i < roomGroups.Count; i++)
        {
            if (roomGroups[i].Count > largestGroupSize)
            {
                largestGroupSize = roomGroups[i].Count;
                largestGroupIndex = i;
            }
        }
        
        List<Vector2Int> mainGroup = roomGroups[largestGroupIndex];
        
        // Her bir izole grubu ana gruba bağla
        for (int i = 0; i < roomGroups.Count; i++)
        {
            if (i == largestGroupIndex) continue;
            
            List<Vector2Int> isolatedGroup = roomGroups[i];
            
            Debug.Log($"<color=yellow>CONNECTING GROUP:</color> Group with {isolatedGroup.Count} rooms to main group with {mainGroup.Count} rooms");
            
            // Bu izole grubu ana gruba bağla
            ConnectGroupToMainGroup(isolatedGroup, mainGroup, roomGrid);
        }
    }
    
    /// <summary>
    /// Oda gruplarını tespit eder
    /// </summary>
    private List<List<Vector2Int>> FindRoomGroups(List<Vector2Int> allRooms, Dictionary<Vector2Int, GameObject> roomGrid)
    {
        List<List<Vector2Int>> groups = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visitedRooms = new HashSet<Vector2Int>();
        
        // Her bir oda için
        foreach (var roomPos in allRooms)
        {
            // Eğer bu oda daha önce ziyaret edilmediyse
            if (!visitedRooms.Contains(roomPos))
            {
                // Yeni bir grup başlat
                List<Vector2Int> currentGroup = new List<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                
                // Bu odayı ziyaret et
                queue.Enqueue(roomPos);
                visitedRooms.Add(roomPos);
                
                // BFS ile bu odaya bağlı tüm odaları bul
                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    currentGroup.Add(current);
                    
                    // Dört yöndeki komşuları kontrol et
                    CheckAndAddNeighbor(current, Direction.North, allRooms, visitedRooms, queue);
                    CheckAndAddNeighbor(current, Direction.South, allRooms, visitedRooms, queue);
                    CheckAndAddNeighbor(current, Direction.East, allRooms, visitedRooms, queue);
                    CheckAndAddNeighbor(current, Direction.West, allRooms, visitedRooms, queue);
                }
                
                // Bu grubu listeye ekle
                groups.Add(currentGroup);
            }
        }
        
        return groups;
    }
    
    /// <summary>
    /// Komşu odayı kontrol eder ve eğer uygunsa ziyaret listesine ekler
    /// </summary>
    private void CheckAndAddNeighbor(Vector2Int current, Direction direction, List<Vector2Int> allRooms, 
                                     HashSet<Vector2Int> visitedRooms, Queue<Vector2Int> queue)
    {
        Vector2Int neighbor = GetPositionInDirection(current, direction);
        
        // Eğer bu komşu pozisyonda bir oda varsa ve daha önce ziyaret edilmediyse
        if (allRooms.Contains(neighbor) && !visitedRooms.Contains(neighbor))
        {
            queue.Enqueue(neighbor);
            visitedRooms.Add(neighbor);
        }
    }
    
    /// <summary>
    /// İzole bir oda grubunu ana gruba bağlar
    /// </summary>
    private void ConnectGroupToMainGroup(List<Vector2Int> isolatedGroup, List<Vector2Int> mainGroup, Dictionary<Vector2Int, GameObject> roomGrid)
    {
        // En kısa mesafeyi bul
        Vector2Int closestIsolated = isolatedGroup[0];
        Vector2Int closestMain = mainGroup[0];
        int shortestDistance = int.MaxValue;
        
        foreach (var isolatedPos in isolatedGroup)
        {
            foreach (var mainPos in mainGroup)
            {
                // Manhattan mesafesi
                int distance = Mathf.Abs(isolatedPos.x - mainPos.x) + Mathf.Abs(isolatedPos.y - mainPos.y);
                
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestIsolated = isolatedPos;
                    closestMain = mainPos;
                }
            }
        }
        
        Debug.Log($"<color=cyan>SHORTEST PATH:</color> From {closestIsolated} to {closestMain}, distance: {shortestDistance}");
        
        // Eğer mesafe 1 ise zaten komşular, bir sorun var demektir
        if (shortestDistance <= 1)
        {
            Debug.LogWarning($"<color=orange>UNEXPECTED:</color> Groups appear to be already adjacent at {closestIsolated} and {closestMain}");
            return;
        }
        
        // İki oda arasında bir yol oluştur
        CreatePathBetweenRooms(closestIsolated, closestMain, roomGrid);
    }
    
    /// <summary>
    /// İki oda arasında bir yol oluşturur
    /// </summary>
    private void CreatePathBetweenRooms(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, GameObject> roomGrid)
    {
        Debug.Log($"<color=yellow>CREATING PATH:</color> From {start} to {end}");
        
        // ENHANCED: Use A* pathfinding to find the shortest path while respecting existing rooms
        List<Vector2Int> pathPositions = FindPathUsingAStar(start, end, roomGrid);
        
        if (pathPositions == null || pathPositions.Count == 0)
        {
            Debug.LogWarning($"<color=orange>PATH FINDING FAILED:</color> Couldn't find path between {start} and {end}, falling back to direct path");
            
            // Fallback to direct path algorithm
            pathPositions = new List<Vector2Int>();
            
            // Önce x ekseni boyunca hareket et
            int xDir = end.x > start.x ? 1 : (end.x < start.x ? -1 : 0);
            Vector2Int current = start;
            
            while (current.x != end.x)
            {
                current.x += xDir;
                
                // Eğer bu pozisyonda bir oda yoksa, listeye ekle
                if (!roomGrid.ContainsKey(current) || roomGrid[current] == null)
                {
                    pathPositions.Add(current);
                }
            }
            
            // Sonra y ekseni boyunca hareket et
            int yDir = end.y > current.y ? 1 : (end.y < current.y ? -1 : 0);
            
            while (current.y != end.y)
            {
                current.y += yDir;
                
                // Eğer bu pozisyonda bir oda yoksa, listeye ekle
                if (!roomGrid.ContainsKey(current) || roomGrid[current] == null)
                {
                    pathPositions.Add(current);
                }
            }
        }
        
        Debug.Log($"<color=cyan>PATH POSITIONS:</color> Found {pathPositions.Count} positions to fill");
        
        // ENHANCED: Use HashSet to track positions we've added rooms to
        HashSet<Vector2Int> filledPositions = new HashSet<Vector2Int>();
        
        // Her pozisyona uygun bir oda yerleştir
        foreach (var pos in pathPositions)
        {
            // Skip if we've already filled this position
            if (filledPositions.Contains(pos)) continue;
            
            // Skip if this position already has a valid room
            if (roomGrid.ContainsKey(pos) && roomGrid[pos] != null)
            {
                filledPositions.Add(pos);
                continue;
            }
            
            // Komşu odaları kontrol et ve bağlantı gereksinimlerini belirle
            bool needsNorth = false, needsSouth = false, needsEast = false, needsWest = false;
            
            // Check path connections first - we need to connect the path rooms to each other
            int posIndex = pathPositions.IndexOf(pos);
            
            // Previous position in path (if it exists)
            if (posIndex > 0)
            {
                Vector2Int prevPos = pathPositions[posIndex - 1];
                if (prevPos.x < pos.x) needsWest = true;
                if (prevPos.x > pos.x) needsEast = true;
                if (prevPos.y < pos.y) needsSouth = true;
                if (prevPos.y > pos.y) needsNorth = true;
            }
            
            // Next position in path (if it exists)
            if (posIndex < pathPositions.Count - 1)
            {
                Vector2Int nextPos = pathPositions[posIndex + 1];
                if (nextPos.x < pos.x) needsWest = true;
                if (nextPos.x > pos.x) needsEast = true;
                if (nextPos.y < pos.y) needsSouth = true;
                if (nextPos.y > pos.y) needsNorth = true;
            }
            
            // Check for connections to start and end rooms if we're adjacent
            if (IsAdjacent(pos, start))
            {
                if (start.x < pos.x) needsWest = true;
                if (start.x > pos.x) needsEast = true;
                if (start.y < pos.y) needsSouth = true;
                if (start.y > pos.y) needsNorth = true;
            }
            
            if (IsAdjacent(pos, end))
            {
                if (end.x < pos.x) needsWest = true;
                if (end.x > pos.x) needsEast = true;
                if (end.y < pos.y) needsSouth = true;
                if (end.y > pos.y) needsNorth = true;
            }
            
            // Also check for existing rooms in cardinal directions
            // Kuzey komşu
            Vector2Int northPos = new Vector2Int(pos.x, pos.y + 1);
            if (roomGrid.ContainsKey(northPos) && roomGrid[northPos] != null)
            {
                needsNorth = true;
            }
            
            // Güney komşu
            Vector2Int southPos = new Vector2Int(pos.x, pos.y - 1);
            if (roomGrid.ContainsKey(southPos) && roomGrid[southPos] != null)
            {
                needsSouth = true;
            }
            
            // Doğu komşu
            Vector2Int eastPos = new Vector2Int(pos.x + 1, pos.y);
            if (roomGrid.ContainsKey(eastPos) && roomGrid[eastPos] != null)
            {
                needsEast = true;
            }
            
            // Batı komşu
            Vector2Int westPos = new Vector2Int(pos.x - 1, pos.y);
            if (roomGrid.ContainsKey(westPos) && roomGrid[westPos] != null)
            {
                needsWest = true;
            }
            
            // ENHANCED: Ensure there's at least two connections
            int connectionCount = 0;
            if (needsNorth) connectionCount++;
            if (needsSouth) connectionCount++;
            if (needsEast) connectionCount++;
            if (needsWest) connectionCount++;
            
            // If we have fewer than 2 connections, add some based on the path direction
            if (connectionCount < 2)
            {
                if (posIndex > 0 && posIndex < pathPositions.Count - 1)
                {
                    // Path room - connect in direction of path
                    Vector2Int prevPos = pathPositions[posIndex - 1];
                    Vector2Int nextPos = pathPositions[posIndex + 1];
                    
                    // Path is primarily horizontal
                    if (prevPos.x != nextPos.x)
                    {
                        if (!needsEast) needsEast = true;
                        if (!needsWest) needsWest = true;
                    }
                    // Path is primarily vertical
                    else
                    {
                        if (!needsNorth) needsNorth = true;
                        if (!needsSouth) needsSouth = true;
                    }
                }
                else if (posIndex == 0)
                {
                    // First path room - connect to next and to start
                    Vector2Int nextPos = pathPositions[posIndex + 1];
                    if (nextPos.x != pos.x)
                    {
                        // Horizontal connection
                        if (nextPos.x > pos.x) needsEast = true;
                        else needsWest = true;
                    }
                    if (nextPos.y != pos.y)
                    {
                        // Vertical connection
                        if (nextPos.y > pos.y) needsNorth = true;
                        else needsSouth = true;
                    }
                    
                    // Connect to start
                    if (start.x < pos.x) needsWest = true;
                    if (start.x > pos.x) needsEast = true;
                    if (start.y < pos.y) needsSouth = true;
                    if (start.y > pos.y) needsNorth = true;
                }
                else if (posIndex == pathPositions.Count - 1)
                {
                    // Last path room - connect to previous and to end
                    Vector2Int prevPos = pathPositions[posIndex - 1];
                    if (prevPos.x != pos.x)
                    {
                        // Horizontal connection
                        if (prevPos.x > pos.x) needsEast = true;
                        else needsWest = true;
                    }
                    if (prevPos.y != pos.y)
                    {
                        // Vertical connection
                        if (prevPos.y > pos.y) needsNorth = true;
                        else needsSouth = true;
                    }
                    
                    // Connect to end
                    if (end.x < pos.x) needsWest = true;
                    if (end.x > pos.x) needsEast = true;
                    if (end.y < pos.y) needsSouth = true;
                    if (end.y > pos.y) needsNorth = true;
                }
            }
            
            Debug.Log($"<color=cyan>PATH ROOM:</color> Position {pos} needs N:{needsNorth}, S:{needsSouth}, E:{needsEast}, W:{needsWest}");
            
            // Uygun bir oda tipi belirle
            RoomData.RoomConnectionType requiredType = DetermineRoomType(needsNorth, needsSouth, needsWest, needsEast);
            
            // Odayı oluştur
            RoomData roomData = FindRoomWithRequiredConnections(requiredType);
            if (roomData != null)
            {
                Vector3 worldPos = new Vector3(pos.x * dungeonGenerator.xOffset, pos.y * dungeonGenerator.yOffset, 0);
                GameObject roomObj = dungeonGenerator.SpawnRoom(roomData, worldPos);
                if (roomObj != null)
                {
                    roomGrid[pos] = roomObj;
                    dungeonGenerator.spawnedRooms.Add(roomObj);
                    filledPositions.Add(pos);
                    Debug.Log($"<color=green>PATH ROOM PLACED:</color> Room at {pos} with connection type {requiredType}");
                }
                else
                {
                    Debug.LogError($"<color=red>PATH ROOM FAILED:</color> Could not spawn room at {pos}");
                }
            }
            else
            {
                Debug.LogError($"<color=red>PATH ROOM TYPE FAILED:</color> No suitable room type found for {requiredType}");
            }
        }
        
        // ENHANCED: Ensure start and end rooms are properly connected to the path
        EnsureEndpointsConnected(start, end, roomGrid, pathPositions);
    }
    
    /// <summary>
    /// Check if two positions are adjacent (sharing an edge)
    /// </summary>
    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }
    
    /// <summary>
    /// Ensures that start and end positions are properly connected to the path
    /// </summary>
    private void EnsureEndpointsConnected(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, GameObject> roomGrid, List<Vector2Int> pathPositions)
    {
        if (pathPositions.Count == 0) return;
        
        // Check that the start room is connected to the first path position
        Vector2Int firstPathPos = pathPositions[0];
        if (!IsAdjacent(start, firstPathPos))
        {
            Debug.LogWarning($"<color=orange>DISCONNECTED START:</color> Start room at {start} is not adjacent to first path position {firstPathPos}");
            
            // Create a connection from start to first path position
            ConnectPositions(start, firstPathPos, roomGrid);
        }
        
        // Check that the end room is connected to the last path position
        Vector2Int lastPathPos = pathPositions[pathPositions.Count - 1];
        if (!IsAdjacent(end, lastPathPos))
        {
            Debug.LogWarning($"<color=orange>DISCONNECTED END:</color> End room at {end} is not adjacent to last path position {lastPathPos}");
            
            // Create a connection from last path position to end
            ConnectPositions(lastPathPos, end, roomGrid);
        }
    }
    
    /// <summary>
    /// Creates a direct connection between two positions
    /// </summary>
    private void ConnectPositions(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, GameObject> roomGrid)
    {
        // Find the positions that need rooms
        List<Vector2Int> directPathPositions = new List<Vector2Int>();
        
        // First move horizontally
        Vector2Int current = from;
        int xDir = to.x > from.x ? 1 : (to.x < from.x ? -1 : 0);
        
        while (current.x != to.x)
        {
            current.x += xDir;
            if (current != to && (!roomGrid.ContainsKey(current) || roomGrid[current] == null))
            {
                directPathPositions.Add(current);
            }
        }
        
        // Then move vertically
        int yDir = to.y > current.y ? 1 : (to.y < current.y ? -1 : 0);
        while (current.y != to.y)
        {
            current.y += yDir;
            if (current != to && (!roomGrid.ContainsKey(current) || roomGrid[current] == null))
            {
                directPathPositions.Add(current);
            }
        }
        
        // Place rooms along the direct path
        foreach (var pos in directPathPositions)
        {
            PlaceRoomWithConnections(pos, true, true, true, true, roomGrid);
        }
    }
    
    /// <summary>
    /// Uses A* pathfinding to find the best path between two points
    /// </summary>
    private List<Vector2Int> FindPathUsingAStar(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, GameObject> roomGrid)
    {
        // The set of nodes already evaluated
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        
        // The set of nodes to be evaluated
        List<Vector2Int> openSet = new List<Vector2Int> { start };
        
        // Map of navigated nodes for path reconstruction
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        
        // Cost from start along best known path
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        gScore[start] = 0;
        
        // Estimated total cost from start to goal through y
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        fScore[start] = ManhattanDistance(start, end);
        
        while (openSet.Count > 0)
        {
            // Get the node in openSet with lowest fScore
            Vector2Int current = GetLowestFScore(openSet, fScore);
            
            // If we reached the end
            if (current == end)
            {
                return ReconstructPath(cameFrom, current);
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            // Check each neighbor
            foreach (Direction dir in new[] { Direction.North, Direction.East, Direction.South, Direction.West })
            {
                Vector2Int neighbor = GetPositionInDirection(current, dir);
                
                // Skip if already evaluated
                if (closedSet.Contains(neighbor))
                    continue;
                
                // Skip if occupied by a room (unless it's the end point)
                if (neighbor != end && roomGrid.ContainsKey(neighbor) && roomGrid[neighbor] != null)
                    continue;
                
                // The distance from start to neighbor
                float tentativeGScore = gScore[current] + 1;
                
                // Add to open set if not already there
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                // This path to neighbor is better than any previous one
                else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }
                
                // Record this best path
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + ManhattanDistance(neighbor, end);
            }
        }
        
        // No path found
        return null;
    }
    
    /// <summary>
    /// Gets Manhattan distance between two points
    /// </summary>
    private float ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    
    /// <summary>
    /// Gets the position with lowest fScore from openSet
    /// </summary>
    private Vector2Int GetLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int lowest = openSet[0];
        float lowestScore = fScore.GetValueOrDefault(lowest, float.MaxValue);
        
        for (int i = 1; i < openSet.Count; i++)
        {
            float score = fScore.GetValueOrDefault(openSet[i], float.MaxValue);
            if (score < lowestScore)
            {
                lowest = openSet[i];
                lowestScore = score;
            }
        }
        
        return lowest;
    }
    
    /// <summary>
    /// Reconstructs the path from start to current
    /// </summary>
    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> totalPath = new List<Vector2Int> { current };
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        
        // Remove the start node
        if (totalPath.Count > 0)
            totalPath.RemoveAt(0);
            
        return totalPath;
    }
    
    /// <summary>
    /// Belirtilen konuma uygun bir oda yerleştirir
    /// </summary>
    private void PlaceRoomWithConnections(Vector2Int pos, bool needsNorth, bool needsSouth, bool needsEast, bool needsWest, Dictionary<Vector2Int, GameObject> roomGrid)
    {
        Vector3 worldPos = new Vector3(pos.x * dungeonGenerator.xOffset, pos.y * dungeonGenerator.yOffset, 0);
        
        // Fiziksel çakışmaları kontrol et
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, dungeonGenerator.roomCheckRadius, dungeonGenerator.whatIsRoom);
        if (colliders != null && colliders.Length > 0)
        {
            Debug.LogWarning($"<color=orange>PATH COLLISION:</color> Cannot place room at {pos} due to physical collisions");
            return;
        }
        
        // Uygun oda tipi belirle
        RoomData.RoomConnectionType requiredType = DetermineRoomType(needsNorth, needsSouth, needsWest, needsEast);
        
        // Odayı oluştur
        RoomData roomData = FindRoomWithRequiredConnections(requiredType);
        if (roomData != null)
        {
            GameObject roomObj = dungeonGenerator.SpawnRoom(roomData, worldPos);
            if (roomObj != null)
            {
                roomGrid[pos] = roomObj;
                dungeonGenerator.spawnedRooms.Add(roomObj);
                Debug.Log($"<color=green>PATH ROOM PLACED:</color> Room at {pos} with connection type {requiredType}");
            }
            else
            {
                Debug.LogError($"<color=red>PATH ROOM FAILED:</color> Could not spawn room at {pos}");
            }
        }
        else
        {
            Debug.LogError($"<color=red>PATH ROOM TYPE FAILED:</color> No suitable room type found for {requiredType}");
        }
    }
    
    #endregion
}
