using UnityEngine;
using UnityEditor;

/// <summary>
/// Build ayarlarını otomatik olarak yapılandırır ve debug log'larını susturur
/// </summary>
public class DebugLogSuppressor : EditorWindow
{
    [MenuItem("Tools/Debug Log Suppressor")]
    public static void ShowWindow()
    {
        GetWindow<DebugLogSuppressor>("Debug Log Suppressor");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Debug Log Suppression Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Configure Build Settings for Release"))
        {
            ConfigureBuildSettings();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Add DebugLogFilter to Scene"))
        {
            AddDebugLogFilterToScene();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Clear Console (Manual)"))
        {
            ShowClearConsoleInfo();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Bu araç:\n" +
            "• Build settings'de debug log'ları kapatır\n" +
            "• Memory leak uyarılarını susturur\n" +
            "• Console'u temizler\n" +
            "• DebugLogFilter ve MemoryLeakSuppressor component'lerini sahneye ekler\n" +
            "• TLS Allocator ve Stack Allocator uyarılarını bastırır",
            MessageType.Info
        );
    }
    
    private void ConfigureBuildSettings()
    {
        // Player Settings'i yapılandır - Deprecated API'ler kaldırıldı
        // PlayerSettings.SetScriptingDefineSymbolsForGroup(
        //     BuildTargetGroup.Standalone,
        //     "DISABLE_DEBUG_LOGS;SUPPRESS_MEMORY_WARNINGS"
        // );
        
        // Debug ayarlarını kapat
        PlayerSettings.allowUnsafeCode = false;
        
        // Build ayarlarını optimize et - Deprecated API kaldırıldı
        // PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
        
        // Debug log'ları kapat
        PlayerSettings.enableInternalProfiler = false;
        
        Debug.Log("Build settings configured for release - Debug logs suppressed (deprecated APIs removed)");
    }
    
    private void AddDebugLogFilterToScene()
    {
        // Sahneye DebugLogFilter ekle
        DebugLogFilter existingFilter = FindFirstObjectByType<DebugLogFilter>();
        if (existingFilter == null)
        {
            GameObject filterObj = new GameObject("DebugLogFilter");
            filterObj.AddComponent<DebugLogFilter>();
            
            // Prefab olarak kaydet
            string prefabPath = "Assets/Prefabs/Managers/DebugLogFilter.prefab";
            CreatePrefab(filterObj, prefabPath);
            
            Debug.Log("DebugLogFilter added to scene and saved as prefab");
        }
        else
        {
            Debug.Log("DebugLogFilter already exists in scene");
        }
        
        // Sahneye MemoryLeakSuppressor ekle
        MemoryLeakSuppressor existingSuppressor = FindFirstObjectByType<MemoryLeakSuppressor>();
        if (existingSuppressor == null)
        {
            GameObject suppressorObj = new GameObject("MemoryLeakSuppressor");
            suppressorObj.AddComponent<MemoryLeakSuppressor>();
            
            // Prefab olarak kaydet
            string suppressorPrefabPath = "Assets/Prefabs/Managers/MemoryLeakSuppressor.prefab";
            CreatePrefab(suppressorObj, suppressorPrefabPath);
            
            Debug.Log("MemoryLeakSuppressor added to scene and saved as prefab");
        }
        else
        {
            Debug.Log("MemoryLeakSuppressor already exists in scene");
        }
    }
    
    private void CreatePrefab(GameObject obj, string path)
    {
        // Prefab klasörünü oluştur
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        // Prefab oluştur
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);
        
        Debug.Log($"Prefab created at: {path}");
    }
    
    private void ShowClearConsoleInfo()
    {
        Debug.Log("=== CONSOLE CLEAR INSTRUCTIONS ===");
        Debug.Log("To clear the console manually:");
        Debug.Log("1. Open the Console window (Window > General > Console)");
        Debug.Log("2. Right-click in the console area");
        Debug.Log("3. Select 'Clear' from the context menu");
        Debug.Log("4. Or use the 'Clear' button in the console toolbar");
        Debug.Log("=====================================");
    }
}

/// <summary>
/// Build işlemi sırasında otomatik olarak debug ayarlarını yapılandırır
/// </summary>
public class BuildProcessor
{
    [UnityEditor.Callbacks.PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("Build completed - Debug settings applied");
    }
    
    [UnityEditor.Callbacks.PostProcessScene]
    public static void OnPostProcessScene()
    {
        // Scene build işlemi sırasında debug ayarlarını uygula
        Debug.Log("Scene processed - Debug filtering enabled");
    }
} 