using UnityEngine;
using System.Collections.Generic;

public class LightToggleSwitch : Interactable
{
    [SerializeField] private int lightGroup = 1;
    [SerializeField, Min(0f)] private float interactionCooldown;
    private float nextInteractionCooldown = 0.25f;

    private LightState[] affectedLights;

    private void Awake()
    {
        LightState[] allLights = FindObjectsByType<LightState>();

        List<LightState> matchingLights = new();

        foreach (LightState light in allLights)
        {
            if (light.LightGroup == lightGroup)
            {
                matchingLights.Add(light);
            }
        }

        affectedLights = matchingLights.ToArray();
    }

    private bool CooldownReady(float nextAllowedTime)
    {
        return Time.time >= nextAllowedTime;
    }

    public override void Interact()
    {
        if (affectedLights.Length == 0) return;

        if (!CooldownReady(nextInteractionCooldown)) return;

        if (interactionCooldown > 0f)
        {
            nextInteractionCooldown = Time.time + interactionCooldown;
        }

        foreach (LightState light in affectedLights)
        {
            light.ToggleLight();
        }
    }
}
