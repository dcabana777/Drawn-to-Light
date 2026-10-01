using UnityEngine;

public class AutomaticLight : LightState
{
    [Header("Automatic Movement")]
    [SerializeField] private bool moveAutomatically = true;
    [SerializeField] private bool keepMovingWhileOff = false;
    [SerializeField] private LightRail rail;
    [SerializeField] private float movementSpeed = 3f;

    [SerializeField, Range(0f, 1f)] private float startingRailPosition = 0.5f;

    [SerializeField] private bool startMovingTowardEnd = true;

    private float currentRailDistance;
    private float savedRailDistance;
    private int movementDirection;
    private int savedMoveDirection;

    [Header("Automatic Rotation")]
    [SerializeField] private bool rotateAutomatically = true;
    [SerializeField] private float rotationSpeed = 60f;

    [SerializeField] private float minRotation = -60f;
    [SerializeField] private float maxRotation = 60f;

    [SerializeField] private float startingRotation = 0f;

    [SerializeField] private bool startRotatingTowardMax = true;

    private float currentRotation;
    private float savedRotation;
    private int rotationDirection;
    private int savedRotateDirection;

    [Header("Rotation Gizmo")]
    [SerializeField] private float rotationGizmoRadius = 2f;
    [SerializeField, Min(2)] private int rotationGizmoSegments = 20;

    protected override void Awake()
    {
        base.Awake();

        movementDirection = startMovingTowardEnd ? 1 : -1;

        rotationDirection = startRotatingTowardMax ? 1 : -1;

        currentRotation = Mathf.Clamp(startingRotation, minRotation, maxRotation);

        transform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);

        if (rail != null)
        {
            rail.RebuildRail();

            currentRailDistance = startingRailPosition * rail.Length;

            transform.position = rail.GetPointAtDistance(currentRailDistance);
        }
    }

    private void Update()
    {
        if (!IsOn && !keepMovingWhileOff) return;

        float deltaTime = Time.deltaTime;

        if (moveAutomatically) HandleMovement(deltaTime);

        if (rotateAutomatically) HandleRotation(deltaTime);
    }

    private void HandleMovement(float deltaTime)
    {
        if (rail == null || rail.Length <= 0f){
            return;
        }

        currentRailDistance += movementDirection * movementSpeed * deltaTime;

        if (rail.AutomaticEndBehavior == LightRail.AutomaticRailEndBehavior.Loop)
        {
            // preserve overshoot so movement remains smooth
            if (currentRailDistance > rail.Length)
                currentRailDistance -= rail.Length;
            else if (currentRailDistance < 0f)
                currentRailDistance += rail.Length;
        }
        
        else
        {
            if (currentRailDistance >= rail.Length)
            {
                currentRailDistance = rail.Length;
                movementDirection = -1;
            }
            else if (currentRailDistance <= 0f)
            {
                currentRailDistance = 0f;
                movementDirection = 1;
            }
        }

        transform.position = rail.GetPointAtDistance(currentRailDistance);
    }

    private void HandleRotation(float deltaTime)
    {
        currentRotation += rotationDirection * rotationSpeed * deltaTime;

        if (currentRotation >= maxRotation)
        {
            currentRotation = maxRotation;
            rotationDirection = -1;
        }
        else if (currentRotation <= minRotation)
        {
            currentRotation = minRotation;
            rotationDirection = 1;
        }

        transform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);
    }

    public override void SaveState()
    {
        base.SaveState();
        savedRailDistance = currentRailDistance;
        savedMoveDirection = movementDirection;
        savedRotateDirection = rotationDirection;
        savedRotation = currentRotation;
    }

    public override void RestoreState()
    {
        base.RestoreState();
        currentRailDistance = savedRailDistance;
        movementDirection = savedMoveDirection;
        currentRotation = savedRotation;
        rotationDirection = savedRotateDirection;
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

        // Your lights point downward at 0 degrees.
        Vector3 baseDirection = Vector3.down;

        Vector3 minDirection =
            parentRotation *
            Quaternion.Euler(
                0f,
                0f,
                minRotation
            ) *
            baseDirection;

        Vector3 maxDirection =
            parentRotation *
            Quaternion.Euler(
                0f,
                0f,
                maxRotation
            ) *
            baseDirection;

        Gizmos.DrawLine(
            center,
            center +
            minDirection * rotationGizmoRadius
        );

        Gizmos.DrawLine(
            center,
            center +
            maxDirection * rotationGizmoRadius
        );

        Vector3 previousPoint =
            center +
            minDirection * rotationGizmoRadius;

        for (int i = 1;
             i <= rotationGizmoSegments;
             i++)
        {
            float t =
                i / (float)rotationGizmoSegments;

            float angle =
                Mathf.Lerp(
                    minRotation,
                    maxRotation,
                    t
                );

            Vector3 direction =
                parentRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                ) *
                baseDirection;

            Vector3 currentPoint =
                center +
                direction * rotationGizmoRadius;

            Gizmos.DrawLine(
                previousPoint,
                currentPoint
            );

            previousPoint = currentPoint;
        }
    }
}