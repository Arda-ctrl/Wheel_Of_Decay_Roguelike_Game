using UnityEngine;
using UnityEngine.UI;

public class MapConnection : MonoBehaviour
{
    public Image connectionImage;
    public Color defaultColor = Color.gray;
    public Color accessibleColor = Color.white;
    public Color completedColor = Color.green;
    
    private RectTransform rectTransform;
    private MapNode sourceNode;
    private MapNode targetNode;
    private bool isAccessible = false;
    private bool isCompleted = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (connectionImage == null)
        {
            connectionImage = GetComponent<Image>();
        }
    }
    
    public void Initialize(MapNode source, MapNode target)
    {
        sourceNode = source;
        targetNode = target;
        UpdateVisual();
    }
    
    public void UpdateVisual()
    {
        if (sourceNode == null || targetNode == null) return;
        
        // Position and rotate connection line
        Vector2 sourcePos = sourceNode.transform.position;
        Vector2 targetPos = targetNode.transform.position;
        
        // Calculate center position
        rectTransform.position = (sourcePos + targetPos) / 2;
        
        // Calculate rotation
        Vector2 direction = targetPos - sourcePos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Calculate width
        float distance = Vector2.Distance(sourcePos, targetPos);
        rectTransform.sizeDelta = new Vector2(distance, rectTransform.sizeDelta.y);
        
        // Update color based on state
        if (connectionImage != null)
        {
            connectionImage.color = isCompleted ? completedColor : 
                                  (isAccessible ? accessibleColor : defaultColor);
        }
    }
    
    public void SetAccessible(bool accessible)
    {
        isAccessible = accessible;
        UpdateVisual();
    }
    
    public void SetCompleted(bool completed)
    {
        isCompleted = completed;
        UpdateVisual();
    }
    
    public MapNode GetTargetNode()
    {
        return targetNode;
    }
    
    public MapNode GetSourceNode()
    {
        return sourceNode;
    }
} 