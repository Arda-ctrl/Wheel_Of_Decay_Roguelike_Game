using System.Collections.Generic;
using UnityEngine;

public class RoomInteractive : MonoBehaviour
{
    public List<RoomConnectionPoint> connectionPoints = new List<RoomConnectionPoint>();

    private void Awake()
    {
        // Odadaki tüm bağlantı noktalarını otomatik bul
        connectionPoints.AddRange(GetComponentsInChildren<RoomConnectionPoint>());
    }

    public RoomConnectionPoint GetAvailableConnection()
    {
        foreach (var point in connectionPoints)
        {
            if (!point.isOccupied)
                return point;
        }
        return null;
    }
}
