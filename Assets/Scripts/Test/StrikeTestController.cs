using UnityEngine;

/// <summary>
/// Strike sistemi için test controller'ı
/// Bu script strike sistemini test etmek için kullanılır
/// </summary>
public class StrikeTestController : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableStrikeTest = true;
    [SerializeField] private KeyCode testStrikeKey = KeyCode.T;
    [SerializeField] private KeyCode testStrikeBuffKey = KeyCode.Y;
    
    [Header("Test Targets")]
    [SerializeField] private GameObject testEnemy;
    
    private WeaponController weaponController;
    
    private void Start()
    {
        // WeaponController referansını al
        weaponController = FindObjectOfType<WeaponController>();
        
        if (weaponController == null)
        {
            Debug.LogError("❌ WeaponController bulunamadı!");
        }
        
        // Test düşmanını bul
        if (testEnemy == null)
        {
            testEnemy = GameObject.FindGameObjectWithTag("Enemy");
        }
        
        if (testEnemy == null)
        {
            Debug.LogWarning("⚠️ Test düşmanı bulunamadı! Strike testi çalışmayabilir.");
        }
    }
    
    private void Update()
    {
        if (!enableStrikeTest) return;
        
        // Strike ability test
        if (Input.GetKeyDown(testStrikeKey))
        {
            TestStrikeAbility();
        }
        
        // Strike buff test
        if (Input.GetKeyDown(testStrikeBuffKey))
        {
            TestStrikeBuff();
        }
    }
    
    /// <summary>
    /// Strike ability'sini test eder
    /// </summary>
    private void TestStrikeAbility()
    {
        if (weaponController == null) return;
        
        Debug.Log("🧪 Strike ability artık SO tabanlı! Ability Data'dan kontrol ediliyor.");
    }
    
    /// <summary>
    /// Strike buff'unu test eder
    /// </summary>
    private void TestStrikeBuff()
    {
        if (weaponController == null) return;
        
        bool currentState = weaponController.GetComponent<WeaponController>() != null;
        weaponController.SetStrikeBuff(!currentState);
        
        Debug.Log($"🧪 Strike buff test: {(currentState ? "Deactivated" : "Activated")}");
    }
    
    /// <summary>
    /// Test düşmanına strike stack ekler
    /// </summary>
    [ContextMenu("Add Strike Stack to Test Enemy")]
    public void AddStrikeStackToTestEnemy()
    {
        if (testEnemy == null)
        {
            Debug.LogError("❌ Test düşmanı bulunamadı!");
            return;
        }
        
        var strikeStack = testEnemy.GetComponent<StrikeStack>();
        if (strikeStack == null)
        {
            strikeStack = testEnemy.AddComponent<StrikeStack>();
        }
        
        strikeStack.AddStrikeStack(1);
        Debug.Log($"🧪 Strike stack eklendi: {testEnemy.name}");
    }
    
    /// <summary>
    /// Test düşmanından strike stack kaldırır
    /// </summary>
    [ContextMenu("Remove Strike Stack from Test Enemy")]
    public void RemoveStrikeStackFromTestEnemy()
    {
        if (testEnemy == null)
        {
            Debug.LogError("❌ Test düşmanı bulunamadı!");
            return;
        }
        
        var strikeStack = testEnemy.GetComponent<StrikeStack>();
        if (strikeStack != null)
        {
            strikeStack.RemoveStrikeStack(1);
            Debug.Log($"🧪 Strike stack kaldırıldı: {testEnemy.name}");
        }
    }
    
    /// <summary>
    /// Test düşmanının strike stack'lerini temizler
    /// </summary>
    [ContextMenu("Clear Strike Stacks from Test Enemy")]
    public void ClearStrikeStacksFromTestEnemy()
    {
        if (testEnemy == null)
        {
            Debug.LogError("❌ Test düşmanı bulunamadı!");
            return;
        }
        
        var strikeStack = testEnemy.GetComponent<StrikeStack>();
        if (strikeStack != null)
        {
            strikeStack.ClearStrikeStacks();
            Debug.Log($"🧪 Strike stack'ler temizlendi: {testEnemy.name}");
        }
    }
    
    private void OnGUI()
    {
        if (!enableStrikeTest) return;
        
        // Test bilgilerini göster
        GUI.color = Color.cyan;
        GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(10, 10, 300, 120), "");
        
        string testInfo = "🧪 Strike Test Controller\n";
        testInfo += $"Test Key: {testStrikeKey}\n";
        testInfo += $"Buff Test Key: {testStrikeBuffKey}\n";
        testInfo += $"Test Enemy: {(testEnemy != null ? testEnemy.name : "Not Found")}\n";
        testInfo += $"Weapon Controller: {(weaponController != null ? "Found" : "Not Found")}";
        
        GUI.Label(new Rect(15, 15, 290, 110), testInfo);
    }
} 