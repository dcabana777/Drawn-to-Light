using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : Interactable
{
    protected override bool OnInteract()
    {
        // assumes the next scene in the buildIndex is the ending screen
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
        return true;
    }
}
