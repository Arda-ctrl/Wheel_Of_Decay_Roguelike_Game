using UnityEngine;
using System.Collections;

public class RootSpikeDamage : MonoBehaviour
{
    [Header("Root Spike Settings")]
    [SerializeField] private float damage = 35f;
    [SerializeField] private float activeTime = 2f;
    [SerializeField] private float damageTickRate = 0.5f; // Damage every 0.5 seconds
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color activeColor = Color.red;
    
    private bool isActive = false;
    private bool hasBeenInitialized = false;
    private Collider2D rootCollider;

    private void Awake()
    {
        rootCollider = GetComponent<Collider2D>();
        if (rootCollider == null)
        {
            rootCollider = gameObject.AddComponent<CircleCollider2D>();
            rootCollider.isTrigger = true;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Başlangıçta collider'ı deaktive et
        rootCollider.enabled = false;
    }

    public void Initialize(float rootDamage, float rootActiveTime)
    {
        damage = rootDamage;
        activeTime = rootActiveTime;
        hasBeenInitialized = true;
        
        StartCoroutine(RootSpikeLifecycle());
    }

    private IEnumerator RootSpikeLifecycle()
    {
        // Warning phase
        if (spriteRenderer != null)
        {
            spriteRenderer.color = warningColor;
        }
        
        // Warning süresi (1 saniye)
        yield return new WaitForSeconds(1f);
        
        // Active phase başlat
        isActive = true;
        rootCollider.enabled = true;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = activeColor;
        }
        
        // Continuous damage coroutine başlat
        StartCoroutine(ContinuousDamage());
        
        // Active time boyunca bekle
        yield return new WaitForSeconds(activeTime);
        
        // Deactivate
        isActive = false;
        rootCollider.enabled = false;
        
        // Fade out effect
        yield return StartCoroutine(FadeOut());
        
        // Destroy
        Destroy(gameObject);
    }

    private IEnumerator ContinuousDamage()
    {
        while (isActive)
        {
            // Damage tick
            yield return new WaitForSeconds(damageTickRate);
            
            if (!isActive) break;
            
            // Check for player in trigger area
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1f);
            
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    var playerHealth = hitCollider.GetComponent<IHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                        Debug.Log($"Root Spike Damage: {damage}");
                        
                        // Damage VFX spawn et
                        if (damageVFX != null)
                        {
                            Instantiate(damageVFX, hitCollider.transform.position, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator FadeOut()
    {
        float fadeTime = 0.5f;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<IHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Root Spike Hit: {damage}");
                
                // Damage VFX spawn et
                if (damageVFX != null)
                {
                    Instantiate(damageVFX, other.transform.position, Quaternion.identity);
                }
            }
        }
    }
}
