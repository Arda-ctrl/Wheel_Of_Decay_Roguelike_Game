using UnityEngine;

[CreateAssetMenu(fileName = "LightningElementData", menuName = "Game/Element Data/Lightning")]
public class LightningElementData : ElementData
{
    [Header("Lightning Element Settings")]
    public float stunChance = 0.1f; // %10 stun şansı
    public float stunDuration = 1f; // 1 saniye stun
    public float lightningDamagePerStack = 8f; // Stack başına hasar
} 