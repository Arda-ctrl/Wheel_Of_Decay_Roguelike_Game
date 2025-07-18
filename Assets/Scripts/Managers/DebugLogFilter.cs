using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unity'nin low-level memory leak uyarılarını ve spam log'larını tamamen susturur
/// 
/// KULLANIM:
/// 1. Bu script'i sahneye ekleyin veya Tools > Debug Log Suppressor menüsünü kullanın
/// 2. Inspector'da ayarları yapılandırın
/// 3. Runtime'da FindObjectOfType<DebugLogFilter>().SetFiltering(true/false) ile kontrol edin
/// 
/// ÖRNEKLER:
/// - FindObjectOfType<DebugLogFilter>().SetFiltering(true); // Filtrelemeyi aç
/// - FindObjectOfType<DebugLogFilter>().AddFilteredKeyword("yeni_keyword"); // Yeni keyword ekle
/// - SafeDebug.Log("Bu mesaj memory leak uyarıları filtrelenerek gösterilir");
/// - ConditionalDebug.Log("Bu mesaj sadece development build'de görünür");
/// </summary>
public class DebugLogFilter : MonoBehaviour
{
    [Header("Log Filter Settings")]
    [SerializeField] private bool enableLogFiltering = true;
    [SerializeField] private bool showWarnings = false;
    [SerializeField] private bool showErrors = true;
    [SerializeField] private bool showExceptions = true;
    
    [Header("Filtered Keywords")]
    [SerializeField] private string[] filteredKeywords = {
        "TLS Allocator",
        "ALLOC_TEMP_TLS",
        "ALLOC_TEMP_MAIN",
        "unfreed allocations",
        "Internal: Stack allocator",
        "69 6d 65 5c 45 78 70 6f 72 74 5c 44 65 62 75 67", // Hex dump
        "To Debug, run app with -diag-temp-memory-leak-validation",
        "has unfreed allocations, size",
        "unfreed allocations, size"
    };
    
    [Header("Allowed Keywords")]
    [SerializeField] private string[] allowedKeywords = {
        "Exception",
        "Error",
        "NullReferenceException",
        "MissingReferenceException"
    };
    
    private bool isInitialized = false;
    private bool originalLogEnabled = true;
    private bool isDestroyed = false;
    
    private void Awake()
    {
        InitializeLogFilter();
    }
    
    private void InitializeLogFilter()
    {
        if (isInitialized || isDestroyed) return;
        
        if (enableLogFiltering)
        {
            // Unity'nin debug log'larını tamamen kapat
            originalLogEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            
            // Sadece error'ları göster
            Debug.unityLogger.filterLogType = LogType.Error;
            
            // Custom log handler'ı ayarla
            Application.logMessageReceived += OnLogMessageReceived;
            
            Debug.Log("DebugLogFilter initialized - Unity debug logs disabled, memory leak warnings suppressed");
        }
        
        isInitialized = true;
    }
    
    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (isDestroyed) return;
        
        // Eğer filtreleme kapalıysa, hiçbir şey yapma
        if (!enableLogFiltering)
        {
            return;
        }
        
        // Filtrelenecek keyword'leri kontrol et - bunları tamamen bastır
        if (ShouldFilterLog(logString))
        {
            return; // Bu log'u tamamen bastır
        }
        
        // Error ve Exception'ları her zaman göster
        if (type == LogType.Error || type == LogType.Exception)
        {
            if (showErrors || showExceptions)
            {
                // Gerçek hataları console'a yazdır (sonsuz döngüyü önlemek için)
                WriteToConsole(type, logString, stackTrace);
            }
            return;
        }
        
        // Warning'leri kontrol et
        if (type == LogType.Warning)
        {
            if (!showWarnings)
            {
                return; // Warning'leri gösterme
            }
        }
        
        // İzin verilen keyword'leri kontrol et
        if (ShouldAllowLog(logString))
        {
            WriteToConsole(type, logString, stackTrace);
            return;
        }
        
        // Normal log'ları göster (Info tipi)
        if (type == LogType.Log)
        {
            WriteToConsole(type, logString, stackTrace);
        }
    }
    
    private void WriteToConsole(LogType type, string message, string stackTrace)
    {
        // Sonsuz döngüyü önlemek için doğrudan console'a yaz
        switch (type)
        {
            case LogType.Error:
                Debug.LogError($"[FILTERED] {message}");
                break;
            case LogType.Warning:
                Debug.LogWarning($"[FILTERED] {message}");
                break;
            case LogType.Log:
                Debug.Log($"[FILTERED] {message}");
                break;
        }
    }
    
    private bool ShouldFilterLog(string logString)
    {
        if (string.IsNullOrEmpty(logString)) return false;
        
        string lowerLog = logString.ToLower();
        
        foreach (string keyword in filteredKeywords)
        {
            if (lowerLog.Contains(keyword.ToLower()))
            {
                return true; // Bu log'u filtrele
            }
        }
        
        return false;
    }
    
    private bool ShouldAllowLog(string logString)
    {
        if (string.IsNullOrEmpty(logString)) return false;
        
        string lowerLog = logString.ToLower();
        
        foreach (string keyword in allowedKeywords)
        {
            if (lowerLog.Contains(keyword.ToLower()))
            {
                return true; // Bu log'u göster
            }
        }
        
        return false;
    }
    
    private void OnDestroy()
    {
        isDestroyed = true;
        
        // Event'ten çık
        Application.logMessageReceived -= OnLogMessageReceived;
        
        // Unity logger'ı geri aç
        if (isInitialized)
        {
            Debug.unityLogger.logEnabled = originalLogEnabled;
        }
    }
    
    /// <summary>
    /// Filtreleme ayarlarını runtime'da değiştir
    /// </summary>
    public void SetFiltering(bool enabled)
    {
        if (isDestroyed) return;
        
        enableLogFiltering = enabled;
        
        if (enabled)
        {
            Debug.unityLogger.logEnabled = false;
        }
        else
        {
            Debug.unityLogger.logEnabled = originalLogEnabled;
        }
        
        Debug.Log($"Debug log filtering {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Yeni keyword ekle
    /// </summary>
    public void AddFilteredKeyword(string keyword)
    {
        if (isDestroyed || string.IsNullOrEmpty(keyword)) return;
        
        var list = new List<string>(filteredKeywords);
        if (!list.Contains(keyword))
        {
            list.Add(keyword);
            filteredKeywords = list.ToArray();
        }
    }
    
    /// <summary>
    /// Yeni izin verilen keyword ekle
    /// </summary>
    public void AddAllowedKeyword(string keyword)
    {
        if (isDestroyed || string.IsNullOrEmpty(keyword)) return;
        
        var list = new List<string>(allowedKeywords);
        if (!list.Contains(keyword))
        {
            list.Add(keyword);
            allowedKeywords = list.ToArray();
        }
    }
} 