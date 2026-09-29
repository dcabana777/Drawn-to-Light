using UnityEngine;
using UnityEngine.Events;

public class ConditionGate : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField] private GameCondition[] requiredConditions;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Effects")]
    [SerializeField] private UnityEvent onConditionsMet;
    [SerializeField] private UnityEvent onConditionsLost;

    private bool conditionsWereMet;
    private bool hasTriggered;

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
        bool allMet = true;

        foreach (GameCondition condition in requiredConditions)
        {
            if (condition == null || !condition.IsMet)
            {
                allMet = false;
                break;
            }
        }

        if (allMet)
        {
            if (!conditionsWereMet && (!triggerOnlyOnce || !hasTriggered))
            {
                conditionsWereMet = true;
                hasTriggered = true;

                onConditionsMet?.Invoke();
            }
        }
        else
        {
            if (conditionsWereMet)
            {
                conditionsWereMet = false;

                if (!triggerOnlyOnce)
                {
                    onConditionsLost?.Invoke();
                }
            }
        }
    }
}
