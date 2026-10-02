using UnityEngine;

[RequireComponent(typeof(ControllableLight))]
public class ConditionalLightMovement : MonoBehaviour
{
    [Header("Conditions")]
    [SerializeField] private GameCondition[] requiredConditions;

    [Header("Movement")]
    [SerializeField, Range(-1, 1)]
    private int moveDirection = 0;

    [SerializeField, Range(-1, 1)]
    private int rotateDirection = 0;

    private ControllableLight controllableLight;

    private void Awake()
    {
        controllableLight = GetComponent<ControllableLight>();
    }

    private void Update()
    {
        if (!AllConditionsMet())
            return;

        controllableLight.HandleRemoteInput(moveDirection, rotateDirection, Time.deltaTime, true);
    }

    private bool AllConditionsMet()
    {
        // Don't allow an accidentally empty condition list
        // to make the light move forever.
        if (requiredConditions == null || requiredConditions.Length == 0)
            return false;
        
        foreach (GameCondition condition in requiredConditions)
        {
            if (condition == null || !condition.IsMet)
            {
                return false;
            }
        }

        return true;
    }
}