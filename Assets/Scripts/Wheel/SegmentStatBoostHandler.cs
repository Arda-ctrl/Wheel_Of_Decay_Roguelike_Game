using UnityEngine;
using System.Collections.Generic;

// Random stat stack sistemi için sınıf
[System.Serializable]
public class RandomStatEntry
{
    public StatType statType;
    public float amount;
    public int slotIndex; // Hangi slot için eklendi

    public RandomStatEntry(StatType type, float amt, int slot)
    {
        statType = type;
        amount = amt;
        slotIndex = slot;
    }
}

public class RandomStatStack
{
    private List<RandomStatEntry> statStack = new List<RandomStatEntry>();
    private SegmentInstance ownerSegment;
    private SegmentData data;
    private float statAmount;

    public RandomStatStack(SegmentInstance segment, SegmentData data, float statAmount)
    {
        ownerSegment = segment;
        this.data = data;
        this.statAmount = statAmount;
        // Seed'i kaldırdık, gerçek random için
    }

    // Dağıtımı bir kere yap, slot sayısı değişirse sadece ekle/çıkar
    public void SetStackCount(int newCount)
    {
        int currentCount = statStack.Count;
        Debug.Log($"[RandomStat] SetStackCount - Eski: {currentCount}, Yeni: {newCount}");
        
        if (newCount > currentCount)
        {
            // Yeni slotlar eklendi, yeni statlar ekle
            for (int i = currentCount; i < newCount; i++)
            {
                StatType randomStat = GetRandomStatType(data);
                statStack.Add(new RandomStatEntry(randomStat, statAmount, i));
                Debug.Log($"[RandomStat] Yeni stat eklendi: {randomStat} +{statAmount}");
            }
        }
        else if (newCount < currentCount)
        {
            // Slotlar azaldı, en son eklenen statları kaldır
            for (int i = 0; i < currentCount - newCount; i++)
            {
                var removed = statStack[statStack.Count - 1];
                Debug.Log($"[RandomStat] Stat kaldırıldı: {removed.statType} +{removed.amount}");
                statStack.RemoveAt(statStack.Count - 1);
            }
        }
    }

    public void ClearAllStats()
    {
        // Sadece stack'i temizle, RemoveStat çağırma (RecalculateAllStatBoosts'ta zaten temizleniyor)
        statStack.Clear();
    }

    // Yeni slot'a göre slot index'lerini güncelle
    public void UpdateSlotIndices(int newStartSlot)
    {
        for (int i = 0; i < statStack.Count; i++)
        {
            var entry = statStack[i];
            entry.slotIndex = newStartSlot + i; // Yeni slot'a göre güncelle
            statStack[i] = entry;
        }
    }

    // Stack'in dolu olup olmadığını kontrol et
    public bool HasStats()
    {
        return statStack.Count > 0;
    }

    // Tüm stat'ları al (kopya olarak)
    public List<RandomStatEntry> GetAllStats()
    {
        List<RandomStatEntry> copy = new List<RandomStatEntry>();
        foreach (var entry in statStack)
        {
            copy.Add(new RandomStatEntry(entry.statType, entry.amount, entry.slotIndex));
        }
        return copy;
    }

    // Saklanan stat'ları geri yükle
    public void RestoreStats(List<RandomStatEntry> savedStats)
    {
        // Mevcut stat'ları temizle
        ClearAllStats();
        
        // Saklanan stat'ları geri yükle
        foreach (var entry in savedStats)
        {
            statStack.Add(entry);
            SegmentStatBoostHandler.Instance.ApplyStat(ownerSegment, entry.amount, entry.statType);
        }
    }

    public StatType GetRandomStatType(SegmentData data)
    {
        List<StatType> availableStats = new List<StatType>();
        if (data.includeAttack) availableStats.Add(StatType.Attack);
        if (data.includeDefence) availableStats.Add(StatType.Defence);
        if (data.includeAttackSpeed) availableStats.Add(StatType.AttackSpeed);
        if (data.includeMovementSpeed) availableStats.Add(StatType.MovementSpeed);
        if (data.includeCriticalChance) availableStats.Add(StatType.CriticalChance);
        if (availableStats.Count == 0) return StatType.Attack;
        
        // Gerçek random seçim yap
        return availableStats[Random.Range(0, availableStats.Count)];
    }

    // LIFO stack metodları
    public void AddStatToTop(StatType statType, float amount)
    {
        statStack.Add(new RandomStatEntry(statType, amount, statStack.Count));
    }

    public RandomStatEntry RemoveStatFromTop()
    {
        if (statStack.Count == 0) return new RandomStatEntry(StatType.Attack, 0, 0);
        var lastEntry = statStack[statStack.Count - 1];
        statStack.RemoveAt(statStack.Count - 1);
        return lastEntry;
    }
}

public class SegmentStatBoostHandler : MonoBehaviour
{
    public static SegmentStatBoostHandler Instance { get; private set; }
    private Dictionary<SegmentInstance, RandomStatStack> randomStatStacks = new Dictionary<SegmentInstance, RandomStatStack>();
    // Segment ID'sine göre stack'leri sakla (segment silinip geri eklendiğinde korumak için)
    private Dictionary<string, RandomStatStack> persistentRandomStatStacks = new Dictionary<string, RandomStatStack>();
    // Decay/Growth segmentlerinin runtime değerleri
    private Dictionary<SegmentInstance, float> decayGrowthValues = new Dictionary<SegmentInstance, float>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Her spin sonrası decay/growth güncelle
    public void OnSpinEnd()
    {
        var wheelManager = FindAnyObjectByType<WheelManager>();
        if (wheelManager == null) return;
        int slotCount = wheelManager.slots.Length;
        for (int i = 0; i < slotCount; i++)
        {
            foreach (Transform child in wheelManager.slots[i])
            {
                var inst = child.GetComponent<SegmentInstance>();
                if (inst == null || inst.data == null) continue;
                var data = inst.data;
                if (data.statBonusMode == StatBonusMode.DecayOverTime)
                {
                    if (!decayGrowthValues.ContainsKey(inst))
                        decayGrowthValues[inst] = data.decayStartValue;
                    decayGrowthValues[inst] -= data.decayAmountPerSpin;
                    if (decayGrowthValues[inst] <= 0f && data.decayRemoveAtZero)
                    {
                        // Segmenti sil
                        WheelManager wm = FindAnyObjectByType<WheelManager>();
                        if (wm != null)
                            wm.RemoveSegmentAtSlot(inst.startSlotIndex);
                    }
                }
                else if (data.statBonusMode == StatBonusMode.GrowthOverTime)
                {
                    if (!decayGrowthValues.ContainsKey(inst))
                        decayGrowthValues[inst] = data.growthStartValue;
                    decayGrowthValues[inst] += data.growthAmountPerSpin;
                }
            }
        }
        // Decay/Growth değerleri güncellendikten sonra stat boost'ları yeniden hesapla
        RecalculateAllStatBoosts();
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

        // 3. Tüm statları sıfırla (random stat'lar hariç)
        foreach (var inst in allSegments)
        {
            StatType statType = inst.data.statType;
            if (inst._appliedStatBoost != 0f)
            {
                RemoveStat(inst, inst._appliedStatBoost, statType);
                inst._appliedStatBoost = 0f;
            }
            // Random stat'ları temizleme - onlar dinamik olarak güncelleniyor
        }

        // 4. Tüm statları tekrar ekle
        foreach (var inst in allSegments)
        {
            StatType statType = inst.data.statType;
            float boost = inst.data.statBonusMode == StatBonusMode.SiblingAdjacency
                ? CalculateStatBoost(inst, wheelManager, siblingMap)
                : CalculateStatBoost(inst, wheelManager);
            inst._appliedStatBoost = boost;
            
            // Random stat değilse normal uygula
            if (statType != StatType.Random)
            {
                ApplyStat(inst, boost, statType);
            }
            else
            {
                // Random stat'ları dinamik olarak güncelle (LIFO stack sistemi)
                Debug.Log($"[RandomStat] Segment: {inst.data.segmentID}, Boost: {boost}");
                if (randomStatStacks.ContainsKey(inst))
                {
                    var stack = randomStatStacks[inst];
                    int currentCount = stack.GetAllStats().Count;
                    int newCount = (int)boost;
                    Debug.Log($"[RandomStat] Stack bulundu, mevcut: {currentCount}, yeni: {newCount}");
                    
                    if (newCount > currentCount)
                    {
                        // Yeni stat'lar ekle (LIFO - en üste ekle)
                        for (int i = currentCount; i < newCount; i++)
                        {
                            StatType randomStat = GetRandomStatType(inst.data);
                            float statAmount = inst.data.statAmount;
                            stack.AddStatToTop(randomStat, statAmount);
                            Debug.Log($"[RandomStat] Yeni stat eklendi: {randomStat} +{statAmount}");
                            ApplyStat(inst, statAmount, randomStat);
                        }
                    }
                    else if (newCount < currentCount)
                    {
                        // En üstten stat'ları çıkar (LIFO - en üstten çıkar)
                        for (int i = 0; i < currentCount - newCount; i++)
                        {
                            var removedStat = stack.RemoveStatFromTop();
                            Debug.Log($"[RandomStat] Stat kaldırıldı: {removedStat.statType} +{removedStat.amount}");
                            RemoveStat(inst, removedStat.amount, removedStat.statType);
                        }
                    }
                    else
                    {
                        Debug.Log($"[RandomStat] Count aynı, hiçbir şey yapılmıyor");
                    }
                }
                else
                {
                    Debug.Log($"[RandomStat] Stack bulunamadı! Yeni stack oluşturuluyor...");
                    // Yeni stack oluştur
                    persistentRandomStatStacks[inst.data.segmentID] = new RandomStatStack(inst, inst.data, inst.data.statAmount);
                    randomStatStacks[inst] = persistentRandomStatStacks[inst.data.segmentID];
                    var newStack = randomStatStacks[inst];
                    newStack.SetStackCount((int)boost);
                    foreach (var entry in newStack.GetAllStats())
                    {
                        Debug.Log($"[RandomStat] Yeni stack'ten stat uygulanıyor: {entry.statType} +{entry.amount}");
                        ApplyStat(inst, entry.amount, entry.statType);
                    }
                }
            }
        }
    }

    // Tüm stat boostları temizle (random stat stack'leri koru)
    public void ClearAllStatBoosts()
    {
        var wheelManager = FindAnyObjectByType<WheelManager>();
        if (wheelManager == null) return;
        int slotCount = wheelManager.slots.Length;

        // Tüm stat boost segmentlerini bul
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

        // Tüm stat boostları sıfırla
        foreach (var inst in allSegments)
        {
            StatType statType = inst.data.statType;
            if (inst._appliedStatBoost != 0f)
            {
                RemoveStat(inst, inst._appliedStatBoost, statType);
                inst._appliedStatBoost = 0f;
            }
        }
    }

    // Stat boost miktarını hesapla (tüm modlar için)
    private float CalculateStatBoost(SegmentInstance inst, WheelManager wheelManager, Dictionary<SegmentInstance, bool> siblingMap = null)
    {
        var data = inst.data;
        int slotCount = wheelManager.slots.Length;
        float baseAmount = data.statAmount;

        // Decay/Growth segmentleri için runtime değerini kullan
        if (data.statBonusMode == StatBonusMode.DecayOverTime)
        {
            if (!decayGrowthValues.ContainsKey(inst))
                decayGrowthValues[inst] = data.decayStartValue;
            return Mathf.Max(0f, decayGrowthValues[inst]);
        }
        if (data.statBonusMode == StatBonusMode.GrowthOverTime)
        {
            if (!decayGrowthValues.ContainsKey(inst))
                decayGrowthValues[inst] = data.growthStartValue;
            return decayGrowthValues[inst];
        }

        // Eğer random stat ise stack sistemiyle yönet
        if (data.statType == StatType.Random)
        {
            Debug.Log($"[RandomStat] CalculateStatBoost - Segment: {inst.data.segmentID}, Mode: {data.statBonusMode}");
            int count = 0;
            switch (data.statBonusMode)
            {
                case StatBonusMode.Fixed:
                    count = 1;
                    break;
                case StatBonusMode.EmptySlotCount:
                    for (int i = 0; i < slotCount; i++) if (!wheelManager.slotOccupied[i]) count++;
                    break;
                case StatBonusMode.FilledSlotCount:
                    for (int i = 0; i < slotCount; i++) if (wheelManager.slotOccupied[i]) count++;
                    break;
                case StatBonusMode.SmallSegmentCount:
                    foreach (var seg in GetAllSegments(wheelManager)) if (seg.data.size == 1) count++;
                    break;
                case StatBonusMode.LargeSegmentCount:
                    foreach (var seg in GetAllSegments(wheelManager)) if (seg.data.size > 1) count++;
                    break;
                case StatBonusMode.SiblingAdjacency:
                    count = (inst.data.size == 1) ? 1 : 0;
                    break;
                case StatBonusMode.Persistent:
                    count = 1;
                    break;
                case StatBonusMode.Isolated:
                    count = 1;
                    break;
                case StatBonusMode.RarityAdjacency:
                    // Yanında targetRarity varsa 1, yoksa 0
                    int leftRarity = (inst.startSlotIndex - 1 + slotCount) % slotCount;
                    int rightRarity = (inst.startSlotIndex + inst.data.size) % slotCount;
                    bool hasTargetRarity = HasSegmentWithRarityInSlot(wheelManager, leftRarity, data.targetRarity, inst) || 
                                          HasSegmentWithRarityInSlot(wheelManager, rightRarity, data.targetRarity, inst);
                    count = hasTargetRarity ? 1 : 0;
                    break;
                case StatBonusMode.FlankGuard:
                    // Her iki yanı da doluysa 1, değilse 0
                    int leftFlank = (inst.startSlotIndex - 1 + slotCount) % slotCount;
                    int rightFlank = (inst.startSlotIndex + inst.data.size) % slotCount;
                    bool leftFilled = IsSlotOccupiedByOtherSegment(wheelManager, leftFlank, inst);
                    bool rightFilled = IsSlotOccupiedByOtherSegment(wheelManager, rightFlank, inst);
                    count = (leftFilled && rightFilled) ? 1 : 0;
                    break;
                default:
                    count = 1;
                    break;
            }
            Debug.Log($"[RandomStat] Hesaplanan count: {count}");
            
            // Persistent stack'te var mı kontrol et
            if (!persistentRandomStatStacks.ContainsKey(inst.data.segmentID))
            {
                Debug.Log($"[RandomStat] Yeni persistent stack oluşturuluyor");
                // Yeni stack oluştur
                persistentRandomStatStacks[inst.data.segmentID] = new RandomStatStack(inst, data, baseAmount);
            }
            
            // Runtime stack'e bağla
            randomStatStacks[inst] = persistentRandomStatStacks[inst.data.segmentID];
            
            // Random stat'lar için count değerini döndür (boost olarak kullanılacak)
            Debug.Log($"[RandomStat] Random stat için count döndürülüyor: {count}");
            return count;
        }

        switch (data.statBonusMode)
        {
            case StatBonusMode.Isolated:
                // Yanındaki slotlar boşsa isolatedBonusAmount ekle
                int left = (inst.startSlotIndex - 1 + slotCount) % slotCount;
                int right = (inst.startSlotIndex + inst.data.size) % slotCount;
                bool leftEmpty = !IsSlotOccupiedByOtherSegment(wheelManager, left, inst);
                bool rightEmpty = !IsSlotOccupiedByOtherSegment(wheelManager, right, inst);
                
                // Temel bonus + isolated bonus (sadece yanı boşsa)
                float isolatedBonus = (leftEmpty && rightEmpty) ? data.isolatedBonusAmount : 0f;
                return baseAmount + isolatedBonus;
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
                if (inst.data.size != 1) return baseAmount;
                int left1 = (inst.startSlotIndex - 1 + slotCount) % slotCount;
                int right1 = (inst.startSlotIndex + 1) % slotCount;
                bool hasSibling1 = HasSegmentWithSameIDInSlot(wheelManager, left1, inst.data.segmentID, inst) || 
                                   HasSegmentWithSameIDInSlot(wheelManager, right1, inst.data.segmentID, inst);
                return hasSibling1 ? baseAmount * 2f : baseAmount;
            case StatBonusMode.Persistent:
                return inst._currentStatAmount;
            case StatBonusMode.RarityAdjacency:
                // Yanında targetRarity varsa bonus ver
                int leftRarity = (inst.startSlotIndex - 1 + slotCount) % slotCount;
                int rightRarity = (inst.startSlotIndex + inst.data.size) % slotCount;
                bool hasTargetRarity = HasSegmentWithRarityInSlot(wheelManager, leftRarity, data.targetRarity, inst) || 
                                      HasSegmentWithRarityInSlot(wheelManager, rightRarity, data.targetRarity, inst);
                
                // Temel bonus + rarity bonus
                float rarityBonus = hasTargetRarity ? data.rarityBonusAmount : 0f;
                return baseAmount + rarityBonus;
            case StatBonusMode.FlankGuard:
                // Her iki yanı da doluysa bonus ver
                int leftFlank = (inst.startSlotIndex - 1 + slotCount) % slotCount;
                int rightFlank = (inst.startSlotIndex + inst.data.size) % slotCount;
                bool leftFilled = IsSlotOccupiedByOtherSegment(wheelManager, leftFlank, inst);
                bool rightFilled = IsSlotOccupiedByOtherSegment(wheelManager, rightFlank, inst);
                
                // Temel bonus + flank bonus
                float flankBonus = (leftFilled && rightFilled) ? data.flankGuardBonusAmount : 0f;
                return baseAmount + flankBonus;
            default:
                return baseAmount;
        }
    }
    
    // Random stat için tek seferlik boost hesapla
    private float GetRandomStatAmount(SegmentData data, float baseAmount)
    {
        StatType randomStat = GetRandomStatType(data);
        // Bu fonksiyon sadece Fixed mod için kullanılır
        return baseAmount;
    }
    
    // Random stat tipi seç
    private StatType GetRandomStatType(SegmentData data)
    {
        List<StatType> availableStats = new List<StatType>();
        if (data.includeAttack) availableStats.Add(StatType.Attack);
        if (data.includeDefence) availableStats.Add(StatType.Defence);
        if (data.includeAttackSpeed) availableStats.Add(StatType.AttackSpeed);
        if (data.includeMovementSpeed) availableStats.Add(StatType.MovementSpeed);
        if (data.includeCriticalChance) availableStats.Add(StatType.CriticalChance);

        if (availableStats.Count == 0) return StatType.Attack; // Fallback
        return availableStats[Random.Range(0, availableStats.Count)];
    }

    // Tüm segmentleri döndür (StatBoost, WheelManipulation, OnRemoveEffect, CurseEffect fark etmez)
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

    // Büyük segmentler için komşuluk kontrolü yapan yardımcı method
    private bool IsSlotOccupiedByOtherSegment(WheelManager wheelManager, int targetSlot, SegmentInstance excludeSegment)
    {
        int slotCount = wheelManager.slots.Length;
        for (int i = 0; i < slotCount; i++)
        {
            foreach (Transform child in wheelManager.slots[i])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != excludeSegment)
                {
                    int segStart = seg.startSlotIndex;
                    int segSize = seg.data.size;
                    for (int s = 0; s < segSize; s++)
                    {
                        int coveredSlot = (segStart + s) % slotCount;
                        if (coveredSlot == targetSlot)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    // Büyük segmentler için belirli nadirlikte segment kontrolü yapan yardımcı method
    private bool HasSegmentWithRarityInSlot(WheelManager wheelManager, int targetSlot, Rarity targetRarity, SegmentInstance excludeSegment)
    {
        int slotCount = wheelManager.slots.Length;
        for (int i = 0; i < slotCount; i++)
        {
            foreach (Transform child in wheelManager.slots[i])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != excludeSegment && seg.data.rarity == targetRarity)
                {
                    int segStart = seg.startSlotIndex;
                    int segSize = seg.data.size;
                    for (int s = 0; s < segSize; s++)
                    {
                        int coveredSlot = (segStart + s) % slotCount;
                        if (coveredSlot == targetSlot)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    // Büyük segmentler için aynı ID'li segment kontrolü yapan yardımcı method
    private bool HasSegmentWithSameIDInSlot(WheelManager wheelManager, int targetSlot, string segmentID, SegmentInstance excludeSegment)
    {
        int slotCount = wheelManager.slots.Length;
        for (int i = 0; i < slotCount; i++)
        {
            foreach (Transform child in wheelManager.slots[i])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != excludeSegment && seg.data.segmentID == segmentID)
                {
                    int segStart = seg.startSlotIndex;
                    int segSize = seg.data.size;
                    for (int s = 0; s < segSize; s++)
                    {
                        int coveredSlot = (segStart + s) % slotCount;
                        if (coveredSlot == targetSlot)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
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

    // Segment yok edilirken random stat stack'teki boostları eksilt
    public static void RemoveAllRandomStatsFor(SegmentInstance inst)
    {
        if (Instance == null) return;
        if (Instance.randomStatStacks.ContainsKey(inst))
        {
            // Stack'teki stat'ları manuel olarak temizle
            var stack = Instance.randomStatStacks[inst];
            foreach (var entry in stack.GetAllStats())
            {
                Instance.RemoveStat(inst, entry.amount, entry.statType);
            }
            stack.ClearAllStats();
            Instance.randomStatStacks.Remove(inst);
        }
        // Persistent stack'i silme, segment geri eklendiğinde kullanılabilir
    }

    // Random stat stack'ini al
    public RandomStatStack GetRandomStatStack(SegmentInstance segment)
    {
        if (randomStatStacks.ContainsKey(segment))
            return randomStatStacks[segment];
        return null;
    }

    // RandomEscapeCurse sonrası random stat stack'lerini yeni slot'lara göre güncelle
    public void UpdateRandomStatStacksAfterRelocation(Dictionary<SegmentInstance, int> segmentNewSlots)
    {
        foreach (var kvp in segmentNewSlots)
        {
            var segment = kvp.Key;
            var newSlot = kvp.Value;
            
            // Persistent stack'te var mı kontrol et
            if (persistentRandomStatStacks.ContainsKey(segment.data.segmentID))
            {
                var stack = persistentRandomStatStacks[segment.data.segmentID];
                // Random stat stack'ini yeni slot'a göre güncelle
                stack.UpdateSlotIndices(newSlot);
            }
        }
    }
} 