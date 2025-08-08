using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AbilityDebugVisualizer - Ability'leri test etmek için görsel debug tools
/// Player'a at, ayarları yapıştır, tüm ability'lerin görsel durumunu gör
/// </summary>
public class AbilityDebugVisualizer : MonoBehaviour
{
    [Header("🎯 Ability Radius Visualization")]
    [SerializeField] private bool showAuraRadius = true;
    [SerializeField] private bool showProjectileRange = true;
    [SerializeField] private bool showAreaEffectRadius = true;
    [SerializeField] private bool showDetectionRadius = true;
    
    [Header("🔥 Element & Stack Visualization")]
    [SerializeField] private bool showEnemyStacks = true;
    [SerializeField] private bool showElementColors = true;
    [SerializeField] private bool showStackNumbers = true;
    
    [Header("⚙️ Debug Settings")]
    [SerializeField] private bool showOnlyWhenSelected = false;
    [SerializeField] private bool showDistanceMarkers = true;
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private Color defaultColor = Color.yellow;
    [SerializeField] private float radiusAlpha = 0.2f;
    
    [Header("📊 Performance")]
    [SerializeField] private bool enableGUI = true;
    [SerializeField] private bool showConsoleInfo = false;
    
    // Cached components
    private ElementalAbilityManager abilityManager;
    private PlayerController playerController;
    private Dictionary<ElementType, Color> elementColors;
    
    private void Start()
    {
        InitializeComponents();
        InitializeElementColors();
    }
    
    /// <summary>
    /// Component'leri initialize eder
    /// </summary>
    private void InitializeComponents()
    {
        abilityManager = GetComponent<ElementalAbilityManager>();
        playerController = GetComponent<PlayerController>();
        
        if (abilityManager == null && showConsoleInfo)
        {
            Debug.LogWarning("🔍 AbilityDebugVisualizer: ElementalAbilityManager bulunamadı!");
        }
    }
    
    /// <summary>
    /// Element renklerini initialize eder
    /// </summary>
    private void InitializeElementColors()
    {
        elementColors = new Dictionary<ElementType, Color>
        {
            { ElementType.Fire, Color.red },
            { ElementType.Ice, Color.cyan },
            { ElementType.Poison, Color.green },
            { ElementType.Lightning, Color.yellow },
            { ElementType.Earth, new Color(0.6f, 0.4f, 0.2f) }, // Brown color
            { ElementType.Wind, Color.white }
        };
    }
    
    /// <summary>
    /// Gizmos çizer (Scene view'de görünür)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showOnlyWhenSelected) return;
        DrawAllDebugVisuals();
    }
    
    /// <summary>
    /// Selected gizmos çizer (Object seçildiğinde görünür)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showOnlyWhenSelected) return;
        DrawAllDebugVisuals();
    }
    
    /// <summary>
    /// Tüm debug görsellerini çizer
    /// </summary>
    private void DrawAllDebugVisuals()
    {
        if (showAuraRadius) DrawAuraRadius();
        if (showProjectileRange) DrawProjectileRange();
        if (showAreaEffectRadius) DrawAreaEffectRadius();
        if (showDetectionRadius) DrawDetectionRadius();
        if (showEnemyStacks) DrawEnemyStacks();
        if (showDistanceMarkers) DrawDistanceMarkers();
    }
    
    /// <summary>
    /// Aura radius'unu çizer
    /// </summary>
    private void DrawAuraRadius()
    {
        if (abilityManager == null) return;
        
        // Her element için aura kontrolü
        foreach (ElementType elementType in System.Enum.GetValues(typeof(ElementType)))
        {
            if (elementType == ElementType.None) continue;
            
            var aura = abilityManager.GetAbility(elementType, AbilityType.ElementalAura) as ElementalAura;
            if (aura != null && aura.IsActive())
            {
                Color auraColor = showElementColors && elementColors.ContainsKey(elementType) 
                    ? elementColors[elementType] 
                    : defaultColor;
                
                // Wireframe sphere
                Gizmos.color = new Color(auraColor.r, auraColor.g, auraColor.b, radiusAlpha + 0.3f);
                Gizmos.DrawWireSphere(transform.position, 6f); // Default aura radius
                
                // Filled sphere (transparent)
                Gizmos.color = new Color(auraColor.r, auraColor.g, auraColor.b, radiusAlpha);
                Gizmos.DrawSphere(transform.position, 6f);
                
                if (showConsoleInfo)
                {
                    Debug.Log($"🔍 {elementType} Aura active - Radius: 6f");
                }
            }
        }
    }
    
    /// <summary>
    /// Projectile range'ini çizer
    /// </summary>
    private void DrawProjectileRange()
    {
        if (abilityManager == null) return;
        
        // Projectile range genelde 10f
        Gizmos.color = new Color(1f, 0.5f, 0f, radiusAlpha + 0.2f); // Orange
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
    
    /// <summary>
    /// Area effect radius'unu çizer
    /// </summary>
    private void DrawAreaEffectRadius()
    {
        if (abilityManager == null) return;
        
        // Area effect radius genelde 5f
        Gizmos.color = new Color(1f, 0f, 1f, radiusAlpha + 0.2f); // Magenta
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
    
    /// <summary>
    /// Detection radius'unu çizer (enemy detection)
    /// </summary>
    private void DrawDetectionRadius()
    {
        // Genel detection radius
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, radiusAlpha); // Gray
        Gizmos.DrawWireSphere(transform.position, 8f);
    }
    
    /// <summary>
    /// Distance marker'ları çizer
    /// </summary>
    private void DrawDistanceMarkers()
    {
        Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        
        // 1, 2, 5, 10 unit marker'ları
        float[] distances = { 1f, 2f, 5f, 10f, 15f };
        
        foreach (float distance in distances)
        {
            if (distance <= maxDistance)
            {
                // İnce wire sphere
                Gizmos.DrawWireSphere(transform.position, distance);
                
                // Distance label (sadece scene view'de çalışır)
                Vector3 labelPos = transform.position + Vector3.right * distance;
                // Unity Handles.Label scene view'de text gösterir ama sadece Editor'da
            }
        }
    }
    
    /// <summary>
    /// Enemy stack'lerini çizer
    /// </summary>
    private void DrawEnemyStacks()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            
            var elementStack = enemy.GetComponent<ElementStack>();
            if (elementStack != null)
            {
                DrawEnemyStackInfo(enemy, elementStack);
            }
        }
    }
    
    /// <summary>
    /// Belirli enemy'nin stack bilgilerini çizer
    /// </summary>
    private void DrawEnemyStackInfo(GameObject enemy, ElementStack elementStack)
    {
        Vector3 enemyPos = enemy.transform.position;
        float distance = Vector3.Distance(transform.position, enemyPos);
        
        // Sadece maxDistance içindeki enemy'leri göster
        if (distance > maxDistance) return;
        
        // Player'dan enemy'ye line çiz
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, enemyPos);
        
        // Stack sayılarını göster
        if (showStackNumbers)
        {
            var stacks = elementStack.GetAllElementStacks();
            int totalStacks = 0;
            
            foreach (var kvp in stacks)
            {
                if (kvp.Value > 0)
                {
                    totalStacks += kvp.Value;
                    
                    // Element renginde küçük sphere çiz
                    if (showElementColors && elementColors.ContainsKey(kvp.Key))
                    {
                        Gizmos.color = elementColors[kvp.Key];
                        Vector3 stackPos = enemyPos + Vector3.up * (2f + (int)kvp.Key * 0.3f);
                        Gizmos.DrawSphere(stackPos, 0.1f * kvp.Value); // Stack sayısına göre boyut
                    }
                }
            }
            
            // Toplam stack sphere
            if (totalStacks > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(enemyPos + Vector3.up * 1.5f, 0.3f);
            }
        }
    }
    
    /// <summary>
    /// Runtime GUI bilgileri
    /// </summary>
    private void OnGUI()
    {
        if (!enableGUI) return;
        
        // Debug panel
        GUILayout.BeginArea(new Rect(10, Screen.height - 200, 300, 190));
        GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
        
        GUILayout.BeginVertical("box");
        GUILayout.Label("🎯 Ability Debug Visualizer", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        
        // Toggle'lar
        showAuraRadius = GUILayout.Toggle(showAuraRadius, "Show Aura Radius");
        showEnemyStacks = GUILayout.Toggle(showEnemyStacks, "Show Enemy Stacks");
        showElementColors = GUILayout.Toggle(showElementColors, "Show Element Colors");
        showDistanceMarkers = GUILayout.Toggle(showDistanceMarkers, "Show Distance Markers");
        
        GUILayout.Space(10);
        
        // Enemy count
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        GUILayout.Label($"Enemies in Scene: {enemyCount}");
        
        // Ability manager status
        if (abilityManager != null)
        {
            GUILayout.Label("✅ Ability Manager: Active");
        }
        else
        {
            GUILayout.Label("❌ Ability Manager: Missing");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Console'a debug bilgilerini yazdırır
    /// </summary>
    [ContextMenu("Print Debug Info")]
    public void PrintDebugInfo()
    {
        Debug.Log("🎯 === ABILITY DEBUG INFO ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Enemy Count: {GameObject.FindGameObjectsWithTag("Enemy").Length}");
        
        if (abilityManager != null)
        {
            Debug.Log("✅ ElementalAbilityManager: Found");
            // Ability durumlarını yazdır
            foreach (ElementType elementType in System.Enum.GetValues(typeof(ElementType)))
            {
                if (elementType == ElementType.None) continue;
                bool auraActive = abilityManager.IsAbilityActive(elementType, AbilityType.ElementalAura);
                Debug.Log($"- {elementType} Aura: {(auraActive ? "✅ Active" : "❌ Inactive")}");
            }
        }
        else
        {
            Debug.Log("❌ ElementalAbilityManager: Not Found");
        }
    }
} 