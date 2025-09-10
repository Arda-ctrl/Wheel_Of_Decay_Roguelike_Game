using UnityEngine;

[CreateAssetMenu(fileName = "GoblinStats", menuName = "Enemies/Goblins/Goblin Stats", order = 0)]
public sealed class GoblinStatsSO : ScriptableObject
{
	public bool overrideGoblinType = false;
	public GoblinType goblinType = GoblinType.DaggerGoblin;
	public GoblinStats stats = new GoblinStats();
}

