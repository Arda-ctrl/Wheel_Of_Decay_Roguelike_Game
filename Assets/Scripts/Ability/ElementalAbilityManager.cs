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
    
    // Active ability'ler - her element için ayrı
    private Dictionary<ElementType, Dictionary<AbilityType, IAbility>> activeAbilities = new Dictionary<ElementType, Dictionary<AbilityType, IAbility>>();
    
    // Events
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
    /// Ability'yi kullanır
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <param name="target">Hedef GameObject</param>
    public void UseAbility(AbilityType abilityType, GameObject target)
    {
        // Mevcut element için ability'yi kullan
        if (activeAbilities.ContainsKey(ElementType.Fire) && 
            activeAbilities[ElementType.Fire].ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[ElementType.Fire][abilityType];
            if (ability.CanUseAbility(gameObject))
            {
                ability.UseAbility(gameObject, target, new FireElement());
            }
        }
    }
    
    /// <summary>
    /// Strike ability'sini kullanır (normal saldırı için)
    /// Artık tüm elementler için strike'ları çalıştırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    public void UseStrike(GameObject target)
    {
        // Tüm elementler için strike ability'lerini çalıştır
        foreach (var elementAbilities in activeAbilities)
        {
            if (elementAbilities.Value.ContainsKey(AbilityType.ElementalStrike))
            {
                var strikeAbility = elementAbilities.Value[AbilityType.ElementalStrike] as ElementalStrike;
                if (strikeAbility != null && strikeAbility.CanUseAbility(gameObject))
                {
                    // Her element için kendi element'ini oluştur
                    IElement element = null;
                    switch (elementAbilities.Key)
                    {
                        case ElementType.Fire:
                            element = new FireElement();
                            break;
                        case ElementType.Ice:
                            element = new IceElement();
                            break;
                        case ElementType.Poison:
                            element = new PoisonElement();
                            break;
                        // Diğer elementler eklenebilir
                        default:
                            continue;
                    }
                    strikeAbility.UseAbility(gameObject, target, element);
                    Debug.Log($"⚔️ {elementAbilities.Key} Strike applied to {target.name}");
                }
            }
        }
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
        // Sadece hedef element için buff'ı kontrol et
        if (activeAbilities.ContainsKey(elementType) && 
            activeAbilities[elementType].ContainsKey(AbilityType.ElementalBuff))
        {
            var buffAbility = activeAbilities[elementType][AbilityType.ElementalBuff] as ElementalBuff;
            if (buffAbility != null && buffAbility.IsActive())
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
        // Tüm elementler için projectile ability'lerini kontrol et
        foreach (var elementAbilities in activeAbilities)
        {
            if (elementAbilities.Value.ContainsKey(AbilityType.ElementalProjectile))
            {
                var projectileAbility = elementAbilities.Value[AbilityType.ElementalProjectile] as ElementalProjectile;
                if (projectileAbility != null && projectileAbility.IsActive())
                {
                    projectileAbility.OnAttack();
                    Debug.Log($"🎯 Attack counter increased for Fire projectile ability");
                }
            }
        }
    }
    
    /// <summary>
    /// Ability'yi aktif/pasif yapar
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <param name="active">Aktif mi?</param>
    public void SetAbilityActive(AbilityType abilityType, bool active)
    {
        if (activeAbilities.ContainsKey(ElementType.Fire) && 
            activeAbilities[ElementType.Fire].ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[ElementType.Fire][abilityType];
            
            if (ability is ElementalStrike strike)
            {
                // Strike ability'si her zaman aktif olmalı
                Debug.Log($"⚔️ Fire Strike ability is always active!");
                return;
            }
            else if (ability is ElementalBuff buff)
            {
                buff.SetActive(active);
                Debug.Log($"🛡️ Fire Buff ability {(active ? "ACTIVATED" : "DEACTIVATED")}");
            }
            else if (ability is ElementalProjectile projectile)
            {
                projectile.SetActive(active);
                Debug.Log($"🎯 Fire Projectile ability {(active ? "ACTIVATED" : "DEACTIVATED")}");
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
        return new FireElement(); // Strike her zaman FireElement kullanır
    }
    
    /// <summary>
    /// Mevcut element türünü döndürür
    /// </summary>
    /// <returns>Mevcut element türü</returns>
    public ElementType GetCurrentElementType()
    {
        return ElementType.Fire; // Strike her zaman FireElement kullanır
    }
    
    /// <summary>
    /// Belirli bir ability'yi döndürür
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Ability instance'ı</returns>
    public IAbility GetAbility(AbilityType abilityType)
    {
        if (activeAbilities.ContainsKey(ElementType.Fire) && 
            activeAbilities[ElementType.Fire].ContainsKey(abilityType))
        {
            return activeAbilities[ElementType.Fire][abilityType];
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
        if (activeAbilities.ContainsKey(ElementType.Fire) && 
            activeAbilities[ElementType.Fire].ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[ElementType.Fire][abilityType];
            
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
    /// Belirli bir element için ability'nin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Ability aktif mi?</returns>
    public bool IsAbilityActive(ElementType elementType, AbilityType abilityType)
    {
        if (activeAbilities.ContainsKey(elementType) && 
            activeAbilities[elementType].ContainsKey(abilityType))
        {
            IAbility ability = activeAbilities[elementType][abilityType];
            
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
    /// Stack ile çalışan ability'lerin durumunu kontrol eder
    /// </summary>
    /// <param name="elementType">Element türü</param>
    /// <param name="abilityType">Ability türü</param>
    /// <returns>Stack varsa active, yoksa deactive</returns>
    public bool IsStackBasedAbilityActive(ElementType elementType, AbilityType abilityType)
    {
        // Stack ile çalışan ability'ler: Buff, Area, Projectile
        if (abilityType == AbilityType.ElementalBuff || 
            abilityType == AbilityType.ElementalArea || 
            abilityType == AbilityType.ElementalProjectile)
        {
            // Düşmanların üzerindeki stack'leri kontrol et
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                var enemyElementStack = enemy.GetComponent<ElementStack>();
                if (enemyElementStack != null)
                {
                    int stackCount = enemyElementStack.GetElementStack(elementType);
                    if (stackCount > 0)
                    {
                        return true; // Herhangi bir düşmanda stack varsa active
                    }
                }
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
        if (activeAbilities.ContainsKey(ElementType.Fire) && 
            activeAbilities[ElementType.Fire].ContainsKey(abilityType))
        {
            return activeAbilities[ElementType.Fire][abilityType].GetCooldownProgress();
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
        info += $"Active Abilities:\n";
        // Düşmanların stack'lerini al
        Dictionary<ElementType, int> enemyStacks = new Dictionary<ElementType, int>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            var enemyElementStack = enemy.GetComponent<ElementStack>();
            if (enemyElementStack != null)
            {
                var stacks = enemyElementStack.GetAllElementStacks();
                info += $"\n{enemy.name} Stacks:\n";
                foreach (var kvp in stacks)
                {
                    info += $"- {kvp.Key}: {kvp.Value}\n";
                    if (enemyStacks.ContainsKey(kvp.Key))
                        enemyStacks[kvp.Key] += kvp.Value;
                    else
                        enemyStacks[kvp.Key] = kvp.Value;
                }
            }
        }
        foreach (var elementAbilities in activeAbilities)
        {
            info += $"\n{elementAbilities.Key} Abilities:\n";
            foreach (var kvp in elementAbilities.Value)
            {
                bool isActive;
                string status;
                // Stack'e bağlı yetenekler için özel kontrol
                if (kvp.Key == AbilityType.ElementalBuff || kvp.Key == AbilityType.ElementalArea || kvp.Key == AbilityType.ElementalProjectile)
                {
                    int stackCount = enemyStacks.ContainsKey(elementAbilities.Key) ? enemyStacks[elementAbilities.Key] : 0;
                    isActive = stackCount > 0;
                    status = isActive ? $"✅ ACTIVE ({stackCount} stack)" : "❌ DEACTIVE (0 stack)";
                }
                else // Strike her zaman aktif
                {
                    isActive = true;
                    status = "✅ ACTIVE";
                }
                info += $"- {kvp.Key}: {status}\n";
            }
        }
        info += $"\n🎮 System Info:\n";
        info += $"Strike: Always active\n";
        info += $"Buff/Area/Projectile: Active when stack > 0\n";
        info += $"Element Effects: Applied based on stacks\n";
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.9f);
        GUI.Box(new Rect(10, 10, 400, 350), "");
        GUI.color = Color.yellow;
        GUI.Label(new Rect(15, 15, 390, 30), "🔥 ELEMENTAL SYSTEM DEBUG 🔥");
        GUI.color = Color.white;
        GUI.Label(new Rect(15, 45, 390, 300), info);
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