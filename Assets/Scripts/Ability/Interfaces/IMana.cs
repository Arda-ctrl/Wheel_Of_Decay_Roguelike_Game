/// <summary>
/// IMana - Mana sistemini implement eden objeler için interface
/// </summary>
public interface IMana
{
    /// <summary>
    /// Mana tüketir
    /// </summary>
    /// <param name="amount">Tüketilecek mana miktarı</param>
    /// <returns>Mana tüketilebildi mi?</returns>
    bool ConsumeMana(float amount);
    
    /// <summary>
    /// Mana kazandırır
    /// </summary>
    /// <param name="amount">Kazanılacak mana miktarı</param>
    void RestoreMana(float amount);
    
    /// <summary>
    /// Mevcut mana miktarını döndürür
    /// </summary>
    /// <returns>Mevcut mana</returns>
    float GetCurrentMana();
    
    /// <summary>
    /// Maksimum mana miktarını döndürür
    /// </summary>
    /// <returns>Maksimum mana</returns>
    float GetMaxMana();
    
    /// <summary>
    /// Yeterli mana var mı kontrol eder
    /// </summary>
    /// <param name="amount">Kontrol edilecek mana miktarı</param>
    /// <returns>Yeterli mana var mı?</returns>
    bool HasEnoughMana(float amount);
    
    /// <summary>
    /// Mana yüzdesini döndürür (0-1 arası)
    /// </summary>
    /// <returns>Mana yüzdesi</returns>
    float GetManaPercentage();
} 