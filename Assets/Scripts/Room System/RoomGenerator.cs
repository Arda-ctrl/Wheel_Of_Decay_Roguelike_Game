using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Settings")]
    public RoomData[] roomPool;
    public int numberOfRooms = 5;
    public float xOffset = 18f;
    public float yOffset = 10f;
    public LayerMask whatIsRoom;
    
    [Header("Generation Settings")]
    public int maxAttempts = 3;
    public float roomCheckRadius = 0.2f;
    
    private List<GameObject> spawnedRooms = new List<GameObject>();
    private Transform generatorPoint;
    private GameObject startRoom;
    private GameObject endRoom;

    private void Start()
    {
        generatorPoint = new GameObject("Generator Point").transform;
        generatorPoint.position = Vector3.zero;
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // Start Room'u yerleştir
        RoomData startRoomData = roomPool.FirstOrDefault(r => r.isStartRoom);
        if (startRoomData == null)
        {
            Debug.LogError("Start Room bulunamadı! Lütfen bir odayı Start Room olarak işaretleyin.");
            return;
        }

        startRoom = SpawnRoom(startRoomData, Vector3.zero);
        spawnedRooms.Add(startRoom);

        // Diğer odaları oluştur
        int currentRooms = 1;
        int failedAttempts = 0;

        while (currentRooms < numberOfRooms && failedAttempts < maxAttempts * numberOfRooms)
        {
            if (TrySpawnNextRoom())
            {
                currentRooms++;
                failedAttempts = 0;
            }
            else
            {
                failedAttempts++;
                ChangeGeneratorDirection();
            }
        }

        // End Room'u yerleştir
        RoomData endRoomData = roomPool.FirstOrDefault(r => r.isEndRoom);
        if (endRoomData != null)
        {
            endRoom = SpawnRoom(endRoomData, generatorPoint.position);
            if (endRoom != null)
            {
                spawnedRooms.Add(endRoom);
            }
        }
    }

    bool TrySpawnNextRoom()
    {
        Vector3 checkPos = generatorPoint.position;
        
        if (Physics2D.OverlapCircle(checkPos, roomCheckRadius, whatIsRoom))
        {
            return false;
        }

        // Komşu odaları kontrol et
        bool roomAbove = Physics2D.OverlapCircle(checkPos + new Vector3(0, yOffset, 0), roomCheckRadius, whatIsRoom);
        bool roomBelow = Physics2D.OverlapCircle(checkPos + new Vector3(0, -yOffset, 0), roomCheckRadius, whatIsRoom);
        bool roomLeft = Physics2D.OverlapCircle(checkPos + new Vector3(-xOffset, 0, 0), roomCheckRadius, whatIsRoom);
        bool roomRight = Physics2D.OverlapCircle(checkPos + new Vector3(xOffset, 0, 0), roomCheckRadius, whatIsRoom);

        // Uygun oda tipini belirle
        RoomData.RoomConnectionType requiredType = DetermineRoomType(roomAbove, roomBelow, roomLeft, roomRight);
        
        // Uygun odaları filtrele
        var suitableRooms = roomPool.Where(r => 
            r.connectionType == requiredType && 
            !r.isStartRoom && 
            !r.isEndRoom).ToArray();

        if (suitableRooms.Length == 0)
        {
            return false;
        }

        // Rastgele bir oda seç ve yerleştir
        RoomData selectedRoom = suitableRooms[Random.Range(0, suitableRooms.Length)];
        GameObject newRoom = SpawnRoom(selectedRoom, checkPos);
        
        if (newRoom != null)
        {
            spawnedRooms.Add(newRoom);
            MoveGeneratorPoint();
            return true;
        }

        return false;
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
        if (roomData == null || roomData.roomPrefab == null) return null;

        GameObject room = Instantiate(roomData.roomPrefab, position, Quaternion.identity);
        
        // Oda rengini ayarla
        var spriteRenderer = room.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = roomData.roomColor;
        }

        // Odayı başlat
        var initializer = room.GetComponent<RoomInitializer>();
        if (initializer != null)
        {
            initializer.InitializeRoom(roomData);
        }

        return room;
    }
}
