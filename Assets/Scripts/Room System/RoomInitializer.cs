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

    private void Start()
    {
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

        // Kapıları kilitle
        foreach (var door in doors)
        {
            door.LockDoor();
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
        foreach (var door in doors)
        {
            door.UnlockDoor();
        }

        Debug.Log("Tüm düşmanlar öldü. Kapılar açıldı!");
    }
}
