using UnityEngine;

public class InteractOnTrigger : Trigger
{
    [SerializeField] private Interactable whatToInteract;

    protected override void TriggerEffect()
    {
        if (whatToInteract == null)
        {
            Debug.LogError("InteractOnTrigger has no Interactable assigned!", this);
            return;
        }
        
        whatToInteract.Interact();
    }
}
