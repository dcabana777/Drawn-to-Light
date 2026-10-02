using UnityEngine;
using System.Collections.Generic;

public class LightToggleSwitch : Interactable
{
    [SerializeField] private int lightGroup = 1;

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

        foreach (LightState light in affectedLights)
        {
            if (light == null)
                continue;
            
            light.ToggleLight();
        }

        return true;
    }
}
