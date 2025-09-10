using UnityEngine;

public enum MudType
{
    Normal,
    Poison,
    Acid
}

public class MudProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private MudType mudType = MudType.Normal;
    
    [Header("Effects")]
    [SerializeField] private GameObject splashEffect;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private float splashRadius = 1.5f;
    
    [Header("Status Effects")]
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float slowAmount = 0.5f;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonDamage = 5f;
    
    private Vector2 direction;
    private GameObject owner;
    private Rigidbody2D rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Initial physics setup - will be configured properly in Initialize()
        rb.gravityScale = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.freezeRotation = false; // Allow rotation for visual effect
        
        // Add collider if missing
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
            circleCollider.radius = 0.3f; // Small radius for mud projectile
        }
    }

    public void Initialize(Vector2 targetPosition, float projectileSpeed, float projectileDamage, GameObject ownerObject, MudType type = MudType.Normal)
    {
        owner = ownerObject;
        speed = projectileSpeed;
        damage = projectileDamage;
        mudType = type;
        
        // Calculate proper trajectory to hit target
        Vector2 startPos = transform.position;
        Vector2 targetDirection = targetPosition - startPos;
        float distance = targetDirection.magnitude;
        
        // Calculate trajectory with arc physics
        Vector2 velocity = CalculateTrajectoryVelocity(startPos, targetPosition, projectileSpeed);
        
        if (rb != null)
        {
            rb.linearVelocity = velocity;
            
            // Ensure physics is set up correctly
            rb.gravityScale = 1f; // Normal gravity for realistic arc
            rb.linearDamping = 0f; // No air resistance for predictable trajectory
            rb.angularDamping = 0f;
        }
        
        // Store direction for reference (normalized)
        direction = targetDirection.normalized;

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
        
        Debug.Log($"Mud projectile launched to {targetPosition} with velocity {velocity}, distance: {distance}");
    }
    
    private Vector2 CalculateTrajectoryVelocity(Vector2 startPos, Vector2 targetPos, float launchSpeed)
    {
        // Guard against invalid inputs
        if (launchSpeed <= 0.01f)
        {
            launchSpeed = Mathf.Max(launchSpeed, 0.1f);
        }

        // Calculate the displacement
        Vector2 displacement = targetPos - startPos;
        float horizontalDistance = displacement.x;
        float verticalDistance = displacement.y;
        float distance = displacement.magnitude;

        // If too close, return a small safe upward-forward velocity
        if (distance < 0.001f)
        {
            return new Vector2(launchSpeed, 2f);
        }
        
        // Use physics to calculate trajectory
        // For projectile motion: v = sqrt(g * d / sin(2θ))
        // We'll use a fixed launch angle for consistent arc
        float launchAngle = 45f * Mathf.Deg2Rad; // 45 degrees for optimal range
        
        // If target is above us, use a steeper angle
        if (verticalDistance > 0)
        {
            launchAngle = 60f * Mathf.Deg2Rad;
        }
        // If target is below us, use a shallower angle
        else if (verticalDistance < -2f)
        {
            launchAngle = 30f * Mathf.Deg2Rad;
        }
        
        float gravity = Physics2D.gravity.magnitude;
        if (gravity == 0) gravity = 9.81f; // Fallback if gravity is disabled
        
        // Calculate required velocity for the trajectory (kept for reference, but we use velocityNeeded below)
        float sinTwoTheta = Mathf.Sin(2 * launchAngle);
        if (Mathf.Abs(sinTwoTheta) < 0.0001f) sinTwoTheta = 0.5f; // avoid division by zero
        float velocityMagnitude = Mathf.Sqrt(Mathf.Max(0.0001f, gravity * distance / sinTwoTheta));
        
        // Clamp the velocity to reasonable values
        velocityMagnitude = Mathf.Clamp(velocityMagnitude, launchSpeed * 0.5f, launchSpeed * 2f);
        
        // Calculate velocity components
        Vector2 velocityDirection = displacement.normalized;
        
        // For more accurate targeting, calculate exact velocity needed
        float timeToTarget = distance / Mathf.Max(launchSpeed, 0.1f);
        Vector2 velocityNeeded = displacement / timeToTarget;
        
        // Add gravity compensation
        velocityNeeded.y += 0.5f * gravity * timeToTarget;
        
        // Ensure minimum upward velocity for arc effect
        if (velocityNeeded.y < 2f)
        {
            velocityNeeded.y = 2f + Mathf.Abs(velocityNeeded.x) * 0.2f;
        }
        
        return velocityNeeded;
    }
    
    private void Update()
    {
        // Rotate the projectile based on its velocity for visual effect
        if (rb != null && !hasHit)
        {
            Vector2 velocity = rb.linearVelocity;
            if (velocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Don't hit the owner
        if (other.gameObject == owner) return;

        // Check if it hits player
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other);
        }
        else
        {
            // Any other collision (non-owner) will splash
            CreateSplash();
        }
    }

    private void DealDamageToPlayer(Collider2D player)
    {
        // Deal damage
        var playerHealth = player.GetComponent<IHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else if (PlayerHealthController.Instance != null)
        {
            PlayerHealthController.Instance.DamagePlayer();
        }

        // Apply status effects based on mud type
        ApplyStatusEffects(player);
        
        CreateSplash();
    }

    private void ApplyStatusEffects(Collider2D target)
    {
        var statusTarget = target.GetComponent<IStatusEffect>();
        if (statusTarget == null) return;
        
        switch (mudType)
        {
            case MudType.Normal:
                // Apply slow effect
                statusTarget.ApplyStatus(StatusEffectType.Chilled, slowDuration);
                break;
                
            case MudType.Poison:
                // Apply poison and slow
                statusTarget.ApplyStatus(StatusEffectType.Poisoned, poisonDuration);
                statusTarget.ApplyStatus(StatusEffectType.Chilled, slowDuration);
                break;
                
            case MudType.Acid:
                // Apply damage over time (using poison system)
                statusTarget.ApplyStatus(StatusEffectType.Poisoned, poisonDuration);
                break;
        }
    }

    private void CreateSplash()
    {
        if (hasHit) return;
        hasHit = true;

        // Create splash effect
        if (splashEffect != null)
        {
            GameObject splash = Instantiate(splashEffect, transform.position, Quaternion.identity);
            
            // Scale splash based on type
            float scale = mudType == MudType.Poison ? 1.2f : 1f;
            splash.transform.localScale = Vector3.one * scale;
        }

        // Play splash sound
        if (splashSound != null)
        {
            AudioSource.PlayClipAtPoint(splashSound, transform.position);
        }

        // Create area effect for poison mud
        if (mudType == MudType.Poison)
        {
            CreatePoisonArea();
        }

        Destroy(gameObject);
    }

    private void CreatePoisonArea()
    {
        // Find all targets in splash radius
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        
        foreach (var target in targets)
        {
            if (target.CompareTag("Player"))
            {
                var statusTarget = target.GetComponent<IStatusEffect>();
                if (statusTarget != null)
                {
                    statusTarget.ApplyStatus(StatusEffectType.Poisoned, poisonDuration * 0.5f); // Reduced duration for area effect
                }
            }
        }
    }

    private void OnBecameInvisible()
    {
        if (!hasHit)
        {
            CreateSplash();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw splash radius
        Gizmos.color = mudType == MudType.Poison ? Color.green : new Color(0.6f, 0.3f, 0f); // Brown color
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
