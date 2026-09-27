using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightController : MonoBehaviour
{
    public int LightGroup { get; private set; }
    private ControllableLight[] controllableLights;

    private int moveInput;
    private int rotateInput;

    
    private void Awake()
    {
        LightGroup = 0; // default
        FindMatchingLights();
    }

    private void OnLightMove(InputValue value)
    {
        float input = value.Get<float>();

        if (input > 0f) moveInput = 1;
        else if (input < 0f) moveInput = -1;
        else moveInput = 0;
    }

    private void OnLightRotate(InputValue value)
    {
        float input = value.Get<float>();

        if (input > 0f) rotateInput = 1;
        else if (input < 0f) rotateInput = -1;
        else rotateInput = 0;
        
    }

    private void Update()
    {
        foreach (ControllableLight light in controllableLights)
        {
            light.HandleRemoteInput(
                moveInput,
                rotateInput,
                Time.deltaTime
            );
        }
    }

    public void ChangeLightGroup(int newGroup)
    {
        LightGroup = newGroup;
        FindMatchingLights();
    }

    private void FindMatchingLights()
    {
        ControllableLight[] allLightControllers = FindObjectsByType<ControllableLight>();
        List<ControllableLight> matchingLights = new();

        foreach (ControllableLight light in allLightControllers)
        {
            if (light.LightGroup == LightGroup)
            {
                matchingLights.Add(light);
            }
        }

        controllableLights = matchingLights.ToArray();
    }
}
