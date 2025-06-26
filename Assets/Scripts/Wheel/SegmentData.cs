using UnityEngine;

// SegmentData: Her bir segmentin (dilim) verisini tutan ScriptableObject.

/// <summary>
/// Segmentin etkisi türü.
/// </summary>
public enum EffectType
{
	Attack,   // Saldırı
	Defense,  // Savunma
	Regen     // Yenilenme
}

[CreateAssetMenu(fileName = "NewSegment", menuName = "Segment")]
public class SegmentData : ScriptableObject
{
	public string segmentName; // Segmentin adı
	[Range(1, 3)]
	public int size = 1; // Kaç slot kaplıyor (1–3 arası)

	public EffectType effectType; // Segmentin etkisi
	public int effectValue; // Etki değeri

	public Sprite icon; // Artistin çizdiği sprite buraya atanacak
}