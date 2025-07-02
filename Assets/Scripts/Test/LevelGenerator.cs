using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    public LevelRoomData roomData;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    public int distancetoEnd = 10;

    [Header("Generation Settings")]
    public Transform generatorPoint;
    public float xOffset = 18f;
    public float yOffset = 10f;
    public LayerMask whatIsRoom;

    private List<GameObject> allRooms = new List<GameObject>();
    private List<GameObject> generatedRooms = new List<GameObject>();
    private GameObject endRoom;
    private GameObject startRoom;

    public enum Direction { up, right, down, left }
    private Direction selectedDirection;

    private void Start()
    {
        if (!ValidateRoomData()) return;
        GenerateLevel();
    }

    private bool ValidateRoomData()
    {
        if (roomData == null)
        {
            Debug.LogError("LevelRoomData is not assigned!");
            return false;
        }

        if (roomData.startRoom == null || roomData.startRoom.roomPrefab == null)
        {
            Debug.LogError("Start room or its prefab is not assigned in LevelRoomData!");
            return false;
        }

        if (roomData.endRoom == null || roomData.endRoom.roomPrefab == null)
        {
            Debug.LogError("End room or its prefab is not assigned in LevelRoomData!");
            return false;
        }

        return true;
    }

    private void GenerateLevel()
    {
        // Başlangıç odasını oluştur
        GameObject startRoomInstance = Instantiate(roomData.startRoom.roomPrefab, generatorPoint.position, Quaternion.identity);
        if (startRoomInstance != null)
        {
            startRoom = startRoomInstance;
            SetRoomColor(startRoom, startColor);
            allRooms.Add(startRoom);

            // RoomInitializer'ı başlat
            var startRoomInit = startRoom.GetComponent<RoomInitializer>();
            if (startRoomInit != null)
            {
                startRoomInit.InitializeRoom(roomData.startRoom);
            }
        }
        else
        {
            Debug.LogError("Failed to create start room!");
            return;
        }

        // Ara odaları oluştur
        Vector3 currentPos = generatorPoint.position;
        for (int i = 0; i < distancetoEnd - 1; i++)
        {
            selectedDirection = (Direction)Random.Range(0, 4);
            MoveGenerationPoint();

            // Çakışma kontrolü
            int attempts = 0;
            while (Physics2D.OverlapCircle(generatorPoint.position, .2f, whatIsRoom) && attempts < 10)
            {
                generatorPoint.position = currentPos;
                selectedDirection = (Direction)Random.Range(0, 4);
                MoveGenerationPoint();
                attempts++;
            }

            if (attempts < 10)
            {
                RoomData randomRoom = GetRandomNormalRoom();
                if (randomRoom != null)
                {
                    GameObject newRoom = SpawnRoom(randomRoom, generatorPoint.position);
                    if (newRoom != null)
                    {
                        allRooms.Add(newRoom);
                        currentPos = generatorPoint.position;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Failed to place room at attempt {i}");
                generatorPoint.position = currentPos;
            }
        }

        // Bitiş odasını oluştur
        selectedDirection = (Direction)Random.Range(0, 4);
        MoveGenerationPoint();

        // Bitiş odası için özel çakışma kontrolü
        int endRoomAttempts = 0;
        while (Physics2D.OverlapCircle(generatorPoint.position, .2f, whatIsRoom) && endRoomAttempts < 20)
        {
            generatorPoint.position = currentPos;
            selectedDirection = (Direction)Random.Range(0, 4);
            MoveGenerationPoint();
            endRoomAttempts++;
        }

        if (endRoomAttempts < 20)
        {
            GameObject endRoomInstance = Instantiate(roomData.endRoom.roomPrefab, generatorPoint.position, Quaternion.identity);
            if (endRoomInstance != null)
            {
                endRoom = endRoomInstance;
                SetRoomColor(endRoom, endColor);

                // RoomInitializer'ı başlat
                var endRoomInit = endRoom.GetComponent<RoomInitializer>();
                if (endRoomInit != null)
                {
                    endRoomInit.InitializeRoom(roomData.endRoom);
                }
            }
            else
            {
                Debug.LogError("Failed to create end room!");
                return;
            }
        }
        else
        {
            Debug.LogError("Could not find a valid position for the end room!");
            return;
        }

        // Odaları bağla
        CreateRoomOutline(startRoom.transform.position);
        foreach (GameObject room in allRooms)
        {
            CreateRoomOutline(room.transform.position);
        }
        if (endRoom != null)
        {
            CreateRoomOutline(endRoom.transform.position);
        }
    }

    private RoomData GetRandomNormalRoom()
    {
        // Tüm normal oda listelerini kontrol et ve rastgele bir oda seç
        List<List<RoomData>> allRoomLists = new List<List<RoomData>>
        {
            roomData.singleUpRooms,
            roomData.singleDownRooms,
            roomData.singleRightRooms,
            roomData.singleLeftRooms,
            roomData.doubleUpDownRooms,
            roomData.doubleLeftRightRooms,
            roomData.doubleUpLeftRooms,
            roomData.doubleUpRightRooms,
            roomData.doubleDownLeftRooms,
            roomData.doubleDownRightRooms,
            roomData.tripleUpRightDownRooms,
            roomData.tripleUpLeftDownRooms,
            roomData.tripleDownRightLeftRooms,
            roomData.tripleUpLeftRightRooms,
            roomData.fourWayRooms
        };

        // Boş olmayan listeleri bul
        List<List<RoomData>> nonEmptyLists = allRoomLists.FindAll(list => list != null && list.Count > 0);

        if (nonEmptyLists.Count == 0)
        {
            Debug.LogError("No normal rooms available in LevelRoomData!");
            return null;
        }

        // Rastgele bir liste seç
        List<RoomData> selectedList = nonEmptyLists[Random.Range(0, nonEmptyLists.Count)];
        // Seçilen listeden rastgele bir oda seç
        return selectedList[Random.Range(0, selectedList.Count)];
    }

    private void SetRoomColor(GameObject room, Color color)
    {
        var spriteRenderer = room.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    private GameObject SpawnRoom(RoomData data, Vector3 position)
    {
        if (data == null || data.roomPrefab == null) return null;
        
        GameObject room = Instantiate(data.roomPrefab, position, Quaternion.identity);
        if (room != null)
        {
            var initializer = room.GetComponent<RoomInitializer>();
            if (initializer != null)
            {
                initializer.InitializeRoom(data);
            }
        }
        return room;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void MoveGenerationPoint()
    {
        switch (selectedDirection)
        {
            case Direction.up:
                generatorPoint.position += new Vector3(0, yOffset, 0);
                break;
            case Direction.down:
                generatorPoint.position += new Vector3(0, -yOffset, 0);
                break;
            case Direction.right:
                generatorPoint.position += new Vector3(xOffset, 0, 0);
                break;
            case Direction.left:
                generatorPoint.position += new Vector3(-xOffset, 0, 0);
                break;
        }
    }

    public void CreateRoomOutline(Vector3 roomPos)
    {
        bool roomAbove = Physics2D.OverlapCircle(roomPos + new Vector3(0, yOffset, 0), .2f, whatIsRoom);
        bool roomBelow = Physics2D.OverlapCircle(roomPos + new Vector3(0, -yOffset, 0), .2f, whatIsRoom);
        bool roomLeft = Physics2D.OverlapCircle(roomPos + new Vector3(-xOffset, 0, 0), .2f, whatIsRoom);
        bool roomRight = Physics2D.OverlapCircle(roomPos + new Vector3(xOffset, 0, 0), .2f, whatIsRoom);

        int directionCount = 0;
        if (roomAbove) directionCount++;
        if (roomBelow) directionCount++;
        if (roomLeft) directionCount++;
        if (roomRight) directionCount++;

        RoomData selectedRoom = null;

        switch (directionCount)
        {
            case 0:
                Debug.LogError("No room detected");
                break;

            case 1:
                if (roomAbove && roomData.singleUpRooms.Count > 0)
                    selectedRoom = roomData.singleUpRooms[Random.Range(0, roomData.singleUpRooms.Count)];
                else if (roomBelow && roomData.singleDownRooms.Count > 0)
                    selectedRoom = roomData.singleDownRooms[Random.Range(0, roomData.singleDownRooms.Count)];
                else if (roomLeft && roomData.singleLeftRooms.Count > 0)
                    selectedRoom = roomData.singleLeftRooms[Random.Range(0, roomData.singleLeftRooms.Count)];
                else if (roomRight && roomData.singleRightRooms.Count > 0)
                    selectedRoom = roomData.singleRightRooms[Random.Range(0, roomData.singleRightRooms.Count)];
                break;

            case 2:
                if (roomAbove && roomBelow && roomData.doubleUpDownRooms.Count > 0)
                    selectedRoom = roomData.doubleUpDownRooms[Random.Range(0, roomData.doubleUpDownRooms.Count)];
                else if (roomLeft && roomRight && roomData.doubleLeftRightRooms.Count > 0)
                    selectedRoom = roomData.doubleLeftRightRooms[Random.Range(0, roomData.doubleLeftRightRooms.Count)];
                else if (roomAbove && roomLeft && roomData.doubleUpLeftRooms.Count > 0)
                    selectedRoom = roomData.doubleUpLeftRooms[Random.Range(0, roomData.doubleUpLeftRooms.Count)];
                else if (roomAbove && roomRight && roomData.doubleUpRightRooms.Count > 0)
                    selectedRoom = roomData.doubleUpRightRooms[Random.Range(0, roomData.doubleUpRightRooms.Count)];
                else if (roomBelow && roomLeft && roomData.doubleDownLeftRooms.Count > 0)
                    selectedRoom = roomData.doubleDownLeftRooms[Random.Range(0, roomData.doubleDownLeftRooms.Count)];
                else if (roomBelow && roomRight && roomData.doubleDownRightRooms.Count > 0)
                    selectedRoom = roomData.doubleDownRightRooms[Random.Range(0, roomData.doubleDownRightRooms.Count)];
                break;

            case 3:
                if (roomAbove && roomBelow && roomRight && roomData.tripleUpRightDownRooms.Count > 0)
                    selectedRoom = roomData.tripleUpRightDownRooms[Random.Range(0, roomData.tripleUpRightDownRooms.Count)];
                else if (roomAbove && roomBelow && roomLeft && roomData.tripleUpLeftDownRooms.Count > 0)
                    selectedRoom = roomData.tripleUpLeftDownRooms[Random.Range(0, roomData.tripleUpLeftDownRooms.Count)];
                else if (roomBelow && roomLeft && roomRight && roomData.tripleDownRightLeftRooms.Count > 0)
                    selectedRoom = roomData.tripleDownRightLeftRooms[Random.Range(0, roomData.tripleDownRightLeftRooms.Count)];
                else if (roomAbove && roomLeft && roomRight && roomData.tripleUpLeftRightRooms.Count > 0)
                    selectedRoom = roomData.tripleUpLeftRightRooms[Random.Range(0, roomData.tripleUpLeftRightRooms.Count)];
                break;

            case 4:
                if (roomData.fourWayRooms.Count > 0)
                    selectedRoom = roomData.fourWayRooms[Random.Range(0, roomData.fourWayRooms.Count)];
                break;
        }

        if (selectedRoom != null)
        {
            GameObject newRoom = SpawnRoom(selectedRoom, roomPos);
            if (newRoom != null)
            {
                generatedRooms.Add(newRoom);
            }
        }
    }
}

[System.Serializable]
public class RoomPrefab
{
    public GameObject[] singleUp, singleDown, singleRight, singleLeft, doubleUpDown, doubleLeftRight, doubleUpLeft, doubleDownRight,doubleDownLeft,doubleUpRight,tripleUpRightDown,tripleUpLeftDown,tripleDownRightLeft,tripleUpLeftRight,fourway;
    
}
