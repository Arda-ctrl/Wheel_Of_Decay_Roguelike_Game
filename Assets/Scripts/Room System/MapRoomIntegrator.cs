using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles the integration between the node map system and the room generation system.
/// This class acts as a bridge between MapManager and RoomGenerator/LevelGenerator.
/// </summary>
public class MapRoomIntegrator : MonoBehaviour
{
    public static MapRoomIntegrator instance;
    
    [Header("Integration Settings")]
    public string mapSceneName = "MapScene";
    public KeyCode mapToggleKey = KeyCode.M;
    
    [Header("Room References")]
    public RoomGenerator roomGenerator;
    public ImprovedDungeonGenerator improvedDungeonGenerator;
    public LevelGenerator levelGenerator;
    
    // Current room data from map node
    private RoomData currentRoomData;
    private MapNodeData currentNodeData;
    private BiomData currentBiom;
    private bool isEndNode = false;
    private bool branchCompleted = false;
    
    // Hangi dungeon generator'ı kullanacağız
    [Header("Generator Selection")]
    public bool useImprovedGenerator = true;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Sahne değişimlerini dinle
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void OnDestroy()
    {
        // Sahne değişim olayından çık
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
        // Yeni sahnede Generator'ları bul
        StartCoroutine(FindGeneratorsAfterSceneLoad());
    }
    
    private IEnumerator FindGeneratorsAfterSceneLoad()
    {
        // Sahne tam olarak yüklenene kadar bekle
        yield return new WaitForSeconds(0.2f);
        
        // Önce ImprovedDungeonGenerator'ı kontrol et
        if (ImprovedDungeonGenerator.instance != null)
        {
            improvedDungeonGenerator = ImprovedDungeonGenerator.instance;
            Debug.Log("Found ImprovedDungeonGenerator.instance");
        }
        else
        {
            // ImprovedDungeonGenerator'ı bul
            improvedDungeonGenerator = FindObjectOfType<ImprovedDungeonGenerator>();
            
            if (improvedDungeonGenerator == null && useImprovedGenerator)
            {
                Debug.LogWarning("ImprovedDungeonGenerator not found in the new scene. Creating one...");
                
                // Eğer ImprovedDungeonGenerator yoksa, bir tane oluştur
                GameObject genObj = new GameObject("ImprovedDungeonGenerator");
                improvedDungeonGenerator = genObj.AddComponent<ImprovedDungeonGenerator>();
                Debug.Log("Created new ImprovedDungeonGenerator");
            }
        }
        
        // RoomGenerator'ı da kontrol et (geriye dönük uyumluluk için)
        if (RoomGenerator.instance != null)
        {
            roomGenerator = RoomGenerator.instance;
            Debug.Log("Found RoomGenerator.instance");
        }
        else
        {
            // RoomGenerator'ı bul
            roomGenerator = FindObjectOfType<RoomGenerator>();
            
            if (roomGenerator == null && !useImprovedGenerator)
            {
                Debug.LogWarning("RoomGenerator not found in the new scene. Creating one...");
                
                // Eğer RoomGenerator yoksa, bir tane oluştur
                GameObject roomGenObj = new GameObject("RoomGenerator");
                roomGenerator = roomGenObj.AddComponent<RoomGenerator>();
                Debug.Log("Created new RoomGenerator");
            }
        }
        
        // Eğer oda verileri varsa, seçilen Generator'a uygula
        if (currentRoomData != null)
        {
            Debug.Log("Applying room data to Generator");
            ApplyRoomData();
        }
        
        // LevelGenerator'ı da bul
        levelGenerator = FindObjectOfType<LevelGenerator>();
    }
    
    private void Start()
    {
        // Find generators if not assigned
        if (roomGenerator == null && !useImprovedGenerator)
        {
            roomGenerator = FindObjectOfType<RoomGenerator>();
        }
        
        if (improvedDungeonGenerator == null && useImprovedGenerator)
        {
            improvedDungeonGenerator = FindObjectOfType<ImprovedDungeonGenerator>();
        }
        
        if (levelGenerator == null)
        {
            levelGenerator = FindObjectOfType<LevelGenerator>();
        }
    }
    
    private void Update()
    {
        // Toggle map with key press
        if (Input.GetKeyDown(mapToggleKey))
        {
            ToggleMap();
        }
        
        // Debug info
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (currentNodeData != null)
            {
                Debug.Log($"Current Node Type: {currentNodeData.nodeType}");
                Debug.Log($"Current Room Data: {currentRoomData?.roomID}");
                Debug.Log($"Using Improved Generator: {useImprovedGenerator}");
            }
            else
            {
                Debug.Log("No current node data");
            }
        }
    }
    
    public void SetCurrentRoomData(RoomData roomData, MapNodeData nodeData, BiomData biom = null, bool isEndNode = false)
    {
        Debug.Log($"Setting current room data for node type: {nodeData?.nodeType}, biom: {biom?.biomID ?? "None"}, isEndNode: {isEndNode}");
        
        // Null kontrolü
        if (roomData == null)
        {
            Debug.LogError("Room data is null! Using fallback if available.");
            
            // Eğer düğüm verisinde oda verisi varsa onu kullan
            if (nodeData != null && nodeData.roomData != null)
            {
                roomData = nodeData.roomData;
                Debug.Log("Using room data from node data as fallback");
            }
        }
        
        // Biom kontrolü
        if (biom == null && roomData != null && roomData.biom != null)
        {
            biom = roomData.biom;
            Debug.Log($"Using biom from room data: {biom.biomID}");
        }
        
        // Eğer hala biom yoksa, Forest biom'unu yükle
        if (biom == null)
        {
            biom = Resources.Load<BiomData>("Bioms/Forest");
            if (biom != null)
            {
                Debug.Log("Loaded Forest biom from Resources as fallback");
            }
            else
            {
                Debug.LogError("Failed to load Forest biom from Resources!");
            }
        }
        
        currentRoomData = roomData;
        currentNodeData = nodeData;
        currentBiom = biom;
        this.isEndNode = isEndNode;
        this.branchCompleted = false;
        
        // Generator referanslarını kontrol et
        if (useImprovedGenerator && improvedDungeonGenerator == null)
        {
            Debug.Log("Looking for ImprovedDungeonGenerator...");
            improvedDungeonGenerator = FindObjectOfType<ImprovedDungeonGenerator>();
            
            if (improvedDungeonGenerator == null)
            {
                Debug.LogWarning("ImprovedDungeonGenerator not found in the current scene. Will try to find it after scene load.");
            }
        }
        else if (!useImprovedGenerator && roomGenerator == null)
        {
            Debug.Log("Looking for RoomGenerator...");
            roomGenerator = FindObjectOfType<RoomGenerator>();
            
            if (roomGenerator == null)
            {
                Debug.LogWarning("RoomGenerator not found in the current scene. Will try to find it after scene load.");
            }
        }
        
        // Apply room data to the appropriate generator
        ApplyRoomData();
    }
    
    private void ApplyRoomData()
    {
        if (currentRoomData == null) return;
        
        Debug.Log($"Applying room data for node type: {currentNodeData?.nodeType}");
        
        // Hangi generator'ı kullanacağımızı belirle
        if (useImprovedGenerator)
        {
            ApplyRoomDataToImprovedGenerator();
        }
        else
        {
            ApplyRoomDataToRoomGenerator();
        }
    }
    
    private void ApplyRoomDataToImprovedGenerator()
    {
        // Apply to ImprovedDungeonGenerator if available
        if (improvedDungeonGenerator != null)
        {
            // Düğüm tipine göre oda yapılandırması
            if (currentNodeData != null)
            {
                ConfigureRoomBasedOnNodeType();
            }
            
            // If we have a biom, use its room pool
            if (currentBiom != null)
            {
                // For end nodes, set the end room from the biom
                if (isEndNode && currentBiom.endRoom != null)
                {
                    // Use the biom's end room
                    improvedDungeonGenerator.roomPool = new RoomData[] { currentBiom.endRoom };
                }
                else if (currentNodeData.nodeType == MapNodeData.NodeType.Start && currentBiom.startRoom != null)
                {
                    // Use the biom's start room
                    improvedDungeonGenerator.roomPool = new RoomData[] { currentBiom.startRoom };
                }
                else
                {
                    // Use the biom's room pool
                    improvedDungeonGenerator.roomPool = currentBiom.roomPool.ToArray();
                }
                
                Debug.Log($"Using biom {currentBiom.biomID} room pool with {currentBiom.roomPool.Count} rooms for ImprovedDungeonGenerator");
                
                // Biom rengini uygula
                RenderSettings.ambientLight = currentBiom.ambientColor;
                
                // Eğer müzik varsa oynat
                if (currentBiom.music != null)
                {
                    AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
                    }
                    audioSource.clip = currentBiom.music;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            
            // Odayı oluştur
            StartCoroutine(DelayedImprovedDungeonGeneration());
        }
        else
        {
            Debug.LogError("ImprovedDungeonGenerator not found but useImprovedGenerator is true!");
        }
    }
    
    private void ApplyRoomDataToRoomGenerator()
    {
        // Apply to RoomGenerator if available
        if (roomGenerator != null)
        {
            // Düğüm tipine göre oda yapılandırması
            if (currentNodeData != null)
            {
                ConfigureRoomBasedOnNodeType();
            }
            
            // If we have a biom, use its room pool
            if (currentBiom != null)
            {
                // For end nodes, set the end room from the biom
                if (isEndNode && currentBiom.endRoom != null)
                {
                    // Use the biom's end room
                    roomGenerator.roomPool = new RoomData[] { currentBiom.endRoom };
                }
                else if (currentNodeData.nodeType == MapNodeData.NodeType.Start && currentBiom.startRoom != null)
                {
                    // Use the biom's start room
                    roomGenerator.roomPool = new RoomData[] { currentBiom.startRoom };
                }
                else
                {
                    // Use the biom's room pool
                    roomGenerator.roomPool = currentBiom.roomPool.ToArray();
                }
                
                Debug.Log($"Using biom {currentBiom.biomID} room pool with {currentBiom.roomPool.Count} rooms");
                
                // Biom rengini uygula
                RenderSettings.ambientLight = currentBiom.ambientColor;
                
                // Eğer müzik varsa oynat
                if (currentBiom.music != null)
                {
                    AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
                    }
                    audioSource.clip = currentBiom.music;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            
            // Odayı oluştur
            StartCoroutine(DelayedRoomGeneration());
        }
        else
        {
            Debug.LogError("RoomGenerator not found but useImprovedGenerator is false!");
        }
    }
    
    private IEnumerator DelayedImprovedDungeonGeneration()
    {
        // Sahnenin tam olarak yüklenmesi için kısa bir süre bekle
        yield return new WaitForSeconds(0.5f);
        
        // Oda oluşturma
        if (improvedDungeonGenerator != null)
        {
            int roomCount = 10; // Varsayılan oda sayısı
            
            // Düğüm tipine göre oda sayısını ayarla
            if (currentNodeData != null)
            {
                switch (currentNodeData.nodeType)
                {
                    case MapNodeData.NodeType.Battle:
                        roomCount = Random.Range(8, 13);
                        break;
                    case MapNodeData.NodeType.Boss:
                        roomCount = Random.Range(3, 6);
                        break;
                    case MapNodeData.NodeType.Shop:
                    case MapNodeData.NodeType.Rest:
                        roomCount = 1;
                        break;
                    case MapNodeData.NodeType.Event:
                    case MapNodeData.NodeType.Mystery:
                        roomCount = Random.Range(1, 4);
                        break;
                    default:
                        roomCount = 10;
                        break;
                }
            }
            
            improvedDungeonGenerator.roomCount = roomCount;
            improvedDungeonGenerator.GenerateDungeon(roomCount);
        }
    }
    
    private void ConfigureRoomBasedOnNodeType()
    {
        if (currentNodeData == null) return;
        
        // Hangi generator'ı kullanıyoruz?
        if (useImprovedGenerator && improvedDungeonGenerator != null)
        {
            // ImprovedDungeonGenerator için düğüm tipine göre yapılandırma
            switch (currentNodeData.nodeType)
            {
                case MapNodeData.NodeType.Battle:
                    // Savaş odası yapılandırması
                    improvedDungeonGenerator.roomCount = Random.Range(8, 13); // 8-12 odalı bir level
                    Debug.Log("Configuring Battle Room for ImprovedDungeonGenerator");
                    break;
                    
                case MapNodeData.NodeType.Shop:
                    // Mağaza odası yapılandırması
                    improvedDungeonGenerator.roomCount = 1; // Tek oda
                    Debug.Log("Configuring Shop Room for ImprovedDungeonGenerator");
                    break;
                    
                case MapNodeData.NodeType.Boss:
                    // Boss odası yapılandırması
                    improvedDungeonGenerator.roomCount = Random.Range(3, 6); // 3-5 odalı bir level
                    Debug.Log("Configuring Boss Room for ImprovedDungeonGenerator");
                    break;
                    
                case MapNodeData.NodeType.Event:
                    // Etkinlik odası yapılandırması
                    improvedDungeonGenerator.roomCount = Random.Range(1, 4); // 1-3 odalı bir level
                    Debug.Log("Configuring Event Room for ImprovedDungeonGenerator");
                    break;
                    
                case MapNodeData.NodeType.Rest:
                    // Dinlenme odası yapılandırması
                    improvedDungeonGenerator.roomCount = 1; // Tek oda
                    Debug.Log("Configuring Rest Room for ImprovedDungeonGenerator");
                    break;
                    
                case MapNodeData.NodeType.Mystery:
                    // Gizem odası yapılandırması
                    improvedDungeonGenerator.roomCount = Random.Range(1, 4); // 1-3 odalı bir level
                    Debug.Log("Configuring Mystery Room for ImprovedDungeonGenerator");
                    break;
                    
                case MapNodeData.NodeType.Start:
                    // Başlangıç odası yapılandırması
                    improvedDungeonGenerator.roomCount = 1; // Tek oda
                    Debug.Log("Configuring Start Room for ImprovedDungeonGenerator");
                    break;
                    
                case MapNodeData.NodeType.End:
                    // Bitiş odası yapılandırması
                    improvedDungeonGenerator.roomCount = 1; // Tek oda
                    Debug.Log("Configuring End Room for ImprovedDungeonGenerator");
                    break;
            }
        }
        else if (!useImprovedGenerator && roomGenerator != null)
        {
            // RoomGenerator için düğüm tipine göre yapılandırma
            switch (currentNodeData.nodeType)
            {
                case MapNodeData.NodeType.Battle:
                    // Savaş odası yapılandırması
                    roomGenerator.numberOfRooms = Random.Range(8, 13); // 8-12 odalı bir level
                    Debug.Log("Configuring Battle Room");
                    break;
                    
                case MapNodeData.NodeType.Shop:
                    // Mağaza odası yapılandırması
                    roomGenerator.numberOfRooms = 1; // Tek oda
                    Debug.Log("Configuring Shop Room");
                    break;
                    
                case MapNodeData.NodeType.Boss:
                    // Boss odası yapılandırması
                    roomGenerator.numberOfRooms = Random.Range(3, 6); // 3-5 odalı bir level
                    Debug.Log("Configuring Boss Room");
                    break;
                    
                case MapNodeData.NodeType.Event:
                    // Etkinlik odası yapılandırması
                    roomGenerator.numberOfRooms = Random.Range(1, 4); // 1-3 odalı bir level
                    Debug.Log("Configuring Event Room");
                    break;
                    
                case MapNodeData.NodeType.Rest:
                    // Dinlenme odası yapılandırması
                    roomGenerator.numberOfRooms = 1; // Tek oda
                    Debug.Log("Configuring Rest Room");
                    break;
                    
                case MapNodeData.NodeType.Mystery:
                    // Gizem odası yapılandırması
                    roomGenerator.numberOfRooms = 3; // Üç odalı küçük bir level
                    Debug.Log("Configuring Mystery Room");
                    break;
                    
                case MapNodeData.NodeType.Start:
                    // Başlangıç odası yapılandırması
                    roomGenerator.numberOfRooms = 1; // Tek oda
                    Debug.Log("Configuring Start Room");
                    break;
                    
                case MapNodeData.NodeType.End:
                    // Bitiş odası yapılandırması
                    roomGenerator.numberOfRooms = 1; // Tek oda
                    Debug.Log("Configuring End Room");
                    break;
            }
        }
    }

    private void ConfigureLevelBasedOnNodeType()
    {
        if (currentNodeData == null || levelGenerator == null || levelGenerator.roomData == null) return;
        
        switch (currentNodeData.nodeType)
        {
            case MapNodeData.NodeType.Battle:
                // Savaş level yapılandırması
                levelGenerator.distancetoEnd = 5; // Örnek: 5 odalı küçük bir level
                break;
                
            case MapNodeData.NodeType.Shop:
                // Mağaza level yapılandırması
                levelGenerator.distancetoEnd = 1; // Tek oda
                break;
                
            case MapNodeData.NodeType.Boss:
                // Boss level yapılandırması
                levelGenerator.distancetoEnd = 1; // Tek büyük oda
                // Boss odası ayarla
                if (currentRoomData != null)
                {
                    levelGenerator.roomData.endRoom = currentRoomData;
                }
                break;
                
            // Diğer düğüm tipleri için benzer yapılandırmalar...
        }
    }
    
    private IEnumerator DelayedRoomGeneration()
    {
        // Wait a frame to ensure everything is set up
        yield return null;
        
        // Sahne değiştiğinde RoomGenerator referansını tekrar bul
        if (roomGenerator == null)
        {
            Debug.Log("RoomGenerator reference lost during scene transition. Looking for RoomGenerator...");
            
            // Önce statik instance'ı kontrol et
            if (RoomGenerator.instance != null)
            {
                roomGenerator = RoomGenerator.instance;
                Debug.Log("Found RoomGenerator.instance");
            }
            else
            {
                roomGenerator = FindObjectOfType<RoomGenerator>();
                
                if (roomGenerator == null)
                {
                    Debug.LogError("RoomGenerator not found in the scene! Creating one...");
                    
                    // Eğer RoomGenerator yoksa, bir tane oluştur
                    GameObject roomGenObj = new GameObject("RoomGenerator");
                    roomGenerator = roomGenObj.AddComponent<RoomGenerator>();
                    Debug.Log("Created new RoomGenerator");
                }
                else
                {
                    Debug.Log("Found RoomGenerator in the scene");
                }
            }
        }
        
        // Call your room generation method
        if (roomGenerator != null)
        {
            // Oda havuzunu kontrol et ve debug bilgisi
            if (roomGenerator.roomPool == null || roomGenerator.roomPool.Length == 0)
            {
                Debug.LogError("Room pool is empty! Attempting to fix...");
                
                // Eğer biom varsa, biom'dan odaları al
                if (currentBiom != null && currentBiom.roomPool.Count > 0)
                {
                    Debug.Log($"Using biom {currentBiom.biomID} room pool with {currentBiom.roomPool.Count} rooms");
                    roomGenerator.roomPool = currentBiom.roomPool.ToArray();
                }
                // Eğer biom yoksa veya biom'un oda havuzu boşsa, tek bir oda kullan
                else if (currentRoomData != null)
                {
                    Debug.Log("Using single room as fallback");
                    roomGenerator.roomPool = new RoomData[] { currentRoomData };
                }
                else
                {
                    Debug.LogError("Cannot find any rooms to use! Room generation will fail.");
                }
            }
            
            Debug.Log("Starting room generation with " + (roomGenerator.roomPool != null ? roomGenerator.roomPool.Length : 0) + " rooms in pool");
            
            // Doğrudan GenerateLevel metodunu çağır
            if (roomGenerator.roomPool != null && roomGenerator.roomPool.Length > 0)
            {
                roomGenerator.SendMessage("GenerateLevel", SendMessageOptions.DontRequireReceiver);
                Debug.Log("Room generation completed");
            }
            else
            {
                Debug.LogError("Room pool is still empty after fix attempt! Cannot generate rooms.");
            }
        }
        else
        {
            Debug.LogError("RoomGenerator is still null, cannot generate rooms!");
        }
    }
    
    private IEnumerator DelayedLevelGeneration()
    {
        // Wait a frame to ensure everything is set up
        yield return null;
        
        // Call your level generation method
        // This is just an example, you'll need to adapt it to your system
        if (levelGenerator != null)
        {
            // Example: Restart the scene to trigger level generation
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void ReturnToMap(bool roomCompleted = true)
    {
        Debug.Log("Returning to map...");
        
        // If this is the end room of a branch and it was completed
        if (isEndNode && roomCompleted)
        {
            branchCompleted = true;
            Debug.Log($"Branch with biom {currentBiom?.biomID ?? "Unknown"} completed!");
            
            // Trigger the branch finished event
            var branchFinishedEvent = MapManager.OnBranchFinished;
            if (branchFinishedEvent != null && currentBiom != null)
            {
                branchFinishedEvent(currentBiom);
            }
            
            // Load hub scene if specified, otherwise fall back to map scene
            string hubSceneName = "Hub";
            if (Application.CanStreamedLevelBeLoaded(hubSceneName))
            {
                Debug.Log("Loading hub scene...");
                SceneManager.LoadScene(hubSceneName);
            }
            else
            {
                Debug.LogWarning($"Hub scene '{hubSceneName}' not found, loading map scene instead.");
                SceneManager.LoadScene(mapSceneName);
            }
        }
        else
        {
            // Regular node completed, return to map
            Debug.Log("Room completed, returning to map scene...");
            SceneManager.LoadScene(mapSceneName);
        }
        
        // Find and clear the minimap if it exists
        DungeonMinimap minimap = FindObjectOfType<DungeonMinimap>();
        if (minimap != null)
        {
            minimap.ClearMinimap();
        }
    }
    
    public void ToggleMap()
    {
        // Find MapManager
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            mapManager.ToggleMap();
        }
        else
        {
            // Map manager not found, load map scene
            ReturnToMap();
        }
    }
} 