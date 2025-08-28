using UnityEngine;
using System.Collections.Generic;

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

    // Utility ve factory fonksiyonları
    public static IWheelEffect CreateWheelEffect(SegmentData data)
    {
        switch (data.wheelManipulationType)
        {
            case WheelManipulationType.BlackHole:
                return new BlackHoleEffect(data.blackHoleRange);
            case WheelManipulationType.Redirector:
                return new RedirectorEffect(data.redirectDirection);
            case WheelManipulationType.Repulsor:
                return new RepulsorEffect(data.repulsorRange);
            case WheelManipulationType.MirrorRedirect:
                return new MirrorRedirectEffect();
            case WheelManipulationType.ReverseMirrorRedirect:
                return new ReverseMirrorRedirectEffect(data.reverseMirrorRedirectRange);
            case WheelManipulationType.CommonRedirector:
                return new CommonRedirectorEffect(data.commonRedirectorRange, data.commonRedirectorMinRarity, data.commonRedirectorMaxRarity);
            case WheelManipulationType.SafeEscape:
                return new SafeEscapeEffect(data.safeEscapeRange);
            case WheelManipulationType.ExplosiveEscape:
                return new ExplosiveEscapeEffect(data.explosiveEscapeRange);
            case WheelManipulationType.SegmentSwapper:
                return new SegmentSwapperEffect(data.swapperRange);

            default:
                return null;
        }
    }

    public static SegmentInstance FindSegmentCoveringSlot(WheelManager wheelManager, int slot)
    {
        for (int i = 0; i < wheelManager.slots.Length; i++)
        {
            foreach (Transform child in wheelManager.slots[i])
            {
                var inst = child.GetComponent<SegmentInstance>();
                if (inst != null && inst.data != null)
                {
                    int start = inst.startSlotIndex;
                    int size = inst.data.size;
                    for (int s = 0; s < size; s++)
                    {
                        int coveredSlot = (start + s) % wheelManager.slots.Length;
                        if (coveredSlot == slot)
                            return inst;
                    }
                }
            }
        }
        return null;
    }

    public static List<SegmentInstance> GetAllSegments(WheelManager wheelManager)
    {
        var list = new List<SegmentInstance>();
        for (int i = 0; i < wheelManager.slots.Length; i++)
        {
            foreach (Transform child in wheelManager.slots[i])
            {
                var inst = child.GetComponent<SegmentInstance>();
                if (inst != null && inst.data != null && !list.Contains(inst))
                    list.Add(inst);
            }
        }
        return list;
    }

    public static void MoveSegmentToSlot(WheelManager wheelManager, SegmentInstance segment, int newStartSlot)
    {
        int oldStart = segment.startSlotIndex;
        int size = segment.data.size;
        segment.transform.SetParent(wheelManager.slots[newStartSlot], false);
        segment.transform.localPosition = Vector3.zero;
        segment.startSlotIndex = newStartSlot;
        for (int s = 0; s < size; s++)
        {
            int idx = (newStartSlot + s) % wheelManager.slots.Length;
            wheelManager.slotOccupied[idx] = true;
        }
        for (int s = 0; s < size; s++)
        {
            int idx = (oldStart + s) % wheelManager.slots.Length;
            wheelManager.slotOccupied[idx] = false;
        }
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
                }
                else if (landedSlot == right)
                {
                    moveNeedleToSlot?.Invoke((right + 1) % slotCount);
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
            }
        }
    }

    // ReverseMirrorRedirect: Yanındaki slotlara iğne gelirse, iğneyi karşısındaki slota yönlendirir
    public class ReverseMirrorRedirectEffect : IWheelEffect
    {
        private int range;
        public ReverseMirrorRedirectEffect(int range)
        {
            this.range = Mathf.Max(1, range);
        }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            // Kendi slotuna gelirse hiçbir şey yapma
            if (landedSlot == mySlot) return;
            
            // Belirtilen menzildeki slotlara bak
            for (int i = 1; i <= range; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (landedSlot == left || landedSlot == right)
                {
                    // Karşısındaki slota yönlendir
                    int oppositeSlot = (mySlot + slotCount / 2) % slotCount;
                    moveNeedleToSlot?.Invoke(oppositeSlot);
                    return;
                }
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
                            if (inst != null && inst.data != null)
                            {
                                var rarity = inst.data.rarity;
                                if (rarity >= minRarity && rarity <= maxRarity)
                                {
                                    validSlots.Add(j);
                                }
                            }
                        }
                    }
                    // Kendi slotunu ve range içindeki tüm slotları hariç tut
                    validSlots.Remove(mySlot);
                    for (int k = 1; k <= range; k++)
                    {
                        int leftSlot = (mySlot - k + slotCount) % slotCount;
                        int rightSlot = (mySlot + k) % slotCount;
                        validSlots.Remove(leftSlot);
                        validSlots.Remove(rightSlot);
                    }
                    
                    if (validSlots.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, validSlots.Count);
                        int targetSlot = validSlots[randomIndex];
                        moveNeedleToSlot?.Invoke(targetSlot);
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
            // 1. İğne etki alanında mı?
            bool inRange = false;
            for (int i = 1; i <= range && !inRange; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (landedSlot == left || landedSlot == right)
                    inRange = true;
            }
            if (!inRange) return;

            var wheelManager = Object.FindAnyObjectByType<WheelManager>();
            if (wheelManager == null) return;

            // 2. landedSlot'u kaplayan segmenti bul
            SegmentInstance targetInstance = SegmentWheelManipulationHandler.FindSegmentCoveringSlot(wheelManager, landedSlot);
            if (targetInstance == null) return;
            
            int segStart = targetInstance.startSlotIndex;
            int segSize = targetInstance.data.size;

            // 3. Boş slotları bul
            var validSlots = new System.Collections.Generic.List<int>();
            for (int j = 0; j < wheelManager.slots.Length; j++)
            {
                bool canPlace = true;
                for (int s = 0; s < segSize; s++)
                {
                    int idx = (j + s) % slotCount;
                    if (wheelManager.slotOccupied[idx] || idx == segStart)
                    {
                        canPlace = false;
                        break;
                    }
                }
                if (canPlace) validSlots.Add(j);
            }

            // 4. Yeni yere taşı (silmeden)
            if (validSlots.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, validSlots.Count);
                int targetSlot = validSlots[randomIndex];
                MoveSegmentToSlotWithoutDestroy(wheelManager, targetInstance, targetSlot);
            }
            else
            {
                // Hiç uygun yer yoksa segmenti yok et
                wheelManager.RemoveSegmentAtSlot(segStart);
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

            // 1. Etki alanındaki slotları bul
            var affectedSlots = new System.Collections.Generic.HashSet<int>();
            for (int i = 1; i <= range; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                affectedSlots.Add(left);
                affectedSlots.Add(right);
            }

            // 2. Etkilenen segmentleri bul
            var affectedSegments = new System.Collections.Generic.HashSet<SegmentInstance>();
            for (int i = 0; i < slotCount; i++)
            {
                foreach (Transform child in wheelManager.slots[i])
                {
                    var inst = child.GetComponent<SegmentInstance>();
                    if (inst != null && inst.data != null)
                    {
                        int segStart = inst.startSlotIndex;
                        int segSize = inst.data.size;
                        for (int s = 0; s < segSize; s++)
                        {
                            int coveredSlot = (segStart + s) % slotCount;
                            if (affectedSlots.Contains(coveredSlot))
                            {
                                affectedSegments.Add(inst);
                                break;
                            }
                        }
                    }
                }
            }

            // 3. Her segmenti rastgele yeni yere taşı (silmeden)
            foreach (var targetInstance in affectedSegments)
            {
                int segStart = targetInstance.startSlotIndex;
                int segSize = targetInstance.data.size;
                var validSlots = new System.Collections.Generic.List<int>();
                
                for (int j = 0; j < wheelManager.slots.Length; j++)
                {
                    bool canPlace = true;
                    for (int s = 0; s < segSize; s++)
                    {
                        int idx = (j + s) % slotCount;
                        if (wheelManager.slotOccupied[idx] || idx == segStart)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (canPlace) validSlots.Add(j);
                }

                if (validSlots.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, validSlots.Count);
                    int targetSlot = validSlots[randomIndex];
                    MoveSegmentToSlotWithoutDestroy(wheelManager, targetInstance, targetSlot);
                }
                else
                {
                    // Hiç uygun yer yoksa segmenti yok et
                    wheelManager.RemoveSegmentAtSlot(segStart);
                }
            }
            
            // 4. Kendi segmentini yok et
            wheelManager.RemoveSegmentAtSlot(mySlot);
        }
    }

    // SegmentCurseEffectHandler'daki yöntemi kullanarak segmenti silmeden taşı
    private static void MoveSegmentToSlotWithoutDestroy(WheelManager wheelManager, SegmentInstance segment, int newSlot)
    {
        // Eski slot'lardan çıkar (büyük segmentler için tüm slot'ları temizle)
        int oldStartSlot = segment.startSlotIndex;
        int segmentSize = segment.data.size;
        for (int i = 0; i < segmentSize; i++)
        {
            int oldSlot = (oldStartSlot + i) % wheelManager.slotCount;
            wheelManager.slotOccupied[oldSlot] = false;
        }
        
        // Yeni slot'a yerleştir
        segment.transform.SetParent(wheelManager.slots[newSlot]);
        segment.transform.localPosition = Vector3.zero;
        segment.transform.localRotation = Quaternion.identity;
        segment.startSlotIndex = newSlot;
        
        // Yeni slot'ları işaretle (büyük segmentler için tüm slot'ları)
        for (int i = 0; i < segmentSize; i++)
        {
            int newSlotIndex = (newSlot + i) % wheelManager.slotCount;
            wheelManager.slotOccupied[newSlotIndex] = true;
        }
    }

    public class SegmentSwapperEffect : IWheelEffect
    {
        private int range;
        public SegmentSwapperEffect(int range)
        {
            this.range = Mathf.Max(1, range);
        }
        public void OnNeedleLanded(int landedSlot, int mySlot, int slotCount, System.Action<int> moveNeedleToSlot, System.Action destroySelf)
        {
            // 1. İğne menzilde mi?
            bool inRange = false;
            for (int i = 1; i <= range && !inRange; i++)
            {
                int left = (mySlot - i + slotCount) % slotCount;
                int right = (mySlot + i) % slotCount;
                if (landedSlot == left || landedSlot == right)
                    inRange = true;
            }
            if (!inRange) return;

            var wheelManager = Object.FindAnyObjectByType<WheelManager>();
            if (wheelManager == null) return;

            // 2. landedSlot'u kaplayan segmenti bul
            SegmentInstance targetInstance = SegmentWheelManipulationHandler.FindSegmentCoveringSlot(wheelManager, landedSlot);
            if (targetInstance == null) return;
            
            int targetStart = targetInstance.startSlotIndex;
            int targetSize = targetInstance.data.size;

            // 3. Swapper segmentini bul
            SegmentInstance swapperInstance = null;
            for (int i = 0; i < wheelManager.slots.Length; i++)
            {
                foreach (Transform child in wheelManager.slots[i])
                {
                    var inst = child.GetComponent<SegmentInstance>();
                    if (inst != null && inst.data != null && inst.data.wheelManipulationType == WheelManipulationType.SegmentSwapper)
                    {
                        swapperInstance = inst;
                        break;
                    }
                }
                if (swapperInstance != null) break;
            }
            if (swapperInstance == null) return;

            // 4. Aynı boyutta başka bir segment bul
            var candidates = new System.Collections.Generic.List<SegmentInstance>();
            for (int i = 0; i < wheelManager.slots.Length; i++)
            {
                foreach (Transform child in wheelManager.slots[i])
                {
                    var inst = child.GetComponent<SegmentInstance>();
                    if (inst != null && inst.data != null && inst != targetInstance && inst != swapperInstance && inst.data.size == targetSize)
                    {
                        candidates.Add(inst);
                    }
                }
            }
            if (candidates.Count == 0) return;
            
            var swapInstance = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            int swapStart = swapInstance.startSlotIndex;

            // 5. Segmentleri yer değiştir (silmeden)
            MoveSegmentToSlotWithoutDestroy(wheelManager, targetInstance, swapStart);
            MoveSegmentToSlotWithoutDestroy(wheelManager, swapInstance, targetStart);
        }
    }


} 
