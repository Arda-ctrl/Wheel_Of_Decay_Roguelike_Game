using UnityEngine;

[CreateAssetMenu(fileName = "New Room Data", menuName = "Rooms/Room Data")]
public class RoomData : ScriptableObject
{
    public string roomID;
    public GameObject roomPrefab;

    [Header("Enemy Settings")]
    public int minEnemyCount;
    public int maxEnemyCount;

    [Header("Optional Rewards")]
    public bool hasReward;
    public GameObject rewardPrefab;
}
