using UnityEngine;

// SlotController: Her slotun tıklanabilirliğini ve WheelManager ile iletişimini sağlar.
public class SlotController : MonoBehaviour
{
	public int slotIndex;
	public WheelManager wheelManager;

	private void OnMouseDown()
	{
		wheelManager?.OnSlotClicked(slotIndex);
	}
}