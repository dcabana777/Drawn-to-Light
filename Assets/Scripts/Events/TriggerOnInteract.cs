using Unity.Jobs;
using UnityEngine;

public class TriggerOnInteract : Interactable
{
    [SerializeField] private Trigger[] whatToTrigger;

    protected override bool OnInteract()
    {
        bool triggeredAnything = false;

        foreach (Trigger trigger in whatToTrigger)
        {
            if (trigger == null)
                continue;

            if (trigger.ManualTrigger())
                triggeredAnything = true;
        }

        return triggeredAnything;
    }
}