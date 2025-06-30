using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class RoomGenerator : MonoBehaviour
{
    public RoomData[] roomPool; // Random seçeceğimiz odalar
    public int numberOfRooms = 5;
    public float roomSpacing = 20f; // Çok çakışma olursa arttırabilirsin
    public int maxAttempts = 3; // Bir odayı yerleştirmek için maksimum deneme sayısı

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
        int currentRooms = 1;
        int failedAttempts = 0;

        while (currentRooms < numberOfRooms && failedAttempts < maxAttempts * numberOfRooms)
        {
            if (SpawnNextRoom())
            {
                currentRooms++;
                failedAttempts = 0;
            }
            else
            {
                failedAttempts++;
            }
        }

        if (currentRooms < numberOfRooms)
        {
            Debug.LogWarning($"Sadece {currentRooms} oda oluşturulabildi. İstenen: {numberOfRooms}");
        }
    }

    bool SpawnNextRoom()
    {
        Debug.Log("Yeni oda spawn etmeye çalışılıyor.");
        
        // Mevcut odaları kontrol et ve yok edilmiş olanları listeden çıkar
        spawnedRooms.RemoveAll(room => room == null);
        
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

            // Kısa bir süre bekleyip odanın yok edilip edilmediğini kontrol et
            StartCoroutine(CheckRoomDestroyed(newRoomInteractive));
            
            return true;
        }

        Debug.LogWarning("Yeni oda yerleştirilemedi, bağlantı bulunamadı.");
        return false;
    }

    System.Collections.IEnumerator CheckRoomDestroyed(RoomInteractive room)
    {
        yield return new WaitForSeconds(0.1f);
        
        // Eğer oda yok edildiyse, yeni bir oda oluşturmayı dene
        if (room == null)
        {
            SpawnNextRoom();
        }
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
