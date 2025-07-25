using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Controller : MonoBehaviour
{
    public static UI_Controller Instance;
    
    [Header("Health UI")]
    public Slider healthSlider;
    public TMP_Text healthText;
    
    [Header("Mana UI")]
    public Slider manaSlider;
    public TMP_Text manaText;
    
    [Header("Game UI")]
    public GameObject deathScreen;
    
    void Awake()
    {
        Instance = this;
    }
}
