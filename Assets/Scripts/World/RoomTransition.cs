using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomTransition : MonoBehaviour
{
    public enum RoomTransitionType
    {
        Pan,
        Transfer
    }
    
    private GameManager gm;

    [Header("Connected Rooms")]
    [SerializeField] private GameRoom transitionRoomOne;
    [SerializeField] private GameRoom transitionRoomTwo;

    [Header("Transition Settings")]
    [SerializeField] private RoomTransitionType transitionType;
    [SerializeField] private float transitionDuration = 1.5f;

    [Tooltip("Optional. If assigned, this position is used instead of the transition objects' " +
        "own position when deciding the camera framing for a Pan transition.")]
    [SerializeField] private Transform panCameraFocusOverride;
    
    [Header("Transfer Entry Points")]
    [Tooltip("Position just outside Room One, before the player walks in.")]
    [SerializeField] private Transform roomOneEntryStart;

    [Tooltip("Position inside Room One where control is restored.")]
    [SerializeField] private Transform roomOneEntryEnd;

    [Tooltip("Position just outside Room Two, before the player walks in.")]
    [SerializeField] private Transform roomTwoEntryStart;

    [Tooltip("Position inside Room Two where control is restored.")]
    [SerializeField] private Transform roomTwoEntryEnd;

    [Header("Transfer Movement")]
    [SerializeField] private float playerEntrySpeed = 3f;

    private void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError(
                "RoomTransition could not find GameManager!"
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || gm == null)
        {
            return;
        }

        if (gm.IsRoomTransitioning) return;
        
        GameRoom currentRoom = gm.CurrentRoom;
        GameRoom newRoom;
        Transform destinationEntryStart;
        Transform destinationEntryEnd;

        if (currentRoom == transitionRoomOne)
        {
            newRoom = transitionRoomTwo;
            destinationEntryStart = roomTwoEntryStart;
            destinationEntryEnd = roomTwoEntryEnd;
        }
        else if (currentRoom == transitionRoomTwo)
        {
            newRoom = transitionRoomOne;
            destinationEntryStart = roomOneEntryStart;
            destinationEntryEnd = roomOneEntryEnd;
        }
        else
        {
            Debug.LogWarning("Entered room transition from a room not connected to it.");
            return;
        }

        if (transitionType == RoomTransitionType.Pan)
        {
            gm.ChangeCurrentRoom(newRoom);

            if (newRoom.CameraAnchor != null)
            {
                StartCoroutine(PerformPanTransition(newRoom));
            }
        }
        else
        {
            StartCoroutine(PerformTransferTransition(other, newRoom, destinationEntryStart, destinationEntryEnd));
        }
    }

    private IEnumerator PerformPanTransition(GameRoom newRoom)
    {
        gm.SetRoomTransitioning(true);

        // transition itself defines where the doorway is
        Vector2 focusPosition = panCameraFocusOverride != null ? panCameraFocusOverride.position : transform.position;

        Vector3 cameraTarget = newRoom.GetCameraPositionForFocus(focusPosition);

        gm.ChangeCurrentRoom(newRoom);

        yield return gm.MoveMainCameraToPosition(cameraTarget, transitionDuration);

        gm.SetRoomTransitioning(false);
    }
    
    
    private IEnumerator PerformTransferTransition(Collider2D playerCollider, GameRoom newRoom, Transform entryStart, Transform entryEnd)
    {
        if (entryStart == null || entryEnd == null)
        {
            Debug.LogError("Transfer transition is missing an entry point!");

            yield break;
        }

        gm.SetRoomTransitioning(true);

        PlayerInput playerInput = playerCollider.GetComponentInParent<PlayerInput>();

        Rigidbody2D rb = playerCollider.attachedRigidbody;

        RigidbodyType2D originalBodyType = RigidbodyType2D.Dynamic;

        // disable player actions
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }

        // stop player physics
        if (rb != null)
        {
            originalBodyType = rb.bodyType;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        gm.ChangeCurrentRoom(newRoom);

        // move player to just outside destination room
        gm.MovePlayer(entryStart.position);

        // destination entry points determines how to initially frame new room
        Vector3 cameraTarget = newRoom.GetCameraPositionForFocus(entryEnd.position);
        
        // move camera
        yield return gm.MoveMainCameraToPosition(cameraTarget, transitionDuration);

        // walk player from outside the room to the inside point
        // skipped if Start and End are identical
        while (
            Vector2.Distance(playerCollider.transform.position, entryEnd.position) > 0.01f
        )
        {
            Vector2 newPosition = 
                Vector2.MoveTowards(playerCollider.transform.position, entryEnd.position, playerEntrySpeed * Time.deltaTime);

            gm.MovePlayer(newPosition);

            yield return null;
        }

        gm.MovePlayer(entryEnd.position);

        // restore normal physics
        if (rb != null)
        {
            rb.bodyType = originalBodyType;
            rb.linearVelocity = Vector2.zero;
        }

        // restore player input
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }

        gm.SetRoomTransitioning(false);
    }
}
