using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ConditionGate : MonoBehaviour, IRoomResettable
{
    public enum ConditionGateType
    {
        All,
        Any,
        ExactlyOne
    }
    
    [Header("Requirements")]
    [SerializeField] private GameCondition[] requiredConditions;
    [SerializeField] private ConditionGateType gateType;
    [SerializeField] private bool invertResult = false;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Effects")]
    [SerializeField] private UnityEvent onMet;
    [SerializeField] private UnityEvent onLost;

    private bool conditionsWereMet;
    private bool hasTriggered;
    private bool savedHasTriggered;

    public bool CurrentResult { get; private set; }
    public bool HasTriggered => hasTriggered;
    public bool TriggerOnce => triggerOnlyOnce;
    public ConditionGateType GateType => gateType; 

    private void OnEnable()
    {
        foreach (GameCondition condition in requiredConditions)
        {
            if (condition != null)
            {
                condition.ConditionChanged += EvaluateConditions;
            }
        }

        EvaluateConditions();
    }

    private void OnDisable()
    {
        foreach (GameCondition condition in requiredConditions)
        {
            if (condition != null)
            {
                condition.ConditionChanged -= EvaluateConditions;
            }
        }
    }

    private void EvaluateConditions()
    {
        bool shouldBeMet = false;

        switch (gateType)
        {
            case ConditionGateType.All:
                shouldBeMet = EvaluateConditionsAll();
                break;
            case ConditionGateType.Any:
                shouldBeMet = EvaluateConditionsAny();
                break;
            case ConditionGateType.ExactlyOne:
                shouldBeMet = EvaluateConditionsOne();
                break;
        }

        if (invertResult)
            shouldBeMet = !shouldBeMet;

        if (shouldBeMet)
        {
            if (!conditionsWereMet && (!triggerOnlyOnce || !hasTriggered))
            {
                conditionsWereMet = true;
                hasTriggered = true;

                onMet?.Invoke();
            }
        }
        else
        {
            if (conditionsWereMet)
            {
                conditionsWereMet = false;

                if (!triggerOnlyOnce)
                {
                    onLost?.Invoke();
                }
            }
        }

        CurrentResult = shouldBeMet;
    }
    
    
    private bool EvaluateConditionsAll()
    {
        foreach (GameCondition condition in requiredConditions)
        {
            if (condition == null || !condition.IsMet)
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateConditionsAny()
    {
        foreach (GameCondition condition in requiredConditions)
        {
            if (condition != null && condition.IsMet)
            {
                return true;
            }
        }

        return false;
    }

    private bool EvaluateConditionsOne()
    {
        bool foundOne = false;

        foreach (GameCondition condition in requiredConditions)
        {
            if (condition != null && condition.IsMet)
            {
                if (!foundOne)
                    foundOne = true;
                else
                    return false;
            }
        }

        return foundOne;
    }

    public void SaveState()
    {
        savedHasTriggered = hasTriggered;
    }

    public void RestoreState()
    {
        hasTriggered = savedHasTriggered;
        EvaluateConditions();
    }
}
