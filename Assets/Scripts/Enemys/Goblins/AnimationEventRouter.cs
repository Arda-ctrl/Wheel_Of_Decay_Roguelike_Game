using UnityEngine;

// Put this on the Body (the GameObject that has the Animator)
public sealed class AnimationEventRouter : MonoBehaviour
{
	[SerializeField] private Transform target; // optional; defaults to parent

	private Transform ResolveTarget()
	{
		return target != null ? target : transform.parent;
	}

	// Animation Event: call this with stringParameter = method name on parent
	public void InvokeOnParent(string methodName)
	{
		var t = ResolveTarget();
		if (t == null || string.IsNullOrEmpty(methodName)) return;
		t.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
	}
}

