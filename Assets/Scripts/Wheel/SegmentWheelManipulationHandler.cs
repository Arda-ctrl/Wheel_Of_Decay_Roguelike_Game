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

    // Repulsor: Komşu mesafesi parametreli, iğneyi bir slot ileri/geri iter
    public class RepulsorEffect : IWheelEffect
    {
        private int range;
        public RepulsorEffect(int range)
        {
            this.range = Mathf.Max(1, range);
        }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            for (int i = 1; i <= range; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (landedSlot == left)
                {
                    moveNeedleToSlot?.Invoke((left - 1 + slotCount) % slotCount);
                    destroySelf?.Invoke();
                }
                else if (landedSlot == right)
                {
                    moveNeedleToSlot?.Invoke((right + 1) % slotCount);
                    destroySelf?.Invoke();
                }
            }
        }
    }

    // MirrorRedirect: Tam karşısındaki slota iğne gelirse, iğneyi kendi slotuna çeker
    public class MirrorRedirectEffect : IWheelEffect
    {
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            int mirrorSlot = (mySlot + slotCount / 2) % slotCount;
            if (landedSlot == mirrorSlot)
            {
                moveNeedleToSlot?.Invoke(mySlot);
                destroySelf?.Invoke();
            }
        }
    }

    // CommonRedirector: Yanındaki slota iğne gelirse, iğneyi rastgele bir common segmente yönlendirir
    public class CommonRedirectorEffect : IWheelEffect
    {
        private int range;
        private Rarity minRarity;
        private Rarity maxRarity;
        public CommonRedirectorEffect(int range, Rarity minRarity, Rarity maxRarity)
        {
            this.range = Mathf.Max(1, range);
            this.minRarity = minRarity;
            this.maxRarity = maxRarity;
        }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            // Belirtilen menzildeki slotlara bak
            for (int i = 1; i <= range; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (landedSlot == left || landedSlot == right)
                {
                    var wheelManager = Object.FindAnyObjectByType<WheelManager>();
                    if (wheelManager == null) return;
                    var slots = wheelManager.slots;
                    var validSlots = new System.Collections.Generic.List<int>();
                    for (int j = 0; j < slots.Length; j++)
                    {
                        foreach (Transform child in slots[j])
                        {
                            var inst = child.GetComponent<SegmentInstance>();
                            if (inst != null && inst.data != null && inst.data.type == Type.StatBoost)
                            {
                                var rarity = inst.data.rarity;
                                if (rarity >= minRarity && rarity <= maxRarity)
                                {
                                    validSlots.Add(j);
                                }
                            }
                        }
                    }
                    // Kendi koruduğu segmenti hariç tut
                    validSlots.Remove(mySlot);
                    if (validSlots.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, validSlots.Count);
                        int targetSlot = validSlots[randomIndex];
                        moveNeedleToSlot?.Invoke(targetSlot);
                        destroySelf?.Invoke();
                    }
                    break;
                }
            }
        }
    }

    // SafeEscape: Belirtilen menzildeki slotlara iğne gelirse, o segmenti rastgele boş bir slota ışınlar
    public class SafeEscapeEffect : IWheelEffect
    {
        private int range;
        public SafeEscapeEffect(int range)
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
                    var wheelManager = Object.FindAnyObjectByType<WheelManager>();
                    if (wheelManager == null) return;
                    // Hedef segmenti bul (iğnenin geldiği slot)
                    SegmentInstance targetInstance = null;
                    foreach (Transform child in wheelManager.slots[landedSlot])
                    {
                        var inst = child.GetComponent<SegmentInstance>();
                        if (inst != null && inst.data != null)
                        {
                            targetInstance = inst;
                            break;
                        }
                    }
                    if (targetInstance == null) return;
                    // Boş slotları bul
                    var emptySlots = new System.Collections.Generic.List<int>();
                    for (int j = 0; j < wheelManager.slots.Length; j++)
                    {
                        if (!wheelManager.slotOccupied[j])
                        {
                            emptySlots.Add(j);
                        }
                    }
                    // Hedef slotu hariç tut
                    emptySlots.Remove(landedSlot);
                    if (emptySlots.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, emptySlots.Count);
                        int targetSlot = emptySlots[randomIndex];
                        // Segmenti yeni slota taşı
                        targetInstance.transform.SetParent(wheelManager.slots[targetSlot], false);
                        targetInstance.transform.localPosition = Vector3.zero;
                        targetInstance.startSlotIndex = targetSlot;
                        // SlotOccupied güncelle
                        wheelManager.slotOccupied[landedSlot] = false;
                        wheelManager.slotOccupied[targetSlot] = true;
                    }
                    break;
                }
            }
        }
    }

    public class ExplosiveEscapeEffect : IWheelEffect
    {
        private int range;
        public ExplosiveEscapeEffect(int range)
        {
            this.range = Mathf.Max(1, range);
        }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            bool shouldExplode = (landedSlot == mySlot);
            for (int i = 1; i <= range && !shouldExplode; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (landedSlot == left || landedSlot == right)
                    shouldExplode = true;
            }
            if (!shouldExplode) return;

            var wheelManager = Object.FindAnyObjectByType<WheelManager>();
            if (wheelManager == null) return;

            var affectedSlots = new System.Collections.Generic.List<int>();
            for (int i = 1; i <= range; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (!affectedSlots.Contains(left)) affectedSlots.Add(left);
                if (!affectedSlots.Contains(right)) affectedSlots.Add(right);
            }

            var emptySlots = new System.Collections.Generic.List<int>();
            for (int j = 0; j < wheelManager.slots.Length; j++)
            {
                if (!wheelManager.slotOccupied[j])
                    emptySlots.Add(j);
            }
            foreach (int slotIdx in affectedSlots)
            {
                SegmentInstance targetInstance = null;
                foreach (Transform child in wheelManager.slots[slotIdx])
                {
                    var inst = child.GetComponent<SegmentInstance>();
                    if (inst != null && inst.data != null)
                    {
                        targetInstance = inst;
                        break;
                    }
                }
                if (targetInstance == null) continue;
                emptySlots.Remove(slotIdx);
                if (emptySlots.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, emptySlots.Count);
                    int targetSlot = emptySlots[randomIndex];
                    targetInstance.transform.SetParent(wheelManager.slots[targetSlot], false);
                    targetInstance.transform.localPosition = Vector3.zero;
                    targetInstance.startSlotIndex = targetSlot;
                    wheelManager.slotOccupied[slotIdx] = false;
                    wheelManager.slotOccupied[targetSlot] = true;
                    emptySlots.Remove(targetSlot);
                }
            }
            destroySelf?.Invoke();
        }
    }
} 