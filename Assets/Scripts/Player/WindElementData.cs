using UnityEngine;

[CreateAssetMenu(fileName = "WindElementData", menuName = "Game/Element Data/Wind")]
public class WindElementData : ElementData
{
    [Header("Wind Element Settings")]
    public float knockbackForce = 5f; // Geri itme kuvveti
    public float windDamagePerStack = 4f; // Stack başına hasar
    public float knockbackDuration = 0.3f; // Knockback süresi
    
    [Header("Wind Strike Settings")]
    public int knockbackStackThreshold = 2; // Kaç stack'te knockback aktif olsun
    public float knockbackDistance = 3f; // Ne kadar uzağa ittirsin
    public bool applyKnockbackOnHit = true; // Vuruşta knockback uygula mı
    public float knockbackStunDuration = 0.5f; // Knockback sırasında stun süresi
} 