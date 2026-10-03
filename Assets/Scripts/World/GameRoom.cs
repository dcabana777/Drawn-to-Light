using UnityEngine;

public class GameRoom : MonoBehaviour
{
    private GameManager gm;
    
    [Header("Room Details")]
    [Min(1)] public int RoomID;
    [SerializeField] private string roomName;
    public string RoomName => roomName;
    [SerializeField] private CheckPoint initialCheckPoint;
    [SerializeField] private int primaryControllableLightGroup;

    public int MainLightGroup { get; private set; }
    public CheckPoint CurrentCheckPoint { get; private set; }
    public Transform CameraAnchor { get; private set; }

    [Header("Camera Follow")]
    [SerializeField] private bool followPlayerHorizontally;
    [SerializeField] private bool followPlayerVertically;

    [Header("Camera Boundaries - Relative to GameRoom")]
    [SerializeField] private float leftCameraBoundary;
    [SerializeField] private float rightCameraBoundary;
    [SerializeField] private float upperCameraBoundary;
    [SerializeField] private float lowerCameraBoundary;

    [Header("Camera Gizmo")]
    [SerializeField] private float previewOrthographicSize = 10f;
    [SerializeField] private float previewAspectRatio = 16f / 9f;
    [SerializeField] private Color previewCenterColor = Color.yellow;
    [SerializeField] private Color previewBoundaryColor = Color.red;

    public bool FollowPlayerHorizontally => followPlayerHorizontally;
    public bool FollowPlayerVertically => followPlayerVertically;
    public float LeftCameraBoundary =>
        GetCameraAnchorPosition().x + leftCameraBoundary;
    public float RightCameraBoundary =>
        GetCameraAnchorPosition().x + rightCameraBoundary;
    public float UpperCameraBoundary =>
        GetCameraAnchorPosition().y + upperCameraBoundary;
    public float LowerCameraBoundary =>
        GetCameraAnchorPosition().y + lowerCameraBoundary;

    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        CurrentCheckPoint = initialCheckPoint;
        MainLightGroup = primaryControllableLightGroup;
        
        CameraAnchor = transform.Find("CameraAnchor");
        if (CameraAnchor == null)
        {
            Debug.LogWarning(
                gameObject.name + "could not find CameraAnchor in room: " + RoomID + "!"
            );
        }

        if (CurrentCheckPoint == null)
        {
            Debug.LogWarning(
                "Room " + RoomID + " has no initial checkpoint assigned!"
            );
        }
    }

    public Vector3 GetCameraPositionForFocus(Vector2 focusPosition)
    {
        Vector3 anchorPosition = GetCameraAnchorPosition();

        float targetX = anchorPosition.x;
        float targetY = anchorPosition.y;

        if (followPlayerHorizontally)
        {
            targetX = Mathf.Clamp(
                focusPosition.x,
                Mathf.Min(LeftCameraBoundary, RightCameraBoundary),
                Mathf.Max(LeftCameraBoundary, RightCameraBoundary)
            );
        }

        if (followPlayerVertically)
        {
            targetY = Mathf.Clamp(
                focusPosition.y,
                Mathf.Min(LowerCameraBoundary, UpperCameraBoundary),
                Mathf.Max(LowerCameraBoundary, UpperCameraBoundary)
            );
        }

        return new Vector3(
            targetX,
            targetY,
            anchorPosition.z
        );
    }

    public void ChangeCheckPoint(CheckPoint checkpoint)
    {
        CurrentCheckPoint = checkpoint;

        gm.UpdateCurrentMajorCheckpoint(checkpoint);
    }

    private void OnDrawGizmos()
    {
        DrawCameraBoundsGizmo();
    }

    private void DrawCameraBoundsGizmo()
    {
        Vector3 anchorPosition =
            GetCameraAnchorPosition();

        float halfHeight;
        float halfWidth;

        Camera camera = Camera.main;

        if (camera != null && camera.orthographic)
        {
            halfHeight =
                camera.orthographicSize;

            FixedAspectCamera fixedAspect = camera.GetComponent<FixedAspectCamera>();

            float aspectRatio = fixedAspect != null ? fixedAspect.TargetAspect : previewAspectRatio;

            halfWidth =
                camera.orthographicSize *
                aspectRatio;
        }
        else
        {
            halfHeight =
                previewOrthographicSize;

            halfWidth =
                previewOrthographicSize *
                previewAspectRatio;
        }

        // Convert LOCAL boundary values into WORLD positions.
        float worldLeft =
            anchorPosition.x + leftCameraBoundary;

        float worldRight =
            anchorPosition.x + rightCameraBoundary;

        float worldBottom =
            anchorPosition.y + lowerCameraBoundary;

        float worldTop =
            anchorPosition.y + upperCameraBoundary;

        // Yellow rectangle:
        // area where camera CENTER may move.
        Vector3 centerBoundsCenter =
            new Vector3(
                (worldLeft + worldRight) * 0.5f,
                (worldBottom + worldTop) * 0.5f,
                0f
            );

        Vector3 centerBoundsSize =
            new Vector3(
                worldRight - worldLeft,
                worldTop - worldBottom,
                0f
            );

        // Red rectangle:
        // complete area that could ever appear onscreen.
        float visibleLeft =
            worldLeft - halfWidth;

        float visibleRight =
            worldRight + halfWidth;

        float visibleBottom =
            worldBottom - halfHeight;

        float visibleTop =
            worldTop + halfHeight;

        Vector3 visibleBoundsCenter =
            new Vector3(
                (visibleLeft + visibleRight) * 0.5f,
                (visibleBottom + visibleTop) * 0.5f,
                0f
            );

        Vector3 visibleBoundsSize =
            new Vector3(
                visibleRight - visibleLeft,
                visibleTop - visibleBottom,
                0f
            );

        Gizmos.color = previewCenterColor;

        Gizmos.DrawWireCube(
            centerBoundsCenter,
            centerBoundsSize
        );

        Gizmos.color = previewBoundaryColor;

        Gizmos.DrawWireCube(
            visibleBoundsCenter,
            visibleBoundsSize
        );
    }

    private Vector3 GetCameraAnchorPosition()
    {
        // Runtime: use cached reference
        if (CameraAnchor != null)
            return CameraAnchor.position;

        // Editor/Gizmo fallback as Awake may not have occured yet
        Transform foundAnchor = transform.Find("CameraAnchor");

        if (foundAnchor != null)
            return foundAnchor.position;

        // fallback case
        return transform.position;
    }
}
