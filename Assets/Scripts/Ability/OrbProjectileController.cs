using UnityEngine;

/// <summary>
/// OrbProjectileController - Orb'ların fırlattığı mermilerin kontrolü
/// </summary>
public class OrbProjectileController : MonoBehaviour
{
    private Vector3 direction;
    private float damage;
    private IElement element;
    private ElementalAbilityData abilityData;
    private float speed = 15f;
    private float lifeTime = 0f;
    private float maxLifeTime = 3f; // 3 saniye sonra yok ol
    
    /// <summary>
    /// Projectile'ı initialize eder
    /// </summary>
    /// <param name="direction">Hareket yönü</param>
    /// <param name="damage">Hasar miktarı</param>
    /// <param name="element">Element türü</param>
    /// <param name="abilityData">Ability verileri</param>
    public void Initialize(Vector3 direction, float damage, IElement element, ElementalAbilityData abilityData)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        this.element = element;
        this.abilityData = abilityData;
        this.speed = abilityData?.orbSpeed ?? 15f;
        
        // Rotasyonu yön vektörüne göre ayarla
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        Debug.Log($"🔫 Orb projectile initialized - Damage: {damage}, Speed: {speed}");
    }
    
    private void Update()
    {
        // Hareket et
        transform.position += direction * speed * Time.deltaTime;
        
        // Yaşam süresini kontrol et
        lifeTime += Time.deltaTime;
        if (lifeTime >= maxLifeTime)
        {
            DestroyProjectile();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Hasar uygula
            var health = other.GetComponent<IHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"🎯 Orb projectile hit {other.name} for {damage} damage");
            }
            
            // Element stack ekle
            if (element != null)
            {
                element.ApplyElementStack(other.gameObject, 1);
            }
            
            // Hit VFX oluştur
            CreateHitVFX(other.transform.position);
            
            // Projectile'ı yok et
            DestroyProjectile();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // Duvara çarptıysa yok et
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// Hit VFX'i oluşturur
    /// </summary>
    /// <param name="position">VFX pozisyonu</param>
    private void CreateHitVFX(Vector3 position)
    {
        // Basit bir hit efekti oluştur
        GameObject hitVFX = new GameObject("Orb Projectile Hit VFX");
        hitVFX.transform.position = position;
        
        var spriteRenderer = hitVFX.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateHitSprite();
        spriteRenderer.color = element?.ElementColor ?? Color.white;
        spriteRenderer.sortingOrder = 10;
        
        // VFX'i 0.5 saniye sonra otomatik yok et
        Destroy(hitVFX, 0.5f);
        
        Debug.Log("💥 Hit VFX created and will be destroyed in 0.5 seconds");
    }
    
    /// <summary>
    /// Hit sprite'ı oluşturur
    /// </summary>
    /// <returns>Hit sprite</returns>
    private Sprite CreateHitSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerRadius = size / 2f - 2f;
        float innerRadius = size / 4f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= outerRadius && distance >= innerRadius)
                {
                    // Ring şeklinde
                    float alpha = 1f - (distance - innerRadius) / (outerRadius - innerRadius);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return sprite;
    }
    

    
    /// <summary>
    /// Projectile'ı yok eder
    /// </summary>
    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
} 