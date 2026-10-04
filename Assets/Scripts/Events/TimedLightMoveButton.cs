using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedLightMoveButton : Interactable
{
    [SerializeField] private int lightGroup = 1;
    [SerializeField] private bool ignoreIsOn = false;
    [SerializeField] private float lightDuration = 1f;
    [SerializeField, Range(-1, 1)] private int moveDirection = 0;
    [SerializeField, Range(-1, 1)] private int rotateDirection = 0;

    private ControllableLight[] affectedLights;
    private Coroutine movementCoroutine;

    protected override void Awake()
    {
        base.Awake();
        
        ControllableLight[] allLights = FindObjectsByType<ControllableLight>();

        List<ControllableLight> matchingLights = new();

        foreach (ControllableLight light in allLights)
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
        if (affectedLights == null || affectedLights.Length == 0)
            return false;

        if (movementCoroutine != null)
            return false;

        movementCoroutine = StartCoroutine(MoveLightsForDuration());

        return true;
    }

    private IEnumerator MoveLightsForDuration()
    {
        float elapsed = 0f;

        while (elapsed < lightDuration)
        {
            foreach (ControllableLight light in affectedLights)
            {
                if (light == null)
                    continue;

                light.HandleRemoteInput(moveDirection, rotateDirection, Time.deltaTime, ignoreIsOn);
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        movementCoroutine = null;
    }

    public override void RestoreState()
    {
        StopAllCoroutines();
        movementCoroutine = null;
        base.RestoreState();
    }
}
