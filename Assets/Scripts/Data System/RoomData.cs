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
    
    [Header("Biom Settings")]
    [Tooltip("Optional: If assigned, this room is specific to this biom")]
    public BiomData biom;

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
    
    [Header("Map Node Settings")]
    public MapNodeData associatedNodeData; // Reference to the associated map node data
    public bool isMapNode; // Is this room part of a map node?
    public MapNodeType mapNodeType = MapNodeType.Battle; // Default to battle type
    
    public enum MapNodeType
    {
        Battle,
        Shop,
        Boss,
        Event,
        Rest,
        Mystery,
        Start,
        End
    }
    
    // Helper method to create a MapNodeData from this RoomData
    public MapNodeData CreateNodeData()
    {
        if (associatedNodeData != null)
        {
            return associatedNodeData;
        }
        
        // Create a new MapNodeData based on this RoomData
        MapNodeData nodeData = ScriptableObject.CreateInstance<MapNodeData>();
        nodeData.nodeID = roomID;
        nodeData.nodeColor = roomColor;
        nodeData.roomData = this;
        
        // Set node type based on map node type
        switch (mapNodeType)
        {
            case MapNodeType.Battle:
                nodeData.nodeType = MapNodeData.NodeType.Battle;
                break;
            case MapNodeType.Shop:
                nodeData.nodeType = MapNodeData.NodeType.Shop;
                break;
            case MapNodeType.Boss:
                nodeData.nodeType = MapNodeData.NodeType.Boss;
                break;
            case MapNodeType.Event:
                nodeData.nodeType = MapNodeData.NodeType.Event;
                break;
            case MapNodeType.Rest:
                nodeData.nodeType = MapNodeData.NodeType.Rest;
                break;
            case MapNodeType.Mystery:
                nodeData.nodeType = MapNodeData.NodeType.Mystery;
                break;
            case MapNodeType.Start:
                nodeData.nodeType = MapNodeData.NodeType.Start;
                break;
            case MapNodeType.End:
                nodeData.nodeType = MapNodeData.NodeType.End;
                break;
        }
        
        return nodeData;
    }
}
