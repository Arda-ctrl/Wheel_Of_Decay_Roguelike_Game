using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour, IHealth, IMoveable, IStatusEffect
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float baseSpeed = 5f;

    [Header("Detection Settings")]
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float stopDistance = 2f;
    [SerializeField] protected bool showDetectionGizmo = true;

    [Header("References")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] private bool facingRightByDefault = true; // Sprite varsayılan olarak sağa mı bakıyor?

    [Header("UI Settings")]
    [SerializeField] private Vector2 statsOffset = new Vector2(0f, 50f);
    [SerializeField] private float lineSpacing = 20f;
    [SerializeField] private bool showDebugStats = true;

    protected float currentHealth;
    protected float speedMultiplier = 1f;
    protected Dictionary<StatusEffectType, float> activeStatusEffects = new Dictionary<StatusEffectType, float>();
    
    protected Transform playerTransform;
    protected Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isPlayerInRange = false;
    private bool isFacingRight;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // SpriteRenderer referansını al
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Player referansını al
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }

        // Başlangıç yönünü ayarla
        isFacingRight = facingRightByDefault;
        UpdateSpriteDirection(isFacingRight);
        
        // Elemental stack sistemi artık ElementalAbilityManager tarafından yönetiliyor
        // StrikeStack component'i artık gerekli değil
        Debug.Log("🔧 Elemental stacks are now managed by ElementalAbilityManager");
    }
    
    private void OnDestroy()
    {
        // Clear status effects to prevent memory leaks
        activeStatusEffects.Clear();
    }

    protected virtual void Update()
    {
        // Update status effects
        List<StatusEffectType> expiredEffects = new List<StatusEffectType>();
        foreach (var effect in activeStatusEffects)
        {
            if (Time.time >= effect.Value)
            {
                expiredEffects.Add(effect.Key);
            }
        }

        // Remove expired effects
        foreach (var effect in expiredEffects)
        {
            RemoveStatus(effect);
        }

        // Player detection and movement
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerInRange = distanceToPlayer <= detectionRange;

            if (isPlayerInRange)
            {
                // Oyuncuya doğru hareket yönünü hesapla
                moveDirection = (playerTransform.position - transform.position).normalized;

                // Eğer durma mesafesinden uzaktaysa hareket et
                if (distanceToPlayer > stopDistance)
                {
                    rb.linearVelocity = moveDirection * GetCurrentSpeed();
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }

                // Sprite yönünü güncelle
                if (spriteRenderer != null)
                {
                    bool shouldFaceRight = playerTransform.position.x > transform.position.x;
                    if (shouldFaceRight != isFacingRight)
                    {
                        FlipSprite();
                    }
                }
            }
            else
            {
                // Oyuncu menzil dışındaysa dur
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        UpdateSpriteDirection(isFacingRight);
    }

    private void UpdateSpriteDirection(bool facingRight)
    {
        if (spriteRenderer != null)
        {
            // Eğer sprite varsayılan olarak sağa bakıyorsa
            if (facingRightByDefault)
            {
                spriteRenderer.flipX = !facingRight;
            }
            // Eğer sprite varsayılan olarak sola bakıyorsa
            else
            {
                spriteRenderer.flipX = facingRight;
            }
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (showDetectionGizmo)
        {
            // Algılama menzilini görselleştir
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Durma mesafesini görselleştir
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }

    // IHealth Implementation
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    // IMoveable Implementation
    public void SetSpeedMultiplier(float multiplier)
    {
        float oldMultiplier = speedMultiplier;
        speedMultiplier = Mathf.Clamp(multiplier, 0f, 1f);
        Debug.Log($"🐛 {gameObject.name} speed multiplier changed from {oldMultiplier:F2} to {speedMultiplier:F2}");
        Debug.Log($"🐛 New effective speed: {GetCurrentSpeed():F2} (base: {baseSpeed:F2})");
    }

    public float GetCurrentSpeed() => baseSpeed * speedMultiplier;
    public float GetBaseSpeed() => baseSpeed;

    // IStatusEffect Implementation
    public void ApplyStatus(StatusEffectType statusType, float duration)
    {
        activeStatusEffects[statusType] = Time.time + duration;
    }

    public void RemoveStatus(StatusEffectType statusType)
    {
        if (activeStatusEffects.ContainsKey(statusType))
        {
            activeStatusEffects.Remove(statusType);
        }
    }

    public bool HasStatus(StatusEffectType statusType)
    {
        return activeStatusEffects.ContainsKey(statusType);
    }

    protected virtual void Die()
    {
        // ElementalArea için ölüm event'ini tetikle
        if (EventManager.Instance != null && gameObject != null)
        {
            try
            {
                EventManager.Instance.TriggerEnemyDeath(gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EnemyController] Failed to trigger death event: {e.Message}");
            }
        }
        
        // Implement death logic here (e.g., play animation, spawn particles, etc.)
        try
        {
            Destroy(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[EnemyController] Failed to destroy enemy: {e.Message}");
        }
    }

    // Debug visualization in editor
    private void OnGUI()
    {
        if (Application.isEditor && showDebugStats)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            
            // Apply the custom offset
            float xPos = screenPos.x - 50f + statsOffset.x;
            float yPos = Screen.height - screenPos.y - statsOffset.y;
            
            // Draw health
            GUI.Label(new Rect(xPos, yPos, 100, 20), 
                     $"HP: {currentHealth:F0}/{maxHealth}");
            
            // Draw speed on the next line using lineSpacing
            GUI.Label(new Rect(xPos, yPos + lineSpacing, 100, 20), 
                     $"Speed: {GetCurrentSpeed():F1}");
        }
    }
}
