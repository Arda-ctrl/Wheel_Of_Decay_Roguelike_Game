using UnityEngine;

/// <summary>
/// Test manager for verifying Goblin system is working correctly
/// Place this script on an empty GameObject in your test scene
/// </summary>
public class GoblinTestManager : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private bool showGizmos = true;
    
    [Header("Goblin Prefabs")]
    [SerializeField] private GameObject daggerGoblinPrefab;
    [SerializeField] private GameObject daggerGoblinBomberPrefab;
    [SerializeField] private GameObject trapperGoblinPrefab;
    [SerializeField] private GameObject trapperGoblinBombsPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 10f);
    [SerializeField] private int maxGoblinsPerType = 2;
    
    private void Start()
    {
        if (enableDebugMode)
        {
            VerifySystemComponents();
        }
    }
    
    private void Update()
    {
        if (enableDebugMode)
        {
            // Test controls
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SpawnGoblin(daggerGoblinPrefab, "Dagger Goblin");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SpawnGoblin(daggerGoblinBomberPrefab, "Dagger Goblin Bomber");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SpawnGoblin(trapperGoblinPrefab, "Trapper Goblin");
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SpawnGoblin(trapperGoblinBombsPrefab, "Trapper Goblin Bombs");
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearAllGoblins();
            }
        }
    }
    
    private void VerifySystemComponents()
    {
        Debug.Log("🧙‍♂️ Goblin System Verification Started...");
        
        // Check PlayerController
        if (PlayerController.Instance == null)
        {
            Debug.LogError("❌ PlayerController.Instance is null! Goblins need PlayerController to work.");
        }
        else
        {
            Debug.Log("✅ PlayerController found");
        }
        
        // Check EventManager
        if (EventManager.Instance == null)
        {
            Debug.LogWarning("⚠️ EventManager.Instance is null! Death events won't work.");
        }
        else
        {
            Debug.Log("✅ EventManager found");
        }
        
        // Check AudioManager
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("⚠️ AudioManager.Instance is null! Goblin sounds won't work.");
        }
        else
        {
            Debug.Log("✅ AudioManager found");
        }
        
        // Check prefabs
        VerifyPrefab(daggerGoblinPrefab, "Dagger Goblin", typeof(DaggerGoblin));
        VerifyPrefab(daggerGoblinBomberPrefab, "Dagger Goblin Bomber", typeof(DaggerGoblinBomber));
        VerifyPrefab(trapperGoblinPrefab, "Trapper Goblin", typeof(TrapperGoblin));
        VerifyPrefab(trapperGoblinBombsPrefab, "Trapper Goblin Bombs", typeof(TrapperGoblinBombs));
        
        Debug.Log("🧙‍♂️ Goblin System Verification Complete!");
        Debug.Log("🎮 Test Controls: 1-4 to spawn goblins, C to clear all");
    }
    
    private void VerifyPrefab(GameObject prefab, string name, System.Type expectedComponent)
    {
        if (prefab == null)
        {
            Debug.LogError($"❌ {name} prefab is null!");
            return;
        }
        
        var component = prefab.GetComponent(expectedComponent);
        if (component == null)
        {
            Debug.LogError($"❌ {name} prefab missing {expectedComponent.Name} component!");
        }
        else
        {
            Debug.Log($"✅ {name} prefab configured correctly");
        }
    }
    
    private void SpawnGoblin(GameObject prefab, string name)
    {
        if (prefab == null)
        {
            Debug.LogError($"Cannot spawn {name} - prefab is null!");
            return;
        }
        
        // Check if we already have too many of this type
        var existingGoblins = FindObjectsOfType(prefab.GetComponent<GoblinController>().GetType());
        if (existingGoblins.Length >= maxGoblinsPerType)
        {
            Debug.LogWarning($"Max {name} limit reached ({maxGoblinsPerType})");
            return;
        }
        
        // Random spawn position around test manager
        Vector2 randomOffset = new Vector2(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
        );
        
        Vector3 spawnPosition = transform.position + (Vector3)randomOffset;
        
        GameObject goblin = Instantiate(prefab, spawnPosition, Quaternion.identity);
        Debug.Log($"🧙‍♂️ Spawned {name} at {spawnPosition}");
    }
    
    private void ClearAllGoblins()
    {
        var allGoblins = FindObjectsOfType<GoblinController>();
        foreach (var goblin in allGoblins)
        {
            if (Application.isPlaying)
            {
                Destroy(goblin.gameObject);
            }
            else
            {
                DestroyImmediate(goblin.gameObject);
            }
        }
        Debug.Log($"🧹 Cleared {allGoblins.Length} goblins");
    }
    
    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            // Draw spawn area
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, spawnAreaSize);
            
            // Draw spawn points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
    
    private void OnGUI()
    {
        if (!enableDebugMode) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("🧙‍♂️ Goblin Test Manager");
        GUILayout.Label("Press 1-4 to spawn goblins");
        GUILayout.Label("Press C to clear all");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Spawn Dagger Goblin"))
            SpawnGoblin(daggerGoblinPrefab, "Dagger Goblin");
        if (GUILayout.Button("Spawn Bomber Goblin"))
            SpawnGoblin(daggerGoblinBomberPrefab, "Dagger Goblin Bomber");
        if (GUILayout.Button("Spawn Trapper Goblin"))
            SpawnGoblin(trapperGoblinPrefab, "Trapper Goblin");
        if (GUILayout.Button("Spawn Trapper Bomber"))
            SpawnGoblin(trapperGoblinBombsPrefab, "Trapper Goblin Bombs");
        
        GUILayout.Space(10);
        if (GUILayout.Button("Clear All Goblins"))
            ClearAllGoblins();
        
        GUILayout.EndArea();
    }
}

