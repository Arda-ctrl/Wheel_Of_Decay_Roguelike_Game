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
        // Debug.Log($"[RandomStat] *** SET STACK COUNT *** from {currentCount} to {newCount} for segment {data.segmentName}");
        
        if (newCount > currentCount)
        {
            // Yeni slotlar eklendi, yeni statlar ekle
            // Debug.Log($"[RandomStat] Adding {newCount - currentCount} new random stats to stack");
            for (int i = currentCount; i < newCount; i++)
            {
                StatType randomStat = GetRandomStatType(data);
                statStack.Add(new RandomStatEntry(randomStat, statAmount, i));
                // Debug.Log($"[RandomStat] Added to stack: {randomStat} +{statAmount}");
            }
        }
        else if (newCount < currentCount)
        {
            // Slotlar azaldı, en son eklenen statları kaldır
            for (int i = 0; i < currentCount - newCount; i++)
            {
                var removed = statStack[statStack.Count - 1];
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

    // Toplam boost değerini hesapla
    public float GetTotalBoost()
    {
        float totalBoost = 0f;
        foreach (var entry in statStack)
        {
            totalBoost += entry.amount;
        }
        return totalBoost;
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
                        decayGrowthValues[inst] = data.statAmount; // statAmount'u başlangıç değeri olarak kullan
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
                        decayGrowthValues[inst] = data.statAmount; // statAmount'u başlangıç değeri olarak kullan
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
        // Debug.Log($"[RandomStat] *** APPLY STAT TO UI *** {inst.data.segmentName} {statType} +{amount}");
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
        // Debug.Log($"[RandomStat] *** REMOVE STAT FROM UI *** {inst.data.segmentName} {statType} -{amount}");
        if (StatsUI.Instance == null)
        {
            return;
        }
        
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

    // Persistent segment'i initialize et
    public void InitializePersistentSegment(SegmentInstance inst)
    {
        if (inst == null || inst.data == null || inst.data.statBonusMode != StatBonusMode.Persistent) return;
        
        // İlk kez yerleştiriliyorsa base değerleri ayarla
        if (inst._baseStatAmount == 0f)
        {
            inst._baseStatAmount = inst.data.statAmount;
            inst._currentStatAmount = inst.data.statAmount;
        }
    }
    
    // Tüm stat boost segmentlerini yeniden hesapla
    public void RecalculateAllStatBoosts()
    {
        // Debug.Log($"[RandomStat] ===== RECALCULATE ALL STAT BOOSTS STARTED =====");
        // Debug.Log($"[RandomStat] Current persistent stacks: {persistentRandomStatStacks.Count}");
        // Debug.Log($"[RandomStat] Current runtime stacks: {randomStatStacks.Count}");
        
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

        // 3. Tüm statları sıfırla (random stat'lar DA DAHİL)
        // Debug.Log($"[RandomStat] Step 3: Clearing ALL existing stat boosts (including random stats)...");
        foreach (var inst in allSegments)
        {
            StatType statType = inst.data.statType;
            if (inst._appliedStatBoost != 0f)
            {
                // Debug.Log($"[RandomStat] REMOVING existing boost: {inst.data.segmentName} {statType} -{inst._appliedStatBoost}");
                RemoveStat(inst, inst._appliedStatBoost, statType);
                inst._appliedStatBoost = 0f;
            }
            
            // Random stat'ları da temizle
            if (statType == StatType.Random && randomStatStacks.ContainsKey(inst))
            {
                var stack = randomStatStacks[inst];
                var allStats = stack.GetAllStats();
                Debug.Log($"[RandomStat] CLEARING {allStats.Count} random stats from {inst.data.segmentName}");
                foreach (var entry in allStats)
                {
                    RemoveStat(inst, entry.amount, entry.statType);
                    Debug.Log($"[RandomStat] *** REMOVED FROM UI *** {entry.statType}: -{entry.amount}");
                }
            }
        }

        // 4. Tüm statları tekrar ekle
        // Debug.Log($"[RandomStat] Step 4: Recalculating and applying all stat boosts...");
        foreach (var inst in allSegments)
        {
            StatType statType = inst.data.statType;
            // Debug.Log($"[RandomStat] Processing segment: {inst.data.segmentName} (Type: {statType})");
            float boost = CalculateStatBoost(inst, wheelManager, siblingMap);
            inst._appliedStatBoost = boost;
            // Debug.Log($"[RandomStat] Calculated boost for {inst.data.segmentName}: {boost}");
            
            // Random stat değilse normal uygula
            if (statType != StatType.Random)
            {
                // Debug.Log($"[RandomStat] APPLYING normal stat: {inst.data.segmentName} {statType} +{boost}");
                ApplyStat(inst, boost, statType);
            }
            else
            {
                // Random stat'ları dinamik olarak güncelle (LIFO stack sistemi)
                if (randomStatStacks.ContainsKey(inst))
                {
                    var stack = randomStatStacks[inst];
                    int currentCount = stack.GetAllStats().Count;
                    int newCount = CalculateRandomStatCount(inst, wheelManager, siblingMap);
                    
                    Debug.Log($"[RandomStat] *** EXISTING STACK *** for {inst.data.segmentName}: currentCount={currentCount}, newCount={newCount}");
                    Debug.Log($"[RandomStat] Stack details: segmentID={inst.data.segmentID}, persistent exists={persistentRandomStatStacks.ContainsKey(inst.data.segmentID)}");
                    
                    if (newCount > currentCount)
                    {
                        // Yeni stat'lar ekle (LIFO - en üste ekle)
                        Debug.Log($"[RandomStat] Adding {newCount - currentCount} stats to {inst.data.segmentName}");
                        for (int i = currentCount; i < newCount; i++)
                        {
                            StatType randomStat = GetRandomStatType(inst.data);
                            float statAmount = inst.data.statAmount;
                            stack.AddStatToTop(randomStat, statAmount);
                            ApplyStat(inst, statAmount, randomStat);
                            Debug.Log($"[RandomStat] Added {randomStat}: +{statAmount}");
                        }
                    }
                    else if (newCount < currentCount)
                    {
                        // En üstten stat'ları çıkar (LIFO - en üstten çıkar)
                        Debug.Log($"[RandomStat] Removing {currentCount - newCount} stats from {inst.data.segmentName}");
                        for (int i = 0; i < currentCount - newCount; i++)
                        {
                            var removedStat = stack.RemoveStatFromTop();
                            RemoveStat(inst, removedStat.amount, removedStat.statType);
                            Debug.Log($"[RandomStat] Removed {removedStat.statType}: -{removedStat.amount}");
                        }
                    }
                    else
                    {
                        // Count aynı - recalculate sırasında tüm statları yeniden uygula
                        Debug.Log($"[RandomStat] No change needed for {inst.data.segmentName} (count={newCount})");
                        Debug.Log($"[RandomStat] Re-applying all stats after recalculate...");
                        
                        // Mevcut stack'teki tüm statları UI'a uygula (recalculate sonrası)
                        var allStats = stack.GetAllStats();
                        foreach (var entry in allStats)
                        {
                            ApplyStat(inst, entry.amount, entry.statType);
                            Debug.Log($"[RandomStat] *** RE-APPLIED *** {entry.statType}: +{entry.amount}");
                        }
                    }
                }
                else
                {
                    // Yeni stack oluştur
                    int newCount = CalculateRandomStatCount(inst, wheelManager, siblingMap);
                    Debug.Log($"[RandomStat] *** CREATING NEW STACK *** for {inst.data.segmentName}: count={newCount}");
                    
                    // Persistent stack'te var mı kontrol et
                    if (persistentRandomStatStacks.ContainsKey(inst.data.segmentID))
                    {
                        Debug.Log($"[RandomStat] WARNING: Persistent stack already exists for {inst.data.segmentID}!");
                        randomStatStacks[inst] = persistentRandomStatStacks[inst.data.segmentID];
                    }
                    else
                    {
                        Debug.Log($"[RandomStat] Creating brand new persistent stack for {inst.data.segmentID}");
                        persistentRandomStatStacks[inst.data.segmentID] = new RandomStatStack(inst, inst.data, inst.data.statAmount);
                        randomStatStacks[inst] = persistentRandomStatStacks[inst.data.segmentID];
                    }
                    
                    var newStack = randomStatStacks[inst];
                    newStack.SetStackCount(newCount);
                    Debug.Log($"[RandomStat] Stack set to count: {newCount}, actual entries: {newStack.GetAllStats().Count}");
                    
                    // Yeni stack'teki tüm statları UI'a uygula
                    var newStackStats = newStack.GetAllStats();
                    Debug.Log($"[RandomStat] Applying {newStackStats.Count} stats from new stack to UI...");
                    foreach (var entry in newStackStats)
                    {
                        ApplyStat(inst, entry.amount, entry.statType);
                        Debug.Log($"[RandomStat] *** APPLIED NEW *** {entry.statType}: +{entry.amount}");
                    }
                }
            }
        }
        // Debug.Log($"[RandomStat] ===== RECALCULATE ALL STAT BOOSTS FINISHED =====");
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
        if (inst == null || inst.data == null) return 0f;
        
        var data = inst.data;
        float boost = 0f;
        
        if (data.effectType == SegmentEffectType.StatBoost)
        {
            switch (data.statType)
            {
                case StatType.Random:
                    // Random stat için mevcut stack'i kontrol et
                    if (randomStatStacks.ContainsKey(inst))
                    {
                        var stack = randomStatStacks[inst];
                        int currentCount = stack.GetAllStats().Count;
                        int newCount = CalculateRandomStatCount(inst, wheelManager, siblingMap);
                        
                        if (newCount != currentCount)
                        {
                            stack.SetStackCount(newCount);
                        }
                        
                        boost = stack.GetTotalBoost();
                    }
                    else
                    {
                        // Yeni stack oluştur
                        int count = CalculateRandomStatCount(inst, wheelManager, siblingMap);
                        if (count > 0)
                        {
                            var newStack = new RandomStatStack(inst, data, data.statAmount);
                            newStack.SetStackCount(count);
                            randomStatStacks[inst] = newStack;
                            persistentRandomStatStacks[inst.data.segmentID] = newStack;
                            
                            boost = newStack.GetTotalBoost();
                        }
                    }
                    break;
                    
                default:
                    // Normal stat boost hesaplama
                    boost = CalculateNormalStatBoost(inst, wheelManager, siblingMap);
                    break;
            }
        }
        
        return boost;
    }
    
    // Normal stat boost hesaplama (Random hariç tüm stat tipleri için)
    private float CalculateNormalStatBoost(SegmentInstance inst, WheelManager wheelManager, Dictionary<SegmentInstance, bool> siblingMap = null)
    {
        if (inst == null || inst.data == null) return 0f;
        
        var data = inst.data;
        float baseAmount = data.statAmount;
        float boost = 0f;
        
        switch (data.statBonusMode)
        {
            case StatBonusMode.Fixed:
                boost = baseAmount;
                break;
                
            case StatBonusMode.EmptySlotCount:
                int emptySlots = CountEmptySlots(wheelManager);
                boost = baseAmount * emptySlots;
                break;
                
            case StatBonusMode.FilledSlotCount:
                int filledSlots = CountFilledSlots(wheelManager);
                boost = baseAmount * filledSlots;
                break;
                
            case StatBonusMode.SiblingAdjacency:
                bool hasSibling = false;
                if (siblingMap != null && siblingMap.ContainsKey(inst))
                {
                    hasSibling = siblingMap[inst];
                }
                else
                {
                    hasSibling = HasAdjacentSibling(inst, wheelManager);
                }
                boost = hasSibling ? baseAmount * 2 : baseAmount;
                break;
                
            case StatBonusMode.DecayOverTime:
                // Decay için runtime değer kullan
                if (decayGrowthValues.ContainsKey(inst))
                {
                    boost = decayGrowthValues[inst];
                }
                else
                {
                    // İlk kez hesaplanıyor, başlangıç değerini kullan
                    boost = baseAmount; // statAmount'u başlangıç değeri olarak kullan
                    decayGrowthValues[inst] = boost;
                }
                break;
                
            case StatBonusMode.GrowthOverTime:
                // Growth için runtime değer kullan
                if (decayGrowthValues.ContainsKey(inst))
                {
                    boost = decayGrowthValues[inst];
                }
                else
                {
                    // İlk kez hesaplanıyor, başlangıç değerini kullan
                    boost = baseAmount; // statAmount'u başlangıç değeri olarak kullan
                    decayGrowthValues[inst] = boost;
                }
                break;
                
            case StatBonusMode.Isolated:
                bool isIsolated = IsSegmentIsolated(inst, wheelManager);
                boost = isIsolated ? baseAmount + data.isolatedBonusAmount : baseAmount;
                break;
                
            case StatBonusMode.RarityAdjacency:
                bool hasTargetRarityAdjacent = HasAdjacentSegmentWithRarity(inst, wheelManager, data.targetRarity);
                boost = hasTargetRarityAdjacent ? baseAmount + data.rarityBonusAmount : baseAmount;
                break;
                
            case StatBonusMode.FlankGuard:
                bool isFlankGuarded = IsSegmentFlankGuarded(inst, wheelManager);
                boost = isFlankGuarded ? baseAmount + data.flankGuardBonusAmount : baseAmount;
                break;
                
            case StatBonusMode.SmallSegmentCount:
                int smallSegmentCount = CountSmallSegments(wheelManager);
                boost = baseAmount * smallSegmentCount;
                break;
                
            case StatBonusMode.LargeSegmentCount:
                int largeSegmentCount = CountLargeSegments(wheelManager);
                boost = baseAmount * largeSegmentCount;
                break;
                
            case StatBonusMode.Persistent:
                // Persistent segment'ler için runtime boost değerini kullan
                boost = inst._currentStatAmount;
                break;
                
            default:
                boost = baseAmount;
                break;
        }
        
        return boost;
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
        if (Instance == null)
        {
            return;
        }
        
        // Debug.Log($"[RandomStat] *** REMOVE ALL RANDOM STATS *** for {inst.data.segmentName}");
        
        if (Instance.randomStatStacks.ContainsKey(inst))
        {
            var stack = Instance.randomStatStacks[inst];
            if (stack != null)
            {
                // Stack'teki tüm statları kaldır
                var allStats = stack.GetAllStats();
                // Debug.Log($"[RandomStat] Removing {allStats.Count} random stats from {inst.data.segmentName}");
                foreach (var entry in allStats)
                {
                    Instance.RemoveStat(inst, entry.amount, entry.statType);
                    // Debug.Log($"[RandomStat] Removed {entry.statType}: -{entry.amount}");
                }
                
                // Stack'i temizle
                stack.ClearAllStats();
                Instance.randomStatStacks.Remove(inst);
            }
        }
        else
        {
            // Debug.Log($"[RandomStat] No random stat stack found for {inst.data.segmentName}");
        }
        
        // Persistent stack'i de temizle
        if (Instance.persistentRandomStatStacks.ContainsKey(inst.data.segmentID))
        {
            Instance.persistentRandomStatStacks.Remove(inst.data.segmentID);
            // Debug.Log($"[RandomStat] Removed persistent stack for {inst.data.segmentID}");
        }
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

    private int CalculateRandomStatCount(SegmentInstance inst, WheelManager wheelManager, Dictionary<SegmentInstance, bool> siblingMap = null)
    {
        if (inst == null || inst.data == null) return 0;
        
        var data = inst.data;
        int count = 0;
        
        // Debug.Log($"[RandomStat] CalculateRandomStatCount for {data.segmentName} (Mode: {data.statBonusMode})");
        
        switch (data.statBonusMode)
        {
            case StatBonusMode.Fixed:
                count = Mathf.RoundToInt(data.statAmount);
                Debug.Log($"[RandomStat] Fixed mode: statAmount={data.statAmount}, count={count}");
                break;
                
            case StatBonusMode.EmptySlotCount:
                int emptySlots = CountEmptySlots(wheelManager);
                count = emptySlots; // Toplam boş slot sayısı
                Debug.Log($"[RandomStat] EmptySlotCount mode: emptySlots={emptySlots}, count={count}");
                break;
                
            case StatBonusMode.FilledSlotCount:
                int filledSlots = CountFilledSlots(wheelManager);
                count = filledSlots; // Toplam dolu slot sayısı
                Debug.Log($"[RandomStat] FilledSlotCount mode: filledSlots={filledSlots}, count={count}");
                break;
                
            case StatBonusMode.SiblingAdjacency:
                bool hasSibling = false;
                if (siblingMap != null && siblingMap.ContainsKey(inst))
                {
                    hasSibling = siblingMap[inst];
                }
                else
                {
                    hasSibling = HasAdjacentSibling(inst, wheelManager);
                }
                count = hasSibling ? 2 : 1;
                Debug.Log($"[RandomStat] SiblingAdjacency mode: hasSibling={hasSibling}, count={count}");
                break;
                
            case StatBonusMode.DecayOverTime:
                // Decay için runtime değer kullan
                if (decayGrowthValues.ContainsKey(inst))
                {
                    count = Mathf.RoundToInt(decayGrowthValues[inst]);
                    Debug.Log($"[RandomStat] DecayOverTime mode: runtime value={decayGrowthValues[inst]}, count={count}");
                }
                else
                {
                    count = Mathf.RoundToInt(data.statAmount); // Başlangıç değeri
                    decayGrowthValues[inst] = data.statAmount;
                    Debug.Log($"[RandomStat] DecayOverTime mode: initial value={data.statAmount}, count={count}");
                }
                break;
                
            case StatBonusMode.GrowthOverTime:
                // Growth için runtime değer kullan
                if (decayGrowthValues.ContainsKey(inst))
                {
                    count = Mathf.RoundToInt(decayGrowthValues[inst]);
                    Debug.Log($"[RandomStat] GrowthOverTime mode: runtime value={decayGrowthValues[inst]}, count={count}");
                }
                else
                {
                    count = Mathf.RoundToInt(data.statAmount); // Başlangıç değeri
                    decayGrowthValues[inst] = data.statAmount;
                    Debug.Log($"[RandomStat] GrowthOverTime mode: initial value={data.statAmount}, count={count}");
                }
                break;
                
            case StatBonusMode.SmallSegmentCount:
                int smallSegmentCount = CountSmallSegments(wheelManager);
                count = Mathf.RoundToInt(data.statAmount * smallSegmentCount);
                // Debug.Log($"[RandomStat] SmallSegmentCount mode: smallSegments={smallSegmentCount}, statAmount={data.statAmount}, count={count}");
                break;
                
            case StatBonusMode.LargeSegmentCount:
                int largeSegmentCount = CountLargeSegments(wheelManager);
                count = Mathf.RoundToInt(data.statAmount * largeSegmentCount);
                // Debug.Log($"[RandomStat] LargeSegmentCount mode: largeSegments={largeSegmentCount}, statAmount={data.statAmount}, count={count}");
                break;
                
            default:
                Debug.LogWarning($"[RandomStat] Unknown statBonusMode: {data.statBonusMode} for {data.segmentName}");
                break;
        }
        
        int finalCount = Mathf.Max(0, count);
        // Debug.Log($"[RandomStat] Final count for {data.segmentName}: {finalCount}");
        return finalCount;
    }

    private int CountSegmentsInRange(SegmentInstance inst, WheelManager wheelManager, int range)
    {
        int count = 0;
        int slotCount = wheelManager.slots.Length;
        int startSlot = inst.startSlotIndex;

        for (int i = -range; i <= range; i++)
        {
            int currentSlot = (startSlot + i + slotCount) % slotCount;
            foreach (Transform child in wheelManager.slots[currentSlot])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != inst && seg.data.segmentID == inst.data.segmentID)
                {
                    count++;
                }
            }
        }
        return count;
    }
    
    // Boş slot sayısını hesapla (WheelManager'ın slotOccupied array'ini kullanarak)
    private int CountEmptySlots(WheelManager wheelManager)
    {
        int emptyCount = 0;
        int slotCount = wheelManager.slots.Length;
        
        for (int i = 0; i < slotCount; i++)
        {
            if (!wheelManager.slotOccupied[i]) // WheelManager'ın slotOccupied array'ini kullan
            {
                emptyCount++;
            }
        }
        
        // Debug.Log($"[RandomStat] CountEmptySlots: {emptyCount}/{slotCount} slots are empty");
        return emptyCount;
    }
    
    // Dolu slot sayısını hesapla (WheelManager'ın slotOccupied array'ini kullanarak)
    private int CountFilledSlots(WheelManager wheelManager)
    {
        int filledCount = 0;
        int slotCount = wheelManager.slots.Length;
        
        for (int i = 0; i < slotCount; i++)
        {
            if (wheelManager.slotOccupied[i]) // WheelManager'ın slotOccupied array'ini kullan
            {
                filledCount++;
            }
        }
        
        // Debug.Log($"[RandomStat] CountFilledSlots: {filledCount}/{slotCount} slots are filled");
        return filledCount;
    }
    
    // Size = 1 olan segmentlerin sayısını say
    private int CountSmallSegments(WheelManager wheelManager)
    {
        int smallCount = 0;
        var allSegments = GetAllSegments(wheelManager);
        
        foreach (var segment in allSegments)
        {
            if (segment.data.size == 1)
            {
                smallCount++;
            }
        }
        
        return smallCount;
    }
    
    // Size = 3 olan segmentlerin sayısını say
    private int CountLargeSegments(WheelManager wheelManager)
    {
        int largeCount = 0;
        var allSegments = GetAllSegments(wheelManager);
        
        foreach (var segment in allSegments)
        {
            if (segment.data.size == 3)
            {
                largeCount++;
            }
        }
        
        return largeCount;
    }
    
    // Segmentin izole olup olmadığını kontrol et
    private bool IsSegmentIsolated(SegmentInstance inst, WheelManager wheelManager)
    {
        int slotCount = wheelManager.slots.Length;
        
        // Segmentin kapladığı tüm slotların yanındaki slotları kontrol et
        for (int s = 0; s < inst.data.size; s++)
        {
            int currentSlot = (inst.startSlotIndex + s) % slotCount;
            int leftSlot = (currentSlot - 1 + slotCount) % slotCount;
            int rightSlot = (currentSlot + 1) % slotCount;
            
            // Sol tarafı kontrol et
            foreach (Transform child in wheelManager.slots[leftSlot])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != inst && child.gameObject.activeInHierarchy)
                {
                    return false; // Yanında segment var, izole değil
                }
            }
            
            // Sağ tarafı kontrol et
            foreach (Transform child in wheelManager.slots[rightSlot])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != inst && child.gameObject.activeInHierarchy)
                {
                    return false; // Yanında segment var, izole değil
                }
            }
        }
        
        return true; // Tüm yanlar boş, izole
    }
    
    // Belirtilen nadirlikte komşu segment olup olmadığını kontrol et
    private bool HasAdjacentSegmentWithRarity(SegmentInstance inst, WheelManager wheelManager, Rarity targetRarity)
    {
        int slotCount = wheelManager.slots.Length;
        
        for (int s = 0; s < inst.data.size; s++)
        {
            int currentSlot = (inst.startSlotIndex + s) % slotCount;
            int leftSlot = (currentSlot - 1 + slotCount) % slotCount;
            int rightSlot = (currentSlot + 1) % slotCount;
            
            // Sol tarafı kontrol et
            foreach (Transform child in wheelManager.slots[leftSlot])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != inst && seg.data.rarity == targetRarity && child.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            
            // Sağ tarafı kontrol et
            foreach (Transform child in wheelManager.slots[rightSlot])
            {
                var seg = child.GetComponent<SegmentInstance>();
                if (seg != null && seg != inst && seg.data.rarity == targetRarity && child.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // Segmentin her iki yanının da dolu olup olmadığını kontrol et (FlankGuard)
    private bool IsSegmentFlankGuarded(SegmentInstance inst, WheelManager wheelManager)
    {
        int slotCount = wheelManager.slots.Length;
        
        // Segmentin başındaki ve sonundaki slotların yanını kontrol et
        int leftSlot = (inst.startSlotIndex - 1 + slotCount) % slotCount;
        int rightSlot = (inst.startSlotIndex + inst.data.size) % slotCount;
        
        bool hasLeftSegment = false;
        bool hasRightSegment = false;
        
        // Sol taraf kontrol
        foreach (Transform child in wheelManager.slots[leftSlot])
        {
            var seg = child.GetComponent<SegmentInstance>();
            if (seg != null && seg != inst && child.gameObject.activeInHierarchy)
            {
                hasLeftSegment = true;
                break;
            }
        }
        
        // Sağ taraf kontrol
        foreach (Transform child in wheelManager.slots[rightSlot])
        {
            var seg = child.GetComponent<SegmentInstance>();
            if (seg != null && seg != inst && child.gameObject.activeInHierarchy)
            {
                hasRightSegment = true;
                break;
            }
        }
        
        return hasLeftSegment && hasRightSegment; // Her iki yan da dolu olmalı
    }
} 