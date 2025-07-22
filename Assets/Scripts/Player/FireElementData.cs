using UnityEngine;

[CreateAssetMenu(fileName = "FireElementData", menuName = "Game/Element Data/Fire")]
public class FireElementData : ElementData
{
    public float burnDuration = 2f;
    public float burnDamagePerTick = 5f;
    public float burnTickRate = 0.5f;
} 