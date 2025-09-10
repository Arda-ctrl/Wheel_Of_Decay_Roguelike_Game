using UnityEngine;
using System.Collections;

public class BindingRootTrap : MonoBehaviour
{
    [Header("Binding Root Settings")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float bindingDuration = 3f;
    [SerializeField] private float slowAmount = 0.7f; // 70% slow
    [SerializeField] private float damageTickRate = 1f; // Damage every second while bound
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject bindingVFX;
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color bindingColor = Color.magenta;
    
    [Header("Binding Chain")]
    [SerializeField] private LineRenderer chainRenderer;
    [SerializeField] private float chainWidth = 0.1f;
    
    private bool isActive = false;
    private bool isBinding = false;
    private GameObject boundPlayer;
    private IMoveable playerMoveable;
    private Collider2D rootCollider;
    private Coroutine bindingCoroutine;

    private void Awake()
    {
        rootCollider = GetComponent<Collider2D>();
        if (rootCollider == null)
        {
            rootCollider = gameObject.AddComponent<CircleCollider2D>();
            rootCollider.isTrigger = true;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Chain renderer setup
        if (chainRenderer == null)
        {
            chainRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        chainRenderer.material = new Material(Shader.Find("Sprites/Default"));
        chainRenderer.startColor = bindingColor;
        chainRenderer.endColor = bindingColor;
        chainRenderer.startWidth = chainWidth;
        chainRenderer.endWidth = chainWidth;
        chainRenderer.positionCount = 2;
        chainRenderer.sortingOrder = 10;
        chainRenderer.enabled = false;
        
        // Başlangıçta collider'ı deaktive et
        rootCollider.enabled = false;
    }

    public void Initialize(float rootDamage, float rootBindingDuration, float rootSlowAmount)
    {
        damage = rootDamage;
        bindingDuration = rootBindingDuration;
        slowAmount = rootSlowAmount;
        
        StartCoroutine(BindingRootLifecycle());
    }

    private IEnumerator BindingRootLifecycle()
    {
        // Warning phase
        if (spriteRenderer != null)
        {
            spriteRenderer.color = warningColor;
        }
        
        // Warning süresi (1.2 saniye)
        yield return new WaitForSeconds(1.2f);
        
        // Active phase başlat
        isActive = true;
        rootCollider.enabled = true;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = activeColor;
        }
        
        // Active time boyunca bekle
        yield return new WaitForSeconds(bindingDuration + 2f);
        
        // Deactivate ve cleanup
        ReleaseBinding();
        
        // Fade out effect
        yield return StartCoroutine(FadeOut());
        
        // Destroy
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || isBinding) return;
        
        if (other.CompareTag("Player"))
        {
            StartBinding(other.gameObject);
        }
    }

    private void StartBinding(GameObject player)
    {
        isBinding = true;
        boundPlayer = player;
        
        // Player movement component'ini al
        playerMoveable = player.GetComponent<IMoveable>();
        
        // Slow effect uygula
        if (playerMoveable != null)
        {
            playerMoveable.SetSpeedMultiplier(1f - slowAmount);
            Debug.Log($"Player bound by roots! Speed reduced by {slowAmount * 100}%");
        }
        
        // Visual binding effect
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bindingColor;
        }
        
        // Binding VFX spawn et
        if (bindingVFX != null)
        {
            Instantiate(bindingVFX, player.transform.position, Quaternion.identity, player.transform);
        }
        
        // Chain göster
        chainRenderer.enabled = true;
        
        // Continuous binding coroutine başlat
        bindingCoroutine = StartCoroutine(BindingLoop());
    }

    private IEnumerator BindingLoop()
    {
        float elapsed = 0f;
        
        while (isBinding && boundPlayer != null && elapsed < bindingDuration)
        {
            elapsed += Time.deltaTime;
            
            // Chain'i güncelle
            if (chainRenderer.enabled && boundPlayer != null)
            {
                chainRenderer.SetPosition(0, transform.position);
                chainRenderer.SetPosition(1, boundPlayer.transform.position);
            }
            
            // Damage tick
            if (elapsed >= damageTickRate && (elapsed % damageTickRate) < Time.deltaTime)
            {
                ApplyBindingDamage();
            }
            
            // Player'ın binding range'den çok uzaklaşıp uzaklaşmadığını kontrol et
            if (boundPlayer != null)
            {
                float distance = Vector2.Distance(transform.position, boundPlayer.transform.position);
                if (distance > 3f) // Max binding range
                {
                    Debug.Log("Player escaped binding range!");
                    break;
                }
            }
            
            yield return null;
        }
        
        ReleaseBinding();
    }

    private void ApplyBindingDamage()
    {
        if (boundPlayer != null)
        {
            var playerHealth = boundPlayer.GetComponent<IHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Binding Root Damage: {damage}");
                
                // Damage VFX spawn et
                if (damageVFX != null)
                {
                    Instantiate(damageVFX, boundPlayer.transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void ReleaseBinding()
    {
        if (isBinding && boundPlayer != null)
        {
            // Speed'i normale döndür
            if (playerMoveable != null)
            {
                playerMoveable.SetSpeedMultiplier(1f);
                Debug.Log("Player released from binding roots!");
            }
            
            isBinding = false;
            boundPlayer = null;
            playerMoveable = null;
            
            // Chain'i gizle
            chainRenderer.enabled = false;
            
            // Color'u normale döndür
            if (spriteRenderer != null)
            {
                spriteRenderer.color = activeColor;
            }
        }
        
        // Binding coroutine'i durdur
        if (bindingCoroutine != null)
        {
            StopCoroutine(bindingCoroutine);
            bindingCoroutine = null;
        }
    }

    private IEnumerator FadeOut()
    {
        float fadeTime = 0.8f;
        float elapsed = 0f;
        
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            
            if (spriteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
            }
            
            yield return null;
        }
    }

    private void OnDestroy()
    {
        // Cleanup on destroy
        ReleaseBinding();
    }
}
