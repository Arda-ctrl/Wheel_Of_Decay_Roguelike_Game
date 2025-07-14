using UnityEngine;

public class SegmentWheelManipulationHandler : MonoBehaviour
{
    public static SegmentWheelManipulationHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public interface IWheelEffect
    {
        void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf);
    }

    // BlackHole: Komşu mesafesi parametreli
    public class BlackHoleEffect : IWheelEffect
    {
        private int range;
        public BlackHoleEffect(int range)
        {
            this.range = Mathf.Max(1, range);
        }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            for (int i = 1; i <= range; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (landedSlot == left || landedSlot == right)
                {
                    moveNeedleToSlot?.Invoke(mySlot);
                    destroySelf?.Invoke();
                }
            }
        }
    }

    // Redirector: Çift yön desteği
    public class RedirectorEffect : IWheelEffect
    {
        public RedirectDirection direction;
        public RedirectorEffect(RedirectDirection dir) { direction = dir; }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            if (direction == RedirectDirection.LeftToRight && landedSlot == (mySlot - 1 + slotCount) % slotCount)
            {
                moveNeedleToSlot?.Invoke((mySlot + 1) % slotCount);
            }
            else if (direction == RedirectDirection.RightToLeft && landedSlot == (mySlot + 1) % slotCount)
            {
                moveNeedleToSlot?.Invoke((mySlot - 1 + slotCount) % slotCount);
            }
            else if (direction == RedirectDirection.BothSides)
            {
                if (landedSlot == (mySlot - 1 + slotCount) % slotCount)
                {
                    moveNeedleToSlot?.Invoke((mySlot + 1) % slotCount);
                }
                else if (landedSlot == (mySlot + 1) % slotCount)
                {
                    moveNeedleToSlot?.Invoke((mySlot - 1 + slotCount) % slotCount);
                }
            }
        }
    }
} 