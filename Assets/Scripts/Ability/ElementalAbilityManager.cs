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
    
    // Active ability'ler - her element için ayrı
    private Dictionary<ElementType, Dictionary<AbilityType, IAbility>> activeAbilities = new Dictionary<ElementType, Dictionary<AbilityType, IAbility>>();
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
        // Her element için ayrı dictionary oluştur
        foreach (ElementType elementType in System.Enum.GetValues(typeof(ElementType)))
        {
            if (elementType == ElementType.None) continue;
            
            activeAbilities[elementType] = new Dictionary<AbilityType, IAbility>();
        }
        
        foreach (var abilityData in availableAbilities)
        {
            if (abilityData != null)
            {
                IAbility ability = abilityData.CreateAbility(gameObject);
                if (ability != null)
                {
                    // Ability'yi ilgili element'in dictionary'sine ekle
                    ElementType elementType = abilityData.elementType;
                    AbilityType abilityType = abilityData.abilityType;
                    
                    if (!activeAbilities.ContainsKey(elementType))
                    {
                        activeAbilities[elementType] = new Dictionary<AbilityType, IAbility>();
                    }
                    
                    activeAbilities[elementType][abilityType] = ability;
                    
                    // Element'i ayarla
                    if (ability is ElementalStrike strike)
                    {
                        strike.SetElement(abilityData.CreateElement());
                    }
                    else if (ability is ElementalBuff buff)
                    {
                        buff.SetElement(abilityData.CreateElement());
                    }
                    else if (ability is ElementalProjectile projectile)
                    {
                        projectile.SetElement(abilityData.CreateElement());
                    }
                    else if (ability is ElementalArea area)
                    {
                        area.SetElement(abilityData.CreateElement());
                        // ElementalArea'yı otomatik olarak aktif et
                        area.SetActive(true);
                    }
                    
                    OnAbilityActivated?.Invoke(abilityType, ability);
                    
                    Debug.Log($"🔥 Loaded {elementType} {abilityType} ability");
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
        // Mevcut element için ability'yi kullan
        if (activeAbilities.ContainsKey(currentElementType) && 
            activeAbilities[currentElementType].ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[currentElementType][abilityType];
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
        UseAbility(AbilityType.ElementalStrike, target);
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
        if (activeAbilities.ContainsKey(elementType) && 
            activeAbilities[elementType].ContainsKey(AbilityType.ElementalBuff))
        {
            var buffAbility = activeAbilities[elementType][AbilityType.ElementalBuff] as ElementalBuff;
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
        if (activeAbilities.ContainsKey(currentElementType) && 
            activeAbilities[currentElementType].ContainsKey(AbilityType.ElementalProjectile))
        {
            var projectileAbility = activeAbilities[currentElementType][AbilityType.ElementalProjectile] as ElementalProjectile;
            projectileAbility?.OnAttack();
            Debug.Log($"🎯 Attack counter increased for {currentElementType} projectile ability");
        }
    }
    
    /// <summary>
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <param name="active">Aktif mi?</param>
    public void SetAbilityActive(AbilityType abilityType, bool active)
    {
        if (activeAbilities.ContainsKey(currentElementType) && 
            activeAbilities[currentElementType].ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[currentElementType][abilityType];
            
            if (ability is ElementalStrike strike)
            {
                // Strike ability'si her zaman aktif olmalı
                Debug.Log($"⚔️ {currentElementType} Strike ability is always active!");
                return;
            }
            else if (ability is ElementalBuff buff)
            {
                buff.SetActive(active);
                Debug.Log($"🛡️ {currentElementType} Buff ability {(active ? "ACTIVATED" : "DEACTIVATED")}");
            }
            else if (ability is ElementalProjectile projectile)
            {
                projectile.SetActive(active);
                Debug.Log($"🎯 {currentElementType} Projectile ability {(active ? "ACTIVATED" : "DEACTIVATED")}");
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
    /// Belirli bir ability'yi döndürür
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Ability instance'ı</returns>
    public IAbility GetAbility(AbilityType abilityType)
    {
        if (activeAbilities.ContainsKey(currentElementType) && 
            activeAbilities[currentElementType].ContainsKey(abilityType))
        {
            return activeAbilities[currentElementType][abilityType];
        }
        return null;
    }
    
    /// <summary>
    /// Belirli bir element ve ability'yi döndürür
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Ability instance'ı</returns>
    public IAbility GetAbility(ElementType elementType, AbilityType abilityType)
    {
        if (activeAbilities.ContainsKey(elementType) && 
            activeAbilities[elementType].ContainsKey(abilityType))
        {
            return activeAbilities[elementType][abilityType];
        }
        return null;
    }
    
    /// <summary>
    /// Ability'nin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Ability aktif mi?</returns>
    public bool IsAbilityActive(AbilityType abilityType)
    {
        if (activeAbilities.ContainsKey(currentElementType) && 
            activeAbilities[currentElementType].ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[currentElementType][abilityType];
            
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
            else if (ability is ElementalArea area)
            {
                return area.IsActive();
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
        if (activeAbilities.ContainsKey(currentElementType) && 
            activeAbilities[currentElementType].ContainsKey(abilityType))
        {
            return activeAbilities[currentElementType][abilityType].GetCooldownProgress();
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Tüm ability'leri sıfırlar
    /// </summary>
    public void ResetAllAbilities()
    {
        foreach (var elementAbilities in activeAbilities.Values)
        {
            foreach (var ability in elementAbilities.Values)
            {
                if (ability is ElementalProjectile projectile)
                {
                    projectile.ResetAttackCounter();
                }
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
        
        foreach (var elementAbilities in activeAbilities)
        {
            info += $"\n{elementAbilities.Key} Abilities:\n";
            foreach (var kvp in elementAbilities.Value)
            {
                bool isActive = IsAbilityActive(kvp.Key);
                string status = isActive ? "✅ ACTIVE" : "❌ INACTIVE";
                info += $"- {kvp.Key}: {status}\n";
            }
        }
        
        info += $"\n🎮 System Info:\n";
        info += $"ElementalArea: Auto-activated when available\n";
        info += $"Strike: Always active\n";
        info += $"Buff/Projectile: Manual toggle\n";
        
        // Daha büyük ve görünür box
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.9f);
        GUI.Box(new Rect(10, 10, 400, 300), "");
        
        // Başlık
        GUI.color = Color.yellow;
        GUI.Label(new Rect(15, 15, 390, 30), "🔥 ELEMENTAL SYSTEM DEBUG 🔥");
        
        // İçerik
        GUI.color = Color.white;
        GUI.Label(new Rect(15, 45, 390, 250), info);
        
        // Element bilgisi için özel renk
        GUI.color = Color.cyan;
        GUI.Label(new Rect(15, 295, 390, 20), $"Current Element: {currentElementType}");
    }
    
    private void Update()
    {
        // ElementalArea'yı her zaman aktif tut
        EnsureElementalAreaActive();
    }
    
    /// <summary>
    /// ElementalArea'yı her zaman aktif tutar
    /// </summary>
    private void EnsureElementalAreaActive()
    {
        foreach (var elementAbilities in activeAbilities.Values)
        {
            if (elementAbilities.ContainsKey(AbilityType.ElementalArea))
            {
                var area = elementAbilities[AbilityType.ElementalArea] as ElementalArea;
                if (area != null && !area.IsActive())
                {
                    area.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// ElementalArea'yı aktif/pasif yapar
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetElementalAreaActive(bool active)
    {
        foreach (var elementAbilities in activeAbilities.Values)
        {
            if (elementAbilities.ContainsKey(AbilityType.ElementalArea))
            {
                var area = elementAbilities[AbilityType.ElementalArea] as ElementalArea;
                area?.SetActive(active);
            }
        }
    }
} 