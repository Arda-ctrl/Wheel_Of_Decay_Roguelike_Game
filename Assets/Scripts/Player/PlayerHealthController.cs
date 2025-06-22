using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;
    public int currentHealth;
    public int maxHealth;
    public float damageInvisibleLenght = 1f;
    private float invisCount;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
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

            if (invisCount <= 0)
            {
                PlayerController.instance.bodySR.color = new Color(PlayerController.instance.bodySR.color.r, PlayerController.instance.bodySR.color.g, PlayerController.instance.bodySR.color.b, 1f);
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

            PlayerController.instance.bodySR.color = new Color(PlayerController.instance.bodySR.color.r, PlayerController.instance.bodySR.color.g, PlayerController.instance.bodySR.color.b, .5f);

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

        PlayerController.instance.bodySR.color = new Color(PlayerController.instance.bodySR.color.r, PlayerController.instance.bodySR.color.g, PlayerController.instance.bodySR.color.b, .5f);

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
    
    
}
