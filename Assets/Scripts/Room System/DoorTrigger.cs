using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPointInNextRoom;
    [SerializeField] private LevelCamera targetRoomCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // 1. Oyuncuyu ışınla
        collision.transform.position = playerSpawnPointInNextRoom.position;

        // 2. Tüm kameraları kapat, sadece bu kamerayı aktif et
        LevelCameraManager.Instance.SetActiveCamera(targetRoomCamera, collision.transform);
    }
}
