using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// OrbStackManager - Düşman ölümünde stack toplar ve 10 olunca orb spawn eder
/// </summary>
public class OrbStackManager : MonoBehaviour
{
    public static OrbStackManager Instance;
    
    [Header("Orb Stack Settings")]
    [SerializeField] private int requiredStacksForOrb = 10;
    [SerializeField] private int maxOrbs = 4;
    [SerializeField] private float orbSize = 0.5f; // Orb boyutu
    
    [Header("Orb Data References")]
    [SerializeField] private ElementalAbilityData[] orbAbilityData; // Her element için orb verileri
    
    // Element stack sayacları
    private Dictionary<ElementType, int> collectedStacks = new Dictionary<ElementType, int>();
    
    // Aktif orb'lar
    private List<ElementalOrbController> activeOrbs = new List<ElementalOrbController>();
    
    // Events
    public System.Action<ElementType, int> OnStackCollected;
    public System.Action<ElementType> OnOrbSpawned;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Enemy death event'ini dinle
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEnemyDeath += OnEnemyDied;
        }
        
        // Stack dictionary'sini initialize et
        foreach (ElementType elementType in System.Enum.GetValues(typeof(ElementType)))
        {
            if (elementType != ElementType.None)
            {
                collectedStacks[elementType] = 0;
            }
        }
        
        // ElementalAbilityManager'dan orb SO'larını otomatik al
        AutoLoadOrbDataFromAbilityManager();
        
        Debug.Log("🔮 OrbStackManager initialized");
    }
    
    /// <summary>
    /// ElementalAbilityManager'dan otomatik olarak orb SO'larını yükler
    /// </summary>
    private void AutoLoadOrbDataFromAbilityManager()
    {
        // Player'ı bul
        var player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogWarning("⚠️ Player not found, cannot auto-load orb data");
            return;
        }
        
        // ElementalAbilityManager'ı bul
        var abilityManager = player.GetComponent<ElementalAbilityManager>();
        if (abilityManager == null)
        {
            Debug.LogWarning("⚠️ ElementalAbilityManager not found on player");
            return;
        }
        
        // Available abilities'den orb SO'larını bul
        var orbDataList = new System.Collections.Generic.List<ElementalAbilityData>();
        
        // Reflection ile private field'a erişim
        var availableAbilitiesField = typeof(ElementalAbilityManager).GetField("availableAbilities", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (availableAbilitiesField != null)
        {
            var availableAbilities = availableAbilitiesField.GetValue(abilityManager) as ElementalAbilityData[];
            
            if (availableAbilities != null)
            {
                foreach (var abilityData in availableAbilities)
                {
                    if (abilityData != null && abilityData.abilityType == AbilityType.ElementalOrb)
                    {
                        orbDataList.Add(abilityData);
                        Debug.Log($"🔮 Auto-loaded {abilityData.elementType} orb data from AbilityManager");
                    }
                }
            }
        }
        
        // Bulunan orb SO'larını ata
        if (orbDataList.Count > 0)
        {
            orbAbilityData = orbDataList.ToArray();
            Debug.Log($"🔮 Successfully auto-loaded {orbAbilityData.Length} orb SO(s) from ElementalAbilityManager");
        }
        else
        {
            Debug.LogWarning("⚠️ No ElementalOrb abilities found in ElementalAbilityManager");
        }
    }
    
    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEnemyDeath -= OnEnemyDied;
        }
    }
    
    /// <summary>
    /// Düşman öldüğünde çağrılır
    /// </summary>
    /// <param name="enemy">Ölen düşman</param>
    private void OnEnemyDied(GameObject enemy)
    {
        if (enemy == null) return;
        
        // Düşmanın ElementStack component'ini al
        var elementStack = enemy.GetComponent<ElementStack>();
        if (elementStack == null) return;
        
        // Düşmanın sahip olduğu tüm stack'leri al
        var enemyStacks = elementStack.GetAllElementStacks();
        
        if (enemyStacks.Count == 0) return;
        
        Debug.Log($"💀 {enemy.name} died with stacks:");
        
        // Her element için stack'leri topla
        foreach (var kvp in enemyStacks)
        {
            ElementType elementType = kvp.Key;
            int stackCount = kvp.Value;
            
            CollectStacks(elementType, stackCount);
            Debug.Log($"📊 Collected {stackCount} {elementType} stacks from {enemy.name}");
        }
    }
    
    /// <summary>
    /// Stack'leri toplar ve gerekirse orb spawn eder
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void CollectStacks(ElementType elementType, int stackCount)
    {
        if (elementType == ElementType.None || stackCount <= 0) return;
        
        // Stack'i ekle
        collectedStacks[elementType] += stackCount;
        
        OnStackCollected?.Invoke(elementType, collectedStacks[elementType]);
        
        Debug.Log($"📊 {elementType} total stacks: {collectedStacks[elementType]}");
        
        // 10 stack'e ulaştıysa orb spawn et
        if (collectedStacks[elementType] >= requiredStacksForOrb)
        {
            TrySpawnOrb(elementType);
        }
    }
    
    /// <summary>
    /// Orb spawn etmeye çalışır
    /// </summary>
    /// <param name="elementType">Element türü</param>
    private void TrySpawnOrb(ElementType elementType)
    {
        // Maksimum orb sayısını kontrol et
        if (activeOrbs.Count >= maxOrbs)
        {
            Debug.Log($"❌ Cannot spawn {elementType} orb - max orbs ({maxOrbs}) reached");
            return;
        }
        
        // Aynı element türünde orb varsa spawn etme
        foreach (var orb in activeOrbs)
        {
            if (orb != null && orb.GetElementType() == elementType)
            {
                Debug.Log($"❌ Cannot spawn {elementType} orb - orb of same element already exists");
                return;
            }
        }
        
        // Stack'i tüket
        collectedStacks[elementType] -= requiredStacksForOrb;
        
        // Orb spawn et
        SpawnOrb(elementType);
        
        OnOrbSpawned?.Invoke(elementType);
        
        Debug.Log($"🔮 Spawned {elementType} orb! Remaining stacks: {collectedStacks[elementType]}");
    }
    
    /// <summary>
    /// Orb spawn eder
    /// </summary>
    /// <param name="elementType">Element türü</param>
    private void SpawnOrb(ElementType elementType)
    {
        // Player pozisyonunu al
        var player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogError("❌ Player not found for orb spawn");
            return;
        }
        
        // Element için orb data'sını bul
        ElementalAbilityData orbData = GetOrbDataForElement(elementType);
        if (orbData == null)
        {
            Debug.LogError($"❌ No orb data found for {elementType}");
            return;
        }
        
        // Orb prefab'ini oluştur
        GameObject orbGO = new GameObject($"{elementType} Orb");
        orbGO.transform.position = player.transform.position;
        orbGO.transform.localScale = Vector3.one * orbSize;
        
        // Sprite renderer ekle
        var spriteRenderer = orbGO.AddComponent<SpriteRenderer>();
        if (orbData.orbSprite != null)
        {
            spriteRenderer.sprite = orbData.orbSprite;
        }
        else
        {
            // Fallback: Otomatik sprite oluştur
            spriteRenderer.sprite = CreateOrbSprite();
        }
        spriteRenderer.color = GetElementColor(elementType);
        spriteRenderer.sortingOrder = 5;
        
        // Collider ekle
        var collider = orbGO.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.3f;
        
        // Orb controller ekle
        var orbController = orbGO.AddComponent<ElementalOrbController>();
        orbController.InitializeAsPassiveOrb(elementType, player.transform, orbData);
        
        // Aktif orb listesine ekle
        activeOrbs.Add(orbController);
        
        // Orb yok olduğunda listeden çıkar
        orbController.OnOrbDestroyed += () => {
            activeOrbs.Remove(orbController);
        };
    }
    
    /// <summary>
    /// Element türü için orb data'sını bulur
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>ElementalAbilityData veya null</returns>
    private ElementalAbilityData GetOrbDataForElement(ElementType elementType)
    {
        if (orbAbilityData == null) return null;
        
        foreach (var data in orbAbilityData)
        {
            if (data != null && data.elementType == elementType && data.abilityType == AbilityType.ElementalOrb)
            {
                return data;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Orb için circle sprite oluşturur
    /// </summary>
    /// <returns>Circle sprite</returns>
    private Sprite CreateOrbSprite()
    {
        // 64x64 circle texture oluştur
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f; // Biraz kenar boşluğu
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    // Circle içi - beyaz
                    float alpha = 1f - (distance / radius) * 0.3f; // Kenarlar biraz transparan
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    // Circle dışı - transparan
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        // Sprite oluştur
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return sprite;
    }
    
    /// <summary>
    /// Element rengini döndürür
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>Element rengi</returns>
    private Color GetElementColor(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return Color.red;
            case ElementType.Ice:
                return Color.cyan;
            case ElementType.Poison:
                return Color.green;
            case ElementType.Lightning:
                return Color.yellow;
            case ElementType.Earth:
                return new Color(0.6f, 0.4f, 0.2f); // Brown
            case ElementType.Wind:
                return Color.gray;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Mevcut stack sayısını döndürür
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>Stack sayısı</returns>
    public int GetCollectedStacks(ElementType elementType)
    {
        return collectedStacks.ContainsKey(elementType) ? collectedStacks[elementType] : 0;
    }
    
    /// <summary>
    /// Aktif orb sayısını döndürür
    /// </summary>
    /// <returns>Aktif orb sayısı</returns>
    public int GetActiveOrbCount()
    {
        return activeOrbs.Count;
    }
    

    
    /// <summary>
    /// Orb boyutunu ayarlar
    /// </summary>
    /// <param name="size">Yeni orb boyutu</param>
    public void SetOrbSize(float size)
    {
        orbSize = size;
        Debug.Log($"🔮 Orb size set to: {size}");
    }
    
    /// <summary>
    /// Orb için gerekli stack sayısını ayarlar
    /// </summary>
    /// <param name="stacks">Yeni gerekli stack sayısı</param>
    public void SetRequiredStacksForOrb(int stacks)
    {
        requiredStacksForOrb = stacks;
        Debug.Log($"🔮 Required stacks for orb set to: {stacks}");
    }
    
    /// <summary>
    /// Debug bilgisi gösterir
    /// </summary>
    private void OnGUI()
    {
        string info = "🔮 ORB STACK MANAGER 🔮\n";
        info += $"Active Orbs: {activeOrbs.Count}/{maxOrbs}\n";
        info += $"Required Stacks: {requiredStacksForOrb}\n";
        info += $"Orb Size: {orbSize}\n\n";
        info += "Collected Stacks:\n";
        
        foreach (var kvp in collectedStacks)
        {
            if (kvp.Value > 0)
            {
                info += $"{kvp.Key}: {kvp.Value}\n";
            }
        }
        
        // Orb data kontrolü
        if (orbAbilityData != null)
        {
            info += $"\nOrb Data Count: {orbAbilityData.Length}";
        }
        else
        {
            info += "\n⚠️ No Orb Data Assigned!";
        }
        
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.9f);
        GUI.Box(new Rect(420, 10, 200, 240), "");
        GUI.color = Color.magenta;
        GUI.Label(new Rect(425, 15, 190, 30), "🔮 ORB STACK MANAGER 🔮");
        GUI.color = Color.white;
        GUI.Label(new Rect(425, 45, 190, 190), info);
    }
} 