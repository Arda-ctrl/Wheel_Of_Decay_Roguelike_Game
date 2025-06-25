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
}
