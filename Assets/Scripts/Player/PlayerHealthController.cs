using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;
    public int currentHealth;
    public int maxHealth;
    public float damageInvisibleLenght = 1f;
    private float invisCount;

    [SerializeField] private SpriteRenderer playerSprite;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (playerSprite == null)
        {
            playerSprite = GetComponentInChildren<SpriteRenderer>();
        }

        currentHealth = maxHealth;

        UI_Controller.instance.healthSlider.maxValue = maxHealth;
        UI_Controller.instance.healthSlider.value = currentHealth;
        UI_Controller.instance.healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
    }

    void Update()
    {
        if (invisCount > 0)
        {
            invisCount -= Time.deltaTime;

            if (invisCount <= 0 && playerSprite != null)
            {
                SetPlayerAlpha(1f);
            }
        }
    }

    public void DamagePlayer()
    {
        if (invisCount <= 0)
        {
            AudioManager.instance.PlaySFX(11);
            currentHealth--;

            invisCount = damageInvisibleLenght;

            SetPlayerAlpha(0.5f);

            if (currentHealth <= 0)
            {
                PlayerController.instance.gameObject.SetActive(false);
                UI_Controller.instance.deathScreen.SetActive(true);
                AudioManager.instance.PlayGameOver();
                AudioManager.instance.PlaySFX(8);
            }

            UI_Controller.instance.healthSlider.value = currentHealth;
            UI_Controller.instance.healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        }
    }

    public void MakeInvincible(float lenght)
    {
        invisCount = lenght;
        SetPlayerAlpha(0.5f);
    }

    public void HealPlayer(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UI_Controller.instance.healthSlider.value = currentHealth;
        UI_Controller.instance.healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
    }

    private void SetPlayerAlpha(float alpha)
    {
        if (playerSprite != null)
        {
            Color color = playerSprite.color;
            color.a = alpha;
            playerSprite.color = color;
        }
    }
}
