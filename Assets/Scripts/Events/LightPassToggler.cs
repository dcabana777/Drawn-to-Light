using System.Linq;
using UnityEngine;

public class LightPassToggler : Trigger
{
    [SerializeField] private int[] lightGroupsToToggle;
    private LightState lightPassingThrough;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("ExistenceLight"))
            return;

        lightPassingThrough = other.GetComponent<LightState>();

        if (lightPassingThrough == null)
            return;

        TriggerEffect();
    }

    protected override void TriggerEffect()
    {
        if (lightGroupsToToggle.Contains(lightPassingThrough.LightGroup))
        {
            lightPassingThrough.SetLightEnabled(!lightPassingThrough.IsOn);
        }
    }
}
