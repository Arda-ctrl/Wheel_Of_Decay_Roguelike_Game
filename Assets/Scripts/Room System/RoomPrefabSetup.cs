using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class RoomPrefabSetup : MonoBehaviour
{
    [Header("Room Configuration")]
    public RoomData roomData;
    
    [Header("Connection Points")]
    public Transform northConnectionPoint;
    public Transform eastConnectionPoint;
    public Transform southConnectionPoint;
    public Transform westConnectionPoint;

    private void Awake()
    {
        // Oyun başladığında otomatik olarak bağlantı noktalarını oluştur
        if (Application.isPlaying && roomData != null)
        {
            EnsureConnectionPoints();
        }
    }
    
    // Bağlantı noktalarını oluşturmadan önce kontrol et ve gerekirse oluştur
    private void EnsureConnectionPoints()
    {
        // Eğer bağlantı noktaları zaten varsa, tekrar oluşturmaya gerek yok
        var existingPoints = GetComponentsInChildren<RoomConnectionPoint>();
        if (existingPoints != null && existingPoints.Length > 0)
        {
            Debug.Log($"Room {gameObject.name} already has {existingPoints.Length} connection points.");
            return;
        }
        
        // Bağlantı noktaları yoksa, oluştur
        CreateDefaultConnectionPoints();
        SetupConnectionPoints();
    }
    
    // Eğer bağlantı noktaları atanmamışsa varsayılan pozisyonlarda oluştur
    private void CreateDefaultConnectionPoints()
    {
        float roomWidth = 8f;  // Oda genişliği için varsayılan değer
        float roomHeight = 5f; // Oda yüksekliği için varsayılan değer
        
        // Sprite Renderer varsa, gerçek boyutları al
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            roomWidth = spriteRenderer.bounds.size.x;
            roomHeight = spriteRenderer.bounds.size.y;
        }
        
        // Bağlantı noktalarını oluştur
        if (northConnectionPoint == null)
        {
            GameObject northPoint = new GameObject("DefaultNorthPoint");
            northPoint.transform.parent = transform;
            northPoint.transform.localPosition = new Vector3(0, roomHeight/2, 0);
            northConnectionPoint = northPoint.transform;
        }
        
        if (eastConnectionPoint == null)
        {
            GameObject eastPoint = new GameObject("DefaultEastPoint");
            eastPoint.transform.parent = transform;
            eastPoint.transform.localPosition = new Vector3(roomWidth/2, 0, 0);
            eastConnectionPoint = eastPoint.transform;
        }
        
        if (southConnectionPoint == null)
        {
            GameObject southPoint = new GameObject("DefaultSouthPoint");
            southPoint.transform.parent = transform;
            southPoint.transform.localPosition = new Vector3(0, -roomHeight/2, 0);
            southConnectionPoint = southPoint.transform;
        }
        
        if (westConnectionPoint == null)
        {
            GameObject westPoint = new GameObject("DefaultWestPoint");
            westPoint.transform.parent = transform;
            westPoint.transform.localPosition = new Vector3(-roomWidth/2, 0, 0);
            westConnectionPoint = westPoint.transform;
        }
    }
    
    public void SetupConnectionPoints()
    {
        if (roomData == null)
        {
            Debug.LogError("Room data is not assigned!");
            return;
        }
        
        // Eksik bağlantı noktalarını oluştur
        CreateDefaultConnectionPoints();
        
        // Clear existing connection points
        foreach (var point in GetComponentsInChildren<RoomConnectionPoint>())
        {
            if (Application.isPlaying)
                Destroy(point.gameObject);
            else
                DestroyImmediate(point.gameObject);
        }
        
        // Create connection points based on room type
        switch (roomData.connectionType)
        {
            case RoomData.RoomConnectionType.SingleUp:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                break;
            case RoomData.RoomConnectionType.SingleDown:
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                break;
            case RoomData.RoomConnectionType.SingleRight:
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                break;
            case RoomData.RoomConnectionType.SingleLeft:
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                break;
            case RoomData.RoomConnectionType.DoubleUpDown:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                break;
            case RoomData.RoomConnectionType.DoubleLeftRight:
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                break;
            case RoomData.RoomConnectionType.DoubleUpLeft:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                break;
            case RoomData.RoomConnectionType.DoubleUpRight:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                break;
            case RoomData.RoomConnectionType.DoubleDownLeft:
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                break;
            case RoomData.RoomConnectionType.DoubleDownRight:
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                break;
            case RoomData.RoomConnectionType.TripleUpRightDown:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                break;
            case RoomData.RoomConnectionType.TripleUpLeftDown:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                break;
            case RoomData.RoomConnectionType.TripleDownRightLeft:
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                break;
            case RoomData.RoomConnectionType.TripleUpLeftRight:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                break;
            case RoomData.RoomConnectionType.Fourway:
                CreateConnectionPoint(northConnectionPoint, ConnectionDirection.North);
                CreateConnectionPoint(eastConnectionPoint, ConnectionDirection.East);
                CreateConnectionPoint(southConnectionPoint, ConnectionDirection.South);
                CreateConnectionPoint(westConnectionPoint, ConnectionDirection.West);
                break;
        }
        
        Debug.Log($"Setup connection points for room {roomData.roomID} with type {roomData.connectionType}");
    }
    
    private void CreateConnectionPoint(Transform pointTransform, ConnectionDirection direction)
    {
        if (pointTransform == null)
        {
            Debug.LogWarning($"Connection point transform for direction {direction} is not assigned!");
            return;
        }
        
        GameObject pointObj = new GameObject($"ConnectionPoint_{direction}");
        pointObj.transform.parent = transform;
        pointObj.transform.position = pointTransform.position;
        pointObj.transform.rotation = pointTransform.rotation;
        
        RoomConnectionPoint connectionPoint = pointObj.AddComponent<RoomConnectionPoint>();
        connectionPoint.direction = direction;
        
        // Add a door trigger component
        DoorTrigger doorTrigger = pointObj.AddComponent<DoorTrigger>();
        doorTrigger.connectionPoint = connectionPoint;
        
        // Create spawn point for teleporting
        GameObject spawnPointObj = new GameObject("SpawnPoint");
        spawnPointObj.transform.parent = pointObj.transform;
        spawnPointObj.transform.localPosition = new Vector3(0, -1f, 0); // Offset slightly from door
        
        doorTrigger.spawnPoint = spawnPointObj.transform;
        
        // Add collider for triggering
        BoxCollider2D collider = pointObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.5f, 1.5f);
    }
}

// Custom editor for the RoomPrefabSetup component
[CustomEditor(typeof(RoomPrefabSetup))]
public class RoomPrefabSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        RoomPrefabSetup setupScript = (RoomPrefabSetup)target;
        
        if (GUILayout.Button("Setup Connection Points"))
        {
            setupScript.SetupConnectionPoints();
        }
    }
}
#endif 