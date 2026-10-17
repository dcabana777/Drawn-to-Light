using UnityEngine;

public class WorldStateLayerSwitcher : MonoBehaviour
{
    [Header("World-State Sampling")]
    [SerializeField] private Transform worldStateProbe;

    [Header("Physics Layer Targets")]
    [Tooltip("Only these objects have their GameObject layer changed. " +
        "Put objects containing the relevant physics colliders here. " +
        "Leave visual children out so they can remain SharedVisual.")]
    [SerializeField] private GameObject[] layerTargets;

    [Header("Layer Names")]
    [SerializeField] private string darkLayerName = "DarkPlayer";
    [SerializeField] private string lightLayerName = "LightPlayer";

    [Header("Transition Thresholds")]
    [SerializeField, Range(0f, 1f)] private float enterLightAt = 0.5f;
    [SerializeField, Range(0f, 1f)] private float enterDarkAt = 0.5f;

    public WorldState CurrentState { get; private set; }

    public float CurrentLightAmount { get; private set; }

    public int CurrentPhysicsLayer { get; private set; } = -1;

    private int darkLayer;
    private int lightLayer;


    private void Awake()
    {
        darkLayer = LayerMask.NameToLayer(darkLayerName);

        lightLayer = LayerMask.NameToLayer(lightLayerName);

        if (darkLayer < 0 || lightLayer < 0)
        {
            Debug.LogError("WorldStateLayerSwitcher could not find DarkPlayer and/or LightPlayer layers.", this);
        }

        if (worldStateProbe == null)
        {
            worldStateProbe = transform;
        }
    }


    private void Start()
    {
        EvaluateWorldState(forceApply: true);
    }


    private void FixedUpdate()
    {
        EvaluateWorldState(forceApply: false);
    }


    private void EvaluateWorldState(bool forceApply)
    {
        if (WorldStateManager.Instance == null)
            return;

        CurrentLightAmount = WorldStateManager.Instance.GetLightAmount(worldStateProbe.position);

        WorldState nextState = CurrentState;

        if (CurrentState == WorldState.Light)
        {
            if (CurrentLightAmount < enterDarkAt)
            {
                nextState = WorldState.Dark;
            }
        }
        else
        {
            if (CurrentLightAmount >= enterLightAt)
            {
                nextState = WorldState.Light;
            }
        }

        if (forceApply || nextState != CurrentState)
        {
            CurrentState = nextState;
            ApplyPhysicsLayer();
        }
    }


    private void ApplyPhysicsLayer()
    {
        int targetLayer = CurrentState == WorldState.Light ? lightLayer : darkLayer;

        CurrentPhysicsLayer = targetLayer;
        
        if (layerTargets == null || layerTargets.Length == 0)
        {
            gameObject.layer = targetLayer;
            return;
        }

        foreach (GameObject target in layerTargets)
        {
            if (target != null)
            {
                target.layer = targetLayer;
            }
        }
    }

    public string CurrentPhysicsLayerName
    {
        get
        {
            if (CurrentPhysicsLayer < 0)
                return "Unassigned";

            string layerName = LayerMask.LayerToName(CurrentPhysicsLayer);

            return string.IsNullOrEmpty(layerName) ? $"Layer {CurrentPhysicsLayer}" : layerName;
        }
    }
}