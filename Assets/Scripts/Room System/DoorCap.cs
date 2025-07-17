using UnityEngine;

/// <summary>
/// A simple component to mark a door as capped/blocked.
/// This is attached to prefabs used to visually close off unused doorways.
/// </summary>
public class DoorCap : MonoBehaviour
{
    [Tooltip("Visual color for the door cap in scene view")]
    public Color gizmoColor = Color.red;
    
    [Tooltip("Size of the door cap gizmo")]
    public float gizmoSize = 0.5f;
    
    private void OnDrawGizmos()
    {
        // Draw a visual indicator in the scene view
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
        
        // Draw a cross to indicate this is a blocked door
        Vector3 pos = transform.position;
        float size = gizmoSize * 1.5f;
        Gizmos.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size);
        Gizmos.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size);
    }
} 