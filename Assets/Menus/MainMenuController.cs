using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenOptions()
    {
        // placeholder until options is complete
    }

    public void CloseOptions()
    {
        // placeholder until options is complete
    }
    
    public void QuitGame()
    {
        Debug.Log("Exit application.");
        Application.Quit();
    }
}
