using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask darkInteractableLayers;
    [SerializeField] private LayerMask lightInteractableLayers;
    private LayerMask interactableLayers;

    [Header("Interaction Prompt")]
    [SerializeField] private TMP_Text controllerInteractionPrompt;
    [SerializeField] private TMP_Text keyboardInteractionPrompt;

    [Header("Control Scheme Names")]
    [Tooltip("Must exactly match the control scheme name in the Input Actions asset.")]
    [SerializeField] private string keyboardMouseScheme = "MouseAndKeyboard";

    [Tooltip("Must exactly match the control scheme name in the Input Actions asset.")]
    [SerializeField] private string controllerScheme = "Controller";

    private Interactable nearestInteractable;
    private int currentPlayerLayer => gameObject.layer;

    private void Awake()
    {
        if (controllerInteractionPrompt != null)
        {
            controllerInteractionPrompt.gameObject.SetActive(false);
        }
        if (keyboardInteractionPrompt != null)
        {
            keyboardInteractionPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateInteractableLayers();
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayers);

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

    private void UpdateInteractableLayers()
    {
        if (currentPlayerLayer == 10)
            interactableLayers = lightInteractableLayers;
        else
            interactableLayers = darkInteractableLayers;
    }
    
    private void UpdateInteractionPrompt()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();

        string scheme = playerInput.currentControlScheme;

        bool usingController = (scheme == controllerScheme);

        bool usingKeyboardMouse = (scheme == keyboardMouseScheme);

        bool shouldShowPrompt =
            nearestInteractable != null &&
            nearestInteractable.ShowInteractionPrompt;

        if (usingController)
        {
            controllerInteractionPrompt.gameObject.SetActive(shouldShowPrompt);
            keyboardInteractionPrompt.gameObject.SetActive(false);
        }
        else if (usingKeyboardMouse)
        {
            keyboardInteractionPrompt.gameObject.SetActive(shouldShowPrompt);
            controllerInteractionPrompt.gameObject.SetActive(false);
        }
        else
        {
            controllerInteractionPrompt.gameObject.SetActive(false);
            keyboardInteractionPrompt.gameObject.SetActive(false);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
