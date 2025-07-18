using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    private static bool isDestroyed = false;
    
    public static EventManager Instance
    {
        get
        {
            if (instance == null && !isDestroyed)
            {
                GameObject go = new GameObject("EventManager");
                instance = go.AddComponent<EventManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        isDestroyed = false; // Reset the static flag
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        
        // Clear all events to prevent memory leaks
        OnAbilityUsed = null;
        OnAbilityStarted = null;
        OnAbilityEnded = null;
        OnEnemyDeath = null;
        
        // Clear instance reference
        if (instance == this)
        {
            instance = null;
        }
    }

    // Ability Events
    public event Action<AbilityEventData> OnAbilityUsed;
    public event Action<AbilityEventData> OnAbilityStarted;
    public event Action<AbilityEventData> OnAbilityEnded;
    
    // Enemy Events
    public event Action<GameObject> OnEnemyDeath;

    public void TriggerAbilityUsed(AbilityEventData data)
    {
        if (isDestroyed || data == null) return;
        
        try
        {
            OnAbilityUsed?.Invoke(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[EventManager] TriggerAbilityUsed failed: {e.Message}");
        }
    }

    public void TriggerAbilityStarted(AbilityEventData data)
    {
        if (isDestroyed || data == null) return;
        
        try
        {
            OnAbilityStarted?.Invoke(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[EventManager] TriggerAbilityStarted failed: {e.Message}");
        }
    }

    public void TriggerAbilityEnded(AbilityEventData data)
    {
        if (isDestroyed || data == null) return;
        
        try
        {
            OnAbilityEnded?.Invoke(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[EventManager] TriggerAbilityEnded failed: {e.Message}");
        }
    }
    
    public void TriggerEnemyDeath(GameObject deadEnemy)
    {
        if (isDestroyed || deadEnemy == null) return;
        
        try
        {
            OnEnemyDeath?.Invoke(deadEnemy);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[EventManager] TriggerEnemyDeath failed: {e.Message}");
        }
    }
} 