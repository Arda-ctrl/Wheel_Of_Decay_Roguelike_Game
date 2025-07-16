using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime;
    private bool isFiring = false;

    [Header("Ability Settings")]
    [SerializeField] private AbilityData[] abilities; // Unity'de atanacak yetenekler
    [SerializeField] private KeyCode switchAbilityKey = KeyCode.Q;
    private int currentAbilityIndex = 0;
    
    [Header("Elemental Ability Integration")]
    [SerializeField] private ElementalAbilityManager elementalAbilityManager;

    [Header("Strike System")]
    [SerializeField] private bool isStrikeBuffActive = false;

    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Image abilityIcon; // Opsiyonel: Aktif yeteneği göstermek için

    private void Update()
    {
        // Yetenek değiştirme
        if (Input.GetKeyDown(switchAbilityKey))
        {
            SwitchAbility();
        }

        // Ateş etme kontrolü
        if (isFiring && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        
        // Elemental ability input'larını handle et
        HandleElementalInputs();
    }

    public void StartFiring()
    {
        isFiring = true;
        // İlk atışı hemen yap
        Shoot();
        nextFireTime = Time.time + fireRate;
    }

    public void StopFiring()
    {
        isFiring = false;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Mermiyi oluştur
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();

        // Eğer bullet script ve aktif yetenek varsa, yeteneği uygula
        if (bulletScript != null && abilities.Length > 0 && abilities[currentAbilityIndex] != null)
        {
            bulletScript.SetEffectType(abilities[currentAbilityIndex].effectType);
            bulletScript.SetDamageMultiplier(1.0f); // Varsayılan hasar çarpanı
            bulletScript.SetAbilityData(abilities[currentAbilityIndex]); // Ability data'yı ayarla
        }
        
        // Strike sistemi ayarlarını uygula
        if (bulletScript != null)
        {
            bulletScript.SetStrikeBuff(isStrikeBuffActive);
        }

        // Elemental ability sistemini tetikle
        if (elementalAbilityManager != null)
        {
            // En yakın düşmanı bul
            GameObject nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                // Elemental strike uygula
                elementalAbilityManager.UseStrike(nearestEnemy);
                // Projectile sayacını artır
                elementalAbilityManager.OnAttack();
                
                Debug.Log($"🎯 Shot fired! Elemental abilities triggered on {nearestEnemy.name}");
            }
        }

        // Ses efekti
        AudioManager.Instance.PlaySFX(12);
    }

    private void SwitchAbility()
    {
        if (abilities == null || abilities.Length == 0) return;

        // Sonraki yeteneğe geç
        currentAbilityIndex = (currentAbilityIndex + 1) % abilities.Length;

        // UI güncelleme (opsiyonel)
        if (abilityIcon != null && abilities[currentAbilityIndex] != null)
        {
            abilityIcon.sprite = abilities[currentAbilityIndex].icon;
        }

        // Debug log
        Debug.Log($"Switched to ability: {abilities[currentAbilityIndex].abilityName}");
    }
    
    /// <summary>
    /// Elemental ability input'larını handle eder
    /// </summary>
    private void HandleElementalInputs()
    {
        // Element değiştirme input'ları
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeElement(ElementType.Fire);
            Debug.Log("🔥 Element changed to: FIRE");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeElement(ElementType.Ice);
            Debug.Log("❄️ Element changed to: ICE");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeElement(ElementType.Poison);
            Debug.Log("☠️ Element changed to: POISON");
        }
        
        // Ability toggle input'ları
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleAbility(AbilityType.Buff);
            Debug.Log("🛡️ Buff ability toggled!");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleAbility(AbilityType.Projectile);
            Debug.Log("🎯 Projectile ability toggled!");
        }
        
        // Strike sistemi input'ları
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleStrikeBuff();
            Debug.Log("⚡ Strike buff toggled!");
        }
    }
    
    /// <summary>
    /// Elementi değiştirir
    /// </summary>
    /// <param name="elementType">Yeni element türü</param>
    private void ChangeElement(ElementType elementType)
    {
        if (elementalAbilityManager != null)
        {
            elementalAbilityManager.SetElement(elementType);
        }
    }
    
    /// <summary>
    /// Ability'yi toggle eder
    /// </summary>
    /// <param name="abilityType">Ability türü</param>
    private void ToggleAbility(AbilityType abilityType)
    {
        if (elementalAbilityManager != null)
        {
            bool currentState = elementalAbilityManager.IsAbilityActive(abilityType);
            elementalAbilityManager.SetAbilityActive(abilityType, !currentState);
        }
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
    
    /// <summary>
    /// Strike buff'unu toggle eder
    /// </summary>
    private void ToggleStrikeBuff()
    {
        isStrikeBuffActive = !isStrikeBuffActive;
        Debug.Log($"⚡ Strike buff {(isStrikeBuffActive ? "activated" : "deactivated")}");
    }
    
    /// <summary>
    /// Strike buff'unu manuel olarak ayarlar
    /// </summary>
    /// <param name="active">Aktif mi?</param>
    public void SetStrikeBuff(bool active)
    {
        isStrikeBuffActive = active;
        Debug.Log($"⚡ Strike buff {(active ? "activated" : "deactivated")}");
    }
} 