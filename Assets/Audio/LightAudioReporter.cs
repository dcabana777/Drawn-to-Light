using UnityEngine;

[RequireComponent(typeof(LightState))]
public class LightAudioReporter : MonoBehaviour
{
    private LightState lightState;

    [Header("Volumes")]
    [SerializeField, Min(0f)] private float activeVolume = 0.5f;
    [SerializeField, Min(0f)] private float movementVolume = 0.25f;
    [SerializeField, Min(0f)] private float rotationVolume = 0.25f;

    [Header("Detection")]
    [SerializeField, Min(0f)] private float movementThreshold = 0.001f;
    [SerializeField, Min(0f)] private float rotationThreshold = 0.01f;
    [Tooltip("How long movement/rotation remains considered active after the last detected change.")]
    [SerializeField, Min(0f)] private float activityGraceTime = 0.12f;

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private float movementTimer;
    private float rotationTimer;

    public bool CanReport { get; private set; }

    private void Awake()
    {
        lightState = GetComponent<LightState>();
        CanReport = false;
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;

        movementTimer = 0f;
        rotationTimer = 0f;
    }

    private void LateUpdate()
    {
        if (GameAudioManager.Instance == null)
            return;

        bool lightOn = lightState != null && lightState.IsOn;

        bool movedThisFrame = Vector3.Distance(transform.position, previousPosition) > movementThreshold;
        bool rotatedThisFrame = Quaternion.Angle(transform.rotation, previousRotation) > rotationThreshold;

        if (movedThisFrame)
            movementTimer = activityGraceTime;
        else
            movementTimer -= Time.deltaTime;

        if (rotatedThisFrame)
            rotationTimer = activityGraceTime;
        else
            rotationTimer -= Time.deltaTime;

        bool moving = movementTimer > 0f;
        bool rotating = rotationTimer > 0f;

        GameAudioManager.Instance.SetLoopRequest(SharedLoopType.LightFlicker, this, CanReport && lightOn, activeVolume);

        GameAudioManager.Instance.SetLoopRequest(SharedLoopType.Movement, this, CanReport && moving, movementVolume);

        GameAudioManager.Instance.SetLoopRequest(SharedLoopType.Rotation, this, CanReport && rotating, rotationVolume);
        
        previousPosition = transform.position;

        previousRotation = transform.rotation;
    }

    private void OnDisable()
    {
        movementTimer = 0f;
        rotationTimer = 0f;

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.RemoveAllRequests(this);
        }
    }

    public void SetReportable(bool canReport)
    {
        CanReport = canReport;
    }
}
