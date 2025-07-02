using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    public static EventManager Instance
    {
        get
        {
            if (instance == null)
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
        DontDestroyOnLoad(gameObject);
    }

    // Ability Events
    public event Action<AbilityEventData> OnAbilityUsed;
    public event Action<AbilityEventData> OnAbilityStarted;
    public event Action<AbilityEventData> OnAbilityEnded;

    public void TriggerAbilityUsed(AbilityEventData data)
    {
        OnAbilityUsed?.Invoke(data);
    }

    public void TriggerAbilityStarted(AbilityEventData data)
    {
        OnAbilityStarted?.Invoke(data);
    }

    public void TriggerAbilityEnded(AbilityEventData data)
    {
        OnAbilityEnded?.Invoke(data);
    }
} 