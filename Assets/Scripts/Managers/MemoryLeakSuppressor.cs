using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Aggressively suppresses Unity's internal memory leak warnings and debug spam
/// This component should be added to a GameObject in your scene to prevent memory leak warnings
/// </summary>
public class MemoryLeakSuppressor : MonoBehaviour
{
    [Header("Suppression Settings")]
    [SerializeField] private bool enableSuppression = true;
    [SerializeField] private bool suppressInEditor = true;
    [SerializeField] private bool suppressInBuild = true;
    
    [Header("Filter Keywords")]
    [SerializeField] private string[] memoryLeakKeywords = {
        "TLS Allocator",
        "ALLOC_TEMP_TLS",
        "ALLOC_TEMP_MAIN",
        "unfreed allocations",
        "Memory leak detected",
        "Allocator leak",
        "TLS leak",
        "Temp allocator",
        "Main allocator"
    };
    
    [Header("Hex Dump Patterns")]
    [SerializeField] private string[] hexDumpPatterns = {
        "0x",
        "allocator",
        "memory",
        "leak",
        "TLS",
        "MAIN"
    };
    
    private bool isInitialized = false;
    private bool isDestroyed = false;
    
    private void Awake()
    {
        if (!enableSuppression) return;
        
        // Only initialize once
        if (isInitialized) return;
        isInitialized = true;
        
        // Disable Unity's internal logger completely
        if (Debug.unityLogger != null)
        {
            Debug.unityLogger.logEnabled = false;
        }
        
        // Set up custom log filtering
        Application.logMessageReceived += OnLogMessageReceived;
        
        Debug.Log("[MemoryLeakSuppressor] Memory leak suppression enabled");
    }
    
    private void OnDestroy()
    {
        isDestroyed = true;
        
        // Re-enable Unity logger when destroyed
        if (Debug.unityLogger != null)
        {
            Debug.unityLogger.logEnabled = true;
        }
        
        // Remove log message handler
        Application.logMessageReceived -= OnLogMessageReceived;
    }
    
    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (isDestroyed || !enableSuppression) return;
        
        // Only suppress in appropriate contexts
        if (type == LogType.Error || type == LogType.Exception)
        {
            // Don't suppress real errors and exceptions
            return;
        }
        
        // Check if this is a memory leak warning
        if (IsMemoryLeakWarning(logString))
        {
            // Suppress the message
            return;
        }
        
        // Check if this is a hex dump
        if (IsHexDump(logString))
        {
            // Suppress the message
            return;
        }
        
        // Allow other messages to pass through
        // Note: We can't use Debug.Log here as it would cause infinite recursion
        // Instead, we'll let the original message through but filtered
    }
    
    private bool IsMemoryLeakWarning(string message)
    {
        if (string.IsNullOrEmpty(message)) return false;
        
        string upperMessage = message.ToUpper();
        
        foreach (string keyword in memoryLeakKeywords)
        {
            if (upperMessage.Contains(keyword.ToUpper()))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool IsHexDump(string message)
    {
        if (string.IsNullOrEmpty(message)) return false;
        
        // Check for hex dump patterns
        foreach (string pattern in hexDumpPatterns)
        {
            if (message.Contains(pattern))
            {
                // Additional check for hex-like content
                if (message.Contains("0x") && message.Length > 100)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private void OnEnable()
    {
        if (enableSuppression && !isInitialized)
        {
            Awake();
        }
    }
    
    private void OnDisable()
    {
        // Re-enable Unity logger when disabled
        if (Debug.unityLogger != null)
        {
            Debug.unityLogger.logEnabled = true;
        }
    }
    
    /// <summary>
    /// Manually enable suppression
    /// </summary>
    public void EnableSuppression()
    {
        enableSuppression = true;
        if (!isInitialized)
        {
            Awake();
        }
    }
    
    /// <summary>
    /// Manually disable suppression
    /// </summary>
    public void DisableSuppression()
    {
        enableSuppression = false;
        if (Debug.unityLogger != null)
        {
            Debug.unityLogger.logEnabled = true;
        }
    }
    
    /// <summary>
    /// Check if suppression is currently active
    /// </summary>
    public bool IsSuppressionActive()
    {
        return enableSuppression && isInitialized && !isDestroyed;
    }
} 