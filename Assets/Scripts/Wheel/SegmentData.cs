using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
public enum Type { Blood, Support, Defence, Attack, Combo, Element }
public enum StatType { Attack, Defence, AttackSpeed, MovementSpeed, CriticalChance }

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
    [Header("Segment Özelliği")]
    [Tooltip("Hangi statı arttıracağını seç")] public StatType statType;
    public float statAmount;
}