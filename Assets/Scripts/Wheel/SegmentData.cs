using UnityEngine;

// SegmentData: Her bir segmentin (dilim) verisini tutan ScriptableObject.

/// <summary>
/// Segmentin nadirlik seviyesi.
/// </summary>
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Segmentin sınıf türü.
/// </summary>
public enum Type
{
    Blood,    // Kan büyüsü
    Support,  // Destek
    Defence,  // Savunma
    Attack,   // Saldırı
    Combo,    // Kombinasyon
    Element   // Element
}

[CreateAssetMenu(fileName = "NewSegment", menuName = "Segment")]
public class SegmentData : ScriptableObject
{
	public string segmentID; // Segmentin ID'si (örn: "BW", "BA1" gibi)
	[Range(1, 3)]
	public int size = 1; // Kaç slot kaplıyor (1–3 arası)

	public Type type; // Segmentin sınıfı
	public Rarity rarity; // Segmentin nadirlik seviyesi
	
	[TextArea(3, 5)]
	public string description; // Segmentin açıklaması

	public Sprite icon; // Artistin çizdiği sprite buraya atanacak
	
	[Header("Segment Özelliği")]
	[Tooltip("Hangi effect'i kullanacağını seç: 1=DamageBoost, 2=DamagePercentageBoost")]
	public int effectID; // Effect ID'si

	// Effect-specific fields
	[HideInInspector]
	public float damageBoostAmount; // Damage boost miktarı (effectID = 1)
	
	[HideInInspector]
	public float damagePercentageBoostAmount; // Damage percentage boost miktarı (effectID = 2)
}