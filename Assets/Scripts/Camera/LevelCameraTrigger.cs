using UnityEngine;

public class LevelCameraTrigger : MonoBehaviour
{
    [SerializeField] private LevelCamera levelCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        levelCamera.EnableCamera(true);
        levelCamera.SetNewTarget(collision.transform);
    }
}

