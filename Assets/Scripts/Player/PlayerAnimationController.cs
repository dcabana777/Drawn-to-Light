using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerState playerData;
    [SerializeField] private PlayerMovementController playerController;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");

    private static readonly int GroundedHash = Animator.StringToHash("Grounded");

    private static readonly int SprintingHash = Animator.StringToHash("Sprinting");

    private static readonly int InteractHash = Animator.StringToHash("Interact");

    private static readonly int LanternToggleHash = Animator.StringToHash("LanternToggle");


    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = playerData.transform.position;
    }

    private void Update()
    {
        float animationSpeed;

        if (playerData.CurrentMoveState == PlayerMovementState.InputLocked)
        {
            float horizontalMovement = Mathf.Abs(playerData.transform.position.x - previousPosition.x);

            animationSpeed = horizontalMovement / Mathf.Max(Time.deltaTime, 0.0001f);
        }
        else
        {
            animationSpeed = Mathf.Abs(playerController.Velocity.x);
        }

        animator.SetFloat(SpeedHash, animationSpeed);
        animator.SetFloat(VerticalSpeedHash, playerController.Velocity.y);

        // Treat InputLocked as Grounded for animation purposes
        bool grounded = playerData.CurrentMoveState != PlayerMovementState.Airborne;

        animator.SetBool(GroundedHash, grounded);
        animator.SetBool(SprintingHash, playerController.Sprinting);
    }

    public void PlayerInteract()
    {
        animator.SetTrigger(InteractHash);
    }

    public void PlayerLanternToggle()
    {
        animator.SetTrigger(LanternToggleHash);
    }
}
