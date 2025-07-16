using UnityEngine;

/// <summary>
/// Ability interface - Her ability bu interface'i implement etmeli
/// Ability'lerin temel özelliklerini ve davranışlarını tanımlar
/// </summary>
public interface IAbility
{
    /// <summary>
    /// Ability adı
    /// </summary>
    string AbilityName { get; }
    
    /// <summary>
    /// Ability açıklaması
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Ability icon'u
    /// </summary>
    Sprite Icon { get; }
    
    /// <summary>
    /// Ability'nin cooldown süresi
    /// </summary>
    float CooldownDuration { get; }
    
    /// <summary>
    /// Ability'nin mana maliyeti
    /// </summary>
    float ManaCost { get; }
    
    /// <summary>
    /// Ability'yi kullanır
    /// </summary>
    /// <param name="caster">Ability'yi kullanan GameObject</param>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="element">Kullanılacak element</param>
    void UseAbility(GameObject caster, GameObject target, IElement element);
    
    /// <summary>
    /// Ability'nin kullanılıp kullanılamayacağını kontrol eder
    /// </summary>
    /// <param name="caster">Ability'yi kullanacak GameObject</param>
    /// <returns>Kullanılabilir mi?</returns>
    bool CanUseAbility(GameObject caster);
    
    /// <summary>
    /// Ability'nin cooldown progress'ini döndürür
    /// </summary>
    /// <returns>0-1 arası progress değeri</returns>
    float GetCooldownProgress();
} 