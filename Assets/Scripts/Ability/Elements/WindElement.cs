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
        // Wind element data'sını al
        var windElementData = GetWindElementData();
        if (windElementData == null) return;
        
        // 2 stack'te knockback uygula
        if (stackCount >= windElementData.knockbackStackThreshold)
        {
            ApplyKnockbackEffect(target, windElementData);
            Debug.Log($"💨 Wind TriggerElementEffect: {stackCount} stack - KNOCKBACK applied to {target.name}");
        }
        else
        {
            Debug.Log($"💨 Wind TriggerElementEffect: {stackCount} stack - No knockback yet");
        }
    }
    
    /// <summary>
    /// Knockback efektini uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="windData">Wind element data</param>
    private void ApplyKnockbackEffect(GameObject target, WindElementData windData)
    {
        // Player'dan uzaklaştırma yönünü hesapla
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 knockbackDirection = (targetPosition - playerPosition).normalized;
        
        // Rigidbody2D ile knockback uygula
        var rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Knockback kuvvetini uygula
            Vector2 knockbackForce = knockbackDirection * windData.knockbackForce;
            rb.AddForce(knockbackForce, ForceMode2D.Impulse);
            
            // Knockback süresi boyunca hareketi kısıtla
            // Target'ta zaten bir MonoBehaviour component'i olmalı (EnemyController gibi)
            var targetMonoBehaviour = target.GetComponent<MonoBehaviour>();
            if (targetMonoBehaviour != null)
            {
                targetMonoBehaviour.StartCoroutine(KnockbackStun(target, windData.knockbackStunDuration));
            }
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
        // Player'dan WindElementData'yı al
        var playerController = PlayerController.Instance;
        if (playerController != null)
        {
            // ElementalAbilityManager'dan wind element data'sını al
            var elementalManager = playerController.GetComponent<ElementalAbilityManager>();
            if (elementalManager != null)
            {
                // Wind element data'sını bul
                var windAbility = elementalManager.GetAbility(ElementType.Wind, AbilityType.ElementalStrike);
                if (windAbility != null)
                {
                    // WindElementData'yı döndür (bu kısım implementasyona bağlı)
                    return Resources.Load<WindElementData>("ElementData/Wind/WindElementData");
                }
            }
        }
        
        return null;
    }
} 