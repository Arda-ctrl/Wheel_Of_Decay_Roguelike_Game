using UnityEngine;

/// <summary>
/// WindElement - Rüzgar elementi
/// Hızlı hasar verir ve düşmanları itebilir
/// </summary>
public class WindElement : IElement
{
    public ElementType ElementType => ElementType.Wind;
    public string ElementName => "Wind";
    public Color ElementColor => Color.cyan;
    
    [Header("Wind Element Settings")]
    private float windDamagePerStack = 6f;
    private float windTickRate = 0.8f; // Her 0.8 saniye hasar
    private float windDuration = 3f;
    
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
            Debug.Log($"💨 Applied {amount} Wind stack to {target.name}");
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
            Debug.Log($"💨 Removed {amount} Wind stack from {target.name}");
        }
    }
    
    /// <summary>
    /// Element efektini çalıştırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Mevcut stack sayısı</param>
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Rüzgar efekti uygula
        StartWindEffect(target, stackCount);
        
        // VFX ve SFX oynat
        PlayWindEffects(target);
    }
    
    /// <summary>
    /// Rüzgar efektini başlatır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void StartWindEffect(GameObject target, int stackCount)
    {
        // Mevcut rüzgar efektini kontrol et
        var existingWind = target.GetComponent<ElementalWindEffect>();
        if (existingWind != null)
        {
            // Mevcut efekti güncelle
            existingWind.UpdateWindEffect(stackCount);
        }
        else
        {
            // Yeni rüzgar efekti ekle
            var windEffect = target.AddComponent<ElementalWindEffect>();
            windEffect.Initialize(stackCount, windDamagePerStack, windTickRate, windDuration);
        }
    }
    
    /// <summary>
    /// Rüzgar efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayWindEffects(GameObject target)
    {
        // VFX oynat
        if (target.GetComponent<ElementalWindEffect>() != null)
        {
            // Rüzgar particle effect'i oynat
            var windVFX = Resources.Load<GameObject>("Prefabs/Effects/WindVFX");
            if (windVFX != null)
            {
                GameObject vfxInstance = Object.Instantiate(windVFX, target.transform.position, Quaternion.identity);
                vfxInstance.transform.SetParent(target.transform);
            }
        }
        
        // SFX oynat
        AudioManager.Instance?.PlaySFX(20);
    }
    
    /// <summary>
    /// Element kombinasyonunu kontrol eder
    /// </summary>
    /// <param name="otherElement">Diğer element</param>
    /// <param name="target">Hedef GameObject</param>
    public void CheckElementCombination(IElement otherElement, GameObject target)
    {
        if (otherElement.ElementType == ElementType.Fire)
        {
            // Rüzgar + Ateş = Alev fırtınası
            ApplyFlameStorm(target);
        }
        else if (otherElement.ElementType == ElementType.Ice)
        {
            // Rüzgar + Buz = Buz fırtınası
            ApplyIceStorm(target);
        }
    }
    
    /// <summary>
    /// Alev fırtınası uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyFlameStorm(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(45f);
        }
        
        Debug.Log($"💨 Flame storm applied to {target.name}");
    }
    
    /// <summary>
    /// Buz fırtınası uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyIceStorm(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(35f);
        }
        
        Debug.Log($"💨 Ice storm applied to {target.name}");
    }
}

/// <summary>
/// WindEffect - Rüzgar efektini yönetir
/// </summary>
public class ElementalWindEffect : MonoBehaviour
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
    
    public void UpdateWindEffect(int newStackCount)
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
            ApplyWindDamage();
            lastTickTime = Time.time;
        }
    }
    
    private void ApplyWindDamage()
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
            numberInstance.GetComponent<DamageNumberUI>()?.SetDamage(damage, Color.cyan);
        }
    }
    
    private void OnDestroy()
    {
        // Rüzgar efekti bittiğinde VFX'i temizle
        var windVFX = transform.Find("WindVFX(Clone)");
        if (windVFX != null)
        {
            Destroy(windVFX.gameObject);
        }
    }
} 