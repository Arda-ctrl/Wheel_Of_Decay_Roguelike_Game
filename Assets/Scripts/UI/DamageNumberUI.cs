using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// DamageNumberUI - Hasar sayılarını göstermek için kullanılan UI component
/// </summary>
public class DamageNumberUI : MonoBehaviour
{
    [Header("Damage Number Settings")]
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float displayDuration = 1f;
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private void Awake()
    {
        // Eğer damageText atanmamışsa, bu GameObject'teki TMP_Text'i bul
        if (damageText == null)
        {
            damageText = GetComponent<TMP_Text>();
        }
    }
    
    /// <summary>
    /// Hasar sayısını ayarlar ve animasyonu başlatır
    /// </summary>
    /// <param name="damage">Hasar miktarı</param>
    /// <param name="color">Hasar rengi</param>
    public void SetDamage(float damage, Color color)
    {
        if (damageText != null)
        {
            // Hasar sayısını ayarla
            damageText.text = damage.ToString("F0");
            damageText.color = color;
            
            // Animasyonu başlat
            StartDamageAnimation();
        }
    }
    
    /// <summary>
    /// Hasar animasyonunu başlatır
    /// </summary>
    private void StartDamageAnimation()
    {
        // Başlangıç pozisyonunu kaydet
        Vector3 startPosition = transform.position;
        
        // Yukarı hareket animasyonu
        transform.DOMove(startPosition + Vector3.up * moveDistance, displayDuration)
            .SetEase(Ease.OutQuad);
        
        // Fade out animasyonu
        if (damageText != null)
        {
            damageText.DOFade(0f, fadeOutDuration)
                .SetDelay(displayDuration - fadeOutDuration)
                .OnComplete(() => {
                    Destroy(gameObject);
                });
        }
        else
        {
            // Eğer TMP_Text yoksa, sadece GameObject'i yok et
            Destroy(gameObject, displayDuration);
        }
    }
    
    /// <summary>
    /// Hasar sayısını ayarlar (string versiyonu)
    /// </summary>
    /// <param name="damageText">Hasar metni</param>
    /// <param name="color">Hasar rengi</param>
    public void SetDamage(string damageText, Color color)
    {
        if (this.damageText != null)
        {
            this.damageText.text = damageText;
            this.damageText.color = color;
            
            StartDamageAnimation();
        }
    }
    
    /// <summary>
    /// Hasar sayısını ayarlar (int versiyonu)
    /// </summary>
    /// <param name="damage">Hasar miktarı</param>
    /// <param name="color">Hasar rengi</param>
    public void SetDamage(int damage, Color color)
    {
        SetDamage((float)damage, color);
    }
} 