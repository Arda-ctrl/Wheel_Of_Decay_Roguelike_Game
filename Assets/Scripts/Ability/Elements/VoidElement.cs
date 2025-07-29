using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// VoidElement - Boşluk/Gölge elementinin davranışlarını tanımlar
/// Void stack'leri hedefin görüş alanını ve menzilini azaltır
/// </summary>
public class VoidElement : IElement
{
    public string ElementName => "Void";
    public ElementType ElementType => ElementType.Void;
    public Color ElementColor => new Color(0.2f, 0.1f, 0.3f); // Dark purple
    
    [Header("Void Element Settings")]
    private float visionReductionPerStack = 0.15f; // Her stack %15 görüş alanı azaltması
    private float rangeReductionPerStack = 0.1f; // Her stack %10 menzil azaltması
    private float voidDamage = 3f; // Stack başına hasar
    private float voidEffectDuration = 4f; // Efekt süresi
    
    public void ApplyElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.AddElementStack(ElementType.Void, stackAmount);
        }
    }
    
    public void RemoveElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType.Void, stackAmount);
        }
    }
    
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Void hasar ver
        ApplyVoidDamage(target, stackCount);
        
        // Görüş ve menzil azaltma efekti uygula
        ApplyVoidDebuffEffect(target, stackCount);
        
        // VFX ve SFX oynat
        PlayVoidEffects(target);
    }
    
    /// <summary>
    /// Void hasar verir
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void ApplyVoidDamage(GameObject target, int stackCount)
    {
        float totalDamage = voidDamage * stackCount;
        
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(totalDamage);
            ShowDamageNumber(target, totalDamage);
        }
    }
    
    /// <summary>
    /// Görüş ve menzil azaltma efektini uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void ApplyVoidDebuffEffect(GameObject target, int stackCount)
    {
        // Mevcut void efektini kontrol et
        var existingVoid = target.GetComponent<ElementalVoidDebuffEffect>();
        if (existingVoid == null)
        {
            // Yeni void efekti ekle
            var voidEffect = target.AddComponent<ElementalVoidDebuffEffect>();
            voidEffect.Initialize(stackCount, visionReductionPerStack, rangeReductionPerStack, voidEffectDuration);
        }
        else
        {
            // Mevcut efekti güncelle
            existingVoid.UpdateVoidEffect(stackCount);
        }
    }
    
    /// <summary>
    /// Void efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayVoidEffects(GameObject target)
    {
        if (target == null) return;
        
        try
        {
            // VFX oynat
            var voidVFX = Resources.Load<GameObject>("Prefabs/Effects/VoidVFX");
            if (voidVFX != null)
            {
                GameObject vfxInstance = Object.Instantiate(voidVFX, target.transform.position, Quaternion.identity);
                if (vfxInstance != null)
                {
                    vfxInstance.transform.SetParent(target.transform);
                }
            }
            
            // SFX oynat
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(27); // Void sound effect
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[VoidElement] PlayVoidEffects failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Hasar sayısını gösterir
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="damage">Hasar miktarı</param>
    private void ShowDamageNumber(GameObject target, float damage)
    {
        try
        {
            // Damage number UI'ı göster
            var damageNumber = Resources.Load<GameObject>("Prefabs/UI/DamageNumber");
            if (damageNumber != null)
            {
                GameObject numberInstance = Object.Instantiate(damageNumber, target.transform.position, Quaternion.identity);
                if (numberInstance != null)
                {
                    var damageUI = numberInstance.GetComponent<DamageNumberUI>();
                    if (damageUI != null)
                    {
                        damageUI.SetDamage(damage, new Color(0.2f, 0.1f, 0.3f)); // Dark purple
                    }
                }
            }
            else
            {
                // Eğer prefab yoksa, runtime'da oluştur
                CreateDamageNumber(target, damage, new Color(0.2f, 0.1f, 0.3f));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[VoidElement] ShowDamageNumber failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Runtime'da damage number oluşturur
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="damage">Hasar miktarı</param>
    /// <param name="color">Hasar rengi</param>
    private void CreateDamageNumber(GameObject target, float damage, Color color)
    {
        try
        {
            // Canvas'ı bul
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                // Eğer canvas yoksa, yeni bir canvas oluştur
                GameObject canvasObj = new GameObject("DamageCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Damage number GameObject'i oluştur
            GameObject damageNumberObj = new GameObject("DamageNumber");
            damageNumberObj.transform.SetParent(canvas.transform, false);
            
            // TMP_Text component'i ekle
            var tmpText = damageNumberObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = damage.ToString("F0");
            tmpText.color = color;
            tmpText.fontSize = 24f;
            tmpText.alignment = TextAlignmentOptions.Center;
            
            // DamageNumberUI component'i ekle
            var damageNumberUI = damageNumberObj.AddComponent<DamageNumberUI>();
            
            // Pozisyonu ayarla
            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
                damageNumberObj.transform.position = screenPos;
            }
            
            // Animasyonu başlat
            if (damageNumberUI != null)
            {
                damageNumberUI.SetDamage(damage, color);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[VoidElement] CreateDamageNumber failed: {e.Message}");
        }
    }
}

/// <summary>
/// VoidDebuffEffect - Void görüş ve menzil azaltma efektini yönetir (Elemental sistem için)
/// </summary>
public class ElementalVoidDebuffEffect : MonoBehaviour
{
    private int stackCount;
    private float visionReductionPerStack;
    private float rangeReductionPerStack;
    private float duration;
    private float elapsedTime;
    private bool isActive;
    
    // Orijinal değerler (geri yüklemek için)
    private float originalVisionRange;
    private float originalAttackRange;
    
    public void Initialize(int stacks, float visionReduction, float rangeReduction, float effectDuration)
    {
        this.stackCount = stacks;
        this.visionReductionPerStack = visionReduction;
        this.rangeReductionPerStack = rangeReduction;
        this.duration = effectDuration;
        this.elapsedTime = 0f;
        this.isActive = false;
        
        StartVoidEffect();
    }
    
    public void UpdateVoidEffect(int newStackCount)
    {
        this.stackCount = newStackCount;
        this.elapsedTime = 0f; // Süreyi sıfırla
        
        // Efekti yeniden uygula
        if (isActive)
        {
            ApplyVoidDebuff();
        }
    }
    
    private void StartVoidEffect()
    {
        isActive = true;
        
        // Orijinal değerleri kaydet
        StoreOriginalValues();
        
        // Debuff'ı uygula
        ApplyVoidDebuff();
        
        // Void VFX'i oynat
        var voidVFX = Resources.Load<GameObject>("Prefabs/Effects/VoidDebuffVFX");
        if (voidVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(voidVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
        }
        
        // Void SFX'i oynat
        AudioManager.Instance?.PlaySFX(28);
    }
    
    private void StoreOriginalValues()
    {
        // Enemy AI componentlerini kontrol et ve orijinal değerleri kaydet
        // Bu kısım enemy AI sisteminize göre değişebilir
        
        // Örnek: Enemy Controller bileşeni varsa
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            // EnemyController'dan orijinal değerleri al
            // originalVisionRange = enemyController.visionRange;
            // originalAttackRange = enemyController.attackRange;
            
            // Geçici olarak varsayılan değerler
            originalVisionRange = 10f;
            originalAttackRange = 8f;
        }
        else
        {
            // Varsayılan değerler
            originalVisionRange = 10f;
            originalAttackRange = 8f;
        }
    }
    
    private void ApplyVoidDebuff()
    {
        // Görüş alanını azalt
        float visionReduction = Mathf.Min(visionReductionPerStack * stackCount, 0.8f); // Maksimum %80 azaltma
        float newVisionRange = originalVisionRange * (1f - visionReduction);
        
        // Menzili azalt
        float rangeReduction = Mathf.Min(rangeReductionPerStack * stackCount, 0.6f); // Maksimum %60 azaltma
        float newAttackRange = originalAttackRange * (1f - rangeReduction);
        
        // Enemy AI bileşenlerine yeni değerleri uygula
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            // EnemyController'a yeni değerleri ata
            // enemyController.visionRange = newVisionRange;
            // enemyController.attackRange = newAttackRange;
            
            Debug.Log($"[VoidElement] Applied debuff to {gameObject.name}: Vision reduced by {visionReduction:P0}, Range reduced by {rangeReduction:P0}");
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= duration)
        {
            EndVoidEffect();
        }
    }
    
    private void EndVoidEffect()
    {
        isActive = false;
        
        // Orijinal değerleri geri yükle
        RestoreOriginalValues();
        
        // Void VFX'ini temizle
        var voidVFX = transform.Find("VoidDebuffVFX(Clone)");
        if (voidVFX != null)
        {
            Destroy(voidVFX.gameObject);
        }
        
        Destroy(this);
    }
    
    private void RestoreOriginalValues()
    {
        // Orijinal değerleri geri yükle
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            // enemyController.visionRange = originalVisionRange;
            // enemyController.attackRange = originalAttackRange;
            
            Debug.Log($"[VoidElement] Restored original values for {gameObject.name}");
        }
    }
    
    private void OnDestroy()
    {
        try
        {
            // Void efekti bittiğinde VFX'i temizle
            var voidVFX = transform.Find("VoidDebuffVFX(Clone)");
            if (voidVFX != null)
            {
                Destroy(voidVFX.gameObject);
            }
            
            // Orijinal değerleri geri yükle
            if (isActive)
            {
                RestoreOriginalValues();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalVoidDebuffEffect] OnDestroy failed: {e.Message}");
        }
    }
} 