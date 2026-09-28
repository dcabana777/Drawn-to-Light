using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Interaction Prompt")]
    [SerializeField] private TMP_Text interactionPrompt;
    [SerializeField] private string interactionPromptText = "Press X";

    private Interactable nearestInteractable;

    private void Awake()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.text = interactionPromptText;
            interactionPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateNearestInteractable();
        UpdateInteractionPrompt();
    }

    private void OnInteract(InputValue value)
    {
        // Debug.Log("Attempt interact");
        
        if (!value.isPressed) return;

        if (nearestInteractable != null)
        {
            nearestInteractable.Interact();
        }
    }

    private void UpdateNearestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);

        nearestInteractable = null;
        float nearestDistanceSquared = float.PositiveInfinity;

        foreach (Collider2D hit in hits)
        {
            Interactable interactable = hit.GetComponentInParent<Interactable>();

            if (interactable == null) continue;

            Vector2 closestPoint = hit.ClosestPoint(transform.position);

            float distanceSquared = (hit.transform.position - transform.position).sqrMagnitude;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestInteractable = interactable;
            }
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt == null)
            return;

        bool shouldShowPrompt =
            nearestInteractable != null &&
            nearestInteractable.ShowInteractionPrompt;

        interactionPrompt.gameObject.SetActive(shouldShowPrompt);

        if (shouldShowPrompt)
        {
            interactionPrompt.text = interactionPromptText;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
