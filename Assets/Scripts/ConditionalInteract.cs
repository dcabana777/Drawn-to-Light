using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ConditionalInteract : Interactable
{
    [Header("Condition")]
    [SerializeField] private GameCondition blockedCondition;

    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Events")]
    [SerializeField] private UnityEvent onAttemptStarted;
    [SerializeField] private UnityEvent onSuccess;
    [SerializeField] private UnityEvent onFailure;

    private bool attemptInProgress = false;
    private bool wasPromptShown = false;

    protected override bool OnInteract()
    {
        if (attemptInProgress)
            return false;

        attemptInProgress = true;
        wasPromptShown = ShowInteractionPrompt;
        showInteractionPrompt = false;

        if (playerInput != null)
            playerInput.DeactivateInput();

        onAttemptStarted?.Invoke();

        if (blockedCondition != null && blockedCondition.IsMet)
        {
            onFailure?.Invoke();
        }
        else
        {
            onSuccess?.Invoke();
        }

        Invoke("FinishAttempt", 5.0f);

        return true;
    }

    public void FinishAttempt()
    {
        attemptInProgress = false;
        showInteractionPrompt = wasPromptShown;

        if (playerInput != null)
            playerInput.ActivateInput();
    }
}
