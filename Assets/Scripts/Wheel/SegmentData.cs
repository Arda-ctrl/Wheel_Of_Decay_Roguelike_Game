using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
public enum Type { Blood, Support, Defence, Attack, Combo, Element }
public enum StatType { Attack, Defence, AttackSpeed, MovementSpeed, CriticalChance }
public enum SegmentEffectType { StatBoost, WheelManipulation }
public enum WheelManipulationType { BlackHole, Redirector }
public enum RedirectDirection { LeftToRight, RightToLeft, BothSides }

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
    // Redirector için yön seçimi
    public RedirectDirection redirectDirection;
    // Black Hole için menzil
    [Range(1, 5)] public int blackHoleRange = 1;
    // Gerekirse ek parametreler eklenebilir
}