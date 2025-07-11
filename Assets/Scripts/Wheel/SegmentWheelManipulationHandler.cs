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
        // landedSlot: iğnenin geldiği slot
        // mySlot: bu segmentin başladığı slot
        // slotCount: toplam slot sayısı
        // moveNeedleToSlot: iğneyi başka slota götürmek için
        // destroySelf: segmenti yok etmek için
        void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf);
    }

    // BlackHole örnek implementasyonu
    public class BlackHoleEffect : IWheelEffect
    {
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            // Komşu slotlara gelindiyse iğneyi bu slota çek ve segmenti yok et
            for (int i = -1; i <= 1; i += 2)
            {
                int neighborSlot = (mySlot + i + slotCount) % slotCount;
                if (landedSlot == neighborSlot)
                {
                    moveNeedleToSlot?.Invoke(mySlot);
                    destroySelf?.Invoke();
                }
            }
        }
    }

    // Redirector örnek implementasyonu
    public class RedirectorEffect : IWheelEffect
    {
        public RedirectDirection direction;
        public RedirectorEffect(RedirectDirection dir) { direction = dir; }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            if (direction == RedirectDirection.LeftToRight && landedSlot == (mySlot - 1 + slotCount) % slotCount)
            {
                moveNeedleToSlot?.Invoke(mySlot + 1);
            }
            else if (direction == RedirectDirection.RightToLeft && landedSlot == (mySlot + 1) % slotCount)
            {
                moveNeedleToSlot?.Invoke(mySlot - 1);
            }
        }
    }
} 