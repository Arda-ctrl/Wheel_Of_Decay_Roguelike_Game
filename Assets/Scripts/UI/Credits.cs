using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    [Header("Credits Settings")]
    [SerializeField] private float scrollSpeed = 30f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("UI References")]
    [SerializeField] private RectTransform creditsText;
    
    void Update()
    {
        HandleScrolling();
        HandleInput();
    }
    
    private void HandleScrolling()
    {
        if (creditsText != null)
        {
            // Move text upward slowly
            Vector2 currentPos = creditsText.anchoredPosition;
            currentPos.y += scrollSpeed * Time.deltaTime;
            creditsText.anchoredPosition = currentPos;
        }
    }
    
    private void HandleInput()
    {
        // ESC key to return to main menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }
    
    public void ReturnToMainMenu()
    {
        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
