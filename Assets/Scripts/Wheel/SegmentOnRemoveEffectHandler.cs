using UnityEngine;
using System.Collections.Generic;

public class SegmentOnRemoveEffectHandler : MonoBehaviour
{
    public static SegmentOnRemoveEffectHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // OnRemoveEffect segmentlerini ekle
    public void HandleOnRemoveEffect(SegmentData removedSegment, int startSlot)
    {
        if (removedSegment.effectType != SegmentEffectType.OnRemoveEffect) return;
        
        // CurseEffect türü ödül olarak çıkmasını engelle
        if (removedSegment.rewardType == Type.CurseEffect)
        {
            Debug.LogWarning($"OnRemoveEffect '{removedSegment.segmentName}' tried to spawn CurseEffect rewards, which is not allowed. Skipping reward generation.");
            return;
        }
        
        var allSegments = Resources.LoadAll<SegmentData>("Wheel/Segment SO");
        WheelManager wm = FindFirstObjectByType<WheelManager>();
        if (wm == null) return;
        
        if (removedSegment.rewardFillMode == RewardFillMode.FillWithOnes)
        {
            List<SegmentData> candidates = new List<SegmentData>();
            foreach (var seg in allSegments)
            {
                // CurseEffect segmentlerini adaylardan çıkar
                if (seg.type == removedSegment.rewardType && 
                    seg.rarity == removedSegment.rewardRarity && 
                    seg.size == 1 && 
                    seg.effectType != SegmentEffectType.CurseEffect) // ← Curse effect'leri engelle
                {
                    candidates.Add(seg);
                }
            }
            
            if (candidates.Count == 0)
            {
                Debug.LogWarning($"OnRemoveEffect '{removedSegment.segmentName}' found no valid candidates for reward generation (excluding CurseEffect).");
                return;
            }
            
            for (int i = 0; i < removedSegment.size && candidates.Count > 0; i++)
            {
                int idx = Random.Range(0, candidates.Count);
                int slotToFill = (startSlot + i) % wm.slots.Length;
                wm.AddSegmentToSlot(candidates[idx], slotToFill);
            }
        }
        else if (removedSegment.rewardFillMode == RewardFillMode.FillWithLargest)
        {
            List<SegmentData> candidates = new List<SegmentData>();
            foreach (var seg in allSegments)
            {
                // CurseEffect segmentlerini adaylardan çıkar
                if (seg.type == removedSegment.rewardType && 
                    seg.rarity == removedSegment.rewardRarity && 
                    seg.size == removedSegment.size && 
                    seg.effectType != SegmentEffectType.CurseEffect) // ← Curse effect'leri engelle
                {
                    candidates.Add(seg);
                }
            }
            
            if (candidates.Count == 0)
            {
                Debug.LogWarning($"OnRemoveEffect '{removedSegment.segmentName}' found no valid candidates for reward generation (excluding CurseEffect).");
                return;
            }
            
            if (candidates.Count > 0)
            {
                int idx = Random.Range(0, candidates.Count);
                wm.AddSegmentToSlot(candidates[idx], startSlot);
            }
        }
    }
} 