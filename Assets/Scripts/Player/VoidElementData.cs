using UnityEngine;

[CreateAssetMenu(fileName = "VoidElementData", menuName = "Game/Element Data/Void")]
public class VoidElementData : ElementData
{
    [Header("Void Element Settings")]
    public float visionReductionPerStack = 0.15f; // Her stack %15 görüş alanı azaltması
    public float rangeReductionPerStack = 0.1f; // Her stack %10 menzil azaltması
    public float voidDamagePerStack = 3f; // Stack başına hasar
    public float voidEffectDuration = 4f; // Efekt süresi
} 