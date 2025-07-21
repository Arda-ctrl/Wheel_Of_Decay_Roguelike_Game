using UnityEngine;

[CreateAssetMenu(fileName = "ElementData_None", menuName = "Game/Element Data/None")]
public class ElementData : ScriptableObject
{
    public string elementName;
    public Sprite elementIcon;
}

[CreateAssetMenu(fileName = "FireElementData", menuName = "Game/Element Data/Fire")]
public class FireElementData : ElementData
{
    public float burnDuration = 2f;
    public float burnDamagePerTick = 5f;
    public float burnTickRate = 0.5f;
}

[CreateAssetMenu(fileName = "IceElementData", menuName = "Game/Element Data/Ice")]
public class IceElementData : ElementData
{
    public float slowPercent = 0.3f;
    public float slowDuration = 2f;
}

[CreateAssetMenu(fileName = "PoisonElementData", menuName = "Game/Element Data/Poison")]
public class PoisonElementData : ElementData
{
    public float poisonDuration = 3f;
    public float poisonDamagePerTick = 3f;
    public float poisonTickRate = 0.5f;
    public float poisonSlowPercent = 0.2f;
} 