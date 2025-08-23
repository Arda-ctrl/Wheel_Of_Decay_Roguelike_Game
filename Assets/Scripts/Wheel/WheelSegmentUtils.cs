using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Wheel sisteminde segment işlemleri için ortak utility metodları
/// Tekrar eden kodları önlemek için oluşturuldu
/// </summary>
public static class WheelSegmentUtils
{
    /// <summary>
    /// Belirtilen şartlara uyan segmentleri bulur
    /// </summary>
    public static List<SegmentInstance> FindSegments(WheelManager wheelManager, Func<SegmentInstance, bool> predicate = null)
    {
        var segments = new List<SegmentInstance>();
        
        if (wheelManager == null) return segments;
        
        for (int i = 0; i < wheelManager.slotCount; i++)
        {
            if (wheelManager.slotOccupied[i])
            {
                foreach (Transform child in wheelManager.slots[i])
                {
                    var segment = child.GetComponent<SegmentInstance>();
                    if (segment != null && segment.gameObject != null && segment.gameObject.activeInHierarchy)
                    {
                        if (predicate == null || predicate(segment))
                        {
                            segments.Add(segment);
                        }
                    }
                }
            }
        }
        
        return segments;
    }
    
    /// <summary>
    /// Belirtilen slot'ta segment var mı kontrol eder
    /// </summary>
    public static bool IsSlotOccupied(WheelManager wheelManager, int slotIndex)
    {
        if (wheelManager == null || slotIndex < 0 || slotIndex >= wheelManager.slotCount) 
            return false;
        
        return GetSegmentAtSlot(wheelManager, slotIndex) != null;
    }
    
    /// <summary>
    /// Belirtilen slot'taki segmenti getirir
    /// </summary>
    public static SegmentInstance GetSegmentAtSlot(WheelManager wheelManager, int slotIndex)
    {
        if (wheelManager == null || slotIndex < 0 || slotIndex >= wheelManager.slotCount) 
            return null;
        
        // WheelManager'daki RemoveSegmentAtSlot mantığını kullan
        for (int offset = 0; offset < wheelManager.slotCount; offset++)
        {
            int i = (slotIndex - offset + wheelManager.slotCount) % wheelManager.slotCount;
            Transform slot = wheelManager.slots[i];
            
            foreach (Transform child in slot)
            {
                if (child == null) continue;
                SegmentInstance inst = child.GetComponent<SegmentInstance>();
                if (inst != null && inst.data != null && inst.gameObject != null && inst.gameObject.activeInHierarchy)
                {
                    int segStart = inst.startSlotIndex;
                    int segEnd = (segStart + inst.data.size - 1) % wheelManager.slotCount;
                    
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
    
    /// <summary>
    /// Segmenti yeni slot'a taşır (silmeden)
    /// </summary>
    public static void MoveSegmentToSlot(WheelManager wheelManager, SegmentInstance segment, int newSlot)
    {
        // Null check'ler
        if (segment == null || segment.gameObject == null || !segment.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("MoveSegmentToSlot: Segment is null or destroyed!");
            return;
        }
        
        if (segment.data == null)
        {
            Debug.LogWarning("MoveSegmentToSlot: Segment data is null!");
            return;
        }
        
        if (wheelManager == null || wheelManager.slots == null)
        {
            Debug.LogWarning("MoveSegmentToSlot: WheelManager or slots is null!");
            return;
        }
        
        // Eski slot'lardan çıkar (büyük segmentler için tüm slot'ları temizle)
        int oldStartSlot = segment.startSlotIndex;
        int segmentSize = segment.data.size;
        for (int i = 0; i < segmentSize; i++)
        {
            int oldSlot = (oldStartSlot + i) % wheelManager.slotCount;
            if (oldSlot >= 0 && oldSlot < wheelManager.slotOccupied.Length)
                wheelManager.slotOccupied[oldSlot] = false;
        }
        
        // Yeni slot'a yerleştir
        if (newSlot >= 0 && newSlot < wheelManager.slots.Length && wheelManager.slots[newSlot] != null)
        {
            segment.transform.SetParent(wheelManager.slots[newSlot]);
            segment.transform.localPosition = Vector3.zero;
            segment.transform.localRotation = Quaternion.identity;
            segment.startSlotIndex = newSlot;
            
            // Yeni slot'ları işaretle (büyük segmentler için tüm slot'ları)
            for (int i = 0; i < segmentSize; i++)
            {
                int newSlotIndex = (newSlot + i) % wheelManager.slotCount;
                if (newSlotIndex >= 0 && newSlotIndex < wheelManager.slotOccupied.Length)
                    wheelManager.slotOccupied[newSlotIndex] = true;
            }
        }
        else
        {
            Debug.LogWarning($"MoveSegmentToSlot: Invalid slot index {newSlot}!");
        }
    }
    
    /// <summary>
    /// Bond'lanmamış segmentleri bulur (BondingCurse için)
    /// </summary>
    public static List<SegmentInstance> FindUnbondedSegments(WheelManager wheelManager, bool excludeBondingCurses = true)
    {
        return FindSegments(wheelManager, segment =>
        {
            // Bonded segment'leri hariç tut
            bool isNotBonded = !SegmentCurseEffectHandler.BondingCurseEffect.bondedPairs.ContainsKey(segment) &&
                              !SegmentCurseEffectHandler.BondingCurseEffect.reverseBondedPairs.ContainsKey(segment);
            
            // BondingCurse segment'lerini hariç tut (isteğe bağlı)
            if (excludeBondingCurses)
            {
                bool isNotBondingCurse = !(segment.data.effectType == SegmentEffectType.CurseEffect && 
                                         segment.data.curseEffectType == CurseEffectType.BondingCurse);
                return isNotBonded && isNotBondingCurse;
            }
            
            return isNotBonded;
        });
    }
    
    /// <summary>
    /// Self-bond'lanmamış segmentleri bulur (SelfBondingCurse için)
    /// </summary>
    public static List<SegmentInstance> FindUnSelfBondedSegments(WheelManager wheelManager, SegmentInstance excludeSegment = null)
    {
        return FindSegments(wheelManager, segment =>
        {
            // Kendisini hariç tut
            if (excludeSegment != null && segment == excludeSegment)
                return false;
                
            // Self-bonded segment'leri hariç tut
            return !SegmentCurseEffectHandler.SelfBondingCurseEffect.reverseSelfBondedSegments.ContainsKey(segment);
        });
    }
    
    /// <summary>
    /// Aktif BondingCurse segment'ini bulur
    /// </summary>
    public static SegmentInstance FindActiveBondingCurse(WheelManager wheelManager)
    {
        var bondingCurses = FindSegments(wheelManager, segment =>
            segment.data.effectType == SegmentEffectType.CurseEffect && 
            segment.data.curseEffectType == CurseEffectType.BondingCurse);
            
        return bondingCurses.Count > 0 ? bondingCurses[0] : null;
    }
    
    /// <summary>
    /// Rastgele 2 farklı index seçer (infinite loop koruması ile)
    /// </summary>
    public static (int index1, int index2) GetTwoRandomIndices(int maxCount, int maxAttempts = 20)
    {
        if (maxCount < 2) return (-1, -1);
        
        int index1 = UnityEngine.Random.Range(0, maxCount);
        int index2 = UnityEngine.Random.Range(0, maxCount);
        int attempts = 0;
        
        while (index2 == index1 && attempts < maxAttempts)
        {
            index2 = UnityEngine.Random.Range(0, maxCount);
            attempts++;
        }
        
        // Eğer farklı index bulunamadıysa, manuel olarak farklı bir index seç
        if (index2 == index1)
        {
            index2 = (index1 + 1) % maxCount;
        }
        
        return (index1, index2);
    }
}
