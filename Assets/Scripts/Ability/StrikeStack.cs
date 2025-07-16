using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// StrikeStack - Düşmanların üzerinde strike stack'lerini tutar ve hasar hesaplamasını yapar
/// Her düşmanın üzerinde bu component olmalı
/// </summary>
public class StrikeStack : MonoBehaviour
{
    [Header("Strike Stack Settings")]
    [SerializeField] private int maxStacks = 5;
    [SerializeField] private float stackDecayTime = 15f; // Stack'lerin otomatik azalma süresi
    [SerializeField] private float damageMultiplierPerStack = 0.5f; // Her stack için hasar çarpanı
    
    // SO'dan alınan ayarlar
    private AbilityData abilityData;
    
    // Strike stack'lerini tutan değişken
    private int currentStacks = 0;
    
    // Stack decay timer
    private float stackDecayTimer = 0f;
    
    // Events
    public System.Action<int> OnStackChanged;
    public System.Action OnStackRemoved;
    
    private void Update()
    {
        UpdateStackDecay();
    }
    
    /// <summary>
    /// Strike stack ekler
    /// </summary>
    /// <param name="amount">Eklenecek stack miktarı</param>
    public void AddStrikeStack(int amount = 1)
    {
        int oldStacks = currentStacks;
        
        // SO'dan max stack değerini al, yoksa varsayılan değeri kullan
        int maxStacksFromSO = abilityData != null ? abilityData.maxStrikeStacks : maxStacks;
        currentStacks = Mathf.Min(currentStacks + amount, maxStacksFromSO);
        
        // Decay timer'ı sıfırla (SO'dan al, yoksa varsayılan değeri kullan)
        float decayTimeFromSO = abilityData != null ? abilityData.strikeStackDecayTime : stackDecayTime;
        stackDecayTimer = decayTimeFromSO;
        
        // Event'i tetikle
        OnStackChanged?.Invoke(currentStacks);
        
        Debug.Log($"⚡ {gameObject.name} received strike stack: {oldStacks} -> {currentStacks}");
    }
    
    /// <summary>
    /// Strike stack kaldırır
    /// </summary>
    /// <param name="amount">Kaldırılacak stack miktarı</param>
    public void RemoveStrikeStack(int amount = 1)
    {
        int oldStacks = currentStacks;
        currentStacks = Mathf.Max(0, currentStacks - amount);
        
        if (currentStacks == 0)
        {
            // Stack tamamen kaldırıldı
            OnStackRemoved?.Invoke();
            Debug.Log($"🗑️ {gameObject.name} strike stack completely removed");
        }
        else
        {
            // Decay timer'ı sıfırla
            stackDecayTimer = stackDecayTime;
            Debug.Log($"📉 {gameObject.name} strike stack reduced: {oldStacks} -> {currentStacks}");
        }
        
        OnStackChanged?.Invoke(currentStacks);
    }
    
    /// <summary>
    /// Mevcut stack sayısını döndürür
    /// </summary>
    /// <returns>Stack sayısı</returns>
    public int GetStrikeStacks()
    {
        return currentStacks;
    }
    
    /// <summary>
    /// Strike stack'i var mı kontrol eder
    /// </summary>
    /// <returns>Stack var mı?</returns>
    public bool HasStrikeStacks()
    {
        return currentStacks > 0;
    }
    
    /// <summary>
    /// Strike stack'ine göre hasar çarpanını hesaplar
    /// </summary>
    /// <param name="baseDamage">Temel hasar</param>
    /// <returns>Hesaplanmış hasar</returns>
    public float CalculateStrikeDamage(float baseDamage)
    {
        if (currentStacks == 0) return baseDamage;
        
        // SO'dan damage multiplier'ı al, yoksa varsayılan değeri kullan
        float multiplierFromSO = abilityData != null ? abilityData.strikeDamageMultiplierPerStack : damageMultiplierPerStack;
        float damageMultiplier = 1f + (multiplierFromSO * currentStacks);
        float finalDamage = baseDamage * damageMultiplier;
        
        Debug.Log($"⚡ Strike damage calculation: {baseDamage} * {damageMultiplier} = {finalDamage} (stacks: {currentStacks})");
        
        return finalDamage;
    }
    
    /// <summary>
    /// Stack decay'ini günceller
    /// </summary>
    private void UpdateStackDecay()
    {
        if (currentStacks > 0)
        {
            stackDecayTimer -= Time.deltaTime;
            
            if (stackDecayTimer <= 0)
            {
                // Stack'i azalt
                RemoveStrikeStack(1);
            }
        }
    }
    
    /// <summary>
    /// Ability data'sını ayarlar
    /// </summary>
    /// <param name="data">Ability data</param>
    public void SetAbilityData(AbilityData data)
    {
        abilityData = data;
        Debug.Log($"⚡ StrikeStack ability data set: {(data != null ? data.abilityName : "None")}");
    }
    
    /// <summary>
    /// Decay timer'ı sıfırlar
    /// </summary>
    public void ResetDecayTimer()
    {
        float decayTimeFromSO = abilityData != null ? abilityData.strikeStackDecayTime : stackDecayTime;
        stackDecayTimer = decayTimeFromSO;
    }
    
    /// <summary>
    /// Tüm strike stack'lerini temizler
    /// </summary>
    public void ClearStrikeStacks()
    {
        currentStacks = 0;
        stackDecayTimer = 0f;
        OnStackRemoved?.Invoke();
    }
    
    /// <summary>
    /// Debug için stack bilgilerini yazdırır
    /// </summary>
    private void OnGUI()
    {
        // Her zaman göster (Editor ve Build'de)
        if (currentStacks > 0)
        {
            string stackInfo = $"⚡ {gameObject.name} Strike Stacks: {currentStacks}\n";
            stackInfo += $"Damage Multiplier: {1f + (damageMultiplierPerStack * currentStacks):F2}x\n";
            stackInfo += $"Decay Timer: {stackDecayTimer:F1}s";
            
            // Daha görünür debug bilgisi
            GUI.color = Color.yellow;
            GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
            GUI.Box(new Rect(10, 500, 250, 80), "");
            GUI.Label(new Rect(15, 505, 240, 70), stackInfo);
        }
    }
} 