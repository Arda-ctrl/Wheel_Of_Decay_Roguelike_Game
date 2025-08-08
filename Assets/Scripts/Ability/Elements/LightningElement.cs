using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// LightningElement - Yıldırım elementinin davranışlarını tanımlar
/// Lightning stack'leri hedefe stun efekti uygular (%10 şans)
/// </summary>
public class LightningElement : IElement
{
    public string ElementName => "Lightning";
    public ElementType ElementType => ElementType.Lightning;
    public Color ElementColor => Color.yellow;
    
    [Header("Lightning Element Settings")]
    private float stunChance = 0.1f; // %10 stun şansı
    private float stunDuration = 1f; // 1 saniye stun
    private float lightningDamage = 8f; // Stack başına hasar
    
    public void ApplyElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.AddElementStack(ElementType.Lightning, stackAmount);
        }
    }
    
    public void RemoveElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType.Lightning, stackAmount);
        }
    }
    
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Lightning hasar ver
        ApplyLightningDamage(target, stackCount);
        
        // Stun şansını kontrol et
        float randomValue = Random.Range(0f, 1f);
        if (randomValue <= stunChance)
        {
            ApplyStunEffect(target);
        }
        
        // VFX ve SFX oynat
        PlayLightningEffects(target);
    }
    
    /// <summary>
    /// Lightning hasar verir
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void ApplyLightningDamage(GameObject target, int stackCount)
    {
        float totalDamage = lightningDamage * stackCount;
        
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(totalDamage);
            ShowDamageNumber(target, totalDamage);
        }
    }
    
    /// <summary>
    /// Stun efektini uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyStunEffect(GameObject target)
    {
        // Mevcut stun efektini kontrol et
        var existingStun = target.GetComponent<ElementalLightningStunEffect>();
        if (existingStun == null)
        {
            // Yeni stun efekti ekle
            var stunEffect = target.AddComponent<ElementalLightningStunEffect>();
            stunEffect.Initialize(stunDuration);
        }
        else
        {
            // Mevcut efekti yenile
            existingStun.RefreshStun();
        }
    }
    
    /// <summary>
    /// Lightning efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayLightningEffects(GameObject target)
    {
        if (target == null) return;
        
        try
        {
            // VFX oynat
            var lightningVFX = Resources.Load<GameObject>("Prefabs/Effects/LightningVFX");
            if (lightningVFX != null)
            {
                GameObject vfxInstance = Object.Instantiate(lightningVFX, target.transform.position, Quaternion.identity);
                if (vfxInstance != null)
                {
                    vfxInstance.transform.SetParent(target.transform);
                }
            }
            
            // SFX oynat
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(21); // Lightning sound effect
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LightningElement] PlayLightningEffects failed: {e.Message}");
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
                        damageUI.SetDamage(damage, Color.yellow);
                    }
                }
            }
            else
            {
                // Eğer prefab yoksa, runtime'da oluştur
                CreateDamageNumber(target, damage, Color.yellow);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LightningElement] ShowDamageNumber failed: {e.Message}");
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
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
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
            Debug.LogWarning($"[LightningElement] CreateDamageNumber failed: {e.Message}");
        }
    }
}

/// <summary>
/// LightningStunEffect - Lightning stun efektini yönetir (Elemental sistem için)
/// </summary>
public class ElementalLightningStunEffect : MonoBehaviour
{
    private float duration;
    private float elapsedTime;
    private bool isStunned;
    
    public void Initialize(float stunDuration)
    {
        this.duration = stunDuration;
        this.elapsedTime = 0f;
        this.isStunned = false;
        
        StartStun();
    }
    
    public void RefreshStun()
    {
        this.elapsedTime = 0f;
        if (!isStunned)
        {
            StartStun();
        }
    }
    
    private void StartStun()
    {
        isStunned = true;
        
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
        
        // Stun VFX'i oynat
        var stunVFX = Resources.Load<GameObject>("Prefabs/Effects/LightningStunVFX");
        if (stunVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(stunVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
        }
        
        // Stun SFX'i oynat
        AudioManager.Instance?.PlaySFX(22);
    }
    
    private void Update()
    {
        if (!isStunned) return;
        
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= duration)
        {
            EndStun();
        }
    }
    
    private void EndStun()
    {
        isStunned = false;
        
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
        
        // Stun VFX'ini temizle
        var stunVFX = transform.Find("LightningStunVFX(Clone)");
        if (stunVFX != null)
        {
            Destroy(stunVFX.gameObject);
        }
        
        Destroy(this);
    }
    
    private void OnDestroy()
    {
        try
        {
            // Stun efekti bittiğinde VFX'i temizle
            var stunVFX = transform.Find("LightningStunVFX(Clone)");
            if (stunVFX != null)
            {
                Destroy(stunVFX.gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalLightningStunEffect] OnDestroy failed: {e.Message}");
        }
    }
} 