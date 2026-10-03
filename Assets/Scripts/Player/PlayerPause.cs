using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPause : MonoBehaviour
{
    [SerializeField] PauseMenuController pauseController;

    public void OnPause()
    {
        pauseController.PauseGame();
    }
}
