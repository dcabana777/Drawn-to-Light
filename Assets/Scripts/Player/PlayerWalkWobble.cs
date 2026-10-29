using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerWalkWobble : MonoBehaviour
{
    [Header("Wobble Limits (Degrees)")]
    [SerializeField, Min(0f)] private float rightWobbleLimit = 5f;
    [SerializeField, Min(0f)] private float leftWobbleLimit = 5f;

    [Header("Wobble Speed (Cycles Per Second)")]
    [SerializeField, Min(0f)] private float walkWobbleSpeed = 2f;
    [SerializeField, Min(0f)] private float sprintWobbleSpeed = 3f;

    [Header("Sprint Detection")]
    [SerializeField] private float sprintSpeedThreshold = 6.25f;

    private Animator animator;
    private Quaternion originalRotation;
    private float wobbleTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        originalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // During transitions, check the incoming state.
        if (animator.IsInTransition(0))
        {
            state = animator.GetNextAnimatorStateInfo(0);
        }

        bool isWalking = state.IsName("Base Layer.Player_Walk");

        bool isSprinting = state.IsName("Base Layer.Player_Sprint");

        // No wobble in any other animation.
        if (!isWalking && !isSprinting)
        {
            wobbleTime = 0f;
            transform.localRotation = originalRotation;
            return;
        }

        float movementSpeed = animator.GetFloat("Speed");

        // Do not wobble if we're standing still.
        if (movementSpeed <= 0.1f)
        {
            wobbleTime = 0f;
            transform.localRotation = originalRotation;
            return;
        }

        // Drawn to Light currently uses the Walk
        // animation for both walking and sprinting.
        bool useSprintSpeed = isSprinting || movementSpeed >= sprintSpeedThreshold;

        float frequency = useSprintSpeed ? sprintWobbleSpeed : walkWobbleSpeed;

        wobbleTime += Time.deltaTime * frequency;

        float wave = Mathf.Sin(wobbleTime * Mathf.PI * 2f);

        float angle = wave >= 0f ? wave * rightWobbleLimit : wave * leftWobbleLimit;

        transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
    }
}
