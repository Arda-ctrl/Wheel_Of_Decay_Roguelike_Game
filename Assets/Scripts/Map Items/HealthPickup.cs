using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 1;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (PlayerHealthController.Instance.currentHealth == PlayerHealthController.Instance.maxHealth) return;
            PlayerHealthController.Instance.HealPlayer(healAmount);
            AudioManager.Instance.PlaySFX(7);
            Destroy(gameObject);
        }
    }
}
