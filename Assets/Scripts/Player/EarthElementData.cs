using UnityEngine;

[CreateAssetMenu(fileName = "EarthElementData", menuName = "Game/Element Data/Earth")]
public class EarthElementData : ElementData
{
    [Header("Earth Element Settings")]
    public int rootStackThreshold = 3; // 3 stack'te root
    public float rootDuration = 0.75f; // 0.5-1 saniye arası (0.75 ortalama)
    public float earthDamagePerStack = 6f; // Stack başına hasar
} 