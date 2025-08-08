using UnityEngine;

public class RoomInitializer : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform[] enemySpawnPoints;

    [Header("Door Settings")]
    public RoomDoor[] doors;

    private int totalEnemies;
    private int enemiesRemaining;

    private RoomData currentRoomData;

    // Public property to access the current room data
    public RoomData RoomData
    {
        get { return currentRoomData; }
    }

    private void Start()
    {
        // Eğer doors dizisi null ise, log mesajı göster ve devam et
        if (doors == null || doors.Length == 0)
        {
            Debug.LogWarning("No doors found in room " + gameObject.name + ". Skipping door operations.");
        }
        
        // Düşman yoksa kapıları aç
        if (totalEnemies == 0 && enemiesRemaining == 0)
        {
            Debug.Log("Failsafe: Kapılar baştan açılıyor (Start()).");
            OpenDoors();
        }
    }

    public void InitializeRoom(RoomData data)
    {
        currentRoomData = data;

        // Düşmanları spawn et
        if (data.possibleEnemies != null && data.possibleEnemies.Length > 0)
        {
            SpawnEnemies(data.minEnemyCount);
        }

        // Oda rengini ayarla
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = data.roomColor;
        }

        // Diğer oda özelliklerini ayarla
        gameObject.tag = "Room";

        // Düşman sayısını belirle
        totalEnemies = Random.Range(data.minEnemyCount, data.maxEnemyCount + 1);
        enemiesRemaining = totalEnemies;

        // Düşman yoksa kapıları hemen aç
        if (totalEnemies <= 0)
        {
            OpenDoors();
            return;
        }

        // Kapıları kilitle - null kontrolü ekle
        if (doors != null && doors.Length > 0)
        {
            foreach (var door in doors)
            {
                if (door != null)
                {
                    door.LockDoor();
                }
                else
                {
                    Debug.LogWarning("Null door found in room " + gameObject.name);
                }
            }
        }
        else
        {
            Debug.LogWarning("No doors found in room " + gameObject.name + ". Make sure doors array is assigned.");
        }
    }

    private void SpawnEnemies(int count)
    {
        if (currentRoomData == null || currentRoomData.possibleEnemies == null || currentRoomData.possibleEnemies.Length == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            // Rastgele bir düşman seç
            GameObject enemyPrefab = currentRoomData.possibleEnemies[Random.Range(0, currentRoomData.possibleEnemies.Length)];
            
            // Odanın içinde rastgele bir pozisyon bul
            Vector2 randomPosition = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            
            // Düşmanı spawn et
            Instantiate(enemyPrefab, randomPosition, Quaternion.identity, transform);
        }
    }

    public void EnemyKilled()
    {
        enemiesRemaining--;

        if (enemiesRemaining <= 0)
        {
            OpenDoors();
        }
    }

    private void OpenDoors()
    {
        // Kapılar null ise veya boşsa, hata verme
        if (doors == null || doors.Length == 0)
        {
            Debug.LogWarning("No doors found in room " + gameObject.name);
            return;
        }
        
        Debug.Log($"Opening {doors.Length} doors in room {gameObject.name}");
        int openedDoors = 0;
        
        foreach (var door in doors)
        {
            // Her kapı için null kontrolü yap
            if (door != null)
            {
                door.UnlockDoor();
                openedDoors++;
            }
            else
            {
                Debug.LogWarning($"Null door reference in room {gameObject.name}");
            }
        }

        Debug.Log($"Opened {openedDoors} doors out of {doors.Length} in room {gameObject.name}");
        
        // If this is an end room, notify MapRoomIntegrator
        if (currentRoomData != null && currentRoomData.isEndRoom)
        {
            Debug.Log("End room completed!");
            
            // Find MapRoomIntegrator and notify it
            MapRoomIntegrator integrator = FindFirstObjectByType<MapRoomIntegrator>();
            if (integrator != null)
            {
                integrator.ReturnToMap(true);
            }
        }
        
        // Notify any listening components that the room is completed
        OnRoomCompleted();
    }
    
    // Event for room completion
    public delegate void RoomCompletedHandler(RoomData roomData);
    public static event RoomCompletedHandler OnRoomCompletedEvent;
    
    private void OnRoomCompleted()
    {
        if (OnRoomCompletedEvent != null)
        {
            OnRoomCompletedEvent(currentRoomData);
        }
    }
    
    // Notify minimap when player enters this room
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Get room position from transform
            Vector2Int roomPos = Vector2Int.RoundToInt(transform.position / new Vector2(18f, 10f));
            
            // Notify DungeonMinimap
            var roomEnteredEvent = DungeonMinimap.OnRoomEntered;
            if (roomEnteredEvent != null)
            {
                roomEnteredEvent(roomPos);
            }
        }
    }
}
