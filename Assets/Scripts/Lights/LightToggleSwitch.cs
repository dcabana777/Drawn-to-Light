using UnityEngine;
using System.Collections.Generic;

public class LightToggleSwitch : Interactable
{
    [SerializeField] private int lightGroup = 1;
    [SerializeField, Min(0f)] private float interactionCooldown;
    private float nextInteractionTime;

    private LightState[] affectedLights;

    protected override void Awake()
    {
        base.Awake();
        
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

    protected override bool OnInteract()
    {
        if (affectedLights.Length == 0) return false;

        if (Time.time < nextInteractionTime) return false;

        nextInteractionTime = Time.time + interactionCooldown;
        
        foreach (LightState light in affectedLights)
        {
            light.ToggleLight();
        }

        return true;
    }
}
