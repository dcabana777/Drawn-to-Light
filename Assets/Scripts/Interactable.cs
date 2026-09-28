using Unity.VisualScripting;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private bool showInteractionPrompt = false;
    public bool ShowInteractionPrompt => showInteractionPrompt;
    
    [Header("Limited Use Settings")]
    [SerializeField] private bool limitUses;
    [SerializeField, Min(1)] private int maxUses;
    private int currentUseCount;
    private bool isInteracting;

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

        if (!succeeded) return false;
        
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
    
    protected abstract bool OnInteract();
}
