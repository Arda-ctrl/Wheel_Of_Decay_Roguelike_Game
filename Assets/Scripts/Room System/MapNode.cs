using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MapNode : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Node References")]
    public Image nodeImage;
    public Image iconImage;
    public Image selectionOutline;
    public TextMeshProUGUI nodeLabel;
    
    [Header("Connection References")]
    public Transform connectionParent;
    
    [HideInInspector] public MapNodeData nodeData;
    [HideInInspector] public Vector2 gridPosition;
    [HideInInspector] public bool isCompleted;
    [HideInInspector] public bool isAccessible;
    [HideInInspector] public bool isCurrent;
    
    private List<MapNode> connectedNodes = new List<MapNode>();
    private List<MapConnection> connections = new List<MapConnection>();
    
    public void Initialize(MapNodeData data, Vector2 position)
    {
        nodeData = data;
        gridPosition = position;
        
        // Set up visuals
        if (nodeImage != null)
        {
            nodeImage.color = data.nodeColor;
        }
        
        if (iconImage != null && data.nodeIcon != null)
        {
            iconImage.sprite = data.nodeIcon;
            iconImage.enabled = true;
        }
        
        if (nodeLabel != null)
        {
            nodeLabel.text = data.nodeID;
        }
        
        // Initially hide selection outline
        if (selectionOutline != null)
        {
            selectionOutline.enabled = false;
        }
        
        UpdateAccessibility(false);
    }
    
    public void AddConnection(MapNode targetNode, MapConnection connectionObject)
    {
        if (!connectedNodes.Contains(targetNode))
        {
            connectedNodes.Add(targetNode);
            connections.Add(connectionObject);
        }
    }
    
    public void UpdateAccessibility(bool accessible)
    {
        isAccessible = accessible;
        
        // Visual update based on accessibility
        if (nodeImage != null)
        {
            // Tamamlanmış düğümler için tamamlanmış rengi
            // Erişilebilir düğümler için normal renk
            // Erişilemez düğümler için soluk renk
            nodeImage.color = isCompleted ? nodeData.completedColor : 
                              (isAccessible ? nodeData.nodeColor : new Color(nodeData.nodeColor.r, nodeData.nodeColor.g, nodeData.nodeColor.b, 0.5f));
        }
        
        if (iconImage != null)
        {
            // Tamamlanmış düğümler için tamamlanmış simge
            iconImage.sprite = isCompleted && nodeData.completedIcon != null ? nodeData.completedIcon : nodeData.nodeIcon;
            // Erişilebilir düğümler için tam opaklık, erişilemez düğümler için yarı opaklık
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, isAccessible ? 1f : 0.5f);
        }
        
        // Debug log
        Debug.Log($"Node {nodeData.nodeID} accessibility set to {accessible}");
    }
    
    public void SetAsCurrent(bool current)
    {
        isCurrent = current;
        if (selectionOutline != null)
        {
            selectionOutline.enabled = current;
        }
    }
    
    public void SetCompleted()
    {
        isCompleted = true;
        UpdateAccessibility(false);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Node {nodeData?.nodeID ?? "unknown"} clicked. isAccessible: {isAccessible}");
        
        if (isAccessible)
        {
            if (MapManager.instance != null)
            {
                MapManager.instance.OnNodeSelected(this);
            }
            else
            {
                Debug.LogError("MapManager.instance is null! Cannot handle node selection.");
            }
        }
        else
        {
            Debug.Log($"Node {nodeData?.nodeID ?? "unknown"} is not accessible!");
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isAccessible)
        {
            transform.localScale = Vector3.one * 1.1f;
            
            // Show tooltip if node has special reward
            if (nodeData.hasSpecialReward)
            {
                // You can implement a tooltip system here
                Debug.Log($"Node {nodeData.nodeID} reward: {nodeData.rewardDescription}");
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        
        // Hide tooltip if showing
    }
    
    public List<MapNode> GetConnectedNodes()
    {
        return connectedNodes;
    }
} 