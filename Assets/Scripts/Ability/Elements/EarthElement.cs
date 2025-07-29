using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// EarthElement - Toprak elementinin davranışlarını tanımlar
/// Earth stack'leri 3 olduğunda hedefi yere sabitler (root efekti)
/// </summary>
public class EarthElement : IElement
{
    public string ElementName => "Earth";
    public ElementType ElementType => ElementType.Earth;
    public Color ElementColor => new Color(0.6f, 0.4f, 0.2f); // Brown color
    
    [Header("Earth Element Settings")]
    private int rootStackThreshold = 3; // 3 stack'te root
    private float rootDuration = 0.75f; // 0.5-1 saniye arası (0.75 ortalama)
    private float earthDamage = 6f; // Stack başına hasar
    
    public void ApplyElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.AddElementStack(ElementType.Earth, stackAmount);
        }
    }
    
    public void RemoveElementStack(GameObject target, int stackAmount = 1)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType.Earth, stackAmount);
        }
    }
    
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        // Earth hasar ver
        ApplyEarthDamage(target, stackCount);
        
        // 3 stack'te root efekti uygula
        if (stackCount >= rootStackThreshold)
        {
            ApplyRootEffect(target);
            
            // Stack'leri sıfırla (root efekti kullanıldığı için)
            var elementStack = target.GetComponent<ElementStack>();
            if (elementStack != null)
            {
                elementStack.RemoveElementStack(ElementType.Earth, rootStackThreshold);
            }
        }
        
        // VFX ve SFX oynat
        PlayEarthEffects(target);
    }
    
    /// <summary>
    /// Earth hasar verir
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Stack sayısı</param>
    private void ApplyEarthDamage(GameObject target, int stackCount)
    {
        float totalDamage = earthDamage * stackCount;
        
        var health = target.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(totalDamage);
            ShowDamageNumber(target, totalDamage);
        }
    }
    
    /// <summary>
    /// Root efektini uygular (3 stack'te tetiklenir)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void ApplyRootEffect(GameObject target)
    {
        // Mevcut root efektini kontrol et
        var existingRoot = target.GetComponent<ElementalEarthRootEffect>();
        if (existingRoot == null)
        {
            // Yeni root efekti ekle
            var rootEffect = target.AddComponent<ElementalEarthRootEffect>();
            rootEffect.Initialize(rootDuration);
        }
        else
        {
            // Mevcut efekti yenile
            existingRoot.RefreshRoot();
        }
    }
    
    /// <summary>
    /// Earth efektlerini oynatır (VFX ve SFX)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayEarthEffects(GameObject target)
    {
        if (target == null) return;
        
        try
        {
            // VFX oynat
            var earthVFX = Resources.Load<GameObject>("Prefabs/Effects/EarthVFX");
            if (earthVFX != null)
            {
                GameObject vfxInstance = Object.Instantiate(earthVFX, target.transform.position, Quaternion.identity);
                if (vfxInstance != null)
                {
                    vfxInstance.transform.SetParent(target.transform);
                }
            }
            
            // SFX oynat
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(23); // Earth sound effect
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[EarthElement] PlayEarthEffects failed: {e.Message}");
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
                        damageUI.SetDamage(damage, new Color(0.6f, 0.4f, 0.2f)); // Brown color
                    }
                }
            }
            else
            {
                // Eğer prefab yoksa, runtime'da oluştur
                CreateDamageNumber(target, damage, new Color(0.6f, 0.4f, 0.2f));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[EarthElement] ShowDamageNumber failed: {e.Message}");
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
            Debug.LogWarning($"[EarthElement] CreateDamageNumber failed: {e.Message}");
        }
    }
}

/// <summary>
/// EarthRootEffect - Earth root efektini yönetir (Elemental sistem için)
/// </summary>
public class ElementalEarthRootEffect : MonoBehaviour
{
    private float duration;
    private float elapsedTime;
    private bool isRooted;
    
    public void Initialize(float rootDuration)
    {
        this.duration = rootDuration;
        this.elapsedTime = 0f;
        this.isRooted = false;
        
        StartRoot();
    }
    
    public void RefreshRoot()
    {
        this.elapsedTime = 0f;
        if (!isRooted)
        {
            StartRoot();
        }
    }
    
    private void StartRoot()
    {
        isRooted = true;
        
        // Hareketi durdur (ama animasyonu değil, sadece yer değiştirme)
        var moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(0f);
        }
        
        // Root VFX'i oynat
        var rootVFX = Resources.Load<GameObject>("Prefabs/Effects/EarthRootVFX");
        if (rootVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(rootVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(transform);
        }
        
        // Root SFX'i oynat
        AudioManager.Instance?.PlaySFX(24);
    }
    
    private void Update()
    {
        if (!isRooted) return;
        
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= duration)
        {
            EndRoot();
        }
    }
    
    private void EndRoot()
    {
        isRooted = false;
        
        // Hareketi geri yükle
        var moveable = GetComponent<IMoveable>();
        if (moveable != null)
        {
            moveable.SetSpeedMultiplier(1f);
        }
        
        // Root VFX'ini temizle
        var rootVFX = transform.Find("EarthRootVFX(Clone)");
        if (rootVFX != null)
        {
            Destroy(rootVFX.gameObject);
        }
        
        Destroy(this);
    }
    
    private void OnDestroy()
    {
        try
        {
            // Root efekti bittiğinde VFX'i temizle
            var rootVFX = transform.Find("EarthRootVFX(Clone)");
            if (rootVFX != null)
            {
                Destroy(rootVFX.gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ElementalEarthRootEffect] OnDestroy failed: {e.Message}");
        }
    }
} 