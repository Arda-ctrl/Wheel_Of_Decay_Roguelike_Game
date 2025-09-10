using UnityEngine;

public class RotkinsProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool isHoming = false;
    [SerializeField] private float homingStrength = 2f;
    
    [Header("Bounce Settings")]
    [SerializeField] private bool canBounce = true;
    [SerializeField] private int maxBounces = 2;
    [SerializeField] private float bounceSpeedMultiplier = 0.8f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject bounceVFX;
    [SerializeField] private TrailRenderer trail;
    
    private Vector2 direction;
    private Rigidbody2D rb;
    private int bounceCount = 0;
    private Transform playerTarget;
    private bool hasHitPlayer = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        // Player target'ını bul
        if (PlayerController.Instance != null)
        {
            playerTarget = PlayerController.Instance.transform;
        }
    }

    public void Initialize(Vector2 initialDirection, float projectileSpeed, float projectileDamage, bool homingEnabled)
    {
        direction = initialDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        isHoming = homingEnabled;
        
        // İlk velocity'yi ayarla
        rb.linearVelocity = direction * speed;
        
        // Trail rengi ayarla (homing = kırmızı, normal = mavi)
        if (trail != null)
        {
            trail.startColor = isHoming ? Color.red : Color.cyan;
            trail.endColor = isHoming ? Color.yellow : Color.blue;
        }
        
        // Lifetime sonrası yok et
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        // Homing behavior
        if (isHoming && playerTarget != null && !hasHitPlayer)
        {
            Vector2 targetDirection = (playerTarget.position - transform.position).normalized;
            Vector2 currentDirection = rb.linearVelocity.normalized;
            
            // Smooth homing
            Vector2 newDirection = Vector2.Lerp(currentDirection, targetDirection, homingStrength * Time.fixedDeltaTime);
            rb.linearVelocity = newDirection * speed;
            
            // Rotation'ı güncelle
            float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Player'a hasar ver
            var playerHealth = other.GetComponent<IHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Rotkins Crystal Hit Player! Damage: {damage}");
            }
            
            hasHitPlayer = true;
            
            // Hit effect spawn et
            if (hitVFX != null)
            {
                Instantiate(hitVFX, transform.position, Quaternion.identity);
            }
            
            // Ses efekti çal
            AudioManager.Instance?.PlaySFX(5);
            
            // Mermiyi yok et
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // Bounce logic
            if (canBounce && bounceCount < maxBounces)
            {
                HandleBounce(other);
            }
            else
            {
                // Hit effect spawn et
                if (hitVFX != null)
                {
                    Instantiate(hitVFX, transform.position, Quaternion.identity);
                }
                
                // Mermiyi yok et
                Destroy(gameObject);
            }
        }
    }

    private void HandleBounce(Collider2D hitCollider)
    {
        bounceCount++;
        
        // Bounce VFX spawn et
        if (bounceVFX != null)
        {
            Instantiate(bounceVFX, transform.position, Quaternion.identity);
        }
        
        // Bounce direction hesapla
        Vector2 hitNormal = GetHitNormal(hitCollider);
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 bounceDirection = Vector2.Reflect(currentVelocity.normalized, hitNormal);
        
        // Speed'i azalt
        speed *= bounceSpeedMultiplier;
        
        // Yeni velocity ayarla
        rb.linearVelocity = bounceDirection * speed;
        
        // Rotation'ı güncelle
        float angle = Mathf.Atan2(bounceDirection.y, bounceDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Debug.Log($"Crystal bounced! Bounce count: {bounceCount}/{maxBounces}");
        
        // Homing'i deaktive et bounce sonrası (daha chaotic olsun)
        if (bounceCount >= 1)
        {
            isHoming = false;
        }
    }

    private Vector2 GetHitNormal(Collider2D hitCollider)
    {
        // Basit normal hesaplama - collider center'ından hit point'e
        Vector2 hitPoint = transform.position;
        Vector2 colliderCenter = hitCollider.bounds.center;
        Vector2 normal = (hitPoint - colliderCenter).normalized;
        
        return normal;
    }

    private void OnBecameInvisible()
    {
        // Ekran dışına çıktığında yok et
        Destroy(gameObject);
    }
}
