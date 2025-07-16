using UnityEngine;

/// <summary>
/// Test için basit IHealth implementasyonu
/// Bu class test düşmanları için kullanılır
/// </summary>
public class TestEnemyHealth : MonoBehaviour, IHealth
{
    [Header("Test Enemy Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"💥 Test enemy took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Debug.Log("💀 Test enemy died!");
            Destroy(gameObject);
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"💚 Test enemy healed {amount}. Health: {currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// Test için health'i reset eder
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        Debug.Log($"🔄 Test enemy health reset to {currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// Test için max health'i ayarlar
    /// </summary>
    /// <param name="newMaxHealth">Yeni max health</param>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"📊 Test enemy max health set to {maxHealth}");
    }
} 