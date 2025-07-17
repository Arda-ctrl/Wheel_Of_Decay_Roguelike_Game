using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(MapNodeData))]
public class MapNodeDataEditor : Editor
{
    private bool showRoomSettings = true;
    private bool showUISettings = true;
    private bool showRewardSettings = true;
    
    public override void OnInspectorGUI()
    {
        MapNodeData nodeData = (MapNodeData)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Map Node Editor", EditorStyles.boldLabel);
        
        // Quick node type selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Quick Set Type:", GUILayout.Width(100));
        if (GUILayout.Button("Battle", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.Battle;
            nodeData.nodeColor = new Color(0.8f, 0.2f, 0.2f);
            EditorUtility.SetDirty(nodeData);
        }
        if (GUILayout.Button("Shop", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.Shop;
            nodeData.nodeColor = new Color(0.2f, 0.6f, 0.8f);
            EditorUtility.SetDirty(nodeData);
        }
        if (GUILayout.Button("Boss", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.Boss;
            nodeData.nodeColor = new Color(0.8f, 0.1f, 0.1f);
            EditorUtility.SetDirty(nodeData);
        }
        if (GUILayout.Button("Event", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.Event;
            nodeData.nodeColor = new Color(0.8f, 0.7f, 0.2f);
            EditorUtility.SetDirty(nodeData);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(100));
        if (GUILayout.Button("Rest", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.Rest;
            nodeData.nodeColor = new Color(0.2f, 0.8f, 0.4f);
            EditorUtility.SetDirty(nodeData);
        }
        if (GUILayout.Button("Mystery", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.Mystery;
            nodeData.nodeColor = new Color(0.6f, 0.3f, 0.8f);
            EditorUtility.SetDirty(nodeData);
        }
        if (GUILayout.Button("Start", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.Start;
            nodeData.nodeColor = new Color(0.2f, 0.8f, 0.2f);
            EditorUtility.SetDirty(nodeData);
        }
        if (GUILayout.Button("End", EditorStyles.miniButton))
        {
            nodeData.nodeType = MapNodeData.NodeType.End;
            nodeData.nodeColor = new Color(0.8f, 0.2f, 0.2f);
            EditorUtility.SetDirty(nodeData);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Room settings
        showRoomSettings = EditorGUILayout.Foldout(showRoomSettings, "Room Settings", true);
        if (showRoomSettings)
        {
            EditorGUI.indentLevel++;
            
            // Room data field
            nodeData.roomData = (RoomData)EditorGUILayout.ObjectField("Room Data", nodeData.roomData, typeof(RoomData), false);
            
            // Create from RoomData button
            if (nodeData.roomData != null)
            {
                if (GUILayout.Button("Copy Settings from Room Data"))
                {
                    nodeData.nodeID = nodeData.roomData.roomID;
                    nodeData.nodeColor = nodeData.roomData.roomColor;
                    
                    // Set node type based on room data
                    if (nodeData.roomData.isStartRoom)
                    {
                        nodeData.nodeType = MapNodeData.NodeType.Start;
                    }
                    else if (nodeData.roomData.isEndRoom)
                    {
                        nodeData.nodeType = MapNodeData.NodeType.End;
                    }
                    else
                    {
                        // You could expand this to detect other room types based on your RoomData properties
                        nodeData.nodeType = MapNodeData.NodeType.Battle;
                    }
                    
                    EditorUtility.SetDirty(nodeData);
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        // UI settings
        showUISettings = EditorGUILayout.Foldout(showUISettings, "UI Settings", true);
        if (showUISettings)
        {
            EditorGUI.indentLevel++;
            
            // Preview
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview:", GUILayout.Width(60));
            
            GUIStyle previewStyle = new GUIStyle(GUI.skin.box);
            previewStyle.normal.background = MakeColorTexture(nodeData.nodeColor);
            previewStyle.fixedWidth = 50;
            previewStyle.fixedHeight = 50;
            
            GUILayout.Box("", previewStyle);
            
            if (nodeData.nodeIcon != null)
            {
                GUIStyle iconStyle = new GUIStyle(GUI.skin.box);
                iconStyle.fixedWidth = 50;
                iconStyle.fixedHeight = 50;
                
                Rect iconRect = GUILayoutUtility.GetRect(50, 50);
                GUI.DrawTexture(iconRect, nodeData.nodeIcon.texture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUILayout.Box("No Icon", GUILayout.Width(50), GUILayout.Height(50));
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        // Reward settings
        showRewardSettings = EditorGUILayout.Foldout(showRewardSettings, "Reward Settings", true);
        if (showRewardSettings)
        {
            EditorGUI.indentLevel++;
            
            nodeData.hasSpecialReward = EditorGUILayout.Toggle("Has Special Reward", nodeData.hasSpecialReward);
            if (nodeData.hasSpecialReward)
            {
                nodeData.rewardDescription = EditorGUILayout.TextArea(nodeData.rewardDescription, GUILayout.Height(60));
            }
            
            EditorGUI.indentLevel--;
        }
        
        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(nodeData);
        }
    }
    
    private Texture2D MakeColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
#endif 