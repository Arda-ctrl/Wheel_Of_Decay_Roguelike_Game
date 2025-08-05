using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        void OnCurseActivated(SegmentData curseSegment, int slotIndex);
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
            default:
                return null;
        }
    }

    // Curse Effect'leri işle
    public void HandleCurseEffect(SegmentData curseSegment, int slotIndex)
    {
        if (curseSegment.effectType != SegmentEffectType.CurseEffect) return;
        
        var curseEffect = CreateCurseEffect(curseSegment);
        curseEffect?.OnCurseActivated(curseSegment, slotIndex);
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

    // ReSpin Curse Effect - Çarkı tekrar döndürür (Queue ile)
    public class ReSpinCurseEffect : ICurseEffect
    {
        private int spinCount;
        
        public ReSpinCurseEffect(int spinCount)
        {
            this.spinCount = spinCount;
        }
        
        public void OnCurseActivated(SegmentData curseSegment, int slotIndex)
        {
            // Queue'ya ekle
            SegmentCurseEffectHandler.Instance.AddReSpinToQueue(spinCount);
        }
    }

    // RandomEscape Curse Effect - Tüm segmentleri rastgele dağıtır
    public class RandomEscapeCurseEffect : ICurseEffect
    {
        public void OnCurseActivated(SegmentData curseSegment, int slotIndex)
        {
            var wheelManager = FindFirstObjectByType<WheelManager>();
            if (wheelManager == null) return;
            
            // Tüm segmentleri rastgele dağıt
            SegmentCurseEffectHandler.Instance.StartCoroutine(RandomizeAllSegments(wheelManager));
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
            
            Debug.Log("[RandomEscapeCurse] Segmentler yer değiştiriliyor...");
            
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
            Debug.Log("[RandomEscapeCurse] Stat boostları yeniden hesaplanıyor...");
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

    // BlurredMemory Curse Effect - Tooltip'leri kapatır
    public class BlurredMemoryCurseEffect : ICurseEffect
    {
        public void OnCurseActivated(SegmentData curseSegment, int slotIndex)
        {
            Debug.Log("[BlurredMemoryCurse] Tooltip'ler tekrar aktif ediliyor...");
            
            // Global tooltip disable flag'ini kapat (tooltip'ler geri gelsin)
            SetGlobalTooltipDisabled(false);
            
            // Tüm segmentlerin tooltip'lerini tekrar aç
            var wheelManager = FindFirstObjectByType<WheelManager>();
            if (wheelManager == null) return;
            
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
            
            // Debug log
            if (disabled)
            {
                Debug.Log("[BlurredMemoryCurse] Global tooltip'ler devre dışı bırakıldı!");
            }
            else
            {
                Debug.Log("[BlurredMemoryCurse] Global tooltip'ler tekrar aktif edildi!");
            }
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