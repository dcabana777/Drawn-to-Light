using UnityEngine;

public class MultiInteractable : Interactable
{
    [SerializeField] private Interactable[] interactables;

    protected override void OnInteract()
    {
        foreach (Interactable interactable in interactables)
        {
            if (interactable == null)
                continue;

            interactable.Interact();
        }
    }
}
