using UnityEngine;

/// <summary>
/// LightningElement - Şimşek elementi
/// Elektrik hasarı verir ve düşmanları sersemletir
/// </summary>
public class LightningElement : IElement
{
    public ElementType ElementType => ElementType.Lightning;
    public string ElementName => "Lightning";
    public Color ElementColor => Color.yellow;
    
    [Header("Lightning Element Settings")]
    private float shockDamagePerStack = 8f;
    private float shockTickRate = 0.5f; // Her 0.5 saniye hasar
    private float shockDuration = 4f;
    
    /// <summary>
    /// Element stack'ini hedefe uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="amount">Stack miktarı</param>
    public void ApplyElementStack(GameObject target, int amount)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.AddElementStack(ElementType, amount);
            Debug.Log($"⚡ Applied {amount} Lightning stack to {target.name}");
        }
    }
    
    /// <summary>
    /// Element stack'ini hedeften kaldırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="amount">Stack miktarı</param>
    public void RemoveElementStack(GameObject target, int amount)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType, amount);
            Debug.Log($"⚡ Removed {amount} Lightning stack from {target.name}");
        }
    }
    
    /// <summary>
    /// Element efektini çalıştırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Mevcut stack sayısı</param>
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Şimşek efekti uygula
        StartShockEffect(target, stackCount);
        
        // VFX ve SFX oynat
        PlayLightningEffects(target);
    }
    
    /// <summary>
    /// Şimşek efektini başlatır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void StartShockEffect(GameObject target, int stackCount)
    {
        // Mevcut şimşek efektini kontrol et
        var existingShock = target.GetComponent<ElementalLightningShockEffect>();
        if (existingShock != null)
        {
            // Mevcut efekti güncelle
            existingShock.UpdateShockEffect(stackCount);
        }
        else
        {
            // Yeni şimşek efekti ekle
            var shockEffect = target.AddComponent<ElementalLightningShockEffect>();
            shockEffect.Initialize(stackCount, shockDamagePerStack, shockTickRate, shockDuration);
        }
    }
    
    /// <summary>
    /// Şimşek efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayLightningEffects(GameObject target)
    {
        // VFX oynat
        if (target.GetComponent<ElementalLightningShockEffect>() != null)
        {
            // Şimşek particle effect'i oynat
            var lightningVFX = Resources.Load<GameObject>("Prefabs/Effects/LightningShockVFX");
            if (lightningVFX != null)
            {
                GameObject vfxInstance = Object.Instantiate(lightningVFX, target.transform.position, Quaternion.identity);
                vfxInstance.transform.SetParent(target.transform);
            }
        }
        
        // SFX oynat
        AudioManager.Instance?.PlaySFX(18);
    }
    
    /// <summary>
    /// Element kombinasyonunu kontrol eder
    /// </summary>
    /// <param name="otherElement">Diğer element</param>
    /// <param name="target">Hedef GameObject</param>
    public void CheckElementCombination(IElement otherElement, GameObject target)
    {
        if (otherElement.ElementType == ElementType.Ice)
        {
            // Şimşek + Buz = Elektrik şoku
            ApplyElectricShock(target);
        }
        else if (otherElement.ElementType == ElementType.Fire)
        {
            // Şimşek + Ateş = Plazma patlaması
            ApplyPlasmaExplosion(target);
        }
    }
    
    /// <summary>
    /// Elektrik şoku uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyElectricShock(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(50f);
        }
        
        Debug.Log($"⚡ Electric shock applied to {target.name}");
    }
    
    /// <summary>
    /// Plazma patlaması uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyPlasmaExplosion(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(75f);
        }
        
        Debug.Log($"⚡ Plasma explosion applied to {target.name}");
    }
}

/// <summary>
/// LightningShockEffect - Şimşek şok efektini yönetir
/// </summary>
public class ElementalLightningShockEffect : MonoBehaviour
{
    private int stackCount;
    private float damagePerStack;
    private float tickRate;
    private float duration;
    private float lastTickTime;
    private float elapsedTime;
    
    public void Initialize(int stacks, float damage, float tickRate, float duration)
    {
        this.stackCount = stacks;
        this.damagePerStack = damage;
        this.tickRate = tickRate;
        this.duration = duration;
        this.lastTickTime = 0f;
        this.elapsedTime = 0f;
    }
    
    public void UpdateShockEffect(int newStackCount)
    {
        this.stackCount = newStackCount;
        this.elapsedTime = 0f; // Süreyi sıfırla
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // Süre doldu mu kontrol et
        if (elapsedTime >= duration)
        {
            Destroy(this);
            return;
        }
        
        // Tick zamanı geldi mi kontrol et
        if (Time.time - lastTickTime >= tickRate)
        {
            ApplyShockDamage();
            lastTickTime = Time.time;
        }
    }
    
    private void ApplyShockDamage()
    {
        float totalDamage = damagePerStack * stackCount;
        
        // Hedefin health component'ine hasar ver
        var health = GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(totalDamage);
            
            // Hasar sayısını göster
            ShowDamageNumber(totalDamage);
        }
    }
    
    private void ShowDamageNumber(float damage)
    {
        // Damage number UI'ı göster
        var damageNumber = Resources.Load<GameObject>("Prefabs/UI/DamageNumber");
        if (damageNumber != null)
        {
            GameObject numberInstance = Object.Instantiate(damageNumber, transform.position, Quaternion.identity);
            numberInstance.GetComponent<DamageNumberUI>()?.SetDamage(damage, Color.yellow);
        }
    }
    
    private void OnDestroy()
    {
        // Şimşek efekti bittiğinde VFX'i temizle
        var lightningVFX = transform.Find("LightningShockVFX(Clone)");
        if (lightningVFX != null)
        {
            Destroy(lightningVFX.gameObject);
        }
    }
} 