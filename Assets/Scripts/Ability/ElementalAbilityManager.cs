using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ElementalAbilityManager - Elemental ability sistemini yönetir
/// Player'ın ability'lerini ve element'lerini yönetir
/// </summary>
public class ElementalAbilityManager : MonoBehaviour
{
    [Header("Elemental Ability Manager Settings")]
    [SerializeField] private ElementalAbilityData[] availableAbilities;
    [SerializeField] private ElementType currentElementType = ElementType.Fire;
    
    // Active ability'ler
    private Dictionary<AbilityType, IAbility> activeAbilities = new Dictionary<AbilityType, IAbility>();
    private IElement currentElement;
    
    // Events
    public System.Action<ElementType> OnElementChanged;
    public System.Action<AbilityType, IAbility> OnAbilityActivated;
    public System.Action<AbilityType> OnAbilityDeactivated;
    
    private void Start()
    {
        InitializeElementalSystem();
    }
    
    /// <summary>
    /// Elemental sistemi initialize eder
    /// </summary>
    private void InitializeElementalSystem()
    {
        // Mevcut elementi ayarla
        SetElement(currentElementType);
        
        // Mevcut ability'leri yükle
        LoadActiveAbilities();
        
        Debug.Log("Elemental Ability System initialized");
    }
    
    /// <summary>
    /// Mevcut ability'leri yükler
    /// </summary>
    private void LoadActiveAbilities()
    {
        foreach (var abilityData in availableAbilities)
        {
            if (abilityData != null)
            {
                IAbility ability = abilityData.CreateAbility(gameObject);
                if (ability != null)
                {
                    activeAbilities[abilityData.abilityType] = ability;
                    
                    // Element'i ayarla
                    if (ability is ElementalStrike strike)
                    {
                        strike.SetElement(currentElement);
                    }
                    else if (ability is ElementalBuff buff)
                    {
                        buff.SetElement(currentElement);
                    }
                    else if (ability is ElementalProjectile projectile)
                    {
                        projectile.SetElement(currentElement);
                    }
                    
                    OnAbilityActivated?.Invoke(abilityData.abilityType, ability);
                }
            }
        }
    }
    
    /// <summary>
    /// Elementi değiştirir
    /// </summary>
    /// <param name="elementType">Yeni element türü</param>
    public void SetElement(ElementType elementType)
    {
        currentElementType = elementType;
        currentElement = CreateElement(elementType);
        
        // Tüm active ability'lere yeni elementi ayarla
        foreach (var kvp in activeAbilities)
        {
            IAbility ability = kvp.Value;
            
            if (ability is ElementalStrike strike)
            {
                strike.SetElement(currentElement);
            }
            else if (ability is ElementalBuff buff)
            {
                buff.SetElement(currentElement);
            }
            else if (ability is ElementalProjectile projectile)
            {
                projectile.SetElement(currentElement);
            }
        }
        
        OnElementChanged?.Invoke(elementType);
        Debug.Log($"🔥 Element changed to: {elementType}");
    }
    
    /// <summary>
    /// Element oluşturur
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <returns>Oluşturulan element</returns>
    private IElement CreateElement(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return new FireElement();
            case ElementType.Ice:
                return new IceElement();
            case ElementType.Poison:
                return new PoisonElement();
            default:
                return new FireElement(); // Default element
        }
    }
    
    /// <summary>
    /// Ability'yi kullanır
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <param name="target">Hedef GameObject</param>
    public void UseAbility(AbilityType abilityType, GameObject target)
    {
        if (activeAbilities.ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[abilityType];
            if (ability.CanUseAbility(gameObject))
            {
                ability.UseAbility(gameObject, target, currentElement);
            }
        }
    }
    
    /// <summary>
    /// Strike ability'sini kullanır (normal saldırı için)
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    public void UseStrike(GameObject target)
    {
        UseAbility(AbilityType.Strike, target);
    }
    
    /// <summary>
    /// Buff'lanmış hasarı hesaplar
    /// </summary>
    /// <param name="baseDamage">Temel hasar</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="elementType">Hasarın element türü</param>
    /// <returns>Buff'lanmış hasar</returns>
    public float CalculateBuffDamage(float baseDamage, GameObject target, ElementType elementType)
    {
        if (activeAbilities.ContainsKey(AbilityType.Buff))
        {
            var buffAbility = activeAbilities[AbilityType.Buff] as ElementalBuff;
            if (buffAbility != null)
            {
                return buffAbility.CalculateBuffDamage(baseDamage, target, elementType);
            }
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// Saldırı sayacını artırır (projectile için)
    /// </summary>
    public void OnAttack()
    {
        if (activeAbilities.ContainsKey(AbilityType.Projectile))
        {
            var projectileAbility = activeAbilities[AbilityType.Projectile] as ElementalProjectile;
            projectileAbility?.OnAttack();
            Debug.Log($"🎯 Attack counter increased for projectile ability");
        }
    }
    
    /// <summary>
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <param name="active">Aktif mi?</param>
    public void SetAbilityActive(AbilityType abilityType, bool active)
    {
        if (activeAbilities.ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[abilityType];
            
            if (ability is ElementalStrike strike)
            {
                // Strike ability'si her zaman aktif olmalı
                Debug.Log($"⚔️ Strike ability is always active!");
                return;
            }
            else if (ability is ElementalBuff buff)
            {
                buff.SetActive(active);
                Debug.Log($"🛡️ Buff ability {(active ? "ACTIVATED" : "DEACTIVATED")}");
            }
            else if (ability is ElementalProjectile projectile)
            {
                projectile.SetActive(active);
                Debug.Log($"🎯 Projectile ability {(active ? "ACTIVATED" : "DEACTIVATED")}");
            }
            
            if (active)
            {
                OnAbilityActivated?.Invoke(abilityType, ability);
            }
            else
            {
                OnAbilityDeactivated?.Invoke(abilityType);
            }
        }
    }
    
    /// <summary>
    /// Mevcut elementi döndürür
    /// </summary>
    /// <returns>Mevcut element</returns>
    public IElement GetCurrentElement()
    {
        return currentElement;
    }
    
    /// <summary>
    /// Mevcut element türünü döndürür
    /// </summary>
    /// <returns>Mevcut element türü</returns>
    public ElementType GetCurrentElementType()
    {
        return currentElementType;
    }
    
    /// <summary>
    /// Ability'nin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Ability aktif mi?</returns>
    public bool IsAbilityActive(AbilityType abilityType)
    {
        if (activeAbilities.ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[abilityType];
            
            if (ability is ElementalStrike)
            {
                return true; // Strike her zaman aktif
            }
            else if (ability is ElementalBuff buff)
            {
                return buff.IsActive();
            }
            else if (ability is ElementalProjectile projectile)
            {
                return projectile.IsActive();
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Ability'nin cooldown progress'ini döndürür
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>0-1 arası progress değeri</returns>
    public float GetAbilityCooldownProgress(AbilityType abilityType)
    {
        if (activeAbilities.ContainsKey(abilityType))
        {
            return activeAbilities[abilityType].GetCooldownProgress();
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Tüm ability'leri sıfırlar
    /// </summary>
    public void ResetAllAbilities()
    {
        foreach (var kvp in activeAbilities)
        {
            IAbility ability = kvp.Value;
            
            if (ability is ElementalProjectile projectile)
            {
                projectile.ResetAttackCounter();
            }
        }
    }
    
    /// <summary>
    /// Debug için bilgileri yazdırır
    /// </summary>
    private void OnGUI()
    {
        // Her zaman göster (Editor ve Build'de)
        string info = $"🔥 ELEMENTAL SYSTEM DEBUG 🔥\n";
        info += $"Current Element: {currentElementType}\n";
        info += $"Active Abilities:\n";
        
        foreach (var kvp in activeAbilities)
        {
            bool isActive = IsAbilityActive(kvp.Key);
            string status = isActive ? "✅ ACTIVE" : "❌ INACTIVE";
            string colorTag = isActive ? "green" : "red";
            info += $"- {kvp.Key}: {status}\n";
        }
        
        info += $"\n🎮 Input Controls:\n";
        info += $"1, 2, 3: Change Element\n";
        info += $"Q: Toggle Buff\n";
        info += $"E: Toggle Projectile\n";
        
        // Daha büyük ve görünür box
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.9f);
        GUI.Box(new Rect(10, 10, 350, 250), "");
        
        // Başlık
        GUI.color = Color.yellow;
        GUI.Label(new Rect(15, 15, 340, 30), "🔥 ELEMENTAL SYSTEM DEBUG 🔥");
        
        // İçerik
        GUI.color = Color.white;
        GUI.Label(new Rect(15, 45, 340, 200), info);
        
        // Element bilgisi için özel renk
        GUI.color = Color.cyan;
        GUI.Label(new Rect(15, 65, 340, 20), $"Current Element: {currentElementType}");
        
        // Ability durumları için renkli gösterim
        int yPos = 85;
        foreach (var kvp in activeAbilities)
        {
            bool isActive = IsAbilityActive(kvp.Key);
            GUI.color = isActive ? Color.green : Color.red;
            string status = isActive ? "✅ ACTIVE" : "❌ INACTIVE";
            GUI.Label(new Rect(15, yPos, 340, 20), $"- {kvp.Key}: {status}");
            yPos += 20;
        }
    }
} 