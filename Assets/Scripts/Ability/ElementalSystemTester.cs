using UnityEngine;

/// <summary>
/// ElementalSystemTester - Elemental sistemin çalışıp çalışmadığını test eder
/// Bu script'i Player GameObject'ine ekleyin
/// </summary>
public class ElementalSystemTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableTesting = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private ElementalAbilityManager elementalManager;
    private PlayerElementalIntegration playerIntegration;
    
    private void Start()
    {
        if (!enableTesting) return;
        
        Debug.Log("🧪 ElementalSystemTester starting...");
        
        // Component'leri bul
        elementalManager = GetComponent<ElementalAbilityManager>();
        playerIntegration = GetComponent<PlayerElementalIntegration>();
        
        if (elementalManager == null)
        {
            Debug.LogError("❌ ElementalAbilityManager not found!");
        }
        else
        {
            Debug.Log("✅ ElementalAbilityManager found");
        }
        
        if (playerIntegration == null)
        {
            Debug.LogError("❌ PlayerElementalIntegration not found!");
        }
        else
        {
            Debug.Log("✅ PlayerElementalIntegration found");
        }
        
        // Test için basit bir düşman oluştur
        CreateTestEnemy();
    }
    
    private void Update()
    {
        if (!enableTesting) return;
        
        // Test input'ları
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestElementalStrike();
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestElementalBuff();
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            TestElementalProjectile();
        }
    }
    
    private void CreateTestEnemy()
    {
        // Basit bir test düşmanı oluştur
        GameObject testEnemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testEnemy.name = "Test Enemy";
        testEnemy.transform.position = transform.position + Vector3.right * 3f;
        
        // ElementStack ekle
        ElementStack elementStack = testEnemy.AddComponent<ElementStack>();
        if (elementStack != null)
        {
            Debug.Log("✅ Test enemy created with ElementStack");
        }
        
        // IHealth interface'i ekle (basit implementasyon)
        TestEnemyHealth enemyHealth = testEnemy.AddComponent<TestEnemyHealth>();
        if (enemyHealth != null)
        {
            Debug.Log("✅ Test enemy created with TestEnemyHealth");
        }
        
        // Düşmanı kırmızı yap (test için)
        Renderer renderer = testEnemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        Debug.Log("🎯 Test enemy created successfully at position: " + testEnemy.transform.position);
    }
    
    private void TestElementalStrike()
    {
        Debug.Log("⚔️ Testing Elemental Strike...");
        
        GameObject testEnemy = GameObject.Find("Test Enemy");
        if (testEnemy != null && elementalManager != null)
        {
            elementalManager.UseStrike(testEnemy);
            Debug.Log("✅ Elemental Strike test completed");
        }
        else
        {
            Debug.LogError("❌ Cannot test Elemental Strike - missing components");
        }
    }
    
    private void TestElementalBuff()
    {
        Debug.Log("🛡️ Testing Elemental Buff...");
        
        GameObject testEnemy = GameObject.Find("Test Enemy");
        if (testEnemy != null && elementalManager != null)
        {
            float baseDamage = 10f;
            float buffedDamage = elementalManager.CalculateBuffDamage(baseDamage, testEnemy, ElementType.Fire);
            Debug.Log($"✅ Buff test: Base damage {baseDamage} -> Buffed damage {buffedDamage}");
        }
        else
        {
            Debug.LogError("❌ Cannot test Elemental Buff - missing components");
        }
    }
    
    private void TestElementalProjectile()
    {
        Debug.Log("🎯 Testing Elemental Projectile...");
        
        if (elementalManager != null)
        {
            elementalManager.OnAttack();
            Debug.Log("✅ Elemental Projectile test completed");
        }
        else
        {
            Debug.LogError("❌ Cannot test Elemental Projectile - missing components");
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        string testInfo = "🧪 ELEMENTAL SYSTEM TESTER 🧪\n";
        testInfo += "T: Test Strike\n";
        testInfo += "Y: Test Buff\n";
        testInfo += "U: Test Projectile\n";
        testInfo += "1,2,3: Change Element\n";
        testInfo += "Q,E: Toggle Abilities\n";
        
        GUI.color = Color.magenta;
        GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(10, 270, 300, 120), "");
        GUI.Label(new Rect(15, 275, 290, 110), testInfo);
    }
} 