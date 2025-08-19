using UnityEngine;
using System.Collections;

/// <summary>
/// TempEarthEffect - Earth element için geçici efekt
/// Düşmanı kısa süreliğine köklendirir ve hareket edemez hale getirir
/// </summary>
public class TempEarthEffect : MonoBehaviour
{
    [Header("Earth Effect Settings")]
    public float rootDuration = 1.5f;
    public float earthDamage = 7f;
    public bool applyRoot = true;
    
    private float elapsedTime;
    private IMoveable moveable;
    private Rigidbody2D rb;
    private bool isRooted = false;
    
    private void Start()
    {
        elapsedTime = 0f;
        moveable = GetComponent<IMoveable>();
        rb = GetComponent<Rigidbody2D>();
        
        // Earth efektini uygula
        ApplyEarthEffect();
    }
    
    private void Update()
    {
        if (!isRooted) return;
        
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= rootDuration)
        {
            RemoveEarthEffect();
            Destroy(this);
        }
    }
    
    /// <summary>
    /// Earth efektini uygular
    /// </summary>
    private void ApplyEarthEffect()
    {
        // Tek seferlik earth hasarı
        var health = GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(earthDamage);
        }
        
        if (applyRoot && moveable != null && rb != null)
        {
            isRooted = true;
            
            // Hızı sıfırla
            moveable.SetSpeedMultiplier(0f);
            
            // Rigidbody'yi dondur
            RigidbodyType2D originalBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            
            // Köklenme VFX'i ekle
            PlayRootEffect();
            
            // Köklenme SFX'i oynat
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(26); // Earth/root sound effect
            }
            
            Debug.Log($"🌱 Earth root effect applied to {gameObject.name} for {rootDuration} seconds!");
        }
    }
    
    /// <summary>
    /// Earth efektini kaldırır
    /// </summary>
    private void RemoveEarthEffect()
    {
        if (moveable != null)
        {
            // Hız çarpanını normale döndür
            moveable.SetSpeedMultiplier(1f);
            
            // Rigidbody'yi normale döndür
            if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        
        isRooted = false;
        Debug.Log($"🌱 Earth root effect removed from {gameObject.name}");
    }
    
    /// <summary>
    /// Köklenme efektini oynatır
    /// </summary>
    private void PlayRootEffect()
    {
        // Köklenme VFX'i
        var rootVFX = Resources.Load<GameObject>("Prefabs/Effects/RootVFX");
        if (rootVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(rootVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
            
            // Root efekti rootDuration kadar sürsün
            Destroy(vfxInstance, rootDuration);
        }
        
        // Yer parçacıkları oluştur
        StartCoroutine(CreateEarthParticles());
    }
    
    /// <summary>
    /// Yer parçacıkları oluşturur
    /// </summary>
    private IEnumerator CreateEarthParticles()
    {
        for (int i = 0; i < 3; i++)
        {
            // Düşmanın etrafında rastgele noktalarda toprak parçacıkları
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            );
            
            var particleObj = new GameObject("EarthParticle");
            particleObj.transform.position = transform.position + randomOffset;
            
            var particleSystem = particleObj.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startColor = new Color(0.6f, 0.4f, 0.2f);
            main.startSize = 0.3f;
            main.startLifetime = 1f;
            
            // Particle System'i başlat
            particleSystem.Play();
            
            // 1 saniye sonra yok et
            Destroy(particleObj, 1f);
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private void OnDestroy()
    {
        // Component yok edilirken efektleri temizle
        RemoveEarthEffect();
    }
}
