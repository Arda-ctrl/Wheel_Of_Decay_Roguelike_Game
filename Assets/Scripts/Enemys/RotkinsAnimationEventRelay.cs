using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class RotkinsAnimationEventRelay : MonoBehaviour
{
    private TankRotkinsController controller;

    private void Awake()
    {
        controller = GetComponentInParent<TankRotkinsController>();
        if (controller == null)
        {
            Debug.LogWarning("[RotkinsAnimationEventRelay] Parent TankRotkinsController not found. Animation events will be ignored.");
        }
    }

    // Called by Animation Event on the Attack clip (impact frame)
    public void OnBranchHit()
    {
        controller?.OnBranchHit();
    }

    // Called by Animation Event on the Area/Charge clip (impact frame)
    public void OnAreaHit()
    {
        controller?.OnAreaHit();
    }
}


