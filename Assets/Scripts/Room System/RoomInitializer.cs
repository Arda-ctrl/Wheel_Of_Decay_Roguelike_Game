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

        // Düşmanları spawn et
        for (int i = 0; i < totalEnemies; i++)
        {
            Vector3 spawnPos;

            if (enemySpawnPoints != null && enemySpawnPoints.Length > 0)
            {
                spawnPos = enemySpawnPoints[i % enemySpawnPoints.Length].position;
            }
            else
            {
                spawnPos = transform.position + new Vector3(i * 2, 0f, 0f); // fallback pozisyon
            }

            GameObject enemyGO = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyController enemy = enemyGO.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Init(this);
            }
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
