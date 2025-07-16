using UnityEngine;

/// <summary>
/// Element interface - Her element bu interface'i implement etmeli
/// Elementlerin temel özelliklerini ve davranışlarını tanımlar
/// </summary>
public interface IElement
{
    /// <summary>
    /// Element adı (Fire, Ice, Poison vb.)
    /// </summary>
    string ElementName { get; }
    
    /// <summary>
    /// Element rengi (UI ve efektler için)
    /// </summary>
    Color ElementColor { get; }
    
    /// <summary>
    /// Element stack'i hedefe uygular
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackAmount">Stack miktarı</param>
    void ApplyElementStack(GameObject target, int stackAmount = 1);
    
    /// <summary>
    /// Element stack'ini hedeften kaldırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackAmount">Kaldırılacak stack miktarı</param>
    void RemoveElementStack(GameObject target, int stackAmount = 1);
    
    /// <summary>
    /// Element efektini çalıştırır
    /// </summary>
    /// <param name="target">Hedef GameObject</param>
    /// <param name="stackCount">Mevcut stack sayısı</param>
    void TriggerElementEffect(GameObject target, int stackCount);
} 