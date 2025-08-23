using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tüm curse effect'ler için ortak base class
/// Tekrar eden kodları önlemek için oluşturuldu
/// </summary>
public abstract class BaseCurseEffect : SegmentCurseEffectHandler.ICurseEffect
{
    /// <summary>
    /// WheelManager referansını güvenli şekilde getirir
    /// </summary>
    protected WheelManager GetWheelManager()
    {
        var wheelManager = Object.FindFirstObjectByType<WheelManager>();
        if (wheelManager == null)
        {
            Debug.LogWarning($"{GetType().Name}: WheelManager not found!");
        }
        return wheelManager;
    }
    
    /// <summary>
    /// Kendi slot'una gelip gelmediğini kontrol eder
    /// </summary>
    protected bool IsTriggered(int landedSlot, int mySlot)
    {
        return landedSlot == mySlot;
    }
    
    /// <summary>
    /// Belirtilen slot'taki segmenti güvenli şekilde getirir
    /// </summary>
    protected SegmentInstance GetSegmentAtSlot(WheelManager wheelManager, int slotIndex)
    {
        return WheelSegmentUtils.GetSegmentAtSlot(wheelManager, slotIndex);
    }
    
    /// <summary>
    /// Belirtilen şartlara uyan segmentleri bulur
    /// </summary>
    protected List<SegmentInstance> FindSegments(WheelManager wheelManager, System.Func<SegmentInstance, bool> predicate = null)
    {
        return WheelSegmentUtils.FindSegments(wheelManager, predicate);
    }
    
    /// <summary>
    /// Segmenti güvenli şekilde taşır
    /// </summary>
    protected void MoveSegmentToSlot(WheelManager wheelManager, SegmentInstance segment, int newSlot)
    {
        WheelSegmentUtils.MoveSegmentToSlot(wheelManager, segment, newSlot);
    }
    
    /// <summary>
    /// Boş slot bulur (güvenli loop ile)
    /// </summary>
    protected int FindEmptySlot(WheelManager wheelManager, int maxAttempts = 50)
    {
        if (wheelManager == null) return -1;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomSlot = Random.Range(0, wheelManager.slotCount);
            if (!WheelSegmentUtils.IsSlotOccupied(wheelManager, randomSlot))
            {
                return randomSlot;
            }
        }
        
        Debug.LogWarning($"{GetType().Name}: Could not find empty slot after {maxAttempts} attempts");
        return -1;
    }
    
    /// <summary>
    /// Rastgele başka segment bulur (kendisi hariç)
    /// </summary>
    protected SegmentInstance FindRandomOtherSegment(WheelManager wheelManager, SegmentInstance excludeSegment)
    {
        var candidates = FindSegments(wheelManager, segment => segment != excludeSegment);
        
        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }
        
        Debug.LogWarning($"{GetType().Name}: No valid candidate segments found!");
        return null;
    }
    
    /// <summary>
    /// Curse effect'in ana mantığı - her alt sınıf implement etmeli
    /// </summary>
    public abstract bool OnNeedleLanded(int landedSlot, int mySlot, int slotCount);
}
