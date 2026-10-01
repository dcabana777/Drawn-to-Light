using UnityEngine;

public class MultiInteractable : Interactable
{
    [SerializeField] private Interactable[] interactables;

    protected override bool OnInteract()
    {
        bool interactedWithAnything = false;
        
        foreach (Interactable interactable in interactables)
        {
            if (interactable == null)
                continue;

            if (interactable.Interact())
            {
                interactedWithAnything = true;
            }
        }

        return interactedWithAnything;
    }
}
