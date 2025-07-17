using UnityEngine;

/// <summary>
/// IceElement - Buz elementinin davranışlarını tanımlar
/// Buz stack'leri yavaşlatır ve donma efekti uygular
/// </summary>
public class IceElement : IElement
{
    public string ElementName => "Ice";
    public ElementType ElementType => ElementType.Ice;
    public Color ElementColor => Color.cyan;
    
    [Header("Ice Element Settings")]
    private float slowAmountPerStack = 0.1f; // Her stack %10 yavaşlatır
    private float freezeThreshold = 3; // 3 stack'te donma
    private float freezeDuration = 2f;
    
    public void ApplyElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.AddElementStack(ElementType.Ice, stackAmount);
        }
    }
    
    public void RemoveElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType.Ice, stackAmount);
        }
    }
    
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Yavaşlatma efekti uygula
        ApplySlowEffect(target, stackCount);
        
        // Donma kontrolü
        if (stackCount >= freezeThreshold)
        {
            ApplyFreezeEffect(target);
        }
        
        // VFX ve SFX oynat
        PlayIceEffects(target);
    }
    
    /// <summary>
    /// Yavaşlatma efektini uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void ApplySlowEffect(GameObject target, int stackCount)
    {
        var moveable = target.GetComponent<IMoveable>();
        if (moveable != null)
        {
            float slowAmount = Mathf.Min(slowAmountPerStack * stackCount, 0.8f); // Maksimum %80 yavaşlatma
            moveable.SetSpeedMultiplier(1f - slowAmount);
        }
    }
    
    /// <summary>
    /// Donma efektini uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyFreezeEffect(GameObject target)
    {
        // Mevcut donma efektini kontrol et
        var existingFreeze = target.GetComponent<ElementalIceFreezeEffect>();
        if (existingFreeze == null)
        {
            // Yeni donma efekti ekle
            var freezeEffect = target.AddComponent<ElementalIceFreezeEffect>();
            freezeEffect.Initialize(freezeDuration);
        }
        else
        {
            // Mevcut efekti yenile
            existingFreeze.RefreshFreeze();
        }
    }
    
    /// <summary>
    /// Buz efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayIceEffects(GameObject target)
    {
        // VFX oynat
        var iceVFX = Resources.Load<GameObject>("Prefabs/Effects/IceSlowVFX");
        if (iceVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(iceVFX, target.transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(target.transform);
        }
        
        // SFX oynat
        AudioManager.Instance?.PlaySFX(13);
    }
}

/// <summary>
/// IceFreezeEffect - Buz donma efektini yönetir (Elemental sistem için)
/// </summary>
public class ElementalIceFreezeEffect : MonoBehaviour
{
    private float duration;
    private float elapsedTime;
    private bool isFrozen;
    
    public void Initialize(float freezeDuration)
    {
        this.duration = freezeDuration;
        this.elapsedTime = 0f;
        this.isFrozen = false;
        
        StartFreeze();
    }
    
    public void RefreshFreeze()
    {
        this.elapsedTime = 0f;
        if (!isFrozen)
        {
            StartFreeze();
        }
    }
    
    private void StartFreeze()
    {
        isFrozen = true;
        
        // Hareketi durdur
        var moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(0f);
        }
        
        // Animasyonu durdur
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0f;
        }
        
        // Donma VFX'i oynat
        var freezeVFX = Resources.Load<GameObject>("Prefabs/Effects/IceFreezeVFX");
        if (freezeVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(freezeVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
        }
        
        // Donma SFX'i oynat
        AudioManager.Instance?.PlaySFX(14);
    }
    
    private void Update()
    {
        if (!isFrozen) return;
        
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= duration)
        {
            EndFreeze();
        }
    }
    
    private void EndFreeze()
    {
        isFrozen = false;
        
        // Hareketi geri yükle
        var moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(1f);
        }
        
        // Animasyonu geri yükle
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 1f;
        }
        
        // Donma VFX'ini temizle
        var freezeVFX = transform.Find("IceFreezeVFX(Clone)");
        if (freezeVFX != null)
        {
            Destroy(freezeVFX.gameObject);
        }
        
        // Donma çözülme SFX'i oynat
        AudioManager.Instance?.PlaySFX(16);
        
        Destroy(this);
    }
} 