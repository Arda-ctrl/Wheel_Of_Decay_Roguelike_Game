using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    private Collider2D doorCollider;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    public void LockDoor()
    {
        if (doorCollider != null)
            doorCollider.enabled = true;
    }

    public void UnlockDoor()
    {
        // Kapıyı tamamen yok et
        Destroy(gameObject);
    }
}
