using UnityEngine;

[CreateAssetMenu(fileName = "New Room Data", menuName = "Rooms/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Room Settings")]
    public string roomID;
    public GameObject roomPrefab;
    public Color roomColor = Color.white;
    public bool isStartRoom;
    public bool isEndRoom;

    [Header("Room Type")]
    public RoomConnectionType connectionType;
    public enum RoomConnectionType
    {
        SingleUp, SingleDown, SingleRight, SingleLeft,
        DoubleUpDown, DoubleLeftRight, DoubleUpLeft, DoubleDownRight, DoubleDownLeft, DoubleUpRight,
        TripleUpRightDown, TripleUpLeftDown, TripleDownRightLeft, TripleUpLeftRight,
        Fourway
    }

    [Header("Enemy Settings")]
    public GameObject[] possibleEnemies;
    public int minEnemyCount;
    public int maxEnemyCount;

    [Header("Pickup Settings")]
    public bool hasReward;
    public GameObject rewardPrefab;
    public GameObject[] possiblePickups;
    public float pickupSpawnChance = 0.3f;
}
