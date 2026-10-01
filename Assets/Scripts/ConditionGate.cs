using UnityEngine;
using UnityEngine.Events;

public class ConditionGate : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField] private GameCondition[] requiredConditions;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnlyOnce = true;
    [Tooltip("How often whileMet and whileNotMet events are invoked. If set to 0 will call every frame.")]

    [Header("Effects")]
    [SerializeField] private UnityEvent onMet;
    [SerializeField] private UnityEvent onLost;

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
    }
}
