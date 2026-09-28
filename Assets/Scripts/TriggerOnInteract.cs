using UnityEngine;

[RequireComponent(typeof(Trigger))]
public class TriggerOnInteract : Interactable
{
    private Trigger[] whatToTrigger;

    protected override void Awake()
    {
        base.Awake();
        whatToTrigger = GetComponents<Trigger>();
    }

    protected override void OnInteract()
    {
        foreach (Trigger trigger in whatToTrigger)
        {
            trigger.ManualTrigger();
        }
    }
}