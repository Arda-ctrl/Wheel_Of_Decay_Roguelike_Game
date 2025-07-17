using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Controller : MonoBehaviour
{
    public static UI_Controller Instance;
    public Slider healthSlider;
    public TMP_Text healthText;
    public GameObject deathScreen;
    void Awake()
    {
        Instance = this;
    }
}
