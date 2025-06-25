using Unity.Cinemachine;
using UnityEngine;

public class LevelCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachine;

    private void Awake()
    {
        if (cinemachine == null)
            cinemachine = GetComponentInChildren<CinemachineCamera>(true);

        EnableCamera(false); // Başlangıçta kapalı
    }

    public void Activate(Transform target)
    {
        SnapToTarget(target);
        EnableCamera(true);
        SetNewTarget(target);
    }

    public void EnableCamera(bool enable)
    {
        cinemachine.gameObject.SetActive(enable);
    }

    public void SetNewTarget(Transform newTarget)
    {
        cinemachine.Follow = newTarget;
    }

    public void SnapToTarget(Transform newTarget)
    {
        cinemachine.Follow = newTarget;
        cinemachine.transform.position = new Vector3(
            newTarget.position.x,
            newTarget.position.y,
            cinemachine.transform.position.z
        );
    }
}
