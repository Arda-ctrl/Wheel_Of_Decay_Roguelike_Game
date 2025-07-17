using System.Collections.Generic;
using UnityEngine;

public class RoomInteractive : MonoBehaviour
{
    public List<RoomConnectionPoint> connectionPoints = new List<RoomConnectionPoint>();
    private bool isDestroyed = false;
    private bool isFullyInitialized = false;
    private float initializationTime;
    private const float INITIALIZATION_GRACE_PERIOD = 0.5f; // Half a second grace period for initialization

    private void Awake()
    {
        // Önce listeyi temizle
        connectionPoints.Clear();
        
        // RoomPrefabSetup bileşenini kontrol et ve gerekirse bağlantı noktalarını oluştur
        RoomPrefabSetup prefabSetup = GetComponent<RoomPrefabSetup>();
        if (prefabSetup != null && prefabSetup.roomData != null)
        {
            // Eğer bağlantı noktaları yoksa, RoomPrefabSetup'ı kullanarak oluştur
            var points = GetComponentsInChildren<RoomConnectionPoint>();
            if (points == null || points.Length == 0)
            {
                // RoomPrefabSetup içindeki EnsureConnectionPoints metodunu çağır
                // Bu metod, reflection kullanarak private metodu çağırmak için
                System.Reflection.MethodInfo ensureMethod = prefabSetup.GetType().GetMethod("EnsureConnectionPoints", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (ensureMethod != null)
                {
                    ensureMethod.Invoke(prefabSetup, null);
                    Debug.Log($"Auto-created connection points for room {gameObject.name}");
                }
                else
                {
                    // Eğer reflection çalışmazsa, direkt SetupConnectionPoints'i çağır
                    prefabSetup.SetupConnectionPoints();
                }
            }
        }
        
        // Odadaki tüm bağlantı noktalarını bir kere bul
        var updatedPoints = GetComponentsInChildren<RoomConnectionPoint>();
        foreach (var point in updatedPoints)
        {
            if (point != null && !connectionPoints.Contains(point))
            {
                connectionPoints.Add(point);
            }
        }

        // Eğer hala bağlantı noktası yoksa, uyarı ver
        if (connectionPoints.Count == 0)
        {
            Debug.LogWarning($"Room {gameObject.name} has no connection points! Room connections may not work properly.");
        }
        else
        {
            Debug.Log($"Room {gameObject.name} initialized with {connectionPoints.Count} connection points");
        }

        // Room tag'ini ekle
        gameObject.tag = "Room";
        
        // Set initialization time
        initializationTime = Time.time;
    }
    
    private void Start()
    {
        // Mark as fully initialized after grace period
        Invoke("MarkAsFullyInitialized", INITIALIZATION_GRACE_PERIOD);
    }
    
    private void MarkAsFullyInitialized()
    {
        isFullyInitialized = true;
        Debug.Log($"Room {gameObject.name} fully initialized");
    }

    public RoomConnectionPoint GetAvailableConnection()
    {
        foreach (var point in connectionPoints)
        {
            if (point != null && !point.isOccupied)
                return point;
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore collisions during initialization grace period
        if (!isFullyInitialized || Time.time - initializationTime < INITIALIZATION_GRACE_PERIOD)
        {
            return;
        }
        
        // Eğer çarpışan obje Room tag'ine sahipse ve bu oda henüz yok edilmemişse
        if (collision.CompareTag("Room") && !isDestroyed)
        {
            // Kendi kendisiyle çarpışmayı kontrol et
            if (collision.gameObject == gameObject)
            {
                return;
            }
            
            // Check if this is a valid connection
            bool isValidConnection = false;
            foreach (var point in connectionPoints)
            {
                if (point != null && point.connectedTo != null)
                {
                    // If this room is connected to the other room, it's a valid connection
                    if (point.connectedTo.transform.IsChildOf(collision.transform))
                    {
                        isValidConnection = true;
                        Debug.Log($"Valid connection between {gameObject.name} and {collision.gameObject.name}");
                        break;
                    }
                }
            }
            
            // If it's not a valid connection, handle collision
            if (!isValidConnection)
            {
                Debug.LogWarning("Oda çakışması tespit edildi: " + gameObject.name + " ve " + collision.gameObject.name);
                RoomInteractive otherRoom = collision.GetComponent<RoomInteractive>();
                
                // Eğer diğer oda daha önce oluşturulmuşsa, bu odayı yok et
                if (otherRoom != null && otherRoom.gameObject.GetInstanceID() < gameObject.GetInstanceID())
                {
                    Debug.Log("Çakışan oda kaldırılıyor: " + gameObject.name);
                    isDestroyed = true;
                    
                    // Bağlantı noktalarını temizle
                    foreach (var point in connectionPoints)
                    {
                        if (point != null && point.connectedTo != null)
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
}
