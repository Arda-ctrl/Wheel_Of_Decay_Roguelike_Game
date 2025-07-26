using UnityEngine;
using System.Collections.Generic;

public class SegmentStatBoostHandler : MonoBehaviour
{
    public static SegmentStatBoostHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Yardımcı fonksiyonlar
    public void ApplyStat(SegmentInstance inst, float amount, StatType statType)
    {
        if (StatsUI.Instance == null) return;
        switch (statType)
        {
            case StatType.Attack:
                StatsUI.Instance.AddStatBonus(attackDamage: amount);
                break;
            case StatType.Defence:
                StatsUI.Instance.AddStatBonus(defense: amount);
                break;
            case StatType.AttackSpeed:
                StatsUI.Instance.AddStatBonus(attackSpeed: amount);
                break;
            case StatType.MovementSpeed:
                StatsUI.Instance.AddStatBonus(movementSpeed: amount);
                break;
            case StatType.CriticalChance:
                StatsUI.Instance.AddStatBonus(criticalChance: amount);
                break;
        }
    }
    public void RemoveStat(SegmentInstance inst, float amount, StatType statType)
    {
        if (StatsUI.Instance == null) return;
        switch (statType)
        {
            case StatType.Attack:
                StatsUI.Instance.RemoveStatBonus(attackDamage: amount);
                break;
            case StatType.Defence:
                StatsUI.Instance.RemoveStatBonus(defense: amount);
                break;
            case StatType.AttackSpeed:
                StatsUI.Instance.RemoveStatBonus(attackSpeed: amount);
                break;
            case StatType.MovementSpeed:
                StatsUI.Instance.RemoveStatBonus(movementSpeed: amount);
                break;
            case StatType.CriticalChance:
                StatsUI.Instance.RemoveStatBonus(criticalChance: amount);
                break;
        }
    }

    // Tüm stat boost segmentlerini yeniden hesapla
    public void RecalculateAllStatBoosts()
    {
        var wheelManager = FindAnyObjectByType<WheelManager>();
        if (wheelManager == null) return;
        int slotCount = wheelManager.slots.Length;

        // 1. Aktif tüm stat boost segmentlerini bul
        List<SegmentInstance> allSegments = new List<SegmentInstance>();
        for (int i = 0; i < slotCount; i++)
        {
            foreach (Transform child in wheelManager.slots[i])
            {
                var inst = child.GetComponent<SegmentInstance>();
                if (inst != null && inst.data != null && inst.data.effectType == SegmentEffectType.StatBoost && !allSegments.Contains(inst) && child.gameObject.activeInHierarchy)
                    allSegments.Add(inst);
            }
        }

        // 2. SiblingAdjacency segmentlerini ve komşuluk haritasını oluştur
        var siblingSegments = new List<SegmentInstance>();
        foreach (var inst in allSegments)
        {
            if (inst.data.statBonusMode == StatBonusMode.SiblingAdjacency)
                siblingSegments.Add(inst);
        }
        var siblingMap = new Dictionary<SegmentInstance, bool>();
        foreach (var inst in siblingSegments)
        {
            bool hasSibling = false;
            for (int s = 0; s < inst.data.size; s++)
            {
                int left = (inst.startSlotIndex + s - 1 + slotCount) % slotCount;
                int right = (inst.startSlotIndex + s + 1) % slotCount;
                foreach (Transform child in wheelManager.slots[left])
                {
                    var seg = child.GetComponent<SegmentInstance>();
                    if (seg != null && seg != inst && seg.data.segmentID == inst.data.segmentID)
                        hasSibling = true;
                }
                foreach (Transform child in wheelManager.slots[right])
                {
                    var seg = child.GetComponent<SegmentInstance>();
                    if (seg != null && seg != inst && seg.data.segmentID == inst.data.segmentID)
                        hasSibling = true;
                }
            }
            siblingMap[inst] = hasSibling;
        }

        // 3. Tüm statları sıfırla
        foreach (var inst in allSegments)
        {
            StatType statType = inst.data.statType;
            if (inst._appliedStatBoost != 0f)
            {
                RemoveStat(inst, inst._appliedStatBoost, statType);
                inst._appliedStatBoost = 0f;
            }
        }

        // 4. Tüm statları tekrar ekle
        foreach (var inst in allSegments)
        {
            StatType statType = inst.data.statType;
            float boost = inst.data.statBonusMode == StatBonusMode.SiblingAdjacency
                ? CalculateStatBoost(inst, wheelManager, siblingMap)
                : CalculateStatBoost(inst, wheelManager);
            inst._appliedStatBoost = boost;
            ApplyStat(inst, boost, statType);
        }
    }

    // Stat boost miktarını hesapla (tüm modlar için)
    private float CalculateStatBoost(SegmentInstance inst, WheelManager wheelManager, Dictionary<SegmentInstance, bool> siblingMap = null)
    {
        var data = inst.data;
        int slotCount = wheelManager.slots.Length;
        float baseAmount = data.statAmount;
        switch (data.statBonusMode)
        {
            case StatBonusMode.Fixed:
                return baseAmount;
            case StatBonusMode.EmptySlotCount:
                int empty = 0;
                for (int i = 0; i < slotCount; i++) if (!wheelManager.slotOccupied[i]) empty++;
                return baseAmount * empty;
            case StatBonusMode.FilledSlotCount:
                int filled = 0;
                for (int i = 0; i < slotCount; i++) if (wheelManager.slotOccupied[i]) filled++;
                return baseAmount * filled;
            case StatBonusMode.SmallSegmentCount:
                int small = 0;
                foreach (var seg in GetAllSegments(wheelManager)) if (seg.data.size == 1) small++;
                return baseAmount * small;
            case StatBonusMode.LargeSegmentCount:
                int large = 0;
                foreach (var seg in GetAllSegments(wheelManager)) if (seg.data.size > 1) large++;
                return baseAmount * large;
            case StatBonusMode.SiblingAdjacency:
                // Sadece boyut 1 segmentler için yan yana boost
                if (inst.data.size != 1) return baseAmount;
                int left1 = (inst.startSlotIndex - 1 + slotCount) % slotCount;
                int right1 = (inst.startSlotIndex + 1) % slotCount;
                bool hasSibling1 = false;
                foreach (Transform child in wheelManager.slots[left1])
                {
                    var seg = child.GetComponent<SegmentInstance>();
                    if (seg != null && seg != inst && seg.data.segmentID == inst.data.segmentID)
                        hasSibling1 = true;
                }
                foreach (Transform child in wheelManager.slots[right1])
                {
                    var seg = child.GetComponent<SegmentInstance>();
                    if (seg != null && seg != inst && seg.data.segmentID == inst.data.segmentID)
                        hasSibling1 = true;
                }
                return hasSibling1 ? baseAmount * 2f : baseAmount;
            case StatBonusMode.Persistent:
                // Sadece instance boost'unu uygula
                return inst._currentStatAmount;
            default:
                return baseAmount;
        }
    }

    // Tüm segmentleri döndür (StatBoost, WheelManipulation, OnRemoveEffect fark etmez)
    private List<SegmentInstance> GetAllSegments(WheelManager wheelManager)
    {
        List<SegmentInstance> list = new List<SegmentInstance>();
        int slotCount = wheelManager.slots.Length;
        for (int i = 0; i < slotCount; i++)
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
    private bool HasAdjacentSibling(SegmentInstance inst, WheelManager wheelManager)
    {
        int slotCount = wheelManager.slots.Length;
        int left = (inst.startSlotIndex - 1 + slotCount) % slotCount;
        int right = (inst.startSlotIndex + inst.data.size) % slotCount;
        SegmentInstance leftInst = null, rightInst = null;
        foreach (Transform child in wheelManager.slots[left])
        {
            var seg = child.GetComponent<SegmentInstance>();
            if (seg != null && seg != inst && seg.data.segmentID == inst.data.segmentID) leftInst = seg;
        }
        foreach (Transform child in wheelManager.slots[right])
        {
            var seg = child.GetComponent<SegmentInstance>();
            if (seg != null && seg != inst && seg.data.segmentID == inst.data.segmentID) rightInst = seg;
        }
        return leftInst != null || rightInst != null;
    }

    // Eski Activate/Deactivate fonksiyonları backward compatibility için bırakıldı
    public void ActivateSegmentEffect(SegmentData segmentData, int stackCount)
    {
        // Eski sistemle uyumlu kalması için, yeni sistemde RecalculateAllStatBoosts çağrılmalı
        RecalculateAllStatBoosts();
    }
    public void DeactivateSegmentEffect(SegmentData segmentData, int stackCount)
    {
        // Eski sistemle uyumlu kalması için, yeni sistemde RecalculateAllStatBoosts çağrılmalı
        RecalculateAllStatBoosts();
    }
} 