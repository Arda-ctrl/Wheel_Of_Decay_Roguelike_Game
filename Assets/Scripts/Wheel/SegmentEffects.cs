using UnityEngine;

/// <summary>
/// Saldırı gücünü artıran segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "AttackBoostEffect", menuName = "Segment Effects/Attack Boost")]
public class AttackBoostEffect : SegmentEffect
{
    [Header("Saldırı Artırma Ayarları")]
    public float damageMultiplier = 1.5f;
    public float duration = 10f;
    
    // TODO: Player stats sistemi eklendiğinde aktif edilecek
    // private float originalDamage;
    // private PlayerController playerController;
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData)
    {
        // TODO: PlayerController eklendiğinde bu kısım aktif edilecek
        /*
        playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Mevcut saldırı gücünü kaydet
            originalDamage = playerController.damage;
            
            // Saldırı gücünü artır
            playerController.damage *= damageMultiplier;
            
            Debug.Log($"Saldırı gücü {damageMultiplier}x artırıldı! Yeni güç: {playerController.damage}");
        }
        */
        
        Debug.Log($"Saldırı gücü {damageMultiplier}x artırıldı! (Player stats sistemi henüz eklenmedi)");
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData)
    {
        // TODO: PlayerController eklendiğinde bu kısım aktif edilecek
        /*
        if (playerController != null)
        {
            // Saldırı gücünü eski haline getir
            playerController.damage = originalDamage;
            Debug.Log("Saldırı gücü normale döndü.");
        }
        */
        
        Debug.Log("Saldırı gücü normale döndü. (Player stats sistemi henüz eklenmedi)");
    }
}

/// <summary>
/// Can yenileyen segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "HealthRegenEffect", menuName = "Segment Effects/Health Regen")]
public class HealthRegenEffect : SegmentEffect
{
    [Header("Can Yenileme Ayarları")]
    public float healAmount = 10f;
    public float healInterval = 2f;
    
    private float lastHealTime;
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData)
    {
        lastHealTime = Time.time;
        Debug.Log($"Can yenileme aktif! Her {healInterval} saniyede {healAmount} can yenilenecek.");
        
        // TODO: Can yenileme sistemi eklendiğinde bu kısım aktif edilecek
        // Bu effect için bir Coroutine veya Update metodu gerekebilir
    }
    
    // TODO: Bu metod bir MonoBehaviour'da çağrılmalı veya Coroutine kullanılmalı
    /*
    public void Update()
    {
        if (Time.time - lastHealTime >= healInterval)
        {
            // Can yenileme mantığı burada
            lastHealTime = Time.time;
        }
    }
    */
}

/// <summary>
/// Hız artıran segment özelliği
/// </summary>
[CreateAssetMenu(fileName = "SpeedBoostEffect", menuName = "Segment Effects/Speed Boost")]
public class SpeedBoostEffect : SegmentEffect
{
    [Header("Hız Artırma Ayarları")]
    public float speedMultiplier = 1.3f;
    
    // TODO: Player stats sistemi eklendiğinde aktif edilecek
    // private float originalSpeed;
    // private PlayerController playerController;
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData)
    {
        // TODO: PlayerController eklendiğinde bu kısım aktif edilecek
        /*
        playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            originalSpeed = playerController.moveSpeed;
            playerController.moveSpeed *= speedMultiplier;
            Debug.Log($"Hız {speedMultiplier}x artırıldı!");
        }
        */
        
        Debug.Log($"Hız {speedMultiplier}x artırıldı! (Player stats sistemi henüz eklenmedi)");
    }
    
    public override void OnSegmentDeactivated(GameObject player, SegmentData segmentData)
    {
        // TODO: PlayerController eklendiğinde bu kısım aktif edilecek
        /*
        if (playerController != null)
        {
            playerController.moveSpeed = originalSpeed;
            Debug.Log("Hız normale döndü.");
        }
        */
        
        Debug.Log("Hız normale döndü. (Player stats sistemi henüz eklenmedi)");
    }
}

/// <summary>
/// Özel saldırı efekti (örn: ateş, buz, zehir)
/// </summary>
[CreateAssetMenu(fileName = "ElementalEffect", menuName = "Segment Effects/Elemental")]
public class ElementalEffect : SegmentEffect
{
    [Header("Elemental Ayarları")]
    public ElementType elementType;
    public float elementalDamage = 5f;
    public float burnDuration = 3f;
    
    public enum ElementType
    {
        Fire,   // Ateş
        Ice,    // Buz
        Poison, // Zehir
        Lightning // Şimşek
    }
    
    public override void OnSegmentActivated(GameObject player, SegmentData segmentData)
    {
        // TODO: Elemental saldırı sistemi eklendiğinde bu kısım aktif edilecek
        Debug.Log($"{elementType} elemental saldırısı aktif! {elementalDamage} hasar verecek.");
    }
} 