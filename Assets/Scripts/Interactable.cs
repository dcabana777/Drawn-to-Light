using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IRoomResettable
{
    [Header("Interaction Settings")]
    [SerializeField] private bool showInteractionPrompt = false;
    public bool ShowInteractionPrompt => showInteractionPrompt;
    [SerializeField] private UnityEvent onSuccessfulInteract;
    
    [Header("Limited Use Settings")]
    [SerializeField] private bool limitUses;
    [SerializeField, Min(1)] private int maxUses;
    private int currentUseCount;
    private bool isInteracting;

    private int savedUseCount;

    protected virtual void Awake()
    {
        currentUseCount = 0;
    }
    
    public bool Interact()
    {
        if (isInteracting || !isActiveAndEnabled)
            return false;

        if (limitUses && currentUseCount >= maxUses)
            return false;

        isInteracting = true;

        bool succeeded;

        try
        {
            succeeded = OnInteract();
        }
        finally
        {
            isInteracting = false;
        }

        if (succeeded) onSuccessfulInteract?.Invoke();
        else return false;
        
        if (limitUses)
        {
            currentUseCount++;

            if (currentUseCount >= maxUses)
            {
                OnUseLimitReached();
            }
        }
        
        return true;
    }

    protected virtual void OnUseLimitReached()
    {
        showInteractionPrompt = false;
    }
    
    // Default trigger OnInteract() always returns true, do not call base.OnTrigger() in override functions
    protected virtual bool OnInteract()
    { 
        return true;
    }

    public virtual void SaveState()
    {
        savedUseCount = currentUseCount;
    }

    public virtual void RestoreState()
    {
        currentUseCount = savedUseCount;
    }
}
