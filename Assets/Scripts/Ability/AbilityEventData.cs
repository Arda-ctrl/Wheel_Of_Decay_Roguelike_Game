using UnityEngine;

public class AbilityEventData
{
    public GameObject Caster { get; private set; }
    public GameObject Target { get; private set; }
    public AbilityData AbilityData { get; private set; }
    public Vector3 Position { get; private set; }

    public AbilityEventData(GameObject caster, GameObject target, AbilityData abilityData, Vector3 position)
    {
        Caster = caster;
        Target = target;
        AbilityData = abilityData;
        Position = position;
    }
} 