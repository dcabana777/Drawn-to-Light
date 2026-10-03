using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameRoom initialRoom;
    [SerializeField] private GameObject player;

    public GameRoom CurrentRoom { get; private set; }
    public bool IsCameraTransitioning { get; private set; }
    public bool IsRoomTransitioning { get; private set; }

    public GameObject Player => player;
    private Transform mainCamera;

    public CheckPoint CurrentMajorCheckpoint;


    private void Awake()
    {
        CurrentRoom = initialRoom;

        Camera foundCamera = Camera.main;

        if (foundCamera != null)
        {
            mainCamera = foundCamera.transform;
        }
        else
        {
            Debug.LogError(
                "Could not find a camera tagged MainCamera!"
            );
        }
    }

    private void Start()
    {
        if (CurrentRoom == null)
        {
            Debug.LogError(
                "GameManager has no Initial Room assigned!"
            );

            return;
        }

        if (CurrentRoom.CameraAnchor != null)
        {
            // Initial placement should be immediate.
            StartCoroutine(MoveMainCameraToAnchor(CurrentRoom.CameraAnchor, 0.01f));
        }

        // Initial camera position also respects the room's
        // follow settings and boundaries.
        Vector3 startingCameraPosition = CurrentRoom.GetCameraPositionForFocus(player.transform.position);

        StartCoroutine(MoveMainCameraToPosition(startingCameraPosition,0f));

        ChangePlayerLightGroup(CurrentRoom.MainLightGroup);

        UpdateCurrentMajorCheckpoint(CurrentRoom.CurrentCheckPoint);
    }

    public IEnumerator MoveMainCameraToPosition(Vector3 targetPosition, float duration)
    {
        if (mainCamera == null)
        {
            Debug.LogError(
                "Cannot move camera because Main Camera is null."
            );

            yield break;
        }

        IsCameraTransitioning = true;

        Vector3 startingPos = mainCamera.position;

        // maintain camera's current z position
        targetPosition.z = startingPos.z;

        // no transition, move immediately
        if (duration <= 0f)
        {
            mainCamera.position = targetPosition;
            IsCameraTransitioning = false;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            mainCamera.position = Vector3.Lerp(startingPos, targetPosition, t);

            yield return null;
        }

        mainCamera.position = targetPosition;

        IsCameraTransitioning = false;
    }
    
    public IEnumerator MoveMainCameraToAnchor(Transform cameraAnchor, float duration)
    {
        if (mainCamera == null)
        {
            Debug.LogError(
                "Cannot move camera because Main Camera is null."
            );

            yield break;
        }

        if (cameraAnchor == null)
        {
            Debug.LogError(
                "Cannot move camera because CameraAnchor is null."
            );

            yield break;
        }

        yield return MoveMainCameraToPosition(cameraAnchor.position, duration);
    }

    public void ChangeCurrentRoom(GameRoom newRoom)
    {
        if (newRoom == null)
        {
            Debug.LogError(
                "Attempted to change to a null GameRoom."
            );

            return;
        }

        CurrentRoom = newRoom;

        ChangePlayerLightGroup(CurrentRoom.MainLightGroup);

        UpdateCurrentMajorCheckpoint(CurrentRoom.CurrentCheckPoint);
    }

    public void UpdateCurrentMajorCheckpoint(CheckPoint newCheckpoint)
    {
        if (CurrentMajorCheckpoint == null && newCheckpoint.Type == CheckPoint.CheckPointType.Major)
            CurrentMajorCheckpoint = newCheckpoint;
        
        if (newCheckpoint.Type == CheckPoint.CheckPointType.Major)
        {
            // don't update MostRecentMajorCheckPoint if higher than new checkpoint's priority
            if (CurrentMajorCheckpoint.CheckPointPriority > newCheckpoint.CheckPointPriority)
                return;

            CurrentMajorCheckpoint = newCheckpoint;
        }
    }

    public void SetRoomTransitioning(bool transitioning)
    {
        IsRoomTransitioning = transitioning;
    }

    public void RestoreCheckPoint()
    {
        if (CurrentMajorCheckpoint == null)
            return;
        if (CurrentMajorCheckpoint.ThingsToReset == null)
            return;

        if (!CurrentMajorCheckpoint.HasSaved)
        {
            Debug.LogError("Tried to reset a room when no save exists!");
            return;
        }

        IRoomResettable[] resettables = CurrentMajorCheckpoint.ThingsToReset;

        foreach (IRoomResettable resettable in resettables)
        {
            resettable.RestoreState();
        }
        
        // update current room and checkpoint, then respawn player at it
        ChangeCurrentRoom(CurrentMajorCheckpoint.CheckPointOwnerRoom);

        CurrentRoom.ChangeCheckPoint(CurrentMajorCheckpoint);

        RespawnPlayer();
    }
    
    public void RespawnPlayer()
    {
        if (player == null)
        {
            Debug.LogError(
                "GameManager has no Player assigned!"
            );

            return;
        }

        if (CurrentRoom == null || CurrentRoom.CurrentCheckPoint == null)
        {
            Debug.LogError(
                "Cannot respawn: current room or checkpoint is null."
            );

            return;
        }

        MovePlayer(CurrentRoom.CurrentCheckPoint.PlayerRespawnLocation);
        ChangePlayerLightGroup(CurrentRoom.CurrentCheckPoint.InitialLightGroup);
    }

    public void ChangePlayerLightGroup(int newGroup)
    {
        if (player == null)
        {
            Debug.LogError(
                "GameManager has no Player assigned!"
            );

            return;
        }

        LightController controller = player.GetComponent<LightController>();

        if (controller == null)
        {
            Debug.LogError(
                "Player does not have a LightController!"
            );

            return;
        }

        controller.ChangeLightGroup(newGroup);
    }

    public void MovePlayer(Vector2 newLocation)
    {
        if (player == null)
            return;
        
        player.transform.position = newLocation;
    }
}
