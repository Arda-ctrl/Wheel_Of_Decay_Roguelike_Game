using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public RoomData[] possibleRooms;

    private void Start()
    {
        SpawnRandomRoom(Vector3.zero); // Başlangıçta 0,0,0'a bir oda çağır
    }

    public void SpawnRandomRoom(Vector3 position)
    {
        if (possibleRooms.Length == 0)
        {
            Debug.LogError("RoomSpawner: No rooms assigned!");
            return;
        }

        RoomData roomData = possibleRooms[Random.Range(0, possibleRooms.Length)];
        GameObject roomGO = Instantiate(roomData.roomPrefab, position, Quaternion.identity);

        RoomInitializer initializer = roomGO.GetComponent<RoomInitializer>();
        if (initializer != null)
        {
            initializer.InitializeRoom(roomData);
        }
    }
}
