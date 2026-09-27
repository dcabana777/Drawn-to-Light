using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    private void OnInteract(InputValue value)
    {
        // Debug.Log("Attempt interact");
        
        if (!value.isPressed) return;

        InteractWithNearest();
    }

    private void InteractWithNearest()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);

        Interactable nearestInteractable = null;
        float nearestDistanceSquared = float.PositiveInfinity;

        foreach (Collider2D hit in hits)
        {
            Interactable interactable = hit.GetComponentInParent<Interactable>();

            if (interactable == null) continue;

            float distanceSquared = (hit.transform.position - transform.position).sqrMagnitude;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestInteractable = interactable;
            }
        }

        if (nearestInteractable != null)
        {
            // Debug.Log("Interact proceeds");
            nearestInteractable.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
