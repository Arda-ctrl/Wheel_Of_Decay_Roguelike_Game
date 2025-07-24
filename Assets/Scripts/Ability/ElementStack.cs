using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ElementStack - Düşmanların üzerinde element stack'lerini tutar ve efektleri çalıştırır
/// Her düşmanın üzerinde bu component olmalı
/// </summary>
public class ElementStack : MonoBehaviour
{
    [Header("Element Stack Settings")]
    [SerializeField] private int maxStacksPerElement = 5;
    [SerializeField] private float stackDecayTime = 10f; // Stack'lerin otomatik azalma süresi
    
    // Element stack'lerini tutan dictionary
    private Dictionary<ElementType, int> elementStacks = new Dictionary<ElementType, int>();
    
    // Stack decay timer'ları
    private Dictionary<ElementType, float> stackDecayTimers = new Dictionary<ElementType, float>();
    
    // Element efektlerini tutan dictionary
    private Dictionary<ElementType, IElement> elementEffects = new Dictionary<ElementType, IElement>();
    
    // Events
    public System.Action<ElementType, int> OnStackChanged;
    public System.Action<ElementType> OnStackRemoved;
    
    private void Start()
    {
        InitializeElementEffects();
    }
    
    private void Update()
    {
        UpdateStackDecay();
    }
    
    /// <summary>
    /// Element efektlerini initialize eder
    /// </summary>
    private void InitializeElementEffects()
    {
        // Element efektlerini oluştur
        elementEffects[ElementType.Fire] = new FireElement();
        elementEffects[ElementType.Ice] = new IceElement();
        elementEffects[ElementType.Poison] = new PoisonElement();
        // Yeni elementler buraya eklenebilir
    }
    
    /// <summary>
    /// Belirtilen elemente stack ekler
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <param name="amount">Eklenecek stack miktarı</param>
    public void AddElementStack(ElementType elementType, int amount = 1)
    {
        if (elementType == ElementType.None) return;
        
        // Mevcut stack sayısını al
        int currentStacks = GetElementStack(elementType);
        
        // Yeni stack sayısını hesapla
        int newStacks = Mathf.Min(currentStacks + amount, maxStacksPerElement);
        
        // Stack'i güncelle
        elementStacks[elementType] = newStacks;
        
        // Decay timer'ı sıfırla
        stackDecayTimers[elementType] = stackDecayTime;
        
        // Element efektini tetikle
        if (elementEffects.ContainsKey(elementType))
        {
            elementEffects[elementType].TriggerElementEffect(gameObject, newStacks);
        }
        
        // Event'i tetikle
        OnStackChanged?.Invoke(elementType, newStacks);
        
        Debug.Log($"📊 {gameObject.name} received {elementType} stack: {currentStacks} -> {newStacks}");
    }
    
    /// <summary>
    /// Belirtilen elementten stack kaldırır
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <param name="amount">Kaldırılacak stack miktarı</param>
    public void RemoveElementStack(ElementType elementType, int amount = 1)
    {
        if (elementType == ElementType.None || !elementStacks.ContainsKey(elementType)) return;
        
        int currentStacks = elementStacks[elementType];
        int newStacks = Mathf.Max(0, currentStacks - amount);
        
        Debug.Log($"🗑️ [ElementStack] {gameObject.name} - Removing {amount} {elementType} stack(s): {currentStacks} -> {newStacks}");
        
        elementStacks[elementType] = newStacks;
        
        if (newStacks == 0)
        {
            // Stack tamamen kaldırıldı
            elementStacks.Remove(elementType);
            stackDecayTimers.Remove(elementType);
            OnStackRemoved?.Invoke(elementType);
            Debug.Log($"🗑️ {gameObject.name} {elementType} stack completely removed - cleaning up effects");

            // Poison ise, aktif PoisonEffect'leri bitir
            if (elementType == ElementType.Poison)
            {
                Debug.Log($"☠️ [ElementStack] Cleaning up Poison effects on {gameObject.name}");
                var poisonEffects = GetComponentsInChildren<PoisonEffect>();
                Debug.Log($"☠️ [ElementStack] Found {poisonEffects.Length} PoisonEffect components");
                foreach (var poisonEffect in poisonEffects)
                {
                    if (poisonEffect != null)
                    {
                        Debug.Log($"☠️ [ElementStack] Ending PoisonEffect on {gameObject.name}");
                        poisonEffect.EndEffect();
                    }
                }
                
                // ElementalPoisonEffect'leri de temizle
                var elementalPoisonEffects = GetComponentsInChildren<ElementalPoisonEffect>();
                Debug.Log($"☠️ [ElementStack] Found {elementalPoisonEffects.Length} ElementalPoisonEffect components");
                foreach (var elementalPoisonEffect in elementalPoisonEffects)
                {
                    if (elementalPoisonEffect != null)
                    {
                        Debug.Log($"☠️ [ElementStack] Destroying ElementalPoisonEffect on {gameObject.name}");
                        Destroy(elementalPoisonEffect);
                    }
                }
            }
            // Ice ise, hız ve animasyonu normale döndür, donma efektlerini temizle
            if (elementType == ElementType.Ice)
            {
                var moveable = GetComponent<IMoveable>();
                if (moveable != null)
                    moveable.SetSpeedMultiplier(1f);
                var animator = GetComponent<Animator>();
                if (animator != null)
                    animator.speed = 1f;
                // Aktif donma efektini temizle
                var freezeEffect = GetComponent<ElementalIceFreezeEffect>();
                if (freezeEffect != null)
                    Destroy(freezeEffect);
            }
        }
        else
        {
            // Decay timer'ı sıfırla
            stackDecayTimers[elementType] = stackDecayTime;
            Debug.Log($"📉 {gameObject.name} {elementType} stack reduced: {currentStacks} -> {newStacks} - timer reset");
        }
        
        OnStackChanged?.Invoke(elementType, newStacks);
    }
    
    /// <summary>
    /// Belirtilen elementin stack sayısını döndürür
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>Stack sayısı</returns>
    public int GetElementStack(ElementType elementType)
    {
        return elementStacks.ContainsKey(elementType) ? elementStacks[elementType] : 0;
    }
    
    /// <summary>
    /// Belirtilen elementin stack sayısını döndürür (GetElementStack ile aynı)
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>Stack sayısı</returns>
    public int GetElementStackCount(ElementType elementType)
    {
        return GetElementStack(elementType);
    }
    
    /// <summary>
    /// Belirtilen elementin stack'i var mı kontrol eder
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>Stack var mı?</returns>
    public bool HasElementStack(ElementType elementType)
    {
        return GetElementStack(elementType) > 0;
    }
    
    /// <summary>
    /// Tüm element stack'lerini döndürür
    /// </summary>
    /// <returns>Element stack dictionary'si</returns>
    public Dictionary<ElementType, int> GetAllElementStacks()
    {
        return new Dictionary<ElementType, int>(elementStacks);
    }
    
    /// <summary>
    /// Stack decay'lerini günceller
    /// </summary>
    private void UpdateStackDecay()
    {
        // Dictionary'yi güvenli şekilde iterate etmek için key'leri kopyala
        List<ElementType> elementTypes = new List<ElementType>(stackDecayTimers.Keys);
        
        foreach (ElementType elementType in elementTypes)
        {
            if (!stackDecayTimers.ContainsKey(elementType)) continue;
            
            float remainingTime = stackDecayTimers[elementType];
            remainingTime -= Time.deltaTime;
            stackDecayTimers[elementType] = remainingTime;
            
            if (remainingTime <= 0)
            {
                // Stack'i azalt
                Debug.Log($"⏰ [ElementStack] {gameObject.name} - {elementType} stack decay triggered. Current stacks: {GetElementStack(elementType)}");
                RemoveElementStack(elementType, 1);
            }
        }
    }
    
    /// <summary>
    /// Belirtilen elementin decay timer'ını sıfırlar
    /// </summary>
    /// <param name="elementType">Element türü</param>
    public void ResetDecayTimer(ElementType elementType)
    {
        if (stackDecayTimers.ContainsKey(elementType))
        {
            stackDecayTimers[elementType] = stackDecayTime;
        }
    }
    
    /// <summary>
    /// Tüm element stack'lerini temizler
    /// </summary>
    public void ClearAllStacks()
    {
        elementStacks.Clear();
        stackDecayTimers.Clear();
        
        // Tüm elementler için event tetikle
        foreach (ElementType elementType in System.Enum.GetValues(typeof(ElementType)))
        {
            if (elementType != ElementType.None)
            {
                OnStackRemoved?.Invoke(elementType);
            }
        }
    }
    
    /// <summary>
    /// Debug için stack bilgilerini yazdırır
    /// </summary>
    private void OnGUI()
    {
        // Her zaman göster (Editor ve Build'de)
        if (elementStacks.Count > 0)
        {
            string stackInfo = $"📊 {gameObject.name} Element Stacks:\n";
            foreach (var kvp in elementStacks)
            {
                stackInfo += $"{kvp.Key}: {kvp.Value}\n";
            }
            
            // Daha görünür debug bilgisi
            GUI.color = Color.cyan;
            GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
            GUI.Box(new Rect(10, 400, 250, 100), "");
            GUI.Label(new Rect(15, 405, 240, 90), stackInfo);
        }
    }
} 