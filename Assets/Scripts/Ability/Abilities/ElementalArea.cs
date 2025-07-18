using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ElementalArea - 5+ stack ile ölen düşman alan hasarı
/// Bir düşmanın üzerinde 5 den fazla elemental stack ile ölürse öldüğü alana dümanlara aynı elementten hasar veren ve stack yerleştiren bir alan oluşturur
/// </summary>
public class ElementalArea : MonoBehaviour, IAbility
{
    [Header("Elemental Area Settings")]
    [SerializeField] private string abilityName = "Elemental Area";
    [SerializeField] private string description = "5+ stack ile ölen düşman alan hasarı oluşturur";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownDuration = 0f; // Pasif ability
    [SerializeField] private float manaCost = 0f;
    [SerializeField] private int requiredStacksForArea = 5;
    [SerializeField] private float areaDamage = 20f;
    [SerializeField] private float areaRadius = 5f;
    [SerializeField] private float areaDuration = 5f;
    [SerializeField] private float damageInterval = 1f; // Hasar uygulama aralığı - 1 saniyeye çıkarıldı
    [SerializeField] private ElementType targetElementType = ElementType.Fire; // Bu area hangi element için
    
    private IElement currentElement;
    private ElementalAbilityData abilityData;
    private bool isActive = true;
    private Coroutine currentAreaCoroutine; // Coroutine referansını sakla
    private bool isDestroyed = false; // Destroy edildiğini takip et
    private bool isInitialized = false; // Initialize edildiğini takip et
    
    private void Start()
    {
        if (isDestroyed) return;
        
        // Düşman ölüm event'lerini dinle
        if (EventManager.Instance != null)
        {
            try
            {
                EventManager.Instance.OnEnemyDeath += OnEnemyDeath;
                isInitialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ElementalArea] Failed to subscribe to enemy death event: {e.Message}");
            }
        }
    }
    
    private void OnDestroy()
    {
        isDestroyed = true;
        
        // Event listener'ı temizle
        if (EventManager.Instance != null && isInitialized)
        {
            try
            {
                EventManager.Instance.OnEnemyDeath -= OnEnemyDeath;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ElementalArea] Failed to unsubscribe from enemy death event: {e.Message}");
            }
        }
        
        // Aktif coroutine'i durdur
        StopAreaCoroutine();
    }
    
    private void OnDisable()
    {
        // Aktif coroutine'i durdur
        StopAreaCoroutine();
    }
    
    /// <summary>
    /// Güvenli şekilde coroutine'i durdur
    /// </summary>
    private void StopAreaCoroutine()
    {
        if (currentAreaCoroutine != null && !isDestroyed)
        {
            try
            {
                StopCoroutine(currentAreaCoroutine);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ElementalArea] Failed to stop coroutine: {e.Message}");
            }
            currentAreaCoroutine = null;
        }
    }
    
    // IAbility Interface Implementation
    public string AbilityName => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public float CooldownDuration => cooldownDuration;
    public float ManaCost => manaCost;
    
    /// <summary>
    /// Ability'yi ElementalAbilityData ile başlatır
    /// </summary>
    /// <param name="data">Ability verileri</param>
    public void Initialize(ElementalAbilityData data)
    {
        if (isDestroyed) return;
        
        try
        {
            abilityData = data;
            abilityName = data.abilityName;
            description = data.description;
            icon = data.icon;
            cooldownDuration = data.cooldownDuration;
            manaCost = data.manaCost;
            requiredStacksForArea = data.requiredStacksForArea;
            areaDamage = data.areaDamage;
            areaRadius = data.areaRadius;
            areaDuration = data.areaDuration;
            targetElementType = data.elementType; // Element tipini ayarla
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalArea] Initialize failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Ability'yi kullanır (pasif ability olduğu için sadece element ayarlar)
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="element">Kullanılacak element</param>
    public void UseAbility(GameObject caster, GameObject target, IElement element)
    {
        if (isDestroyed) return;
        currentElement = element;
    }
    
    /// <summary>
    /// Ability'nin kullanılıp kullanılamayacağını kontrol eder
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Kullanılabilir mi?</returns>
    public bool CanUseAbility(GameObject caster)
    {
        return isActive && !isDestroyed && isInitialized;
    }
    
    /// <summary>
    /// Ability'nin cooldown progress'ini döndürür
    /// </summary>
    /// <returns>0-1 arası progress değeri</returns>
    public float GetCooldownProgress()
    {
        return 0f; // Pasif ability olduğu için cooldown yok
    }
    
    /// <summary>
    /// Düşman öldüğünde alan hasarı kontrol eder ve uygular
    /// </summary>
    /// <param name="deadEnemy">Ölen düşman GameObject</param>
    public void OnEnemyDeath(GameObject deadEnemy)
    {
        // Erken çıkış kontrolleri
        if (!isActive || isDestroyed || !isInitialized || deadEnemy == null) return;
        if (deadEnemy.transform == null) return;
        if (gameObject == null || !gameObject.activeInHierarchy) return;
        
        try
        {
            // Düşmanın element stack'lerini kontrol et
            var elementStack = deadEnemy.GetComponent<ElementStack>();
            if (elementStack == null) return;
            
            // Sadece bu area'nın hedef elementinin stack'ini kontrol et
            int targetElementStacks = elementStack.GetElementStack(targetElementType);
            
            // Gerekli stack miktarı var mı kontrol et
            if (targetElementStacks >= requiredStacksForArea)
            {
                // Tetikleyen elementi ayarla
                if (currentElement == null)
                {
                    // Eğer currentElement null ise, tetikleyen elementi kullan
                    InitializeElementForType(targetElementType);
                }
                
                // Güvenli şekilde pozisyon al
                Vector3 position = deadEnemy.transform.position;
                CreateAreaDamage(position);
                
                Debug.Log($"💥 {targetElementType} ElementalArea triggered! {targetElementStacks} stacks on {deadEnemy.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalArea] OnEnemyDeath failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Alan hasarı oluşturur
    /// </summary>
    /// <param name="centerPosition">Merkez pozisyon</param>
    private void CreateAreaDamage(Vector3 centerPosition)
    {
        // Erken çıkış kontrolleri
        if (isDestroyed || gameObject == null || !gameObject.activeInHierarchy) return;
        if (currentElement == null) return;
        
        try
        {
            // Önceki coroutine'i durdur
            StopAreaCoroutine();
            
            // Alan hasarı uygula (coroutine'i güvenli şekilde başlat)
            if (gameObject != null && gameObject.activeInHierarchy && !isDestroyed)
            {
                try
                {
                    currentAreaCoroutine = StartCoroutine(ApplyAreaDamageOverTime(centerPosition));
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ElementalArea] Coroutine start failed: {e.Message}");
                    currentAreaCoroutine = null;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalArea] CreateAreaDamage failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Zaman içinde alan hasarı uygular
    /// </summary>
    /// <param name="centerPosition">Merkez pozisyon</param>
    private System.Collections.IEnumerator ApplyAreaDamageOverTime(Vector3 centerPosition)
    {
        if (isDestroyed) yield break;
        
        float elapsedTime = 0f;
        HashSet<GameObject> enemiesHit = new HashSet<GameObject>(); // Hangi düşmanlara stack eklendiğini takip et
        
        while (elapsedTime < areaDuration && !isDestroyed && gameObject != null && gameObject.activeInHierarchy)
        {
            try
            {
                // Alan içindeki düşmanları bul
                Collider2D[] colliders = Physics2D.OverlapCircleAll(centerPosition, areaRadius);
                
                foreach (var collider in colliders)
                {
                    if (collider != null && collider.CompareTag("Enemy") && !isDestroyed && currentElement != null)
                    {
                        var health = collider.GetComponent<IHealth>();
                        if (health != null)
                        {
                            // Hasar uygula (sabit hasar)
                            health.TakeDamage(areaDamage);
                            
                            // Element stack ekleme kaldırıldı - sadece hasar ver
                            // Stack sadece mermi çarptığında eklenmeli
                            enemiesHit.Add(collider.gameObject);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ElementalArea] Area damage application failed: {e.Message}");
                break;
            }
            
            elapsedTime += damageInterval;
            yield return new WaitForSeconds(damageInterval); // damageInterval kadar bekle
        }
        
        currentAreaCoroutine = null;
    }
    
    /// <summary>
    /// Mevcut elementi ayarlar
    /// </summary>
    /// <param name="element">Yeni element</param>
    public void SetElement(IElement element)
    {
        if (isDestroyed) return;
        currentElement = element;
    }
    
    /// <summary>
    /// Mevcut elementi döndürür
    /// </summary>
    /// <returns>Mevcut element</returns>
    public IElement GetCurrentElement()
    {
        return currentElement;
    }
    
    /// <summary>
    /// Belirtilen element tipine göre elementi initialize eder
    /// </summary>
    /// <param name="elementType">Element tipi</param>
    private void InitializeElementForType(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                currentElement = new FireElement();
                break;
            case ElementType.Ice:
                currentElement = new IceElement();
                break;
            case ElementType.Poison:
                currentElement = new PoisonElement();
                break;
            default:
                Debug.LogWarning($"[ElementalArea] Unknown element type: {elementType}");
                break;
        }
    }
    
    /// <summary>
    /// Ability'nin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <returns>Aktif mi?</returns>
    public bool IsActive()
    {
        return isActive && !isDestroyed && isInitialized;
    }
    
    /// <summary>
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            StopAreaCoroutine();
        }
    }
    
    /// <summary>
    /// Bu area'nın hangi element için olduğunu döndürür
    /// </summary>
    /// <returns>Element tipi</returns>
    public ElementType GetTargetElementType()
    {
        return targetElementType;
    }
} 