using UnityEngine;
using UnityEngine.InputSystem;

public class EndGame : Interactable
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private PlayerInput playerInput;


    protected override bool OnInteract()
    {
        mainCamera.SetActive(false);

        playerInput.DeactivateInput();

        return true;
    }
}
