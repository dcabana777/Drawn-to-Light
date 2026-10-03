using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : Interactable
{
    [SerializeField] private string endSceneName = "EndScreen";
    
    private bool isEnding;
    
    protected override bool OnInteract()
    {
        if (isEnding)
            return false;
        
        // assumes the next scene in the buildIndex is the ending screen
        SceneManager.LoadSceneAsync(endSceneName, LoadSceneMode.Single);
        
        return true;
    }
}
