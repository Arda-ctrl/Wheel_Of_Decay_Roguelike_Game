using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    
    [Header("Map Settings")]
    public bool useSerializedMap = true;
    public MapBranchData serializedMapData;
    public int proceduralMapWidth = 5;
    public int proceduralMapHeight = 8;
    public float nodeSpacingX = 150f;
    public float nodeSpacingY = 120f;
    public int minConnectionsPerNode = 1;
    public int maxConnectionsPerNode = 3;
    
    [Header("UI References")]
    public GameObject mapCanvas;
    public RectTransform mapContainer;
    public GameObject nodePrefab;
    public GameObject connectionPrefab;
    
    [Header("Node Settings")]
    public MapNodeData startNodeData;
    public MapNodeData endNodeData;
    public List<MapNodeData> battleNodeData = new List<MapNodeData>();
    public List<MapNodeData> shopNodeData = new List<MapNodeData>();
    public List<MapNodeData> eventNodeData = new List<MapNodeData>();
    public List<MapNodeData> mysteryNodeData = new List<MapNodeData>();
    public List<MapNodeData> restNodeData = new List<MapNodeData>();
    public List<MapNodeData> bossNodeData = new List<MapNodeData>();
    
    [Header("Scene Management")]
    public string gameSceneName = "Base";
    
    // Runtime data
    private List<MapNode> allNodes = new List<MapNode>();
    private List<MapConnection> allConnections = new List<MapConnection>();
    private MapNode currentNode;
    private MapNode startNode;
    private MapNode endNode;
    
    // Current biom data
    private BiomData currentBiom;
    
    // Delegate
    public delegate void BranchFinishedHandler(BiomData biom);
    public static BranchFinishedHandler OnBranchFinished;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Ensure we have the map canvas
        if (mapCanvas == null)
        {
            Debug.LogError("Map Canvas is not assigned in MapManager!");
        }
        
        // Ensure we have an EventSystem for UI interactions
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.Log("No EventSystem found in scene. Creating one...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
    
    private void Start()
    {
        GenerateMap();
    }
    
    // Update metodu: M tuşuyla haritayı aç/kapat, R tuşuyla haritayı yeniden oluştur
    private void Update()
    {
        // M tuşuna basıldığında haritayı aç/kapat
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("M key pressed - toggling map");
            ToggleMap();
        }
        
        // R tuşuna basıldığında haritayı yeniden oluştur (karıştır)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed - reshuffling map");
            ShuffleMapNodes();
        }
    }
    
    public void GenerateMap()
    {
        ClearMap();
        
        if (useSerializedMap && serializedMapData != null)
        {
            GenerateSerializedMap();
        }
        else
        {
            GenerateProceduralMap();
        }
        
        // Başlangıçta sadece start düğümü erişilebilir olmalı,
        // diğer tüm düğümler erişilemez olmalı
        foreach (MapNode node in allNodes)
        {
            if (node == startNode)
            {
                node.isAccessible = true;
                node.SetAsCurrent(true);
            }
            else
            {
                node.isAccessible = false;
                node.SetAsCurrent(false);
            }
        }
        
        // Bağlantıları güncelle
        UpdateNodeAccessibility();
    }
    
    private void GenerateSerializedMap()
    {
        Dictionary<string, MapNode> nodeDict = new Dictionary<string, MapNode>();
        
        // Get and cache the biom for this branch
        currentBiom = serializedMapData.GetBiom();
        
        // Create all nodes first
        foreach (var branchNode in serializedMapData.nodes)
        {
            MapNode node = CreateNode(branchNode.nodeData, branchNode.gridPosition);
            nodeDict.Add(branchNode.nodeID, node);
            
            // Set start and end nodes
            if (branchNode.nodeID == serializedMapData.startNodeID)
            {
                startNode = node;
                currentNode = node;
            }
            
            if (branchNode.nodeID == serializedMapData.endNodeID)
            {
                endNode = node;
            }
        }
        
        // Create connections
        foreach (var connection in serializedMapData.connections)
        {
            if (nodeDict.TryGetValue(connection.sourceNodeID, out MapNode sourceNode) && 
                nodeDict.TryGetValue(connection.targetNodeID, out MapNode targetNode))
            {
                CreateConnection(sourceNode, targetNode);
            }
        }
        
        Debug.Log($"Generated map for biom: {currentBiom?.biomID ?? "Unknown"}");
    }
    
    private void GenerateProceduralMap()
    {
        // Create grid for node placement
        MapNode[,] nodeGrid = new MapNode[proceduralMapWidth, proceduralMapHeight];
        
        // Create start node at the bottom center
        Vector2 startPos = new Vector2(proceduralMapWidth / 2, 0);
        startNode = CreateNode(startNodeData, startPos);
        nodeGrid[(int)startPos.x, (int)startPos.y] = startNode;
        
        // Create end node at the top center
        Vector2 endPos = new Vector2(proceduralMapWidth / 2, proceduralMapHeight - 1);
        endNode = CreateNode(endNodeData, endPos);
        nodeGrid[(int)endPos.x, (int)endPos.y] = endNode;
        
        // Generate middle nodes
        for (int y = 1; y < proceduralMapHeight - 1; y++)
        {
            // Determine how many nodes in this row (1-3)
            int nodesInRow = Random.Range(1, Mathf.Min(4, proceduralMapWidth));
            
            // Create nodes at random x positions
            List<int> xPositions = new List<int>();
            for (int i = 0; i < nodesInRow; i++)
            {
                int x;
                do
                {
                    x = Random.Range(0, proceduralMapWidth);
                } while (xPositions.Contains(x));
                
                xPositions.Add(x);
                
                // Choose node type
                MapNodeData nodeData = ChooseRandomNodeType(y);
                
                // Create the node
                Vector2 pos = new Vector2(x, y);
                MapNode node = CreateNode(nodeData, pos);
                nodeGrid[x, y] = node;
            }
        }
        
        // Create connections
        // First, connect each row to the next row
        for (int y = 0; y < proceduralMapHeight - 1; y++)
        {
            for (int x = 0; x < proceduralMapWidth; x++)
            {
                MapNode sourceNode = nodeGrid[x, y];
                if (sourceNode == null) continue;
                
                // Get possible target nodes in the next row
                List<MapNode> possibleTargets = new List<MapNode>();
                for (int tx = 0; tx < proceduralMapWidth; tx++)
                {
                    MapNode targetNode = nodeGrid[tx, y + 1];
                    if (targetNode != null)
                    {
                        possibleTargets.Add(targetNode);
                    }
                }
                
                // Create 1-3 connections
                int connectionCount = Random.Range(minConnectionsPerNode, Mathf.Min(maxConnectionsPerNode + 1, possibleTargets.Count + 1));
                for (int i = 0; i < connectionCount; i++)
                {
                    if (possibleTargets.Count == 0) break;
                    
                    int randomIndex = Random.Range(0, possibleTargets.Count);
                    MapNode targetNode = possibleTargets[randomIndex];
                    possibleTargets.RemoveAt(randomIndex);
                    
                    CreateConnection(sourceNode, targetNode);
                }
            }
        }
        
        // Set current node to start node
        currentNode = startNode;
    }
    
    // Rastgele harita oluşturma - düğüm tiplerini karıştırır
    public void ShuffleMapNodes()
    {
        Debug.Log("Shuffling map nodes...");
        
        // Mevcut haritayı temizle
        ClearMap();
        
        if (useSerializedMap && serializedMapData != null)
        {
            ShuffleSerializedMap();
        }
        else
        {
            GenerateProceduralMap();
        }
        
        // Başlangıçta sadece start düğümü erişilebilir olmalı
        foreach (MapNode node in allNodes)
        {
            if (node == startNode)
            {
                node.isAccessible = true;
                node.SetAsCurrent(true);
                currentNode = node;
            }
            else
            {
                node.isAccessible = false;
                node.SetAsCurrent(false);
            }
        }
        
        // Bağlantıları güncelle
        UpdateNodeAccessibility();
        
        Debug.Log("Map nodes shuffled successfully!");
    }
    
    // Serialize edilmiş haritanın düğüm tiplerini karıştır
    private void ShuffleSerializedMap()
    {
        Dictionary<string, MapNode> nodeDict = new Dictionary<string, MapNode>();
        
        // Önce tüm düğümleri oluştur
        foreach (var branchNode in serializedMapData.nodes)
        {
            // Düğüm tipini karıştır, ama başlangıç ve bitiş düğümlerini değiştirme
            MapNodeData nodeDataToUse = branchNode.nodeData;
            
            // Başlangıç ve bitiş düğümleri dışındaki düğümleri karıştır
            if (branchNode.nodeID != serializedMapData.startNodeID && 
                branchNode.nodeID != serializedMapData.endNodeID)
            {
                // Düğüm tipine göre rastgele bir yeni düğüm verisi seç
                switch (branchNode.nodeData.nodeType)
                {
                    case MapNodeData.NodeType.Battle:
                        if (battleNodeData.Count > 0)
                            nodeDataToUse = battleNodeData[Random.Range(0, battleNodeData.Count)];
                        break;
                        
                    case MapNodeData.NodeType.Shop:
                        if (shopNodeData.Count > 0)
                            nodeDataToUse = shopNodeData[Random.Range(0, shopNodeData.Count)];
                        break;
                        
                    case MapNodeData.NodeType.Boss:
                        if (bossNodeData.Count > 0)
                            nodeDataToUse = bossNodeData[Random.Range(0, bossNodeData.Count)];
                        break;
                        
                    case MapNodeData.NodeType.Event:
                        if (eventNodeData.Count > 0)
                            nodeDataToUse = eventNodeData[Random.Range(0, eventNodeData.Count)];
                        break;
                        
                    case MapNodeData.NodeType.Rest:
                        if (restNodeData.Count > 0)
                            nodeDataToUse = restNodeData[Random.Range(0, restNodeData.Count)];
                        break;
                        
                    case MapNodeData.NodeType.Mystery:
                        if (mysteryNodeData.Count > 0)
                            nodeDataToUse = mysteryNodeData[Random.Range(0, mysteryNodeData.Count)];
                        break;
                }
            }
            
            // Düğümü oluştur
            MapNode node = CreateNode(nodeDataToUse, branchNode.gridPosition);
            nodeDict.Add(branchNode.nodeID, node);
            
            // Başlangıç ve bitiş düğümlerini ayarla
            if (branchNode.nodeID == serializedMapData.startNodeID)
            {
                startNode = node;
                currentNode = node;
            }
            
            if (branchNode.nodeID == serializedMapData.endNodeID)
            {
                endNode = node;
            }
        }
        
        // Bağlantıları oluştur
        foreach (var connection in serializedMapData.connections)
        {
            if (nodeDict.TryGetValue(connection.sourceNodeID, out MapNode sourceNode) && 
                nodeDict.TryGetValue(connection.targetNodeID, out MapNode targetNode))
            {
                CreateConnection(sourceNode, targetNode);
            }
        }
    }
    
    private MapNodeData ChooseRandomNodeType(int row)
    {
        // Adjust probabilities based on row position
        float bossChance = (row > proceduralMapHeight * 0.7f) ? 0.2f : 0;
        float shopChance = 0.15f;
        float eventChance = 0.2f;
        float restChance = 0.1f;
        float mysteryChance = 0.15f;
        float battleChance = 1f - (bossChance + shopChance + eventChance + restChance + mysteryChance);
        
        float roll = Random.value;
        
        if (roll < battleChance && battleNodeData.Count > 0)
        {
            return battleNodeData[Random.Range(0, battleNodeData.Count)];
        }
        roll -= battleChance;
        
        if (roll < shopChance && shopNodeData.Count > 0)
        {
            return shopNodeData[Random.Range(0, shopNodeData.Count)];
        }
        roll -= shopChance;
        
        if (roll < eventChance && eventNodeData.Count > 0)
        {
            return eventNodeData[Random.Range(0, eventNodeData.Count)];
        }
        roll -= eventChance;
        
        if (roll < restChance && restNodeData.Count > 0)
        {
            return restNodeData[Random.Range(0, restNodeData.Count)];
        }
        roll -= restChance;
        
        if (roll < mysteryChance && mysteryNodeData.Count > 0)
        {
            return mysteryNodeData[Random.Range(0, mysteryNodeData.Count)];
        }
        
        // Boss nodes
        if (bossNodeData.Count > 0)
        {
            return bossNodeData[Random.Range(0, bossNodeData.Count)];
        }
        
        // Fallback to battle nodes
        if (battleNodeData.Count > 0)
        {
            return battleNodeData[Random.Range(0, battleNodeData.Count)];
        }
        
        Debug.LogError("No node data available!");
        return null;
    }
    
    private MapNode CreateNode(MapNodeData nodeData, Vector2 gridPosition)
    {
        if (nodeData == null || nodePrefab == null)
        {
            Debug.LogError("Cannot create node: node data or prefab is null");
            return null;
        }
        
        // Create node instance
        GameObject nodeGO = Instantiate(nodePrefab, mapContainer);
        nodeGO.name = $"Node_{nodeData.nodeID}";
        
        // Position the node
        RectTransform rectTransform = nodeGO.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(
                gridPosition.x * nodeSpacingX, 
                gridPosition.y * nodeSpacingY
            );
        }
        
        // Initialize node
        MapNode node = nodeGO.GetComponent<MapNode>();
        if (node != null)
        {
            node.Initialize(nodeData, gridPosition);
            allNodes.Add(node);
        }
        
        return node;
    }
    
    private MapConnection CreateConnection(MapNode sourceNode, MapNode targetNode)
    {
        if (sourceNode == null || targetNode == null || connectionPrefab == null)
        {
            Debug.LogError("Cannot create connection: source node, target node, or prefab is null");
            return null;
        }
        
        // Create connection instance
        GameObject connGO = Instantiate(connectionPrefab, mapContainer);
        connGO.name = $"Connection_{sourceNode.nodeData.nodeID}_to_{targetNode.nodeData.nodeID}";
        
        // Set the connection behind nodes
        connGO.transform.SetSiblingIndex(0);
        
        // Initialize connection
        MapConnection connection = connGO.GetComponent<MapConnection>();
        if (connection != null)
        {
            connection.Initialize(sourceNode, targetNode);
            allConnections.Add(connection);
            
            // Add connection to source node
            sourceNode.AddConnection(targetNode, connection);
        }
        
        return connection;
    }
    
    private void UpdateNodeAccessibility()
    {
        Debug.Log("Updating node accessibility...");
        
        // Önce tüm düğümleri erişilemez olarak işaretle
        foreach (MapNode node in allNodes)
        {
            // Mevcut düğüm ve tamamlanmış düğümler özel işleme tabi
            if (node == currentNode)
            {
                node.UpdateAccessibility(true);
                node.SetAsCurrent(true);
            }
            else if (node.isCompleted)
            {
                // Tamamlanmış düğümler erişilemez olmalı
                node.UpdateAccessibility(false);
            }
            else
            {
                // Diğer tüm düğümleri erişilemez yap
                node.UpdateAccessibility(false);
            }
        }
        
        // Sadece mevcut düğüme doğrudan bağlı olan ve tamamlanmamış düğümleri erişilebilir yap
        if (currentNode != null)
        {
            var connectedNodes = currentNode.GetConnectedNodes();
            Debug.Log($"Current node {currentNode.nodeData.nodeID} has {connectedNodes.Count} connected nodes");
            
            foreach (MapNode connectedNode in connectedNodes)
            {
                // Sadece tamamlanmamış düğümleri erişilebilir yap
                if (!connectedNode.isCompleted)
                {
                    connectedNode.UpdateAccessibility(true);
                    Debug.Log($"Making node {connectedNode.nodeData.nodeID} accessible");
                }
                else
                {
                    Debug.Log($"Node {connectedNode.nodeData.nodeID} is completed, not making accessible");
                }
            }
        }
        
        // Bağlantıları güncelle
        foreach (MapConnection connection in allConnections)
        {
            MapNode source = connection.GetSourceNode();
            MapNode target = connection.GetTargetNode();
            
            // Bağlantıları güncelle
            if (source == currentNode && target.isAccessible)
            {
                // Mevcut düğümden çıkan ve erişilebilir düğüme giden bağlantı
                connection.SetAccessible(true);
                Debug.Log($"Connection from {source.nodeData.nodeID} to {target.nodeData.nodeID} is accessible");
            }
            else if (source.isCompleted && target.isCompleted)
            {
                // İki tamamlanmış düğüm arasındaki bağlantı
                connection.SetCompleted(true);
                connection.SetAccessible(false);
                Debug.Log($"Connection from {source.nodeData.nodeID} to {target.nodeData.nodeID} is completed");
            }
            else
            {
                // Diğer tüm bağlantılar erişilemez
                connection.SetAccessible(false);
                connection.SetCompleted(false);
            }
        }
        
        // Debug log
        Debug.Log("Node accessibility updated:");
        foreach (MapNode node in allNodes)
        {
            Debug.Log($"Node {node.nodeData.nodeID}: Accessible={node.isAccessible}, Completed={node.isCompleted}, Current={node.isCurrent}");
        }
    }
    
    public void OnNodeSelected(MapNode node)
    {
        if (!node.isAccessible)
        {
            Debug.LogWarning($"Node {node.nodeData.nodeID} is not accessible!");
            return;
        }
        
        Debug.Log($"Node selected: {node.nodeData.nodeID}");
        
        // Set previous node as completed
        if (currentNode != null)
        {
            currentNode.SetCompleted();
            currentNode.SetAsCurrent(false);
        }
        
        // Update current node
        currentNode = node;
        currentNode.SetAsCurrent(true);
        
        // Update accessibility
        UpdateNodeAccessibility();
        
        // Check if this is the end node
        bool isEndNode = (node == endNode);
        
        // Load the room scene
        LoadNodeRoom(node, isEndNode);
        
        // If this is the end node and it's being completed, trigger the branch finished event
        if (isEndNode)
        {
            Debug.Log($"End node reached for biom: {currentBiom?.biomID ?? "Unknown"}");
            
            // We'll raise the event when the player completes the end room
            // The actual event will be raised in MapRoomIntegrator.ReturnToMap()
        }
    }
    
    private void LoadNodeRoom(MapNode node, bool isEndNode = false)
    {
        if (node.nodeData.roomData == null)
        {
            Debug.LogError($"No room data assigned to node {node.nodeData.nodeID}!");
            return;
        }
        
        // Save current map state
        SaveMapState();
        
        // Pass node data to MapRoomIntegrator before loading the scene
        MapRoomIntegrator integrator = FindObjectOfType<MapRoomIntegrator>();
        if (integrator == null)
        {
            // Create a new MapRoomIntegrator if it doesn't exist
            GameObject integratorObj = new GameObject("MapRoomIntegrator");
            integrator = integratorObj.AddComponent<MapRoomIntegrator>();
            DontDestroyOnLoad(integratorObj);
        }
        
        // Set current room data and node data
        integrator.SetCurrentRoomData(node.nodeData.roomData, node.nodeData, currentBiom, isEndNode);
        
        // Ensure RoomGenerator exists in the target scene
        GameObject roomGenPrefab = Resources.Load<GameObject>("RoomGenerator");
        if (roomGenPrefab == null)
        {
            Debug.LogWarning("RoomGenerator prefab not found in Resources. Will create one in the target scene.");
        }
        
        // Load the game scene
        Debug.Log($"Loading scene for node type: {node.nodeData.nodeType} with biom: {currentBiom?.biomID ?? "Unknown"}");
        SceneManager.LoadScene(gameSceneName);
        
        // Pass room data to the game scene
        DontDestroyOnLoad(gameObject);
        
        // Set up callback for when scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            Debug.Log($"Scene {scene.name} loaded successfully. Looking for RoomGenerator...");
            
            // Find room generator or initializer
            RoomGenerator roomGenerator = FindObjectOfType<RoomGenerator>();
            if (roomGenerator != null)
            {
                Debug.Log("RoomGenerator found. Setting up room...");
                // Set up the room based on current node
                SetupRoom(roomGenerator);
            }
            else
            {
                Debug.LogWarning("RoomGenerator not found in the loaded scene! Make sure it exists in the scene.");
                
                // Try to find MapRoomIntegrator as an alternative
                MapRoomIntegrator integrator = FindObjectOfType<MapRoomIntegrator>();
                if (integrator != null)
                {
                    Debug.Log("MapRoomIntegrator found. Room setup will be handled by the integrator.");
                }
                else
                {
                    Debug.LogError("Neither RoomGenerator nor MapRoomIntegrator found! Room setup failed.");
                }
            }
        }
        
        // Remove the callback
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Hide map UI
        if (mapCanvas != null)
        {
            mapCanvas.SetActive(false);
        }
    }
    
    private void SetupRoom(RoomGenerator roomGenerator)
    {
        if (currentNode == null || currentNode.nodeData == null || currentNode.nodeData.roomData == null)
        {
            Debug.LogError("Current node or room data is null!");
            return;
        }
        
        // Set up room based on node data
        // This could be expanded based on your specific room generation system
        RoomData roomData = currentNode.nodeData.roomData;
        
        // You might need to customize this part based on your RoomGenerator implementation
        // For example, you might need to set specific room data or spawn specific enemies
    }
    
    public void ShowMap()
    {
        if (mapCanvas != null)
        {
            mapCanvas.SetActive(true);
            UpdateNodeAccessibility();
        }
    }
    
    public void HideMap()
    {
        if (mapCanvas != null)
        {
            mapCanvas.SetActive(false);
        }
    }
    
    public void ToggleMap()
    {
        if (mapCanvas != null)
        {
            bool newState = !mapCanvas.activeSelf;
            mapCanvas.SetActive(newState);
            
            if (newState)
            {
                Debug.Log("Map canvas activated - updating accessibility");
                UpdateNodeAccessibility();
            }
            else
            {
                Debug.Log("Map canvas deactivated");
            }
        }
        else
        {
            Debug.LogError("Map canvas reference is missing!");
        }
    }
    
    private void SaveMapState()
    {
        // This could be expanded with a proper save system
        // For now, we're just keeping the state in memory
    }
    
    private void ClearMap()
    {
        // Destroy all nodes and connections
        foreach (MapNode node in allNodes)
        {
            if (node != null && node.gameObject != null)
            {
                Destroy(node.gameObject);
            }
        }
        
        foreach (MapConnection connection in allConnections)
        {
            if (connection != null && connection.gameObject != null)
            {
                Destroy(connection.gameObject);
            }
        }
        
        allNodes.Clear();
        allConnections.Clear();
        startNode = null;
        endNode = null;
        currentNode = null;
    }
} 