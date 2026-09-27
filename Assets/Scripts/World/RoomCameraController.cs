using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RoomCameraController : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.15f;

    private GameManager gm;
    private Vector3 smoothVelocity;

    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError("RoomCameraController could not find GameManager!");
        }
    }

    private void LateUpdate()
    {
        if (gm == null || gm.CurrentRoom == null || player == null)
        {
            return;
        }

        // don't fight with a fixed transition camera movement
        if (gm.IsCameraTransitioning)
            return;

        Vector3 targetPosition = gm.CurrentRoom.GetCameraPositionForFocus(player.position);

        targetPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref smoothVelocity, smoothTime);
    }
}
