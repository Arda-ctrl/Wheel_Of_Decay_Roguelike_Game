using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// PoisonElement - Zehir elementinin davranışlarını tanımlar
/// Zehir stack'leri sürekli hasar verir ve yavaşlatır
/// </summary>
public class PoisonElement : IElement
{
    public string ElementName => "Poison";
    public ElementType ElementType => ElementType.Poison;
    public Color ElementColor => Color.green;
    
    [Header("Poison Element Settings")]
    private float poisonDamagePerStack = 3f;
    private float poisonTickRate = 0.5f; // Her 0.5 saniyede hasar
    private float slowAmountPerStack = 0.05f; // Her stack %5 yavaşlatır
    private float maxSlowAmount = 0.6f; // Maksimum %60 yavaşlatma
    
    public void ApplyElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.AddElementStack(ElementType.Poison, stackAmount);
        }
    }
    
    public void RemoveElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType.Poison, stackAmount);
        }
    }
    
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Zehir efekti uygula
        StartPoisonEffect(target, stackCount);
        
        // VFX ve SFX oynat
        PlayPoisonEffects(target);
    }
    
    /// <summary>
    /// Zehir efektini başlatır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void StartPoisonEffect(GameObject target, int stackCount)
    {
        // Mevcut zehir efektini kontrol et
        var existingPoison = target.GetComponent<ElementalPoisonEffect>();
        if (existingPoison != null)
        {
            // Mevcut efekti güncelle
            existingPoison.UpdatePoisonEffect(stackCount);
        }
        else
        {
            // Yeni zehir efekti ekle
            var poisonEffect = target.AddComponent<ElementalPoisonEffect>();
            poisonEffect.Initialize(stackCount, poisonDamagePerStack, poisonTickRate, slowAmountPerStack, maxSlowAmount);
        }
    }
    
    /// <summary>
    /// Zehir efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayPoisonEffects(GameObject target)
    {
        // VFX oynat
        var poisonVFX = Resources.Load<GameObject>("Prefabs/Effects/PoisonVFX");
        if (poisonVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(poisonVFX, target.transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(target.transform);
        }
        
        // SFX oynat
        AudioManager.Instance?.PlaySFX(20);
    }
}

/// <summary>
/// PoisonEffect - Zehir efektini yönetir (Elemental sistem için)
/// </summary>
public class ElementalPoisonEffect : MonoBehaviour
{
    private int stackCount;
    private float damagePerStack;
    private float tickRate;
    private float slowAmountPerStack;
    private float maxSlowAmount;
    private float lastTickTime;
    private float elapsedTime;
    private float originalSpeedMultiplier = 1f;
    
    public void Initialize(int stacks, float damage, float tickRate, float slowAmount, float maxSlow)
    {
        this.stackCount = stacks;
        this.damagePerStack = damage;
        this.tickRate = tickRate;
        this.slowAmountPerStack = slowAmount;
        this.maxSlowAmount = maxSlow;
        this.lastTickTime = 0f;
        this.elapsedTime = 0f;
        
        ApplySlowEffect();
    }
    
    public void UpdatePoisonEffect(int newStackCount)
    {
        this.stackCount = newStackCount;
        this.elapsedTime = 0f; // Süreyi sıfırla
        
        ApplySlowEffect();
    }
    
    private void ApplySlowEffect()
    {
        var moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            float slowAmount = Mathf.Min(slowAmountPerStack * stackCount, maxSlowAmount);
            moveable.SetSpeedMultiplier(1f - slowAmount);
        }
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // Tick zamanı geldi mi kontrol et
        if (Time.time - lastTickTime >= tickRate)
        {
            ApplyPoisonDamage();
            lastTickTime = Time.time;
        }
    }
    
    private void ApplyPoisonDamage()
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
            numberInstance.GetComponent<DamageNumberUI>()?.SetDamage(damage, Color.green);
        }
        else
        {
            // Eğer prefab yoksa, runtime'da oluştur
            CreateDamageNumber(damage, Color.green);
        }
    }
    
    /// <summary>
    /// Runtime'da damage number oluşturur
    /// </summary>
    /// <param name="damage">Hasar miktarı</param>
    /// <param name="color">Hasar rengi</param>
    private void CreateDamageNumber(float damage, Color color)
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
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        damageNumberObj.transform.position = screenPos;
        
        // Animasyonu başlat
        damageNumberUI.SetDamage(damage, color);
    }
    
    private void OnDestroy()
    {
        // Zehir efekti bittiğinde hızı geri yükle
        var moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(1f);
        }
        
        // Zehir VFX'ini temizle
        var poisonVFX = transform.Find("PoisonVFX(Clone)");
        if (poisonVFX != null)
        {
            Destroy(poisonVFX.gameObject);
        }
    }
} 