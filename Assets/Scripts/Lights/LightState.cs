using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class LightState : MonoBehaviour, IRoomResettable
{
    [Header("Light Settings")]
    [SerializeField] private int lightGroup = 1;
    [SerializeField] private bool startsOn = true;
    
    [Tooltip("Delay before the visual light turns off. " + 
             "The gameplay light collider turns off immediately.")]
    [SerializeField, Min(0f)] private float disappearanceDelay = 0.15f;
    private Coroutine disappearanceCoroutine;
    [SerializeField, Min(0f)] private float appearanceDelay = 0.15f;
    private Coroutine appearanceCoroutine;

    [Header("Light Output")]
    [SerializeField] private Light2D visualLight;
    [SerializeField] private Collider2D gameplayLightCollider;

    // reset information storage
    private bool savedIsOn;


    public int LightGroup => lightGroup;
    public bool IsOn { get; private set; }

    protected virtual void Awake()
    {
        // initialize immediately so a light that starts off
        // doesn't appear briefly when the scene loads
        IsOn = startsOn;

        if (visualLight != null)
            visualLight.enabled = startsOn;
        if (gameplayLightCollider != null)
            gameplayLightCollider.enabled = startsOn;
    }

    public void SetLightEnabled(bool enabled)
    {
        if (enabled)
        {
            EnableLight();
        }
        else
        {
            DisableLight();
        }
    }

    private void EnableLight()
    {
        IsOn = true;

        // if visual light was waiting to turn off, cancel that
        if (disappearanceCoroutine != null)
        {
            StopCoroutine(disappearanceCoroutine);
            disappearanceCoroutine = null;
        }

        // cancel any previous pending visual enabling
        if (appearanceCoroutine != null)
        {
            StopCoroutine(appearanceCoroutine);
            appearanceCoroutine = null;
        }

        // if no delay, turn on light immediately
        if (appearanceDelay <= 0f)
        {
            if (visualLight != null)
                visualLight.enabled = true;
            if (gameplayLightCollider != null)
                gameplayLightCollider.enabled = true;
            return;
        }

        appearanceCoroutine = StartCoroutine(EnableAfterDelay());
    }

    private void DisableLight()
    {
        IsOn = false;

        // if visual light was waiting to turn on, cancel that
        if (appearanceCoroutine != null)
        {
            StopCoroutine(appearanceCoroutine);
            appearanceCoroutine = null;
        }
        
        // disable gameplay light collider immediately so affected
        // ExistencePieces can begin their disappearance delays
        if (gameplayLightCollider != null)
            gameplayLightCollider.enabled = false;

        // cancel any previous pending visual shutdown
        if (disappearanceCoroutine != null)
        {
            StopCoroutine(disappearanceCoroutine);
            disappearanceCoroutine = null;
        }

        // if no delay, turn off visual immediately
        if (disappearanceDelay <= 0f)
        {
            if (visualLight != null)
                visualLight.enabled = false;
            return;
        }

        disappearanceCoroutine = StartCoroutine(DisableVisualAfterDelay());
    }

    private IEnumerator DisableVisualAfterDelay()
    {
        yield return new WaitForSeconds(disappearanceDelay);

        if (visualLight != null)
            visualLight.enabled = false;

        disappearanceCoroutine = null;
    }

    private IEnumerator EnableAfterDelay()
    {
        yield return new WaitForSeconds(appearanceDelay);

        if (visualLight != null)
            visualLight.enabled = true;
        if (gameplayLightCollider != null)
            gameplayLightCollider.enabled = true;

        appearanceCoroutine = null;
    }

    public void ToggleLight()
    {
        SetLightEnabled(!IsOn);
    }

    public virtual void SaveState()
    {
        savedIsOn = IsOn;
    }

    public virtual void RestoreState()
    {
        IsOn = savedIsOn;
    }
}