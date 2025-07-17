using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
public enum Type { StatBoost, WheelManipulation, OnRemoveEffect }
public enum StatType { Attack, Defence, AttackSpeed, MovementSpeed, CriticalChance }
public enum SegmentEffectType { StatBoost, WheelManipulation, OnRemoveEffect }
public enum WheelManipulationType { BlackHole, Redirector, Repulsor, MirrorRedirect, CommonRedirector, SafeEscape, ExplosiveEscape }
public enum RedirectDirection { LeftToRight, RightToLeft, BothSides }
public enum RewardFillMode { FillWithOnes, FillWithLargest }

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

    // Wheel manipulation için
    [Header("Wheel Manipulation Ayarları")]
    public WheelManipulationType wheelManipulationType;
    public RedirectDirection redirectDirection;
    [Range(1, 5)] public int blackHoleRange = 1;
    [Range(1, 5)] public int repulsorRange = 1;
    // CommonRedirector için
    [Range(1, 5)] public int commonRedirectorRange = 1;
    public Rarity commonRedirectorMinRarity = Rarity.Common;
    public Rarity commonRedirectorMaxRarity = Rarity.Common;
    [Range(1, 5)] public int safeEscapeRange = 1;
    [Range(1, 5)] public int explosiveEscapeRange = 1;

    // Sadece OnRemoveEffect için
    [Header("Yok Olunca Ödül Bırakma (Sadece OnRemoveEffect)")]
    public Rarity rewardRarity;
    public Type rewardType;
    public RewardFillMode rewardFillMode = RewardFillMode.FillWithOnes;
    // Gerekirse ek parametreler eklenebilir
}