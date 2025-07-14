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
        var allSegments = Resources.LoadAll<SegmentData>("Wheel/Segment SO");
        WheelManager wm = FindFirstObjectByType<WheelManager>();
        if (wm == null) return;
        if (removedSegment.rewardFillMode == RewardFillMode.FillWithOnes)
        {
            List<SegmentData> candidates = new List<SegmentData>();
            foreach (var seg in allSegments)
            {
                if (seg.type == removedSegment.rewardType && seg.rarity == removedSegment.rewardRarity && seg.effectType == SegmentEffectType.StatBoost && seg.size == 1)
                {
                    candidates.Add(seg);
                }
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
                if (seg.type == removedSegment.rewardType && seg.rarity == removedSegment.rewardRarity && seg.effectType == SegmentEffectType.StatBoost && seg.size == removedSegment.size)
                {
                    candidates.Add(seg);
                }
            }
            if (candidates.Count > 0)
            {
                int idx = Random.Range(0, candidates.Count);
                wm.AddSegmentToSlot(candidates[idx], startSlot);
            }
        }
    }
} 