using UnityEngine;

/// <summary>
/// EarthElement - Toprak elementi
/// Fiziksel hasar verir ve düşmanları yavaşlatır
/// </summary>
public class EarthElement : IElement
{
    public ElementType ElementType => ElementType.Earth;
    public string ElementName => "Earth";
    public Color ElementColor => new Color(0.6f, 0.4f, 0.2f); // Brown color
    
    [Header("Earth Element Settings")]
    private float crushDamagePerStack = 10f;
    private float crushTickRate = 1.5f; // Her 1.5 saniye hasar
    private float crushDuration = 5f;
    
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
            Debug.Log($"🌍 Applied {amount} Earth stack to {target.name}");
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
            Debug.Log($"🌍 Removed {amount} Earth stack from {target.name}");
        }
    }
    
    /// <summary>
    /// Element efektini çalıştırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Mevcut stack sayısı</param>
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Toprak efekti uygula
        StartCrushEffect(target, stackCount);
        
        // VFX ve SFX oynat
        PlayEarthEffects(target);
    }
    
    /// <summary>
    /// Toprak efektini başlatır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void StartCrushEffect(GameObject target, int stackCount)
    {
        // Mevcut toprak efektini kontrol et
        var existingCrush = target.GetComponent<ElementalEarthCrushEffect>();
        if (existingCrush != null)
        {
            // Mevcut efekti güncelle
            existingCrush.UpdateCrushEffect(stackCount);
        }
        else
        {
            // Yeni toprak efekti ekle
            var crushEffect = target.AddComponent<ElementalEarthCrushEffect>();
            crushEffect.Initialize(stackCount, crushDamagePerStack, crushTickRate, crushDuration);
        }
    }
    
    /// <summary>
    /// Toprak efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayEarthEffects(GameObject target)
    {
        // VFX oynat
        if (target.GetComponent<ElementalEarthCrushEffect>() != null)
        {
            // Toprak particle effect'i oynat
            var earthVFX = Resources.Load<GameObject>("Prefabs/Effects/EarthCrushVFX");
            if (earthVFX != null)
            {
                GameObject vfxInstance = Object.Instantiate(earthVFX, target.transform.position, Quaternion.identity);
                vfxInstance.transform.SetParent(target.transform);
            }
        }
        
        // SFX oynat
        AudioManager.Instance?.PlaySFX(19);
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
            // Toprak + Ateş = Lav patlaması
            ApplyLavaExplosion(target);
        }
        else if (otherElement.ElementType == ElementType.Ice)
        {
            // Toprak + Buz = Çamur tuzağı
            ApplyMudTrap(target);
        }
    }
    
    /// <summary>
    /// Lav patlaması uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyLavaExplosion(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(60f);
        }
        
        Debug.Log($"🌍 Lava explosion applied to {target.name}");
    }
    
    /// <summary>
    /// Çamur tuzağı uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyMudTrap(GameObject target)
    {
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(30f);
        }
        
        Debug.Log($"🌍 Mud trap applied to {target.name}");
    }
}

/// <summary>
/// EarthCrushEffect - Toprak ezme efektini yönetir
/// </summary>
public class ElementalEarthCrushEffect : MonoBehaviour
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
    
    public void UpdateCrushEffect(int newStackCount)
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
            ApplyCrushDamage();
            lastTickTime = Time.time;
        }
    }
    
    private void ApplyCrushDamage()
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
            numberInstance.GetComponent<DamageNumberUI>()?.SetDamage(damage, new Color(0.6f, 0.4f, 0.2f));
        }
    }
    
    private void OnDestroy()
    {
        // Toprak efekti bittiğinde VFX'i temizle
        var earthVFX = transform.Find("EarthCrushVFX(Clone)");
        if (earthVFX != null)
        {
            Destroy(earthVFX.gameObject);
        }
    }
} 