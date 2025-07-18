using UnityEngine;

/// <summary>
/// Define symbol'lar kullanarak debug log'ları koşullu olarak etkinleştirir/devre dışı bırakır
/// </summary>
public static class ConditionalDebug
{
    /// <summary>
    /// Debug log'ları sadece development build'de göster
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message)
    {
        Debug.Log(message);
    }
    
    /// <summary>
    /// Debug warning'leri sadece development build'de göster
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }
    
    /// <summary>
    /// Debug error'ları her zaman göster (release'de de)
    /// </summary>
    public static void LogError(string message)
    {
        Debug.LogError(message);
    }
    
    /// <summary>
    /// Debug exception'ları her zaman göster (release'de de)
    /// </summary>
    public static void LogException(System.Exception exception)
    {
        Debug.LogException(exception);
    }
    
    /// <summary>
    /// Memory leak uyarılarını filtrele
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogMemoryInfo(string message)
    {
        // Memory leak uyarılarını filtrele
        if (!message.Contains("TLS Allocator") && 
            !message.Contains("ALLOC_TEMP") && 
            !message.Contains("unfreed allocations"))
        {
            Debug.Log($"[Memory] {message}");
        }
    }
    
    /// <summary>
    /// Performance debug log'ları
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogPerformance(string message)
    {
        Debug.Log($"[Performance] {message}");
    }
    
    /// <summary>
    /// Network debug log'ları
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogNetwork(string message)
    {
        Debug.Log($"[Network] {message}");
    }
}

/// <summary>
/// Memory leak uyarılarını susturmak için özel debug wrapper
/// </summary>
public static class SafeDebug
{
    private static readonly string[] suppressedKeywords = {
        "TLS Allocator",
        "ALLOC_TEMP_TLS",
        "ALLOC_TEMP_MAIN",
        "unfreed allocations",
        "Internal: Stack allocator",
        "69 6d 65 5c 45 78 70 6f 72 74 5c 44 65 62 75 67",
        "To Debug, run app with -diag-temp-memory-leak-validation"
    };
    
    /// <summary>
    /// Memory leak uyarılarını filtreleyerek log yazdır
    /// </summary>
    public static void Log(string message)
    {
        if (!ShouldSuppress(message))
        {
            Debug.Log(message);
        }
    }
    
    /// <summary>
    /// Memory leak uyarılarını filtreleyerek warning yazdır
    /// </summary>
    public static void LogWarning(string message)
    {
        if (!ShouldSuppress(message))
        {
            Debug.LogWarning(message);
        }
    }
    
    /// <summary>
    /// Error'ları her zaman göster
    /// </summary>
    public static void LogError(string message)
    {
        Debug.LogError(message);
    }
    
    /// <summary>
    /// Exception'ları her zaman göster
    /// </summary>
    public static void LogException(System.Exception exception)
    {
        Debug.LogException(exception);
    }
    
    private static bool ShouldSuppress(string message)
    {
        if (string.IsNullOrEmpty(message)) return false;
        
        string lowerMessage = message.ToLower();
        
        foreach (string keyword in suppressedKeywords)
        {
            if (lowerMessage.Contains(keyword.ToLower()))
            {
                return true; // Bu mesajı bastır
            }
        }
        
        return false;
    }
} 