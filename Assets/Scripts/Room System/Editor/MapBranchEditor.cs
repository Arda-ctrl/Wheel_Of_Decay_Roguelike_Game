using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MapBranchData))]
public class MapBranchEditor : Editor
{
    private bool showNodesSection = true;
    private bool showConnectionsSection = true;
    
    public override void OnInspectorGUI()
    {
        MapBranchData branchData = (MapBranchData)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // Biom section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Biom Settings", EditorStyles.boldLabel);
        
        // Display biom information
        if (branchData.biom != null)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Show biom icon if available
            if (branchData.biom.minimapTile != null)
            {
                GUILayout.Label(AssetPreview.GetAssetPreview(branchData.biom.minimapTile), 
                    GUILayout.Width(64), GUILayout.Height(64));
            }
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Biom: {branchData.biom.biomID}");
            EditorGUILayout.LabelField($"Room Count: {branchData.biom.roomPool.Count}");
            
            // Quick link to biom asset
            if (GUILayout.Button("Edit Biom"))
            {
                Selection.activeObject = branchData.biom;
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("No biom assigned. Please assign a biom to this branch.", MessageType.Warning);
            
            // Create a new biom button
            if (GUILayout.Button("Create New Biom"))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Create New Biom",
                    "New Biom",
                    "asset",
                    "Create a new biom asset for this branch");
                
                if (!string.IsNullOrEmpty(path))
                {
                    BiomData newBiom = ScriptableObject.CreateInstance<BiomData>();
                    newBiom.biomID = branchData.branchName;
                    
                    AssetDatabase.CreateAsset(newBiom, path);
                    AssetDatabase.SaveAssets();
                    
                    branchData.biom = newBiom;
                    EditorUtility.SetDirty(branchData);
                }
            }
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Nodes section
        showNodesSection = EditorGUILayout.Foldout(showNodesSection, "Nodes", true);
        if (showNodesSection)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Display nodes count
            EditorGUILayout.LabelField($"Node Count: {branchData.nodes.Count}");
            
            // Display node list
            for (int i = 0; i < branchData.nodes.Count; i++)
            {
                var node = branchData.nodes[i];
                
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"{node.nodeID}", GUILayout.Width(80));
                
                if (node.nodeData != null)
                {
                    EditorGUILayout.LabelField($"Type: {node.nodeData.nodeType}", GUILayout.Width(120));
                }
                else
                {
                    EditorGUILayout.LabelField("No node data", GUILayout.Width(120));
                }
                
                EditorGUILayout.LabelField($"Pos: ({node.gridPosition.x}, {node.gridPosition.y})");
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // Connections section
        showConnectionsSection = EditorGUILayout.Foldout(showConnectionsSection, "Connections", true);
        if (showConnectionsSection)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Display connections count
            EditorGUILayout.LabelField($"Connection Count: {branchData.connections.Count}");
            
            // Display connection list
            for (int i = 0; i < branchData.connections.Count; i++)
            {
                var connection = branchData.connections[i];
                
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"{connection.sourceNodeID}", GUILayout.Width(100));
                EditorGUILayout.LabelField("→", GUILayout.Width(20));
                EditorGUILayout.LabelField($"{connection.targetNodeID}", GUILayout.Width(100));
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // Branch completion statistics
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Branch Statistics", EditorStyles.boldLabel);
        
        if (branchData.nodes.Count > 0)
        {
            // Calculate different node types
            Dictionary<MapNodeData.NodeType, int> nodeTypeCounts = new Dictionary<MapNodeData.NodeType, int>();
            
            foreach (var node in branchData.nodes)
            {
                if (node.nodeData != null)
                {
                    if (!nodeTypeCounts.ContainsKey(node.nodeData.nodeType))
                    {
                        nodeTypeCounts[node.nodeData.nodeType] = 0;
                    }
                    nodeTypeCounts[node.nodeData.nodeType]++;
                }
            }
            
            // Display node type counts
            foreach (var kvp in nodeTypeCounts)
            {
                EditorGUILayout.LabelField($"- {kvp.Key}: {kvp.Value} nodes");
            }
            
            // Display path length
            int pathLength = CalculatePathLength(branchData);
            EditorGUILayout.LabelField($"Minimum path length: {pathLength} nodes");
        }
        else
        {
            EditorGUILayout.LabelField("No nodes in branch");
        }
        
        EditorGUILayout.EndVertical();
        
        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    
    private int CalculatePathLength(MapBranchData branchData)
    {
        // Calculate the minimum path length from start to end
        // This is a simplified breadth-first search
        
        if (string.IsNullOrEmpty(branchData.startNodeID) || 
            string.IsNullOrEmpty(branchData.endNodeID))
        {
            return 0;
        }
        
        // Build adjacency list
        Dictionary<string, List<string>> adjacencyList = new Dictionary<string, List<string>>();
        
        foreach (var connection in branchData.connections)
        {
            if (!adjacencyList.ContainsKey(connection.sourceNodeID))
            {
                adjacencyList[connection.sourceNodeID] = new List<string>();
            }
            adjacencyList[connection.sourceNodeID].Add(connection.targetNodeID);
        }
        
        // BFS to find shortest path
        Queue<string> queue = new Queue<string>();
        Dictionary<string, int> distances = new Dictionary<string, int>();
        
        queue.Enqueue(branchData.startNodeID);
        distances[branchData.startNodeID] = 0;
        
        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            
            if (current == branchData.endNodeID)
            {
                return distances[current] + 1; // +1 to count the nodes, not the edges
            }
            
            if (adjacencyList.ContainsKey(current))
            {
                foreach (string neighbor in adjacencyList[current])
                {
                    if (!distances.ContainsKey(neighbor))
                    {
                        distances[neighbor] = distances[current] + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        
        // No path found
        return 0;
    }
} 