using UnityEngine;

public enum ConnectionDirection
{
    North, South, East, West
}

public class RoomConnectionPoint : MonoBehaviour
{
    public ConnectionDirection direction;
    public bool isOccupied = false;
    public RoomConnectionPoint connectedTo;
    
    // Visual indicator for debugging
    public bool showGizmo = true;
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 0.3f;
    
    public void Connect(RoomConnectionPoint other)
    {
        if (other == null)
        {
            Debug.LogError("Cannot connect to null connection point");
            return;
        }
        
        this.isOccupied = true;
        other.isOccupied = true;
        
        this.connectedTo = other;
        other.connectedTo = this;
        
        Debug.Log($"Connected {gameObject.name} ({direction}) to {other.gameObject.name} ({other.direction})");
    }
    
    public void Disconnect()
    {
        if (connectedTo != null)
        {
            connectedTo.isOccupied = false;
            connectedTo.connectedTo = null;
            
            this.isOccupied = false;
            this.connectedTo = null;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            // Draw a sphere at the connection point
            Gizmos.color = isOccupied ? Color.green : gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoSize);
            
            // Draw a line to the connected point if it exists
            if (connectedTo != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, connectedTo.transform.position);
            }
        }
    }
}
