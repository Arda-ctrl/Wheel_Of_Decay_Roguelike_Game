using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public RoomData[] roomPool; // Random seçeceğimiz odalar
    public int numberOfRooms = 5;
    public float roomSpacing = 20f; // Çok çakışma olursa arttırabilirsin

    private List<RoomInteractive> spawnedRooms = new List<RoomInteractive>();

    void Start()
    {
        GenerateRooms();
    }

    void GenerateRooms()
    {
        // İlk odayı yerleştir
        RoomData firstRoom = roomPool[Random.Range(0, roomPool.Length)];
        GameObject firstGO = Instantiate(firstRoom.roomPrefab, Vector3.zero, Quaternion.identity);
        RoomInteractive firstInteractive = firstGO.GetComponent<RoomInteractive>();
        spawnedRooms.Add(firstInteractive);

        // Diğer odaları sırayla bağla
        for (int i = 1; i < numberOfRooms; i++)
        {
            SpawnNextRoom();
        }
    }

    void SpawnNextRoom()
    {
        Debug.Log("Yeni oda spawn etmeye çalışılıyor.");
        // Mevcut odalardan bağlantısı boştan birini bul
        foreach (RoomInteractive existingRoom in spawnedRooms)
        {
            RoomConnectionPoint fromPoint = existingRoom.GetAvailableConnection();
            if (fromPoint == null) continue;

            // Yeni bir oda seç
            RoomData newRoomData = roomPool[Random.Range(0, roomPool.Length)];
            GameObject newRoomGO = Instantiate(newRoomData.roomPrefab);
            RoomInteractive newRoomInteractive = newRoomGO.GetComponent<RoomInteractive>();

            // Yeni odadan bağlantı noktası bul (eşleşen yönü)
            RoomConnectionPoint toPoint = FindCompatibleConnection(fromPoint.direction, newRoomInteractive);
            if (toPoint == null)
            {
                 Debug.Log("Bağlantı bulunamadı, yeni oda silindi.");
                Destroy(newRoomGO);
                continue; // başka bağlantı deneriz
            }

            // Pozisyonu ayarla (kapılar hizalanacak şekilde)
            Vector3 offset = fromPoint.transform.position - toPoint.transform.localPosition;
            newRoomGO.transform.position = offset;

            // Bağlantıyı işaretle
            fromPoint.isOccupied = true;
            toPoint.isOccupied = true;

            fromPoint.connectedTo = toPoint;
            toPoint.connectedTo = fromPoint;

            DoorTrigger doorA = fromPoint.GetComponent<DoorTrigger>();
            if (doorA != null)
            {
                doorA.connectionPoint = fromPoint;
            }

            DoorTrigger doorB = toPoint.GetComponent<DoorTrigger>();
            if (doorB != null)
            {
                doorB.connectionPoint = toPoint;
            }
            spawnedRooms.Add(newRoomInteractive);

            RoomInitializer initializer = newRoomGO.GetComponent<RoomInitializer>();
            if (initializer != null)
                initializer.InitializeRoom(newRoomData); // DÜŞMANLARI ODA İÇİNDE BAŞLAT
            return;
        }

        Debug.LogWarning("Yeni oda yerleştirilemedi, bağlantı bulunamadı.");
    }

    RoomConnectionPoint FindCompatibleConnection(ConnectionDirection targetDirection, RoomInteractive room)
    {
        
        // Hedef yönün zıttı lazım
        ConnectionDirection neededDirection = GetOppositeDirection(targetDirection);
        foreach (var point in room.connectionPoints)
        {
            if (!point.isOccupied && point.direction == neededDirection)
                return point;
        }
        return null;
    }

    ConnectionDirection GetOppositeDirection(ConnectionDirection dir)
    {
        switch (dir)
        {
            case ConnectionDirection.North: return ConnectionDirection.South;
            case ConnectionDirection.South: return ConnectionDirection.North;
            case ConnectionDirection.East: return ConnectionDirection.West;
            case ConnectionDirection.West: return ConnectionDirection.East;
            default: return dir;
        }
    }
}
