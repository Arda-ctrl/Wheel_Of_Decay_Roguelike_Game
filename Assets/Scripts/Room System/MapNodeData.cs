using UnityEngine;

[CreateAssetMenu(fileName = "New Map Node", menuName = "Rooms/Map Node Data")]
public class MapNodeData : ScriptableObject
{
    [Header("Node Settings")]
    public string nodeID;
    public Sprite nodeIcon;
    public Color nodeColor = Color.white;
    
    [Header("Node Type")]
    public NodeType nodeType;
    public enum NodeType
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
    
    [Header("Room Connection")]
    public RoomData roomData; // Reference to the actual room that will be loaded
    
    [Header("UI Settings")]
    public Sprite completedIcon; // Icon to show when node is completed
    public Color completedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Grayed out color for completed nodes
    
    [Header("Rewards")]
    public bool hasSpecialReward;
    [TextArea(2, 5)]
    public string rewardDescription;
} 