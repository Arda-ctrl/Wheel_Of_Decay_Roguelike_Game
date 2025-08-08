using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    private float nextFireTime;
    private bool isFiring = false;

    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private GameObject meleeHitboxPrefab; // Sword gibi yakın dövüş için (ileride)

    [Header("Visuals")]
    public SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private float weaponShowTime = 0.3f;

    void Start() {
        if (weaponSpriteRenderer != null && weaponData != null)
        {
            weaponSpriteRenderer.sprite = weaponData.weaponSprite;
            weaponSpriteRenderer.enabled = false; // Başlangıçta gizli
        }
    }

    private void Update()
    {
        // Ateş etme kontrolü
        if (isFiring && Time.time >= nextFireTime)
        {
            if (weaponData != null)
            {
                if (weaponData.weaponType == WeaponType.Pistol || weaponData.weaponType == WeaponType.Rifle)
                {
                    ShootRanged();
                    nextFireTime = Time.time + weaponData.fireRate;
                }
                else if (weaponData.weaponType == WeaponType.Sword)
                {
                    MeleeAttack();
                    nextFireTime = Time.time + 0.5f; // Sword için sabit cooldown, ileride SO'dan alınabilir
                }
            }
        }
    }

    public void StartFiring()
    {
        isFiring = true;
        // İlk atışı hemen yap
        if (weaponData != null)
        {
            if (weaponData.weaponType == WeaponType.Pistol || weaponData.weaponType == WeaponType.Rifle)
            {
                ShootRanged();
                nextFireTime = Time.time + weaponData.fireRate;
            }
            else if (weaponData.weaponType == WeaponType.Sword)
            {
                MeleeAttack();
                nextFireTime = Time.time + 0.5f;
            }
        }
    }

    public void StopFiring()
    {
        isFiring = false;
    }

    private IEnumerator ShowWeaponBriefly()
    {
        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.enabled = true;
            yield return new WaitForSeconds(weaponShowTime);
            weaponSpriteRenderer.enabled = false;
        }
    }

    private void ShootRanged()
    {
        if (weaponData == null || weaponData.bulletPrefab == null || firePoint == null) return;

        // Mermiyi oluştur
        GameObject bullet = Instantiate(weaponData.bulletPrefab, firePoint.position, firePoint.rotation);
        PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();

        // PlayerController'dan hasar çarpanını al
        float damageMultiplier = 1.0f;
        if (PlayerController.Instance != null)
        {
            damageMultiplier = PlayerController.Instance.GetDamageMultiplier();
        }

        // Eğer bullet script varsa, WeaponData'dan değerleri uygula
        if (bulletScript != null && weaponData != null)
        {
            bulletScript.baseDamage = weaponData.damage;
            bulletScript.SetEffectType((AbilityEffectType)weaponData.elementType); // Enumlar uyumlu ise
            bulletScript.SetDamageMultiplier(damageMultiplier);
            bulletScript.SetStackAmount(1);
            bulletScript.elementData = weaponData.elementData; // ElementData'yı ata
            // ElementalAbilityManager referansını ata
            if (PlayerController.Instance != null)
            {
                var abilityManager = PlayerController.Instance.GetComponent<ElementalAbilityManager>();
                bulletScript.SetElementalAbilityManager(abilityManager);
            }
        }

        // Debug log
        Debug.Log($"🎯 Shot fired with damage: {weaponData.damage}, element: {weaponData.elementType}");

        // Ses efekti
        AudioManager.Instance.PlaySFX(12);

        // Silah sprite'ını kısa süreliğine göster
        StartCoroutine(ShowWeaponBriefly());
    }

    private void MeleeAttack()
    {
        // Yakın dövüş saldırısı (ileride animasyon, hitbox vs. eklenecek)
        Debug.Log($"🗡️ Melee attack with {weaponData.weaponName}");
        StartCoroutine(ShowWeaponBriefly());
        // Burada düşmana hasar verme, animasyon tetikleme vs. yapılacak
        // Şimdilik log ve ileride hitbox prefabı ile gerçek saldırı
    }
    
    /// <summary>
    /// En yakın düşmanı bulur
    /// </summary>
    /// <returns>En yakın düşman GameObject</returns>
    private GameObject FindNearestEnemy()
    {
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        // Tüm düşmanları bul
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distance < nearestDistance)
            {
                nearestEnemy = enemy;
                nearestDistance = distance;
            }
        }
        
        return nearestEnemy;
    }
} 