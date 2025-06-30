using UnityEngine;

public class LevelCameraManager : MonoBehaviour
{
    public static LevelCameraManager Instance;

    private LevelCamera[] allCameras;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        allCameras = Object.FindObjectsByType<LevelCamera>(FindObjectsSortMode.None); // Sahnedeki tüm kameraları al
    }

    public void SetActiveCamera(LevelCamera targetCam, Transform player)
    {
        foreach (var cam in allCameras)
        {
            cam.EnableCamera(false);
        }

        targetCam.Activate(player);
    }
}
