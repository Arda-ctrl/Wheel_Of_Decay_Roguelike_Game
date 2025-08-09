using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
public enum Type { StatBoost, WheelManipulation, OnRemoveEffect, CurseEffect }
public enum StatType { Attack, Defence, AttackSpeed, MovementSpeed, CriticalChance, Random }
public enum SegmentEffectType { StatBoost, WheelManipulation, OnRemoveEffect, CurseEffect }
public enum CurseEffectType { ReSpinCurse, RandomEscapeCurse, BlurredMemoryCurse, TeleportEscapeCurse, ExplosiveCurse }
public enum WheelManipulationType { BlackHole, Redirector, Repulsor, MirrorRedirect, ReverseMirrorRedirect, CommonRedirector, SafeEscape, ExplosiveEscape, SegmentSwapper }
public enum RedirectDirection { LeftToRight, RightToLeft, BothSides }
public enum RewardFillMode { FillWithOnes, FillWithLargest }
public enum StatBonusMode { Fixed, EmptySlotCount, FilledSlotCount, SmallSegmentCount, LargeSegmentCount, SiblingAdjacency, Persistent, Isolated, DecayOverTime, GrowthOverTime, RarityAdjacency, FlankGuard }

[CreateAssetMenu(fileName = "NewSegment", menuName = "Segment")]
public class SegmentData : ScriptableObject
{
    public string segmentID;
    [Range(1, 3)] public int size = 1;
    public Type type;
    public Rarity rarity;
    [TextArea(3, 5)] public string description;
    [Header("Görsel")] public GameObject segmentPrefab;
    [Header("Görünüm Ayarları")] public Color segmentColor = Color.white;

    [Header("Segment Tipi")] public SegmentEffectType effectType;

    // Stat boost için
    [Header("Stat Boost Ayarları")]
    public StatType statType;
    public float statAmount;
    public StatBonusMode statBonusMode = StatBonusMode.Fixed;

    // Isolated Segment Boost için
    [Header("Isolated Segment Boost Ayarları")]
    public float isolatedBonusAmount = 0f; // Yanı boşken

    // Rarity Adjacency için
    [Header("Rarity Adjacency Ayarları")]
    public Rarity targetRarity = Rarity.Common; // Hangi nadirlikte segment aranacak
    public float rarityBonusAmount = 0f; // Yanında targetRarity varsa bonus

    // FlankGuard için
    [Header("FlankGuard Ayarları")]
    public float flankGuardBonusAmount = 0f; // Her iki yanı da doluysa bonus

    // Decay Over Time
    [Header("Decay Over Time Ayarları")]
    public float decayStartValue = 0f;
    public float decayAmountPerSpin = 1f;
    public bool decayRemoveAtZero = true;

    // Growth Over Time
    [Header("Growth Over Time Ayarları")]
    public float growthStartValue = 0f;
    public float growthAmountPerSpin = 1f;

    // Random Stat için (sadece statType Random seçiliyse kullanılır)
    [Header("Random Stat Ayarları")]
    public bool includeAttack = true;
    public bool includeDefence = true;
    public bool includeAttackSpeed = true;
    public bool includeMovementSpeed = true;
    public bool includeCriticalChance = true;

    // Wheel manipulation için
    [Header("Wheel Manipulation Ayarları")]
    public WheelManipulationType wheelManipulationType;
    public RedirectDirection redirectDirection;
    [Range(1, 5)] public int blackHoleRange = 1;
    [Range(1, 5)] public int repulsorRange = 1;
    // ReverseMirrorRedirect için
    [Range(1, 5)] public int reverseMirrorRedirectRange = 1;
    // CommonRedirector için
    [Range(1, 5)] public int commonRedirectorRange = 1;
    public Rarity commonRedirectorMinRarity = Rarity.Common;
    public Rarity commonRedirectorMaxRarity = Rarity.Common;
    [Range(1, 5)] public int safeEscapeRange = 1;
    [Range(1, 5)] public int explosiveEscapeRange = 1;
    [Range(1, 5)] public int swapperRange = 1;

    // Sadece OnRemoveEffect için
    [Header("Yok Olunca Ödül Bırakma (Sadece OnRemoveEffect)")]
    public Rarity rewardRarity;
    public Type rewardType;
    public RewardFillMode rewardFillMode = RewardFillMode.FillWithOnes;
    // Gerekirse ek parametreler eklenebilir

    // Sadece CurseEffect için
    [Header("Lanet Efektleri (Sadece CurseEffect)")]
    public CurseEffectType curseEffectType;
    // ReSpinCurse için
    public int curseReSpinCount = 3;
    // BlurredMemoryCurse için
    public bool tooltipDisabled = false;
    // ExplosiveCurse için
    [Range(1, 3)] public int explosiveRange = 1;
}