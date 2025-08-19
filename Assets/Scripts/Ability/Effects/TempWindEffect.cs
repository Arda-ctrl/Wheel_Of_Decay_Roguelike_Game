using UnityEngine;
using System.Collections;

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
    public float knockbackDistance = 3f; // İtme mesafesi
    public float knockbackDuration = 0.3f; // İtme süresi
    
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
                // Hedefin orijinal pozisyonunu al
                Vector3 originalPosition = transform.position;
                
                // Knockback kuvvetini uygula - çok yüksek değer uygulamak için AddForce yerine pozisyonu doğrudan değiştirme
                Vector3 knockbackPosition = originalPosition + (knockbackDirection * knockbackDistance);
                
                // Rigidbody2D'yi kullanarak pozisyonu güncelle - anlık değişim için
                StartCoroutine(MoveToPosition(knockbackPosition, knockbackDuration));
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
    
    /// <summary>
    /// Hedefe doğru hareket ettiren coroutine
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        // Hedefe doğrudan hareket etmek için düşmanı sabitlememiz gerekiyor
        RigidbodyType2D originalBodyType = rb.bodyType;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Orijinal pozisyonu kaydet
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            // Lerp ile pozisyonu güncelle
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Son pozisyona ulaştığından emin ol
        transform.position = targetPosition;
        
        // Rigidbody durumunu geri yükle
        rb.bodyType = originalBodyType;
        
        Debug.Log($"💨 Knockback completed for {gameObject.name} - moved to {targetPosition}");
    }

    private void OnDestroy()
    {
        // Component yok edilirken efektleri temizle
        RemoveWindEffect();
    }
} 