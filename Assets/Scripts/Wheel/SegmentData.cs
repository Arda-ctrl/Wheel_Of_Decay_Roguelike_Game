using UnityEngine;

public enum EffectType
{
	Attack,
	Defense,
	Regen
}

[CreateAssetMenu(fileName = "NewSegment", menuName = "Segment")]
public class SegmentData : ScriptableObject
{
	public string segmentName;
	[Range(1, 3)]
	public int size = 1; // Kaç slot kaplıyor (1–3 arası)

	public EffectType effectType;
	public int effectValue;

	public Sprite icon; // Artistin çizdiği sprite buraya atanacak
}