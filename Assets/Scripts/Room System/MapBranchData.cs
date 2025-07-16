using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq; // Added for FirstOrDefault

[CreateAssetMenu(fileName = "New Map Branch", menuName = "Rooms/Map Branch Data")]
public class MapBranchData : ScriptableObject
{
    [Serializable]
    public class NodeConnection
    {
        public string sourceNodeID;
        public string targetNodeID;
    }
    
    [Serializable]
    public class BranchNode
    {
        public string nodeID;
        public MapNodeData nodeData;
        public Vector2 gridPosition;
    }
    
    [Header("Branch Settings")]
    public string branchID;
    public string branchName;
    [TextArea(2, 5)]
    public string branchDescription;
    
    [Header("Biom Settings")]
    public BiomData biom;
    
    [Header("Nodes")]
    public List<BranchNode> nodes = new List<BranchNode>();
    
    [Header("Connections")]
    public List<NodeConnection> connections = new List<NodeConnection>();
    
    [Header("Special Nodes")]
    public string startNodeID; // ID of the start node
    public string endNodeID;   // ID of the end node
    
    // Helper methods
    public BranchNode GetNodeByID(string id)
    {
        return nodes.Find(n => n.nodeID == id);
    }
    
    public List<NodeConnection> GetConnectionsFromNode(string nodeID)
    {
        return connections.FindAll(c => c.sourceNodeID == nodeID);
    }
    
    public List<NodeConnection> GetConnectionsToNode(string nodeID)
    {
        return connections.FindAll(c => c.targetNodeID == nodeID);
    }
    
    // Get the biom for this branch, with fallback to default
    public BiomData GetBiom()
    {
        if (biom != null)
        {
            Debug.Log($"Using branch-specific biom: {biom.biomID}");
            return biom;
        }
        
        // Try to load default Forest biom
        BiomData defaultBiom = Resources.Load<BiomData>("Bioms/Forest");
        if (defaultBiom == null)
        {
            Debug.LogError($"Default Forest biom not found for branch {branchID}. Please assign a biom.");
            
            // Son çare olarak, Resources klasöründeki herhangi bir biom'u bul
            defaultBiom = Resources.FindObjectsOfTypeAll<BiomData>().FirstOrDefault();
            if (defaultBiom != null)
            {
                Debug.Log($"Found fallback biom: {defaultBiom.biomID}");
            }
            else
            {
                Debug.LogError("No bioms found in the project! Room generation will fail.");
            }
        }
        else
        {
            Debug.Log($"Using default Forest biom for branch {branchID}");
        }
        
        return defaultBiom;
    }
} 