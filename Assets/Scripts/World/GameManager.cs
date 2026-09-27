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
    }

    public void SetRoomTransitioning(bool transitioning)
    {
        IsRoomTransitioning = transitioning;
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
