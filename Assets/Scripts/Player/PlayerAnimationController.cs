using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerState playerData;
    [SerializeField] private PlayerMovementController playerController;
    [SerializeField] private PlayerInput playerInput;

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip interactClip;
    [SerializeField] private AnimationClip lanternToggleClip;
    [SerializeField] private float extraLockTime = 0.03f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");

    private static readonly int GroundedHash = Animator.StringToHash("Grounded");

    private static readonly int SprintingHash = Animator.StringToHash("Sprinting");

    private static readonly int InteractHash = Animator.StringToHash("Interact");

    private static readonly int LanternToggleHash = Animator.StringToHash("LanternToggle");


    private Vector3 previousPosition;

    private bool isLocked = false;
    private bool wasInputLocked = false;
    private bool wasGameplayDeactivated = false;

    InputAction pauseAction;
    private Coroutine actionRoutine;

    private void Start()
    {
        previousPosition = playerData.transform.position;
        pauseAction = playerInput.actions["Pause"];
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

        previousPosition = playerData.transform.position;
    }

    public void PlayerInteract()
    {
        animator.SetTrigger(InteractHash);
        LockActionForAnimation(interactClip.length);
    }

    public void PlayerLanternToggle()
    {
        animator.SetTrigger(LanternToggleHash);
        LockActionForAnimation(lanternToggleClip.length);
    }

    public void LockActionForAnimation(float duration)
    {
        if (isLocked || playerData.CurrentMoveState == PlayerMovementState.InputLocked)
            return;

        isLocked = true;
        wasInputLocked = playerData.CurrentMoveState == PlayerMovementState.InputLocked;
        wasGameplayDeactivated = !playerInput.enabled;

        playerData.SetMovementState(PlayerMovementState.InputLocked);
        playerController.StopMovementForAnimation();

        playerInput.DeactivateInput();

        if (pauseAction != null)
            pauseAction.Disable();

        actionRoutine = StartCoroutine(UnlockAfterAnimation(duration + extraLockTime));
    }

    private IEnumerator UnlockAfterAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);

        actionRoutine = null;
        FinishAction();
    }

    private void FinishAction()
    {
        if (!isLocked)
            return;

        isLocked = false;

        // apply new player movement state if we were NOT InputLocked before
        if (!wasInputLocked)
            playerController.PerformGroundChecks();

        // re-enable inputs
        if (!wasGameplayDeactivated)
            playerInput.ActivateInput();
        if (pauseAction != null)
            pauseAction.Enable();
    }

    private void OnDisable()
    {
        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }

        if (isLocked)
            FinishAction();
    }
}
