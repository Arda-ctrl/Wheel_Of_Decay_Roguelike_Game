#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DoorCapWizard : EditorWindow
{
    private Sprite capSprite;
    private GameObject roomGeneratorObj;
    private string prefabName = "DoorCap";
    private string prefabPath = "Assets/Prefabs/Room Objects/";
    private bool createAllDirections = true;
    private Vector2 spriteSize = new Vector2(3f, 2f);
    private Vector2 colliderSize = new Vector2(2.5f, 1.5f);
    
    [MenuItem("Room System/Door Cap Wizard")]
    public static void ShowWindow()
    {
        GetWindow<DoorCapWizard>("Door Cap Wizard");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Door Cap Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Sprite selection
        EditorGUILayout.LabelField("Cap Visual", EditorStyles.boldLabel);
        capSprite = (Sprite)EditorGUILayout.ObjectField("Cap Sprite", capSprite, typeof(Sprite), false);
        spriteSize = EditorGUILayout.Vector2Field("Sprite Size", spriteSize);
        colliderSize = EditorGUILayout.Vector2Field("Collider Size", colliderSize);
        
        EditorGUILayout.Space();
        
        // Room Generator selection
        EditorGUILayout.LabelField("Room Generator", EditorStyles.boldLabel);
        roomGeneratorObj = (GameObject)EditorGUILayout.ObjectField("Room Generator", roomGeneratorObj, typeof(GameObject), true);
        
        EditorGUILayout.Space();
        
        // Prefab settings
        EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);
        prefabName = EditorGUILayout.TextField("Base Prefab Name", prefabName);
        prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);
        createAllDirections = EditorGUILayout.Toggle("Create All Directions", createAllDirections);
        
        EditorGUILayout.Space();
        
        // Create button
        if (GUILayout.Button("Create Door Cap Prefabs"))
        {
            CreateDoorCaps();
        }
        
        // Auto-assign button
        EditorGUI.BeginDisabledGroup(roomGeneratorObj == null);
        if (GUILayout.Button("Auto-Assign to Room Generator"))
        {
            AutoAssignToRoomGenerator();
        }
        EditorGUI.EndDisabledGroup();
    }
    
    private void CreateDoorCaps()
    {
        if (capSprite == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a sprite for the door cap.", "OK");
            return;
        }
        
        // Create directory if it doesn't exist
        if (!System.IO.Directory.Exists(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
        }
        
        GameObject mainPrefab = null;
        
        // Create prefabs for each direction if requested
        if (createAllDirections)
        {
            ConnectionDirection[] directions = new ConnectionDirection[] 
            {
                ConnectionDirection.North,
                ConnectionDirection.East,
                ConnectionDirection.South,
                ConnectionDirection.West
            };
            
            foreach (var direction in directions)
            {
                GameObject doorCap = CreateDoorCapForDirection(direction);
                string path = $"{prefabPath}/{prefabName}_{direction}.prefab";
                
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(doorCap, path);
                DestroyImmediate(doorCap);
                
                if (mainPrefab == null)
                {
                    mainPrefab = prefab;
                }
            }
        }
        else
        {
            // Create a single generic door cap
            GameObject doorCap = CreateDoorCapForDirection(ConnectionDirection.North);
            string path = $"{prefabPath}/{prefabName}.prefab";
            
            mainPrefab = PrefabUtility.SaveAsPrefabAsset(doorCap, path);
            DestroyImmediate(doorCap);
        }
        
        EditorUtility.DisplayDialog("Success", "Door cap prefabs created successfully!", "OK");
        
        // Ping the main prefab in the Project window
        if (mainPrefab != null)
        {
            EditorGUIUtility.PingObject(mainPrefab);
        }
    }
    
    private GameObject CreateDoorCapForDirection(ConnectionDirection direction)
    {
        GameObject doorCap = new GameObject($"DoorCap_{direction}");
        
        // Add sprite renderer
        SpriteRenderer spriteRenderer = doorCap.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = capSprite;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        spriteRenderer.size = spriteSize;
        
        // Add collider
        BoxCollider2D collider = doorCap.AddComponent<BoxCollider2D>();
        collider.size = colliderSize;
        collider.isTrigger = false;
        
        // Add door cap component
        DoorCap doorCapComponent = doorCap.AddComponent<DoorCap>();
        
        // Add setup component for direction
        DoorCapSetup setup = doorCap.AddComponent<DoorCapSetup>();
        setup.capSprite = capSprite;
        setup.direction = direction;
        setup.spriteSize = spriteSize;
        setup.colliderSize = colliderSize;
        setup.ApplyDirection();
        
        return doorCap;
    }
    
    private void AutoAssignToRoomGenerator()
    {
        if (roomGeneratorObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Room Generator GameObject.", "OK");
            return;
        }
        
        RoomGenerator roomGenerator = roomGeneratorObj.GetComponent<RoomGenerator>();
        if (roomGenerator == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected GameObject does not have a RoomGenerator component.", "OK");
            return;
        }
        
        // Find the first created door cap prefab
        string searchPath = createAllDirections ? $"{prefabName}_North" : prefabName;
        string[] guids = AssetDatabase.FindAssets($"t:Prefab {searchPath}", new[] { prefabPath });
        
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No door cap prefab found. Please create the prefabs first.", "OK");
            return;
        }
        
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject doorCapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        
        if (doorCapPrefab != null)
        {
            Undo.RecordObject(roomGenerator, "Assign Door Cap Prefab");
            roomGenerator.doorCapPrefab = doorCapPrefab;
            EditorUtility.SetDirty(roomGenerator);
            
            EditorUtility.DisplayDialog("Success", "Door cap prefab assigned to Room Generator.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to load door cap prefab.", "OK");
        }
    }
}
#endif 