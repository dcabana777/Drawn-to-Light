using UnityEngine;

public class WorldStateCondition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lightProbe;
    [SerializeField] private GameCondition condition;

    [Header("Light Thresholds")]
    [SerializeField, Range(0f, 1f)] private float enterLightAt = 0.55f;
    [SerializeField, Range(0f, 1f)] private float enterDarkAt = 0.45f;

    [Header("Output")]
    [SerializeField] private bool meetConditionWhenLit = true;

    public WorldState CurrentState { get; private set; }

    private float currentLightAmount;

    private void Awake()
    {
        if (lightProbe == null)
            lightProbe = gameObject.transform;
    }



    private void Start()
    {
        CurrentState = WorldState.Dark;
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

        currentLightAmount = WorldStateManager.Instance.GetLightAmount(lightProbe.position);

        WorldState nextState = CurrentState;

        if (CurrentState == WorldState.Light)
        {
            if (currentLightAmount < enterDarkAt)
            {
                nextState = WorldState.Dark;
            }
        }
        else
        {
            if (currentLightAmount >= enterLightAt)
            {
                nextState = WorldState.Light;
            }
        }

        if (forceApply || nextState != CurrentState)
        {
            CurrentState = nextState;
            UpdateCondition();

            Debug.Log("Current State: " + CurrentState.ToString(), this);
        }
    }

    private void UpdateCondition()
    {
        if (CurrentState == WorldState.Light)
        {
            condition.SetCondition(meetConditionWhenLit);
        }
        else if (CurrentState == WorldState.Dark)
        {
            condition.SetCondition(!meetConditionWhenLit);
        }
    }
}
