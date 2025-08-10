using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SegmentCurseEffectHandler : MonoBehaviour
{
    public static SegmentCurseEffectHandler Instance { get; private set; }
    
    // Queue sistemi için
    private Queue<int> respinQueue = new Queue<int>();
    private bool isProcessingQueue = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Curse Effect interface'i
    public interface ICurseEffect
    {
        bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount);
    }

    // Factory fonksiyonu
    public static ICurseEffect CreateCurseEffect(SegmentData data)
    {
        switch (data.curseEffectType)
        {
            case CurseEffectType.ReSpinCurse:
                return new ReSpinCurseEffect(data.curseReSpinCount);
            case CurseEffectType.RandomEscapeCurse:
                return new RandomEscapeCurseEffect();
            case CurseEffectType.BlurredMemoryCurse:
                return new BlurredMemoryCurseEffect();
            case CurseEffectType.TeleportEscapeCurse:
                return new TeleportEscapeCurseEffect();
            case CurseEffectType.ExplosiveCurse:
                return new ExplosiveCurseEffect(data.explosiveRange);
            case CurseEffectType.BondingCurse:
                return new BondingCurseEffect();
            case CurseEffectType.SelfBondingCurse:
                return new SelfBondingCurseEffect(data.selfBondingCount);
            default:
                return null;
        }
    }

    // Curse Effect'leri işle
    public bool HandleCurseEffect(SegmentData curseSegment, int landedSlot, int mySlot, int slotCount)
    {
        if (curseSegment.effectType != SegmentEffectType.CurseEffect) return false;
        
        var curseEffect = CreateCurseEffect(curseSegment);
        return curseEffect?.OnNeedleLanded(landedSlot, mySlot, slotCount) ?? false;
    }

    // Queue'ya ReSpin ekle
    public void AddReSpinToQueue(int spinCount)
    {
        respinQueue.Enqueue(spinCount);
        
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessReSpinQueue());
        }
    }

    // Queue'daki ReSpin'leri sırayla işle
    private IEnumerator ProcessReSpinQueue()
    {
        isProcessingQueue = true;
        
        while (respinQueue.Count > 0)
        {
            int spinCount = respinQueue.Dequeue();
            
            yield return StartCoroutine(SpinWheelSequence(spinCount));
        }
        
        isProcessingQueue = false;
    }
    
    // Debug metodları için public wrapper
    public void DebugAllBonds()
    {
        BondingCurseEffect.DebugBondedSegments();
        SelfBondingCurseEffect.DebugSelfBondedSegments();
    }

    // ReSpin Curse Effect - Çarkı tekrar döndürür (Queue ile)
    public class ReSpinCurseEffect : ICurseEffect
    {
        private int spinCount;
        
        public ReSpinCurseEffect(int spinCount)
        {
            this.spinCount = spinCount;
        }
        
        public bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount)
        {
            // Kendi slotuna gelirse tetikle
            if (landedSlot == mySlot)
            {
                // Queue'ya ekle
                SegmentCurseEffectHandler.Instance.AddReSpinToQueue(spinCount);
                return true;
            }
            return false;
        }
    }

    // RandomEscape Curse Effect - Tüm segmentleri rastgele dağıtır
    public class RandomEscapeCurseEffect : ICurseEffect
    {
        public bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount)
        {
            // Kendi slotuna gelirse tetikle
            if (landedSlot == mySlot)
            {
                var wheelManager = FindFirstObjectByType<WheelManager>();
                if (wheelManager == null) return false;
                
                // Tüm segmentleri rastgele dağıt
                SegmentCurseEffectHandler.Instance.StartCoroutine(RandomizeAllSegments(wheelManager));
                return true;
            }
            return false;
        }
        
        private IEnumerator RandomizeAllSegments(WheelManager wheelManager)
        {
            // Tüm segmentleri topla
            List<SegmentInstance> allSegments = new List<SegmentInstance>();
            
            for (int i = 0; i < wheelManager.slotCount; i++)
            {
                if (IsSlotOccupied(wheelManager, i))
                {
                    var segment = GetSegmentAtSlot(wheelManager, i);
                    if (segment != null)
                    {
                        allSegments.Add(segment);
                    }
                }
            }
            
            // Segmentleri rastgele dağıt (silmeden, sadece pozisyon değiştirerek)
            foreach (var segment in allSegments)
            {
                int randomSlot = Random.Range(0, wheelManager.slotCount);
                
                // Boş slot bulana kadar dene
                while (IsSlotOccupied(wheelManager, randomSlot))
                {
                    randomSlot = Random.Range(0, wheelManager.slotCount);
                }
                
                // Segmenti yeni slot'a taşı (silmeden)
                MoveSegmentToSlot(wheelManager, segment, randomSlot);
                
                yield return new WaitForSeconds(0.1f); // Animasyon için kısa bekleme
            }
            
            // Stat boostları yeniden hesapla
            SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
        }
        
        // Segmenti silmeden yeni slot'a taşı
        private void MoveSegmentToSlot(WheelManager wheelManager, SegmentInstance segment, int newSlot)
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
        
        // Slot durumu kontrol metodları
        private bool IsSlotOccupied(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return false;
            
            // GetSegmentAtSlot metodunu kullanarak slot'ta segment var mı kontrol et
            return GetSegmentAtSlot(wheelManager, slotIndex) != null;
        }
        
        private SegmentInstance GetSegmentAtSlot(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return null;
            
            // WheelManager'daki RemoveSegmentAtSlot mantığını kullan
            for (int offset = 0; offset < wheelManager.slotCount; offset++)
            {
                int i = (slotIndex - offset + wheelManager.slotCount) % wheelManager.slotCount;
                Transform slot = wheelManager.slots[i];
                foreach (Transform child in slot)
                {
                    if (child == null) continue;
                    SegmentInstance inst = child.GetComponent<SegmentInstance>();
                    if (inst != null && inst.data != null)
                    {
                        int segStart = inst.startSlotIndex;
                        int segEnd = (segStart + inst.data.size - 1) % wheelManager.slotCount;
                        int size = inst.data.size;
                        bool inRange = false;
                        if (segStart <= segEnd)
                            inRange = (slotIndex >= segStart && slotIndex <= segEnd);
                        else
                            inRange = (slotIndex >= segStart || slotIndex <= segEnd);
                        if (inRange)
                        {
                            return inst;
                        }
                    }
                }
            }
            return null;
        }
    }

    // BlurredMemory Curse Effect - Tooltip'leri kapatır
    public class BlurredMemoryCurseEffect : ICurseEffect
    {
        public bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount)
        {
            // Kendi slotuna gelirse tetikle
            if (landedSlot == mySlot)
            {
                // Global tooltip disable flag'ini kapat (tooltip'ler geri gelsin)
                SetGlobalTooltipDisabled(false);
                
                // Tüm segmentlerin tooltip'lerini tekrar aç
                var wheelManager = FindFirstObjectByType<WheelManager>();
                if (wheelManager == null) return false;
                
                // Tüm segmentleri bul ve tooltip'lerini tekrar aç
                for (int i = 0; i < wheelManager.slotCount; i++)
                {
                    if (IsSlotOccupied(wheelManager, i))
                    {
                        var segment = GetSegmentAtSlot(wheelManager, i);
                        if (segment != null)
                        {
                            // Segment'in tooltip'ini tekrar aç
                            EnableSegmentTooltip(segment);
                        }
                    }
                }
                return true;
            }
            return false;
        }
        
        private void EnableSegmentTooltip(SegmentInstance segment)
        {
            // Segment'in tooltip'ini tekrar açmak için data'yı güncelle
            if (segment.data != null)
            {
                segment.data.tooltipDisabled = false;
            }
        }
        
        private void SetGlobalTooltipDisabled(bool disabled)
        {
            // Global tooltip disable flag'ini ayarla
            // Bu flag tüm yeni segmentler için de geçerli olacak
            PlayerPrefs.SetInt("GlobalTooltipDisabled", disabled ? 1 : 0);
        }
        
        // Slot durumu kontrol metodları (RandomEscapeCurse'den kopyalandı)
        private bool IsSlotOccupied(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return false;
            return wheelManager.slotOccupied[slotIndex];
        }
        
        private SegmentInstance GetSegmentAtSlot(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return null;
            if (!wheelManager.slotOccupied[slotIndex]) return null;
            
            foreach (Transform child in wheelManager.slots[slotIndex])
            {
                var segment = child.GetComponent<SegmentInstance>();
                if (segment != null) return segment;
            }
            return null;
        }
    }

    // TeleportEscape Curse Effect - Segment yok olmadan önce başka bir segmentle yer değiştirir
    public class TeleportEscapeCurseEffect : ICurseEffect
    {
        public bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount)
        {
            // Kendi slotuna gelirse tetikle
            if (landedSlot == mySlot)
            {
                var wheelManager = FindFirstObjectByType<WheelManager>();
                if (wheelManager == null) return false;
                
                // Kaçmaya çalışan segmenti bul
                SegmentInstance escapingSegment = GetSegmentAtSlot(wheelManager, mySlot);
                if (escapingSegment == null) return false;
                
                // Rastgele başka bir segment bul
                SegmentInstance targetSegment = FindRandomOtherSegment(wheelManager, escapingSegment);
                if (targetSegment == null) return false;
                
                // İki segmenti yer değiştir
                SwapSegments(wheelManager, escapingSegment, targetSegment);
                
                // Yer değiştirdikten sonra hedef segmenti normal silme sürecine yönlendir
                // Hedef segmentin slot'unu bul ve normal silme işlemini başlat
                int targetSlot = targetSegment.startSlotIndex;
                wheelManager.StartCoroutine(wheelManager.SpinEndSequence());
                
                // Stat boostları yeniden hesapla
                SegmentStatBoostHandler.Instance?.RecalculateAllStatBoosts();
                return true;
            }
            return false;
        }
        

        
        private SegmentInstance FindRandomOtherSegment(WheelManager wheelManager, SegmentInstance excludeSegment)
        {
            var candidates = new List<SegmentInstance>();
            
            for (int i = 0; i < wheelManager.slotCount; i++)
            {
                if (IsSlotOccupied(wheelManager, i))
                {
                    var segment = GetSegmentAtSlot(wheelManager, i);
                    if (segment != null && segment != excludeSegment)
                    {
                        candidates.Add(segment);
                    }
                }
            }
            
            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }
            
            return null;
        }
        
        private void SwapSegments(WheelManager wheelManager, SegmentInstance segment1, SegmentInstance segment2)
        {
            int slot1 = segment1.startSlotIndex;
            int slot2 = segment2.startSlotIndex;
            
            // İki segmenti yer değiştir
            MoveSegmentToSlotWithoutDestroy(wheelManager, segment1, slot2);
            MoveSegmentToSlotWithoutDestroy(wheelManager, segment2, slot1);
        }
        
        private void MoveSegmentToSlotWithoutDestroy(WheelManager wheelManager, SegmentInstance segment, int newSlot)
        {
            // Eski slot'lardan çıkar
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
            
            // Yeni slot'ları işaretle
            for (int i = 0; i < segmentSize; i++)
            {
                int newSlotIndex = (newSlot + i) % wheelManager.slotCount;
                wheelManager.slotOccupied[newSlotIndex] = true;
            }
        }
        
        // Slot durumu kontrol metodları
        private bool IsSlotOccupied(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return false;
            return wheelManager.slotOccupied[slotIndex];
        }
        
        private SegmentInstance GetSegmentAtSlot(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return null;
            if (!wheelManager.slotOccupied[slotIndex]) return null;
            
            foreach (Transform child in wheelManager.slots[slotIndex])
            {
                var segment = child.GetComponent<SegmentInstance>();
                if (segment != null) return segment;
            }
            return null;
        }
    }

    // ExplosiveCurse Curse Effect - Segment yok olacağı zaman yanındaki segmentleri de siler
    public class ExplosiveCurseEffect : ICurseEffect
    {
        private int explosiveRange;
        
        public ExplosiveCurseEffect(int explosiveRange)
        {
            this.explosiveRange = explosiveRange;
        }
        
        public bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount)
        {
            // Kendi slotuna gelirse tetikle
            if (landedSlot == mySlot)
            {
                var wheelManager = FindFirstObjectByType<WheelManager>();
                if (wheelManager == null) return false;
                
                // Patlayan segmenti bul
                SegmentInstance explosiveSegment = GetSegmentAtSlot(wheelManager, mySlot);
                if (explosiveSegment == null) return false;
                
                // Patlama alanındaki tüm segmentleri bul ve sil
                List<SegmentInstance> segmentsToDestroy = GetSegmentsInExplosionRange(wheelManager, mySlot, explosiveRange, slotCount);
                
                // Patlama alanındaki segmentleri sil
                foreach (var segment in segmentsToDestroy)
                {
                    if (segment != null && segment.gameObject != null)
                    {
                        // Segment'i sil (normal silme sürecini bypass et)
                        DestroySegmentImmediately(wheelManager, segment);
                    }
                }
                
                // Stat boostları yeniden hesaplama işlemini SpinEndSequence'e bırak
                // Burada çağırmıyoruz çünkü segmentler henüz tam olarak yok edilmemiş olabilir
                return true;
            }
            return false;
        }
        
        private List<SegmentInstance> GetSegmentsInExplosionRange(WheelManager wheelManager, int centerSlot, int range, int slotCount)
        {
            var segmentsInRange = new List<SegmentInstance>();
            
            // Patlama alanındaki tüm slotları kontrol et
            for (int offset = -range; offset <= range; offset++)
            {
                int targetSlot = (centerSlot + offset + slotCount) % slotCount;
                
                // Slot'ta segment var mı kontrol et
                if (IsSlotOccupied(wheelManager, targetSlot))
                {
                    var segment = GetSegmentAtSlot(wheelManager, targetSlot);
                    if (segment != null && !segmentsInRange.Contains(segment))
                    {
                        segmentsInRange.Add(segment);
                    }
                }
            }
            
            return segmentsInRange;
        }
        
        private void DestroySegmentImmediately(WheelManager wheelManager, SegmentInstance segment)
        {
            if (segment == null || segment.gameObject == null) return;
            
            // WheelManager'daki ClearWheel metodunun mantığını kullan
            
            // 1. Stat boost segmenti ise, statı silmeden önce sıfırla
            if (segment.data.effectType == SegmentEffectType.StatBoost && segment._appliedStatBoost != 0f)
            {
                StatType statType = segment.data.statType;
                if (SegmentStatBoostHandler.Instance != null)
                {
                    SegmentStatBoostHandler.Instance.RemoveStat(segment, segment._appliedStatBoost, statType);
                }
                segment._appliedStatBoost = 0f;
            }
            
            // 2. Random stat stack'i varsa temizle
            if (segment.data.effectType == SegmentEffectType.StatBoost && segment.data.statType == StatType.Random)
            {
                SegmentStatBoostHandler.RemoveAllRandomStatsFor(segment);
            }
            
            // 3. Segment'in slot'larını temizle
            int startSlot = segment.startSlotIndex;
            int segmentSize = segment.data.size;
            
            for (int i = 0; i < segmentSize; i++)
            {
                int slotIndex = (startSlot + i) % wheelManager.slotCount;
                wheelManager.slotOccupied[slotIndex] = false;
            }
            
            // 4. Segment'i hemen inactive yap (RecalculateAllStatBoosts'in görmemesi için)
            segment.gameObject.SetActive(false);
            
            // 5. Segment'i yok et
            Destroy(segment.gameObject);
        }
        
        // Slot durumu kontrol metodları
        private bool IsSlotOccupied(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return false;
            return wheelManager.slotOccupied[slotIndex];
        }
        
        private SegmentInstance GetSegmentAtSlot(WheelManager wheelManager, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= wheelManager.slotCount) return null;
            
            // WheelManager'daki RemoveSegmentAtSlot mantığını kullan
            for (int offset = 0; offset < wheelManager.slotCount; offset++)
            {
                int i = (slotIndex - offset + wheelManager.slotCount) % wheelManager.slotCount;
                Transform slot = wheelManager.slots[i];
                foreach (Transform child in slot)
                {
                    if (child == null) continue;
                    SegmentInstance inst = child.GetComponent<SegmentInstance>();
                    if (inst != null && inst.data != null)
                    {
                        int segStart = inst.startSlotIndex;
                        int segEnd = (segStart + inst.data.size - 1) % wheelManager.slotCount;
                        int size = inst.data.size;
                        bool inRange = false;
                        if (segStart <= segEnd)
                            inRange = (slotIndex >= segStart && slotIndex <= segEnd);
                        else
                            inRange = (slotIndex >= segStart || slotIndex <= segEnd);
                        if (inRange)
                        {
                            return inst;
                        }
                    }
                }
            }
            return null;
        }
    }

    // BondingCurse Curse Effect - İki segmenti birbirine bağlar, biri silinince diğeri de silinir
    public class BondingCurseEffect : ICurseEffect
    {
        // Bond'ları takip eden dictionary'ler
        public static Dictionary<SegmentInstance, SegmentInstance> bondedPairs = new Dictionary<SegmentInstance, SegmentInstance>();
        public static Dictionary<SegmentInstance, SegmentInstance> reverseBondedPairs = new Dictionary<SegmentInstance, SegmentInstance>();
        
        // Hangi bond'u hangi BondingCurse segment'inin oluşturduğunu takip eden dictionary
        public static Dictionary<SegmentInstance, SegmentInstance> bondCreator = new Dictionary<SegmentInstance, SegmentInstance>();
        
        public bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount)
        {
            // Kendi slotuna gelirse sadece flag set et, silme işlemi WheelManager'da yapılacak
            if (landedSlot == mySlot)
            {
                // Sonsuz döngüyü önlemek için WheelManager.RemoveSegmentAtSlot çağırmıyoruz
                // Bunun yerine sadece true döndürüyoruz, silme işlemi normal akışta yapılacak
                return true;
            }
            return false;
        }
        
        // WheelManager tarafından çağrılacak bond removal handler
        public static void HandleBondedSegmentRemoval(SegmentInstance segmentToRemove)
        {
            if (segmentToRemove == null || segmentToRemove.gameObject == null || !segmentToRemove.gameObject.activeInHierarchy) return;
            
            // Bu segment BondingCurse segment'i mi kontrol et
            bool isBondingCurseSegment = segmentToRemove.data.effectType == SegmentEffectType.CurseEffect && 
                                        segmentToRemove.data.curseEffectType == CurseEffectType.BondingCurse;
            
            // Bu segment bonded mı kontrol et
            if (bondedPairs.ContainsKey(segmentToRemove))
            {
                var bondedSegment = bondedPairs[segmentToRemove];
                
                // Bond'u temizle
                bondedPairs.Remove(segmentToRemove);
                if (reverseBondedPairs.ContainsKey(bondedSegment))
                    reverseBondedPairs.Remove(bondedSegment);
                
                // Eğer silinen segment BondingCurse segment'i ise, bağladığı segment'i silme
                // Sadece bağlantıyı kaldır
                if (!isBondingCurseSegment && bondedSegment != null && bondedSegment.gameObject != null && bondedSegment.gameObject.activeInHierarchy)
                {
                    // Recursive çağrıyı önlemek için direkt silme işlemi yap (bonding check'siz)
                    BondingCurseEffect.RemoveSegmentDirectly(bondedSegment);
                }
            }
            
            // Reverse bond kontrol et
            if (reverseBondedPairs.ContainsKey(segmentToRemove))
            {
                var bondedSegment = reverseBondedPairs[segmentToRemove];
                
                // Bond'u temizle
                reverseBondedPairs.Remove(segmentToRemove);
                if (bondedPairs.ContainsKey(bondedSegment))
                    bondedPairs.Remove(bondedSegment);
                
                // Eğer silinen segment BondingCurse segment'i ise, bağladığı segment'i silme
                // Sadece bağlantıyı kaldır
                if (!isBondingCurseSegment && bondedSegment != null && bondedSegment.gameObject != null && bondedSegment.gameObject.activeInHierarchy)
                {
                    // Recursive çağrıyı önlemek için direkt silme işlemi yap (bonding check'siz)
                    BondingCurseEffect.RemoveSegmentDirectly(bondedSegment);
                }
            }
            
            // Eğer silinen segment BondingCurse segment'i ise, TÜM bond'ları temizle
            if (isBondingCurseSegment)
            {
                // TÜM bond'ları temizle (sadece bu segment'in değil, hepsini)
                int totalBonds = bondedPairs.Count;
                
                bondedPairs.Clear();
                reverseBondedPairs.Clear();
                bondCreator.Clear();
                
                Debug.Log($"BondingCurse segment removed, cleared ALL bonds ({totalBonds} total)");
            }
            // Eğer silinen segment bonded segment ise de yeni bond oluştur
            else if (bondedPairs.ContainsKey(segmentToRemove) || reverseBondedPairs.ContainsKey(segmentToRemove))
            {
                CreateNewBond();
            }
        }
        
        // Yeni bond oluştur
        private static void CreateNewBond()
        {
            var wheelManager = FindFirstObjectByType<WheelManager>();
            if (wheelManager == null) return;
            
            // Aktif BondingCurse var mı kontrol et
            bool hasActiveBondingCurse = false;
            for (int i = 0; i < wheelManager.slotCount; i++)
            {
                if (wheelManager.slotOccupied[i])
                {
                    foreach (Transform child in wheelManager.slots[i])
                    {
                        SegmentInstance segment = child.GetComponent<SegmentInstance>();
                        if (segment != null && segment.data.effectType == SegmentEffectType.CurseEffect && 
                            segment.data.curseEffectType == CurseEffectType.BondingCurse)
                        {
                            hasActiveBondingCurse = true;
                            break;
                        }
                    }
                    if (hasActiveBondingCurse) break;
                }
            }
            
            // Aktif BondingCurse yoksa yeni bond oluşturma
            if (!hasActiveBondingCurse)
            {
                Debug.Log("No active BondingCurse found, skipping new bond creation");
                return;
            }
            
            // Mevcut segment'lerden 2 tanesini bul (bond'lanmamış olanlar)
            List<SegmentInstance> availableSegments = new List<SegmentInstance>();
            
            for (int i = 0; i < wheelManager.slotCount; i++)
            {
                if (wheelManager.slotOccupied[i])
                {
                    foreach (Transform child in wheelManager.slots[i])
                    {
                        SegmentInstance segment = child.GetComponent<SegmentInstance>();
                        if (segment != null && !bondedPairs.ContainsKey(segment) && !reverseBondedPairs.ContainsKey(segment))
                        {
                            availableSegments.Add(segment);
                        }
                    }
                }
            }
            
            // En az 2 segment varsa yeni bond oluştur
            if (availableSegments.Count >= 2)
            {
                // Rastgele 2 segment seç
                int index1 = Random.Range(0, availableSegments.Count);
                int index2 = Random.Range(0, availableSegments.Count);
                while (index2 == index1)
                {
                    index2 = Random.Range(0, availableSegments.Count);
                }
                
                SegmentInstance segment1 = availableSegments[index1];
                SegmentInstance segment2 = availableSegments[index2];
                
                // Yeni bond'u oluştur
                bondedPairs[segment1] = segment2;
                reverseBondedPairs[segment2] = segment1;
                
                // bondCreator'da null olarak kaydet (otomatik oluşturulan bond)
                bondCreator[segment1] = null;
                bondCreator[segment2] = null;
                
                Debug.Log($"New bond auto-created: {segment1.data.segmentName} (Slot {segment1.startSlotIndex}) <-> {segment2.data.segmentName} (Slot {segment2.startSlotIndex})");
            }
        }
        
        // Debug için bonded segmentleri göster
        public static void DebugBondedSegments()
        {
            Debug.Log($"=== BONDED SEGMENTS DEBUG ===");
            Debug.Log($"Total bonded pairs: {bondedPairs.Count}");
            
            foreach (var pair in bondedPairs)
            {
                var segment1 = pair.Key;
                var segment2 = pair.Value;
                
                if (segment1 != null && segment2 != null)
                {
                    Debug.Log($"Bonded: {segment1.data.segmentName} (Slot {segment1.startSlotIndex}) <-> {segment2.data.segmentName} (Slot {segment2.startSlotIndex})");
                }
            }
            Debug.Log($"=== END DEBUG ===");
        }
        
        // Bonding check'siz direkt segment silme metodu (recursive çağrıları önlemek için)
        public static void RemoveSegmentDirectly(SegmentInstance segment)
        {
            if (segment == null || segment.gameObject == null || !segment.gameObject.activeInHierarchy) return;
            
            var wheelManager = FindFirstObjectByType<WheelManager>();
            if (wheelManager == null) return;
            
            // Segment'i hemen inactive yap (recursive çağrıları önlemek için)
            segment.gameObject.SetActive(false);
            
            // Segment'in slot bilgilerini al
            int startSlot = segment.startSlotIndex;
            int segmentSize = segment.data.size;
            
            // Stat boost temizleme (ExplosiveCurse'den kopyalandı)
            if (segment.data.effectType == SegmentEffectType.StatBoost && segment._appliedStatBoost != 0f)
            {
                StatType statType = segment.data.statType;
                if (SegmentStatBoostHandler.Instance != null)
                {
                    SegmentStatBoostHandler.Instance.RemoveStat(segment, segment._appliedStatBoost, statType);
                }
                segment._appliedStatBoost = 0f;
            }
            
            // Random stat stack temizleme
            if (segment.data.effectType == SegmentEffectType.StatBoost && segment.data.statType == StatType.Random)
            {
                SegmentStatBoostHandler.RemoveAllRandomStatsFor(segment);
            }
            
            // Slot'ları temizle
            for (int i = 0; i < segmentSize; i++)
            {
                int slotIndex = (startSlot + i) % wheelManager.slotCount;
                wheelManager.slotOccupied[slotIndex] = false;
            }
            
            // Segment'i sil
            Destroy(segment.gameObject);
            
            // Bonded segment silindikten sonra yeni bond oluştur
            CreateNewBond();
        }
    }

    // SelfBondingCurse Curse Effect - Kendini başka segmentlere bağlar, kendisi silinince bağladıkları da silinir
    public class SelfBondingCurseEffect : ICurseEffect
    {
        private int selfBondingCount;
        
        // Static dictionary to track self-bonded segments
        public static Dictionary<SegmentInstance, List<SegmentInstance>> selfBondedSegments = new Dictionary<SegmentInstance, List<SegmentInstance>>();
        public static Dictionary<SegmentInstance, SegmentInstance> reverseSelfBondedSegments = new Dictionary<SegmentInstance, SegmentInstance>();
        
        public SelfBondingCurseEffect(int selfBondingCount)
        {
            this.selfBondingCount = selfBondingCount;
        }
        
        public bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount)
        {
            // Kendi slotuna gelirse sadece flag set et, silme işlemi WheelManager'da yapılacak
            if (landedSlot == mySlot)
            {
                // Sonsuz döngüyü önlemek için WheelManager.RemoveSegmentAtSlot çağırmıyoruz
                // Bunun yerine sadece true döndürüyoruz, silme işlemi normal akışta yapılacak
                return true;
            }
            return false;
        }
        
        // WheelManager tarafından çağrılacak self-bond removal handler
        public static void HandleSelfBondedSegmentRemoval(SegmentInstance segmentToRemove)
        {
            if (segmentToRemove == null || segmentToRemove.gameObject == null || !segmentToRemove.gameObject.activeInHierarchy) return;
            
            // Bu segment self-bonding curse mu kontrol et
            if (selfBondedSegments.ContainsKey(segmentToRemove))
            {
                var bondedSegments = selfBondedSegments[segmentToRemove];
                
                // Tüm bonded segmentleri sil
                foreach (var bondedSegment in bondedSegments)
                {
                    if (bondedSegment != null && bondedSegment.gameObject != null && bondedSegment.gameObject.activeInHierarchy)
                    {
                        // Reverse bond'u temizle
                        if (reverseSelfBondedSegments.ContainsKey(bondedSegment))
                            reverseSelfBondedSegments.Remove(bondedSegment);
                        
                        // Recursive çağrıyı önlemek için direkt silme işlemi yap (bonding check'siz)
                        BondingCurseEffect.RemoveSegmentDirectly(bondedSegment);
                    }
                }
                
                // Self-bond'u temizle
                selfBondedSegments.Remove(segmentToRemove);
            }
            
            // Bu segment başka bir curse'un bonded'ı mı kontrol et
            if (reverseSelfBondedSegments.ContainsKey(segmentToRemove))
            {
                var curseSegment = reverseSelfBondedSegments[segmentToRemove];
                
                // Reverse bond'u temizle
                reverseSelfBondedSegments.Remove(segmentToRemove);
                
                // Curse segment'in listesinden bu segment'i çıkar
                if (selfBondedSegments.ContainsKey(curseSegment))
                {
                    selfBondedSegments[curseSegment].Remove(segmentToRemove);
                    
                    // Yeni bağlantı yap (bağlantı sayısını korumak için)
                    CreateNewSelfBond(curseSegment);
                }
                
                // NOT: Curse segment'i silmiyoruz, sadece bond'u kırıyoruz
            }
        }
        
        // Yeni self bond oluştur (bağlantı sayısını korumak için)
        public static void CreateNewSelfBond(SegmentInstance curseSegment)
        {
            if (curseSegment == null || curseSegment.data == null) return;
            
            var wheelManager = FindFirstObjectByType<WheelManager>();
            if (wheelManager == null) return;
            
            // Aktif SelfBondingCurse var mı kontrol et
            bool hasActiveSelfBondingCurse = false;
            for (int i = 0; i < wheelManager.slotCount; i++)
            {
                if (wheelManager.slotOccupied[i])
                {
                    foreach (Transform child in wheelManager.slots[i])
                    {
                        SegmentInstance segment = child.GetComponent<SegmentInstance>();
                        if (segment != null && segment.data.effectType == SegmentEffectType.CurseEffect && 
                            segment.data.curseEffectType == CurseEffectType.SelfBondingCurse)
                        {
                            hasActiveSelfBondingCurse = true;
                            break;
                        }
                    }
                    if (hasActiveSelfBondingCurse) break;
                }
            }
            
            // Aktif SelfBondingCurse yoksa yeni bond oluşturma
            if (!hasActiveSelfBondingCurse)
            {
                Debug.Log("No active SelfBondingCurse found, skipping new self bond creation");
                return;
            }
            
            // Hedef bağlantı sayısını al
            int targetBondCount = curseSegment.data.selfBondingCount;
            
            // Mevcut bağlantı sayısını kontrol et
            int currentBondCount = 0;
            if (selfBondedSegments.ContainsKey(curseSegment))
            {
                currentBondCount = selfBondedSegments[curseSegment].Count;
            }
            
            // Eksik bağlantı sayısını hesapla
            int neededBonds = targetBondCount - currentBondCount;
            
            if (neededBonds <= 0)
            {
                Debug.Log($"SelfBondingCurse already has enough bonds ({currentBondCount}/{targetBondCount})");
                return;
            }
            
            // Mevcut segment'lerden bond'lanmamış olanları bul
            List<SegmentInstance> availableSegments = new List<SegmentInstance>();
            
            for (int i = 0; i < wheelManager.slotCount; i++)
            {
                if (wheelManager.slotOccupied[i])
                {
                    foreach (Transform child in wheelManager.slots[i])
                    {
                        SegmentInstance segment = child.GetComponent<SegmentInstance>();
                        if (segment != null && segment != curseSegment && 
                            !reverseSelfBondedSegments.ContainsKey(segment))
                        {
                            availableSegments.Add(segment);
                        }
                    }
                }
            }
            
            // Yeni bağlantılar yap
            int actualNewBonds = Mathf.Min(neededBonds, availableSegments.Count);
            
            if (actualNewBonds > 0)
            {
                // Curse segment'in bond listesi yoksa oluştur
                if (!selfBondedSegments.ContainsKey(curseSegment))
                {
                    selfBondedSegments[curseSegment] = new List<SegmentInstance>();
                }
                
                for (int i = 0; i < actualNewBonds; i++)
                {
                    var randomSegment = availableSegments[Random.Range(0, availableSegments.Count)];
                    availableSegments.Remove(randomSegment);
                    
                    // Yeni bond'u ekle
                    selfBondedSegments[curseSegment].Add(randomSegment);
                    reverseSelfBondedSegments[randomSegment] = curseSegment;
                    
                    Debug.Log($"New self bond created: {curseSegment.data.segmentName} (Slot {curseSegment.startSlotIndex}) -> {randomSegment.data.segmentName} (Slot {randomSegment.startSlotIndex})");
                }
                
                Debug.Log($"SelfBondingCurse bond count maintained: {selfBondedSegments[curseSegment].Count}/{targetBondCount}");
            }
            else
            {
                Debug.Log("No available segments for new self bond");
            }
        }
        
        // Debug için self-bonded segmentleri göster
        public static void DebugSelfBondedSegments()
        {
            Debug.Log($"=== SELF-BONDED SEGMENTS DEBUG ===");
            Debug.Log($"Total self-bonding curses: {selfBondedSegments.Count}");
            
            foreach (var pair in selfBondedSegments)
            {
                var curseSegment = pair.Key;
                var bondedSegments = pair.Value;
                
                if (curseSegment != null)
                {
                    Debug.Log($"Self-Bonding Curse: {curseSegment.data.segmentName} (Slot {curseSegment.startSlotIndex})");
                    foreach (var bonded in bondedSegments)
                    {
                        if (bonded != null)
                        {
                            Debug.Log($"  -> Bonded to: {bonded.data.segmentName} (Slot {bonded.startSlotIndex})");
                        }
                    }
                }
            }
            Debug.Log($"=== END DEBUG ===");
        }
    }

    // Queue için ReSpin sequence
    private IEnumerator SpinWheelSequence(int remainingSpins)
    {
        var wheelManager = FindFirstObjectByType<WheelManager>();
        if (wheelManager == null) yield break;
        
        // ReSpin efekti başladığını işaretle
        wheelManager.isReSpinEffectActive = true;
        
        for (int i = 0; i < remainingSpins; i++)
        {

            
            bool spinCompleted = false;
            
            // Çarkın dönüşünü başlat (callback ile)
            wheelManager.SpinWheel(() => spinCompleted = true);
            
            // Çarkın dönüşünün bitmesini bekle
            while (!spinCompleted)
            {
                yield return null;
            }
            

            
            // Kısa bir bekleme süresi
            yield return new WaitForSeconds(0.5f);
        }
        
        // Queue'da başka ReSpin varsa geri dönme
        if (respinQueue.Count > 0)
        {
            // Queue'da başka ReSpin var, geri dönme
        }
        else
        {
            // ReSpin efekti bittiğini işaretle
            wheelManager.isReSpinEffectActive = false;
            
            // Son dönüşten sonra geri dönüşü manuel olarak yap
            yield return new WaitForSeconds(0.5f);
            wheelManager.StartCoroutine(wheelManager.SmoothResetWheel());
        }
    }
} 