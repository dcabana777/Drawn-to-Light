using UnityEngine;

public class ControllableLight : LightState
{
    [Header("Light Rotation")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float minRotation = -60f;
    [SerializeField] private float maxRotation = 60f;
    private float currentRotation;
    private float savedRotation;

    [Header("Light Movement")]
    [SerializeField] private LightRail rail;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField, Range(0f, 1f)] private float startingRailPosition = 0.5f;
    private float currentRailDistance;
    private float savedRailDistance;

    [Header("Rotation Gizmo")]
    [SerializeField] private float rotationGizmoRadius;
    [SerializeField, Min(2)] private int rotationGizmoSegments = 20;
    [SerializeField] private float rotationGizmoOffset;

    protected override void Awake()
    {
        base.Awake();
        
        currentRotation = transform.localEulerAngles.z;

        if (currentRotation > 180f)
            currentRotation -= 360f;

        currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);

        if (rail != null)
        {
            rail.RebuildRail();

            currentRailDistance = startingRailPosition * rail.Length;

            transform.position = rail.GetPointAtDistance(currentRailDistance);
        }
    }


    public void HandleRemoteInput(int moveInput, int rotateInput, float deltaTime, bool ignoreIsOn)
    {
        if (!IsOn && !ignoreIsOn) return;
        
        HandleRotation(rotateInput, deltaTime);
        HandleMovement(moveInput, deltaTime);
    }

    private void HandleRotation(int input, float deltaTime)
    {
        currentRotation += input * rotationSpeed * deltaTime;

        currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);

        transform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);
    }

    private void HandleMovement(int input, float deltaTime)
    {
        if (rail == null) return;

        currentRailDistance += input * movementSpeed * deltaTime;

        currentRailDistance = Mathf.Clamp(currentRailDistance, 0f, rail.Length);

        transform.position = rail.GetPointAtDistance(currentRailDistance);
    }

    public override void SaveState()
    {
        base.SaveState();
        savedRailDistance = currentRailDistance;
        savedRotation = currentRotation;
    }

    public override void RestoreState()
    {
        base.RestoreState();
        currentRailDistance = savedRailDistance;
        currentRotation = savedRotation;
        transform.position = rail.GetPointAtDistance(currentRailDistance);
        transform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawRotationGizmos();
    }

    private void DrawRotationGizmos()
    {
        Vector3 center = transform.position;

        Quaternion parentRotation =
            transform.parent != null
                ? transform.parent.rotation
                : Quaternion.identity;

        // Lights point downward when their local rotation is 0.
        Vector3 baseDirection = Vector3.down;

        Vector3 minDirection =
            parentRotation *
            Quaternion.Euler(0f, 0f, minRotation) *
            baseDirection;

        Vector3 maxDirection =
            parentRotation *
            Quaternion.Euler(0f, 0f, maxRotation) *
            baseDirection;

        // Draw minimum and maximum boundary lines.
        Gizmos.DrawLine(
            center,
            center + minDirection * rotationGizmoRadius
        );

        Gizmos.DrawLine(
            center,
            center + maxDirection * rotationGizmoRadius
        );

        // Draw arc between them.
        Vector3 previousPoint =
            center + minDirection * rotationGizmoRadius;

        for (int i = 1; i <= rotationGizmoSegments; i++)
        {
            float t = i / (float)rotationGizmoSegments;

            float angle = Mathf.Lerp(
                minRotation,
                maxRotation,
                t
            );

            Vector3 direction =
                parentRotation *
                Quaternion.Euler(0f, 0f, angle) *
                baseDirection;

            Vector3 currentPoint =
                center + direction * rotationGizmoRadius;

            Gizmos.DrawLine(
                previousPoint,
                currentPoint
            );

            previousPoint = currentPoint;
        }
    }
}
