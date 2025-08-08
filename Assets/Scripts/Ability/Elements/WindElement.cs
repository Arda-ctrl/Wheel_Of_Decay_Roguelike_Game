using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Wind Element - Rüzgar element'i için temel sınıf
/// Knockback ve hız artışı efektleri sağlar
/// </summary>
public class WindElement : IElement
{
    public string ElementName => "Wind";
    public Color ElementColor => Color.cyan;
    public ElementType ElementType => ElementType.Wind;
    
    /// <summary>
    /// Wind element stack'ini hedefe uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackAmount">Stack miktarı</param>
    public void ApplyElementStack(GameObject target, int stackAmount)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack == null)
        {
            elementStack = target.AddComponent<ElementStack>();
        }
        
        elementStack.AddElementStack(ElementType.Wind, stackAmount);
        Debug.Log($"💨 Wind element stack applied to {target.name}: +{stackAmount}");
    }
    
    /// <summary>
    /// Wind element efektini uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    public void ApplyElementEffect(GameObject target)
    {
        // Wind element efektleri burada uygulanabilir
        // Örneğin: Hız artışı, knockback, vb.
        Debug.Log($"💨 Wind element effect applied to {target.name}");
    }
    
    /// <summary>
    /// Wind element stack'ini kaldırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackAmount">Kaldırılacak stack miktarı</param>
    public void RemoveElementStack(GameObject target, int stackAmount)
    {
        var elementStack = target.GetComponent<ElementStack>();
        if (elementStack != null)
        {
            elementStack.RemoveElementStack(ElementType.Wind, stackAmount);
            Debug.Log($"💨 Wind element stack removed from {target.name}: -{stackAmount}");
        }
    }
    
    /// <summary>
    /// Wind element efektini çalıştırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Mevcut stack sayısı</param>
    public void TriggerElementEffect(GameObject target, int stackCount)
    {
        Debug.Log($"💨 Wind TriggerElementEffect called for {target.name} with {stackCount} stacks");
        
        // Wind element data'sını al
        var windElementData = GetWindElementData();
        if (windElementData == null) 
        {
            Debug.LogError("💨 Wind TriggerElementEffect: WindElementData is null!");
            return;
        }
        
        Debug.Log($"💨 Wind TriggerElementEffect: Threshold = {windElementData.knockbackStackThreshold}, Current stacks = {stackCount}");
        
        // 2 stack'te knockback uygula
        if (stackCount >= windElementData.knockbackStackThreshold)
        {
            ApplyKnockbackEffect(target, windElementData);
            Debug.Log($"💨 Wind TriggerElementEffect: {stackCount} stack - KNOCKBACK applied to {target.name}");
        }
        else
        {
            Debug.Log($"💨 Wind TriggerElementEffect: {stackCount} stack - No knockback yet (need {windElementData.knockbackStackThreshold})");
        }
    }
    
    /// <summary>
    /// Knockback efektini uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="windData">Wind element data</param>
    private void ApplyKnockbackEffect(GameObject target, WindElementData windData)
    {
        Debug.Log($"💨 ApplyKnockbackEffect called for {target.name} with force {windData.knockbackForce}");
        
        // Player'dan uzaklaştırma yönünü hesapla
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 knockbackDirection = (targetPosition - playerPosition).normalized;
        
        Debug.Log($"💨 Knockback direction: {knockbackDirection}, Force: {windData.knockbackForce}");
        
        // Rigidbody2D ile knockback uygula
        var rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Knockback kuvvetini uygula
            Vector2 knockbackForce = knockbackDirection * windData.knockbackForce;
            rb.AddForce(knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log($"💨 Applied knockback force: {knockbackForce} to {target.name}");
            
            // Knockback süresi boyunca hareketi kısıtla
            // Target'ta zaten bir MonoBehaviour component'i olmalı (EnemyController gibi)
            var targetMonoBehaviour = target.GetComponent<MonoBehaviour>();
            if (targetMonoBehaviour != null)
            {
                targetMonoBehaviour.StartCoroutine(KnockbackStun(target, windData.knockbackStunDuration));
                Debug.Log($"💨 Started knockback stun coroutine for {target.name}");
            }
            else
            {
                Debug.LogWarning($"💨 No MonoBehaviour found on {target.name} for knockback stun");
            }
        }
        else
        {
            Debug.LogError($"💨 No Rigidbody2D found on {target.name} for knockback!");
        }
        
        // VFX ve SFX oynat
        PlayWindKnockbackEffects(target);
    }
    
    /// <summary>
    /// Knockback sırasında stun uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stunDuration">Stun süresi</param>
    private System.Collections.IEnumerator KnockbackStun(GameObject target, float stunDuration)
    {
        var moveable = target.GetComponent<IMoveable>();
        if (moveable != null)
        {
            // Hareketi durdur
            moveable.SetSpeedMultiplier(0f);
            
            yield return new WaitForSeconds(stunDuration);
            
            // Hareketi geri aç
            moveable.SetSpeedMultiplier(1f);
        }
    }
    
    /// <summary>
    /// Wind knockback efektlerini oynatır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    private void PlayWindKnockbackEffects(GameObject target)
    {
        // Wind knockback VFX'i oynat
        var windVFX = Resources.Load<GameObject>("Prefabs/Effects/WindVFX");
        if (windVFX != null)
        {
            GameObject vfxInstance = Object.Instantiate(windVFX, target.transform.position, Quaternion.identity);
            vfxInstance.transform.SetParent(target.transform);
        }
        
        // Wind knockback SFX'i oynat
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(25); // Wind sound effect
        }
    }
    
    /// <summary>
    /// Wind element data'sını alır
    /// </summary>
    /// <returns>Wind element data</returns>
    private WindElementData GetWindElementData()
    {
        // Try to load from Resources first
        var windData = Resources.Load<WindElementData>("SO/ElementData/Wind/WindElementData");
        if (windData != null) return windData;
        
        // Fallback: Try to load from SO folder directly
        windData = Resources.Load<WindElementData>("ElementData/Wind/WindElementData");
        if (windData != null) return windData;
        
        // Create default wind data if none found
        Debug.LogWarning("WindElementData not found in Resources, creating default values");
        var defaultWindData = ScriptableObject.CreateInstance<WindElementData>();
        defaultWindData.knockbackForce = 8f;
        defaultWindData.knockbackStackThreshold = 2;
        defaultWindData.knockbackStunDuration = 0.5f;
        return defaultWindData;
    }
} 