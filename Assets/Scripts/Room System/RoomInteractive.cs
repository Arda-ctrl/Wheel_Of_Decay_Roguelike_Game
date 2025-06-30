using System.Collections.Generic;
using UnityEngine;

public class RoomInteractive : MonoBehaviour
{
    public List<RoomConnectionPoint> connectionPoints = new List<RoomConnectionPoint>();
    private bool isDestroyed = false;

    private void Awake()
    {
        // Önce listeyi temizle
        connectionPoints.Clear();
        
        // Odadaki tüm bağlantı noktalarını bir kere bul
        var points = GetComponentsInChildren<RoomConnectionPoint>();
        foreach (var point in points)
        {
            if (!connectionPoints.Contains(point))
            {
                connectionPoints.Add(point);
            }
        }

        // Room tag'ini ekle
        gameObject.tag = "Room";
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Eğer çarpışan obje Room tag'ine sahipse ve bu oda henüz yok edilmemişse
        if (collision.CompareTag("Room") && !isDestroyed)
        {
            Debug.LogError("Oda çakıştı");
            RoomInteractive otherRoom = collision.GetComponent<RoomInteractive>();
            
            // Eğer diğer oda daha önce oluşturulmuşsa, bu odayı yok et
            if (otherRoom != null && otherRoom.gameObject.GetInstanceID() < gameObject.GetInstanceID())
            {
                isDestroyed = true;
                
                // Bağlantı noktalarını temizle
                foreach (var point in connectionPoints)
                {
                    if (point.connectedTo != null)
                    {
                        point.connectedTo.isOccupied = false;
                        point.connectedTo.connectedTo = null;
                    }
                }
                
                // Odayı yok et
                Destroy(gameObject);
            }
        }
    }
}
