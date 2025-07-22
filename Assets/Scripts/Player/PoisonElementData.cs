using UnityEngine;

[CreateAssetMenu(fileName = "PoisonElementData", menuName = "Game/Element Data/Poison")]
public class PoisonElementData : ElementData
{
    public float poisonDuration = 3f;
    public float poisonDamagePerTick = 3f;
    public float poisonTickRate = 0.5f;
    public float poisonSlowPercent = 0.2f;
} 