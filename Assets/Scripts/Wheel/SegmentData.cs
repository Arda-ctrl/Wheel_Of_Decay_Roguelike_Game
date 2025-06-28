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

/// <summary>
/// Segment özelliklerinin temel sınıfı.
/// Her segment özelliği bu sınıftan türetilecek.
/// </summary>
public abstract class SegmentEffect : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string effectName;
    [TextArea(2, 4)]
    public string effectDescription;
    
    /// <summary>
    /// Segment aktif olduğunda çalışacak özellik.
    /// </summary>
    /// <param name="player">Oyuncu referansı</param>
    /// <param name="segmentData">Segment verisi</param>
    public abstract void OnSegmentActivated(GameObject player, SegmentData segmentData);
    
    /// <summary>
    /// Segment deaktif olduğunda çalışacak özellik.
    /// </summary>
    /// <param name="player">Oyuncu referansı</param>
    /// <param name="segmentData">Segment verisi</param>
    public virtual void OnSegmentDeactivated(GameObject player, SegmentData segmentData)
    {
        // Varsayılan olarak hiçbir şey yapma
    }
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
	public SegmentEffect effect; // Bu segmentin özelliği
}