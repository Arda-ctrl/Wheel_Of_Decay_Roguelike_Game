using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [HideInInspector] public RoomConnectionPoint connectionPoint;

    [Header("Işınlama Ayarları")]
    public Transform spawnPoint; // 🔁 Artık Inspector'dan atanabilir

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (connectionPoint == null || connectionPoint.connectedTo == null)
        {
            Debug.LogWarning("Kapı bağlantısı eksik!");
            return;
        }

        // Bağlı kapının DoorTrigger’ını al
        DoorTrigger otherDoor = connectionPoint.connectedTo.GetComponent<DoorTrigger>();
        if (otherDoor == null || otherDoor.spawnPoint == null)
        {
            Debug.LogWarning("Bağlı kapının DoorTrigger veya spawnPoint'i eksik!");
            return;
        }

        // Oyuncuyu bağlı kapının spawnPoint’ine ışınla
        collision.transform.position = otherDoor.spawnPoint.position;
        Debug.Log("Oyuncu ışınlandı → " + otherDoor.spawnPoint.position);
    }
}
