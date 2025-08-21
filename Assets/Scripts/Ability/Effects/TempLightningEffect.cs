using UnityEngine;
using System.Collections;

/// <summary>
/// TempLightningEffect - Lightning element için geçici efekt
/// Elektrik şoku ve stun etkisi sağlar
/// </summary>
public class TempLightningEffect : MonoBehaviour
{
    [Header("Lightning Effect Settings")]
    public float shockDuration = 3f;
    public float damagePerTick = 4f;
    public float tickRate = 0.5f;
    public float stunChance = 0.15f;
    public float stunDuration = 1f;
    
    private float elapsedTime;
    private float lastTickTime;
    private bool isStunned = false;
    private IHealth health;
    private IMoveable moveable;
    
    private void Start()
    {
        elapsedTime = 0f;
        lastTickTime = 0f;
        health = GetComponent<IHealth>();
        moveable = GetComponent<IMoveable>();
        
        // Elektrik VFX'i ekle
        PlayShockEffects();
        
        // Stun şansını kontrol et
        TryApplyStun();
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // Periyodik elektrik hasarı
        if (Time.time - lastTickTime >= tickRate)
        {
            ApplyShockDamage();
            lastTickTime = Time.time;
        }
        
        // Efektin bitmesi
        if (elapsedTime >= shockDuration)
        {
            RemoveShockEffect();
            Destroy(this);
        }
        
        // Stun bitimi kontrolü
        if (isStunned && elapsedTime >= stunDuration)
        {
            RemoveStun();
        }
    }
    
    /// <summary>
    /// Stun uygulamayı dener (şans faktörü)
    /// </summary>
    private void TryApplyStun()
    {
        if (Random.value < stunChance)
        {
            ApplyStun();
        }
    }
    
    /// <summary>
    /// Elektrik şoku hasarı uygular
    /// </summary>
    private void ApplyShockDamage()
    {
        if (health != null)
        {
            health.TakeDamage(damagePerTick);
            
            // Elektrik çarpması VFX'ini kısaca göster
            PlayDamageEffect();
        }
    }
    
    /// <summary>
    /// Stun etkisi uygular
    /// </summary>
    private void ApplyStun()
    {
        if (moveable != null)
        {
            isStunned = true;
            moveable.SetSpeedMultiplier(0f); // Tam hareketsizlik
            
            // Stun VFX'i ekle
            PlayStunEffect();
            
            Debug.Log($"⚡ Lightning stunned {gameObject.name} for {stunDuration} seconds!");
        }
    }
    
    /// <summary>
    /// Stun etkisini kaldırır
    /// </summary>
    private void RemoveStun()
    {
        if (moveable != null && isStunned)
        {
            isStunned = false;
            moveable.SetSpeedMultiplier(1f); // Normal hıza dön
            
            Debug.Log($"⚡ Stun effect ended for {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Şok efektini kaldırır
    /// </summary>
    private void RemoveShockEffect()
    {
        if (isStunned)
        {
            RemoveStun();
        }
        
        Debug.Log($"⚡ Lightning effect removed from {gameObject.name}");
    }
    
    /// <summary>
    /// Elektrik çarpması efektlerini oynatır
    /// </summary>
    private void PlayShockEffects()
    {
        // Lightning VFX'i oynat
        var lightningVFX = Resources.Load<GameObject>("Prefabs/Effects/LightningVFX");
        if (lightningVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(lightningVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
            
            // 3 saniye sonra VFX'i temizle
            Destroy(vfxInstance, shockDuration);
        }
        
        // Lightning SFX'i oynat
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(28); // Elektrik sesi
        }
    }
    
    /// <summary>
    /// Hasar efektini oynatır
    /// </summary>
    private void PlayDamageEffect()
    {
        // Kısa süreli hasar flash'ı
        StartCoroutine(FlashEffect(0.1f));
    }
    
    /// <summary>
    /// Stun efektini oynatır
    /// </summary>
    private void PlayStunEffect()
    {
        // Stun VFX'i ekle (yıldızlar, vs.)
        var stunVFX = Resources.Load<GameObject>("Prefabs/Effects/StunVFX");
        if (stunVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(stunVFX, transform.position + Vector3.up, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
            
            // Stun süresi kadar göster
            Destroy(vfxInstance, stunDuration);
        }
    }
    
    /// <summary>
    /// Flash efekti için coroutine
    /// </summary>
    private IEnumerator FlashEffect(float duration)
    {
        // Sprite Renderer'ı beyaza çevir (flash efekti)
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = Color.white;
            
            yield return new WaitForSeconds(duration);
            
            // Orijinal renge geri dön
            renderer.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
    
    private void OnDestroy()
    {
        // Component yok edilirken efektleri temizle
        RemoveShockEffect();
    }
}

