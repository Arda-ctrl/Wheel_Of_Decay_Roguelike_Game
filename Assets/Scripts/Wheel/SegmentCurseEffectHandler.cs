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
            // Burada daha sonra CurseEffect türleri eklenecek
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