using UnityEngine;

/// <summary>
/// TempWindEffect - Wind element için geçici efekt
/// Knockback ve hız artışı sağlar
/// </summary>
public class TempWindEffect : MonoBehaviour
{
    [Header("Wind Effect Settings")]
    public float windForce = 5f;
    public float duration = 3f;
    public float speedBoostPercent = 20f; // Hız artışı yüzdesi
    
    private float elapsedTime;
    private IMoveable moveable;
    private Rigidbody2D rb;
    private bool isApplied = false;
    
    private void Start()
    {
        elapsedTime = 0f;
        moveable = GetComponent<IMoveable>();
        rb = GetComponent<Rigidbody2D>();
        
        // Wind efektini uygula
        ApplyWindEffect();
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= duration)
        {
            RemoveWindEffect();
            Destroy(this);
        }
    }
    
    /// <summary>
    /// Wind efektini uygular
    /// </summary>
    private void ApplyWindEffect()
    {
        if (isApplied) return;
        
        // Hız artışı uygula
        if (moveable != null)
        {
            float speedBoost = 1f + (speedBoostPercent / 100f);
            moveable.SetSpeedMultiplier(speedBoost);
        }
        
        // Knockback efekti (eğer player varsa)
        if (PlayerController.Instance != null)
        {
            Vector3 playerPosition = PlayerController.Instance.transform.position;
            Vector3 targetPosition = transform.position;
            Vector3 knockbackDirection = (targetPosition - playerPosition).normalized;
            
            if (rb != null)
            {
                // Knockback kuvvetini uygula
                Vector2 knockbackForce = knockbackDirection * windForce;
                rb.AddForce(knockbackForce, ForceMode2D.Impulse);
            }
        }
        
        // VFX ve SFX oynat
        PlayWindEffects();
        
        isApplied = true;
        Debug.Log($"💨 Wind effect applied to {gameObject.name}");
    }
    
    /// <summary>
    /// Wind efektini kaldırır
    /// </summary>
    private void RemoveWindEffect()
    {
        if (moveable != null)
        {
            // Hız artışını geri al
            moveable.SetSpeedMultiplier(1f);
        }
        
        Debug.Log($"💨 Wind effect removed from {gameObject.name}");
    }
    
    /// <summary>
    /// Wind efektlerini oynatır
    /// </summary>
    private void PlayWindEffects()
    {
        // Wind VFX'i oynat
        var windVFX = Resources.Load<GameObject>("Prefabs/Effects/WindVFX");
        if (windVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(windVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
        }
        
        // Wind SFX'i oynat
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(25); // Wind sound effect
        }
    }
    
    private void OnDestroy()
    {
        // Component yok edilirken efektleri temizle
        RemoveWindEffect();
    }
} 