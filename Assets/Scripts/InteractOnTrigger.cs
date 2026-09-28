using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class InteractOnTrigger : Trigger
{
    private Interactable whatToInteract;

    protected override void Awake()
    {
        base.Awake();
        whatToInteract = GetComponent<Interactable>();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        TriggerEffect();
    }

    protected override void TriggerEffect()
    {
        whatToInteract.Interact();
    }
}
