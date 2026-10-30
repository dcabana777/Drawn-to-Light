
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndingWalkOnly : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovementController playerController;

    private readonly List<InputAction> disabledActions = new();

    private InputAction moveAction;
    private bool moveWasEnabled;
    private bool endingActive;

    public void BeginEnding()
    {
        if (endingActive)
            return;

        moveAction = playerInput.actions.FindAction("Move");

        if (moveAction == null)
        {
            Debug.LogError("Move action not found!", this);
            return;
        }

        endingActive = true;
        moveWasEnabled = moveAction.enabled;

        // Restrict the player to only moving right
        playerController.SetEndingWalkOnly(true);

        // Disable everything except Move, across all maps
        foreach (InputActionMap map in playerInput.actions.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                if (action == moveAction || !action.enabled)
                    continue;

                disabledActions.Add(action);
                action.Disable();
            }
        }

        // Ensure Move remains available.
        moveAction.Enable();
    }

    public void EndEnding()
    {
        if (!endingActive)
            return;

        endingActive = false;

        playerController.SetEndingWalkOnly(false);

        if (!moveWasEnabled)
            moveAction.Disable();

        // Restore only the actions that were previously enabled
        foreach (InputAction action in disabledActions)
        {
            if (action != null)
                action.Enable();
        }

        disabledActions.Clear();
    }

    private void OnDisable()
    {
        EndEnding();
    }
}
