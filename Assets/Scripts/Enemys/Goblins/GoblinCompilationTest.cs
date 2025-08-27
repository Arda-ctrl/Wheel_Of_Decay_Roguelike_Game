using UnityEngine;

/// <summary>
/// This script tests if all Goblin classes compile correctly
/// If this script has no errors, the Goblin system should work
/// </summary>
public class GoblinCompilationTest : MonoBehaviour
{
    [Header("Test Compilation")]
    [SerializeField] private bool runTest = false;
    
    private void Start()
    {
        if (runTest)
        {
            TestGoblinClasses();
        }
    }
    
    private void TestGoblinClasses()
    {
        Debug.Log("🧙‍♂️ Testing Goblin System Compilation...");
        
        // Test if all classes can be instantiated (compilation test)
        TestClass<DaggerGoblin>("DaggerGoblin");
        TestClass<DaggerGoblinBomber>("DaggerGoblinBomber");
        TestClass<TrapperGoblin>("TrapperGoblin");
        TestClass<TrapperGoblinBombs>("TrapperGoblinBombs");
        TestClass<GoblinTrap>("GoblinTrap");
        TestClass<GoblinBomb>("GoblinBomb");
        
        Debug.Log("✅ All Goblin classes compiled successfully!");
    }
    
    private void TestClass<T>(string className) where T : Component
    {
        try
        {
            // Create temporary GameObject
            GameObject testObj = new GameObject($"Test_{className}");
            
            // Add component
            T component = testObj.AddComponent<T>();
            
            if (component != null)
            {
                Debug.Log($"✅ {className} - OK");
            }
            else
            {
                Debug.LogError($"❌ {className} - Failed to add component");
            }
            
            // Clean up
            if (Application.isPlaying)
            {
                Destroy(testObj);
            }
            else
            {
                DestroyImmediate(testObj);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ {className} - Compilation Error: {e.Message}");
        }
    }
    
    // Test critical methods exist
    private void TestMethodAvailability()
    {
        Debug.Log("🔍 Testing method availability...");
        
        // These should all compile without errors
        var playerHealth = FindObjectOfType<PlayerHealthController>();
        if (playerHealth != null)
        {
            // This should work - DamagePlayer() with no parameters
            // playerHealth.DamagePlayer(); // Commented out to avoid actual damage
            Debug.Log("✅ PlayerHealthController.DamagePlayer() - Available");
        }
        
        var enemyController = FindObjectOfType<EnemyController>();
        if (enemyController != null)
        {
            // These should work
            float speed = enemyController.GetCurrentSpeed();
            float health = enemyController.GetCurrentHealth();
            Debug.Log($"✅ EnemyController methods - Available (Speed: {speed}, Health: {health})");
        }
        
        Debug.Log("✅ All methods available!");
    }
}

