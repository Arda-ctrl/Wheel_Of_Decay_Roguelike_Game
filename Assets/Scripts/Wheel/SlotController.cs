using UnityEngine;

public class SlotController : MonoBehaviour
{
    public int slotIndex;
    public WheelManager wheelManager;
    private void OnMouseDown() { wheelManager?.OnSlotClicked(slotIndex); }
}