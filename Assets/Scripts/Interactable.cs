using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class Interactable : MonoBehaviour, IRoomResettable
{
    [Header("Interaction Settings")]
    [SerializeField] private bool showInteractionPrompt = false;
    public bool ShowInteractionPrompt => showInteractionPrompt;
    [SerializeField, Min(0f)] private float interactionCooldown = 0f;
    private float nextInteractionTime;
    [SerializeField] private UnityEvent onSuccessfulInteract;
    
    [Header("Limited Use Settings")]
    [SerializeField] private bool limitUses;
    [SerializeField, Min(1)] private int maxUses;
    private int currentUseCount;
    private bool isInteracting;

    [Header("Indicator Light Settings")]
    [SerializeField] private Light2D indicatorLight;
    [SerializeField] private bool initialIndicatorStatus = true;
    [SerializeField] private bool useCustomColor = false;
    private bool indicatorLightActive;
    private bool savedIndicatorLightActive;

    public int CurrentUseCount => currentUseCount;
    public int MaxUses => maxUses;
    public bool LimitUses => limitUses;

    private int savedUseCount;

    protected virtual void Awake()
    {
        currentUseCount = 0;

        if (indicatorLight == null)
            indicatorLight = GetComponent<Light2D>();

        indicatorLightActive = initialIndicatorStatus;

        if (indicatorLight == null)
            return;

        if (useCustomColor)
        {
            return;
        }
        else
        {
            Color color = GetComponent<SpriteRenderer>().color;

            if (color == null)
                color = Color.white;

            indicatorLight.color = color;
        }

    }
    
    public void Interact()
    {
        if (isInteracting || !isActiveAndEnabled)
            return;

        if (limitUses && currentUseCount >= maxUses)
            return;

        if (Time.time < nextInteractionTime)
            return;

        isInteracting = true;

        bool succeeded;

        try
        {
            succeeded = OnInteract();
        }
        finally
        {
            isInteracting = false;
        }

        if (succeeded) onSuccessfulInteract?.Invoke();
        else return;

        nextInteractionTime = Time.time + interactionCooldown;
        
        if (limitUses)
        {
            currentUseCount++;

            if (currentUseCount >= maxUses)
            {
                OnUseLimitReached();
            }
        }
    }

    protected virtual void OnUseLimitReached()
    {
        showInteractionPrompt = false;

        if (indicatorLight != null)
        {
            indicatorLightActive = false;
            UpdateIndicatorLight();
        }
    }
    
    // Default trigger OnInteract() always returns true, do not call base.OnTrigger() in override functions
    protected virtual bool OnInteract()
    { 
        return true;
    }

    private void UpdateIndicatorLight()
    {
        if (indicatorLight == null)
            return;

        indicatorLight.enabled = indicatorLightActive;
    }

    public virtual void SaveState()
    {
        savedUseCount = currentUseCount;
        if (indicatorLight != null)
        {
            savedIndicatorLightActive = indicatorLightActive;
        }
    }

    public virtual void RestoreState()
    {
        currentUseCount = savedUseCount;

        if (currentUseCount < maxUses)
            showInteractionPrompt = true;

        if (indicatorLight != null)
        {
            indicatorLightActive = savedIndicatorLightActive;
            UpdateIndicatorLight();
        }
    }
}
