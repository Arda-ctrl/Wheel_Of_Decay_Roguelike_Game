using UnityEngine;
using System.Collections;

/// <summary>
/// TempVoidEffect - Void element için geçici efekt
/// Düşmanın görüş alanını ve menzilini azaltır
/// </summary>
public class TempVoidEffect : MonoBehaviour
{
    [Header("Void Effect Settings")]
    public float visionReduction = 0.3f; // %30 görüş alanı azaltma
    public float rangeReduction = 0.2f;  // %20 menzil azaltma
    public float duration = 4f;
    public float voidDamage = 5f;
    
    private float elapsedTime;
    private IEnemy enemy;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Material originalMaterial;
    private Material voidMaterial;
    private float originalDetectionRange;
    private float originalAttackRange;
    
    private void Start()
    {
        elapsedTime = 0f;
        enemy = GetComponent<IEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalMaterial = spriteRenderer.material;
            
            // Void material oluştur (mor renkli shader)
            voidMaterial = new Material(originalMaterial);
            voidMaterial.color = new Color(0.6f, 0.1f, 0.9f, 0.8f);
        }
        
        // Void efektini uygula
        ApplyVoidEffect();
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // Düşmanın üzerinde void parçacıkları göster
        if (elapsedTime % 0.5f < 0.1f)
        {
            CreateVoidParticle();
        }
        
        if (elapsedTime >= duration)
        {
            RemoveVoidEffect();
            Destroy(this);
        }
    }
    
    /// <summary>
    /// Void efektini uygular
    /// </summary>
    private void ApplyVoidEffect()
    {
        // Tek seferlik void hasarı
        var health = GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(voidDamage);
        }
        
        // Düşmanın görüş alanını azalt
        if (enemy != null)
        {
            if (enemy.GetDetectionRange() > 0)
            {
                originalDetectionRange = enemy.GetDetectionRange();
                enemy.SetDetectionRange(originalDetectionRange * (1 - visionReduction));
            }
            
            if (enemy.GetAttackRange() > 0)
            {
                originalAttackRange = enemy.GetAttackRange();
                enemy.SetAttackRange(originalAttackRange * (1 - rangeReduction));
            }
        }
        
        // Düşmana void efekti uygula
        if (spriteRenderer != null)
        {
            spriteRenderer.material = voidMaterial;
        }
        
        // Void VFX'i ekle
        PlayVoidEffect();
        
        // Void SFX'i oynat
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(27); // Void sound effect
        }
        
        Debug.Log($"🌀 Void effect applied to {gameObject.name} for {duration} seconds!");
    }
    
    /// <summary>
    /// Void efektini kaldırır
    /// </summary>
    private void RemoveVoidEffect()
    {
        // Görüş alanını normale döndür
        if (enemy != null)
        {
            enemy.SetDetectionRange(originalDetectionRange);
            enemy.SetAttackRange(originalAttackRange);
        }
        
        // Rengi normale döndür
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log($"🌀 Void effect removed from {gameObject.name}");
    }
    
    /// <summary>
    /// Void parçacıkları oluşturur
    /// </summary>
    private void CreateVoidParticle()
    {
        // Düşmanın etrafında rastgele bir noktada void parçacığı oluştur
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            0f
        );
        
        GameObject particle = new GameObject("VoidParticle");
        particle.transform.position = transform.position + randomOffset;
        
        SpriteRenderer particleRenderer = particle.AddComponent<SpriteRenderer>();
        particleRenderer.sprite = Resources.Load<Sprite>("Sprites/Effects/VoidParticle");
        particleRenderer.color = new Color(0.6f, 0.1f, 0.9f, 0.8f);
        particleRenderer.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;
        
        // Parçacığı küçült ve yavaşça kaybol
        StartCoroutine(FadeAndDestroy(particle));
    }
    
    /// <summary>
    /// Void efektini oynatır
    /// </summary>
    private void PlayVoidEffect()
    {
        // Void VFX'i
        var voidVFX = Resources.Load<GameObject>("Prefabs/Effects/VoidVFX");
        if (voidVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(voidVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
            
            // Efekt süresince göster
            Destroy(vfxInstance, duration);
        }
    }
    
    /// <summary>
    /// Parçacıkları yavaşça yok eder
    /// </summary>
    private IEnumerator FadeAndDestroy(GameObject particleObj)
    {
        SpriteRenderer renderer = particleObj.GetComponent<SpriteRenderer>();
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color startColor = renderer.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);
            renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            // Parçacığı yavaşça küçült
            particleObj.transform.localScale = Vector3.Lerp(
                Vector3.one,
                Vector3.zero,
                elapsedTime / fadeDuration
            );
            
            yield return null;
        }
        
        Destroy(particleObj);
    }
    
    private void OnDestroy()
    {
        // Component yok edilirken efektleri temizle
        RemoveVoidEffect();
        
        // Material'i yok et
        if (voidMaterial != null)
        {
            Destroy(voidMaterial);
        }
    }
}


