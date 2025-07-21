using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// FireElement - Ateş elementinin davranışlarını tanımlar
/// Ateş stack'leri hasar verir ve yanma efekti uygular
/// </summary>
public class FireElement : IElement
{
    public string ElementName => "Fire";
    public ElementType ElementType => ElementType.Fire;
    public Color ElementColor => Color.red;
    
    [Header("Fire Element Settings")]
    private float burnDamagePerStack = 5f;
    private float burnTickRate = 1f; // Her saniye hasar
    private float burnDuration = 3f;
    
    public void ApplyElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.AddElementStack(ElementType.Fire, stackAmount);
        }
    }
    
    public void RemoveElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType.Fire, stackAmount);
        }
    }
    
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Yanma efekti uygula
        StartBurnEffect(target, stackCount);
        
        // VFX ve SFX oynat
        PlayFireEffects(target);
    }
    
    /// <summary>
    /// Yanma efektini başlatır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void StartBurnEffect(GameObject target, int stackCount)
    {
        // Mevcut yanma efektini kontrol et
        var existingBurn = target.GetComponent<ElementalFireBurnEffect>();
        if (existingBurn != null)
        {
            // Mevcut efekti güncelle
            // Artık stackCount ne olursa olsun, yanma sabit kalsın
            existingBurn.UpdateBurnEffect(1); // Sadece 1 stack gibi davran
        }
        else
        {
            // Yeni yanma efekti ekle
            var burnEffect = target.AddComponent<ElementalFireBurnEffect>();
            burnEffect.Initialize(1, burnDamagePerStack, burnTickRate, burnDuration); // Sadece 1 stack gibi davran
        }
    }
    
    /// <summary>
    /// Ateş efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayFireEffects(GameObject target)
    {
        if (target == null) return;
        
        try
        {
            // VFX oynat
            if (target.GetComponent<ElementalFireBurnEffect>() != null)
            {
                // Ateş particle effect'i oynat
                var fireVFX = Resources.Load<GameObject>("Prefabs/Effects/FireBurnVFX");
                if (fireVFX != null)
                {
                    GameObject vfxInstance = Object.Instantiate(fireVFX, target.transform.position, Quaternion.identity);
                    if (vfxInstance != null)
                    {
                        vfxInstance.transform.SetParent(target.transform);
                    }
                }
            }
            
            // SFX oynat
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(17);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FireElement] PlayFireEffects failed: {e.Message}");
        }
    }
}

/// <summary>
/// FireBurnEffect - Ateş yanma efektini yönetir (Elemental sistem için)
/// </summary>
public class ElementalFireBurnEffect : MonoBehaviour
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
    
    public void UpdateBurnEffect(int newStackCount)
    {
        this.stackCount = newStackCount;
        this.elapsedTime = 0f; // Süreyi sıfırla
    }
    
    private void Update()
    {
        try
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
                ApplyBurnDamage();
                lastTickTime = Time.time;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalFireBurnEffect] Update failed: {e.Message}");
            Destroy(this);
        }
    }
    
    private void ApplyBurnDamage()
    {
        try
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
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalFireBurnEffect] ApplyBurnDamage failed: {e.Message}");
        }
    }
    
    private void ShowDamageNumber(float damage)
    {
        try
        {
            // Damage number UI'ı göster
            var damageNumber = Resources.Load<GameObject>("Prefabs/UI/DamageNumber");
            if (damageNumber != null)
            {
                GameObject numberInstance = Object.Instantiate(damageNumber, transform.position, Quaternion.identity);
                if (numberInstance != null)
                {
                    var damageUI = numberInstance.GetComponent<DamageNumberUI>();
                    if (damageUI != null)
                    {
                        damageUI.SetDamage(damage, Color.red);
                    }
                }
            }
            else
            {
                // Eğer prefab yoksa, runtime'da oluştur
                CreateDamageNumber(damage, Color.red);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalFireBurnEffect] ShowDamageNumber failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Runtime'da damage number oluşturur
    /// </summary>
    /// <param name="damage">Hasar miktarı</param>
    /// <param name="color">Hasar rengi</param>
    private void CreateDamageNumber(float damage, Color color)
    {
        try
        {
            // Canvas'ı bul
            Canvas canvas = FindObjectOfType<Canvas>();
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
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
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
            Debug.LogWarning($"[ElementalFireBurnEffect] CreateDamageNumber failed: {e.Message}");
        }
    }
    
    private void OnDestroy()
    {
        try
        {
            // Yanma efekti bittiğinde VFX'i temizle
            var fireVFX = transform.Find("FireBurnVFX(Clone)");
            if (fireVFX != null)
            {
                Destroy(fireVFX.gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalFireBurnEffect] OnDestroy failed: {e.Message}");
        }
    }
} 