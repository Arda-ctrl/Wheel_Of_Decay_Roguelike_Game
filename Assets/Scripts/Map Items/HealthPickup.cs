using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 1;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (PlayerHealthController.instance.currentHealth == PlayerHealthController.instance.maxHealth) return;
            PlayerHealthController.instance.HealPlayer(healAmount);
            AudioManager.instance.PlaySFX(7);
            Destroy(gameObject);
        }
    }
}
