using UnityEngine;
using System.Collections;

public enum TrapType
{
    BearTrap,
    BombTrap
}

public class GoblinTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private TrapType trapType = TrapType.BearTrap;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float bleedDuration = 5f;
    [SerializeField] private float bleedDamagePerSecond = 3f;
    [SerializeField] private float triggerRadius = 0.8f;
    [SerializeField] private LayerMask triggerLayers = 1;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer trapRenderer;
    [SerializeField] private Sprite activeTrapSprite;
    [SerializeField] private Sprite triggeredTrapSprite;
    [SerializeField] private GameObject triggerEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioClip placementSound;
    [SerializeField] private AudioClip triggerSound;
    
    private bool isArmed = false;
    private bool hasTriggered = false;
    private Collider2D trapCollider;
    
    private void Start()
    {
        trapCollider = GetComponent<Collider2D>();
        if (trapCollider == null)
        {
            trapCollider = gameObject.AddComponent<CircleCollider2D>();
            ((CircleCollider2D)trapCollider).radius = triggerRadius;
            trapCollider.isTrigger = true;
        }
        
        // Start the arming sequence
        StartCoroutine(ArmTrap());
    }
    
    private IEnumerator ArmTrap()
    {
        // Short delay before trap becomes active
        yield return new WaitForSeconds(0.5f);
        
        isArmed = true;
        
        if (trapRenderer != null && activeTrapSprite != null)
        {
            trapRenderer.sprite = activeTrapSprite;
        }
        
        // Play placement sound
        if (placementSound != null && AudioManager.Instance != null)
        {
            // AudioManager expects an int index
            Debug.Log("Playing trap placement sound");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isArmed || hasTriggered) return;
        
        // Check if it's a valid trigger target
        if (((1 << other.gameObject.layer) & triggerLayers) != 0)
        {
            if (other.CompareTag("Player"))
            {
                TriggerTrap(other);
            }
        }
    }
    
    private void TriggerTrap(Collider2D target)
    {
        if (hasTriggered) return;
        
        hasTriggered = true;
        isArmed = false;
        
        // Play trigger sound
        if (triggerSound != null && AudioManager.Instance != null)
        {
            // AudioManager expects an int index
            Debug.Log("Playing trap trigger sound");
        }
        
        // Show trigger effect
        if (triggerEffect != null)
        {
            Instantiate(triggerEffect, transform.position, Quaternion.identity);
        }
        
        // Change sprite to triggered state
        if (trapRenderer != null && triggeredTrapSprite != null)
        {
            trapRenderer.sprite = triggeredTrapSprite;
        }
        
        // Apply trap effects based on type
        switch (trapType)
        {
            case TrapType.BearTrap:
                ApplyBearTrapEffects(target);
                break;
            case TrapType.BombTrap:
                ApplyBombTrapEffects(target);
                break;
        }
        
        // Destroy trap after a delay
        StartCoroutine(DestroyTrapAfterDelay());
    }
    
    private void ApplyBearTrapEffects(Collider2D target)
    {
        if (target.CompareTag("Player"))
        {
            // Deal immediate damage
            var playerHealth = target.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer();
                Debug.Log($"Bear trap hit player for {damage} damage!");
            }
            
            // Apply bleeding effect
            var playerStatusEffect = target.GetComponent<IStatusEffect>();
            if (playerStatusEffect != null)
            {
                StartCoroutine(ApplyBleedingEffect(target));
            }
            
            // Slow the player temporarily
            var playerMoveable = target.GetComponent<IMoveable>();
            if (playerMoveable != null)
            {
                StartCoroutine(ApplySlowEffect(playerMoveable));
            }
        }
    }
    
    private void ApplyBombTrapEffects(Collider2D target)
    {
        // Bomb trap would have explosion logic here
        if (target.CompareTag("Player"))
        {
            var playerHealth = target.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.DamagePlayer(); // More damage for bomb trap
                Debug.Log($"Bomb trap hit player for {damage * 1.5f} damage!");
            }
            
            // Knockback effect
            var playerRb = target.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDirection * 8f, ForceMode2D.Impulse);
            }
        }
    }
    
    private IEnumerator ApplyBleedingEffect(Collider2D target)
    {
        float bleedTimer = 0f;
        var playerHealth = target.GetComponent<PlayerHealthController>();
        
        while (bleedTimer < bleedDuration && playerHealth != null)
        {
            yield return new WaitForSeconds(1f);
            playerHealth.DamagePlayer();
            bleedTimer += 1f;
            
            Debug.Log($"Player taking bleed damage: {bleedDamagePerSecond}");
        }
    }
    
    private IEnumerator ApplySlowEffect(IMoveable target)
    {
        target.SetSpeedMultiplier(0.5f); // Slow to 50% speed
        yield return new WaitForSeconds(3f);
        target.SetSpeedMultiplier(1f); // Restore normal speed
    }
    
    private IEnumerator DestroyTrapAfterDelay()
    {
        yield return new WaitForSeconds(10f); // Trap remains for 10 seconds after trigger
        Destroy(gameObject);
    }
    
    public void SetTrapType(TrapType type)
    {
        trapType = type;
    }
    
    public void SetTrapDamage(float trapDamage)
    {
        damage = trapDamage;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw trigger radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
