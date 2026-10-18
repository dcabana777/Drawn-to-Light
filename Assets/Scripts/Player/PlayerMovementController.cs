using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerState))]
public class PlayerMovementController : MonoBehaviour
{
    private PlayerState playerData;
    private Rigidbody2D rb;
    private PlayerInput playerInput;

    [SerializeField] private Transform facingRoot;
    private Vector3 facingRootOriginalPosition;
    [SerializeField] private float inputDeadzone = 0.2f;

    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 7.5f;
    [SerializeField] private float groundAcceleration = 50f;
    [SerializeField] private float groundDeceleration = 50f;
    [SerializeField] private float airAcceleration = 100f;
    [SerializeField] private float airDeceleration = 100f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpVelocity = 5f;
    [SerializeField] private float jumpHeightMultiplier = 1f;
    [SerializeField] private float coyoteTime = 0.5f;
    [SerializeField] private float jumpBufferTime = 0.5f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = 30f;
    [SerializeField] private float maximumFallSpeed = 300f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 checkBox = new Vector2(0.2f, 0.2f);
    [SerializeField] private LayerMask darkGroundLayers;
    [SerializeField] private LayerMask lightGroundLayers;

    private Vector2 moveInput;

    private float jumpHeightMultiplierActual;
    private float jumpVelocityActual => jumpVelocity * jumpHeightMultiplierActual;
    private float gravityActual => gravity * jumpHeightMultiplierActual;

    private bool jumpHeld;
    private bool isJumping;
    private float jumpBufferTimer;
    private float coyoteJumpTimer;

    private float acceleration;
    private float deceleration;

    private bool sprintHeld;

    private Vector2 velocity => rb.linearVelocity;

    private int currentPlayerLayer => gameObject.layer;
    private LayerMask groundLayer;

    private void Awake()
    {
        playerData = GetComponent<PlayerState>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        facingRootOriginalPosition = facingRoot.localPosition;
        jumpHeightMultiplierActual = jumpHeightMultiplier;

        UpdateGroundLayers();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.x > inputDeadzone)
        {
            moveInput.x = 1f;

            playerData.SetFacingDirection(1f);

            facingRoot.localPosition = new Vector3(
                facingRootOriginalPosition.x * playerData.FacingDirection,
                facingRoot.localPosition.y,
                facingRoot.localPosition.z
            );
        }
        else if (moveInput.x < -inputDeadzone)
        {
            moveInput.x = -1f;

            playerData.SetFacingDirection(-1f);

            facingRoot.localPosition = new Vector3(
                facingRootOriginalPosition.x * playerData.FacingDirection,
                facingRoot.localPosition.y,
                facingRoot.localPosition.z
            );
        }
        else
        {
            moveInput.x = 0f;
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpHeld = true;

            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpHeld = false;
        }
    }

    public void OnSprint(InputValue value)
    {
        // Can only start sprinting on the ground
        if (value.isPressed && playerData.CurrentMoveState == PlayerMovementState.Grounded)
            sprintHeld = true;
        else
            sprintHeld = false;
    }

    public void PerformGroundChecks()
    {
        UpdateGroundLayers();

        bool isGrounded = Physics2D.OverlapBox(groundCheck.position, checkBox, 0f, groundLayer);

        // If already grounded and still grounded, do nothing
        if (isGrounded && playerData.CurrentMoveState == PlayerMovementState.Grounded)
            return;

        // If already airborne and still airborne, do nothing
        if (!isGrounded && playerData.CurrentMoveState == PlayerMovementState.Airborne)
            return;

        if (isGrounded)
        {
            playerData.SetMovementState(PlayerMovementState.Grounded);

            acceleration = groundAcceleration;
            deceleration = groundDeceleration;

            // landing ends the current jump
            isJumping = false;

            // no longer need coyote time
            coyoteJumpTimer = 0f;
        }
        else
        {
            playerData.SetMovementState(PlayerMovementState.Airborne);

            acceleration = airAcceleration;
            deceleration = airDeceleration;

            // only give coyote time if the player did not become airborne via a jump
            if (!isJumping)
            {
                coyoteJumpTimer = coyoteTime;
            }
            else
            {
                coyoteJumpTimer = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerData.CurrentMoveState == PlayerMovementState.InputLocked)
            return;

        PerformGroundChecks();

        // Update timers
        if (coyoteJumpTimer > 0f)
            coyoteJumpTimer -= Time.fixedDeltaTime;

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.fixedDeltaTime;

        // --------
        // Sprint
        // --------
        float moveMultiplier = walkSpeed;

        if (sprintHeld)
        {
            moveMultiplier = sprintSpeed;
            jumpHeightMultiplierActual = jumpHeightMultiplier / 2;
        }
        else
        {
            jumpHeightMultiplierActual = jumpHeightMultiplier;
        }


        // --------
        // Jump
        // --------

        bool startedJumpThisStep = false;

        bool canJump = playerData.CurrentMoveState == PlayerMovementState.Grounded ||
            (playerData.CurrentMoveState == PlayerMovementState.Airborne && !isJumping && coyoteJumpTimer > 0f);

        if (canJump && jumpBufferTimer > 0f)
        {
            StartJump();
            startedJumpThisStep = true;
        }


        // Variable jump cut
        if (!startedJumpThisStep && isJumping && !jumpHeld && velocity.y > 0f)
        {
            SetVerticalVelocity(velocity.y / 2);
        }

        // --------------------------
        // Handle horizontal movement
        // --------------------------

        float targetSpeed = moveInput.x * moveMultiplier;

        float horizontalVelocity = velocity.x;

        // Determine if we are accelerating or decelerating
        float targetChangeRate;

        if (Mathf.Abs(targetSpeed) > Mathf.Abs(horizontalVelocity))
            targetChangeRate = acceleration;
        else
            targetChangeRate = deceleration;

        // calculate new velocity with Time.fixedDeltaTime
        horizontalVelocity = Mathf.MoveTowards(horizontalVelocity, targetSpeed, targetChangeRate * Time.fixedDeltaTime);

        SetHorizontalVelocity(horizontalVelocity);

        // ----------
        // Gravity
        // ----------

        if (playerData.CurrentMoveState == PlayerMovementState.Airborne)
        {
            float newVerticalVelocity = velocity.y - gravityActual * Time.fixedDeltaTime;

            newVerticalVelocity = Mathf.Max(newVerticalVelocity, -maximumFallSpeed);

            SetVerticalVelocity(newVerticalVelocity);
        }
    }

    public void UpdateGroundLayers()
    {
        if (currentPlayerLayer == 9)
            groundLayer = darkGroundLayers;
        else if (currentPlayerLayer == 10)
            groundLayer = lightGroundLayers;
        else
            groundLayer = darkGroundLayers;
    }

    private void SetHorizontalVelocity(float velocity)
    {
        rb.linearVelocity = new Vector2(velocity, rb.linearVelocity.y);
    }

    private void SetVerticalVelocity(float velocity)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, velocity);
    }

    private void StartJump()
    {
        //Debug.Log("Starting jump");

        SetVerticalVelocity(jumpVelocityActual);

        isJumping = true;

        jumpBufferTimer = 0f;
        coyoteJumpTimer = 0f;
    }
}