using UnityEngine;
using System.Collections;

public class TempBurnEffect : MonoBehaviour
{
    public float duration = 2f;
    public float damagePerTick = 5f;
    public float tickRate = 0.5f;
    private float elapsed = 0f;
    private float tickTimer = 0f;
    private IHealth health;

    void Start()
    {
        health = GetComponent<IHealth>();
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickRate)
        {
            if (health != null)
                health.TakeDamage(damagePerTick);
            tickTimer = 0f;
        }
        if (elapsed >= duration)
            Destroy(this);
    }
}

public class TempSlowEffect : MonoBehaviour
{
    public float slowPercent = 0.3f;
    public float duration = 2f;
    private float elapsed = 0f;
    private IMoveable moveable;
    private bool slowed = false;
    void Start()
    {
        moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            float currentSpeed = moveable.GetCurrentSpeed();
            float baseSpeed = moveable.GetBaseSpeed();
            Debug.Log($"❄️ TempSlowEffect: Found IMoveable on {gameObject.name}");
            Debug.Log($"❄️ Current speed: {currentSpeed}, Base speed: {baseSpeed}");
            Debug.Log($"❄️ Applying {slowPercent * 100}% slow");
            
            moveable.SetSpeedMultiplier(1f - slowPercent);
            slowed = true;
            
            float newSpeed = moveable.GetCurrentSpeed();
            Debug.Log($"❄️ New speed after slow: {newSpeed}");
        }
        else
        {
            Debug.LogError($"❄️ TempSlowEffect: No IMoveable found on {gameObject.name}!");
        }
    }
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            if (moveable != null && slowed)
            {
                Debug.Log($"❄️ TempSlowEffect: Slow effect ended, restoring speed to 1.0");
                moveable.SetSpeedMultiplier(1f);
            }
            Destroy(this);
        }
    }
}

public class TempPoisonEffect : MonoBehaviour
{
    public float duration = 3f;
    public float damagePerTick = 3f;
    public float tickRate = 0.5f;
    public float slowPercent = 0.2f;
    private float elapsed = 0f;
    private float tickTimer = 0f;
    private IHealth health;
    private IMoveable moveable;
    private bool slowed = false;
    void Start()
    {
        health = GetComponent<IHealth>();
        moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(1f - slowPercent);
            slowed = true;
        }
    }
    void Update()
    {
        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickRate)
        {
            if (health != null)
                health.TakeDamage(damagePerTick);
            tickTimer = 0f;
        }
        if (elapsed >= duration)
        {
            if (moveable != null && slowed)
                moveable.SetSpeedMultiplier(1f);
            Destroy(this);
        }
    }
}

public class TempFreezeEffect : MonoBehaviour
{
    public float duration = 2f;
    private float elapsed = 0f;
    private IMoveable moveable;
    private bool frozen = false;
    private float originalSpeed;
    
    void Start()
    {
        moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            originalSpeed = moveable.GetCurrentSpeed();
            moveable.SetSpeedMultiplier(0f); // Completely freeze
            frozen = true;
        }
    }
    
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            if (moveable != null && frozen)
                moveable.SetSpeedMultiplier(1f); // Restore original speed
            Destroy(this);
        }
    }
} 