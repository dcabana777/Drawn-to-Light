using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerState))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerState playerData;
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction jumpAction;

    private Vector2 moveInput;

    [SerializeField] private Transform facingRoot;
    private Vector3 facingRootOriginalPosition;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float inputDeadzone = 0.2f;
    [SerializeField] private float jumpPower = 5f;

    [Header("Optional Jump Settings")]
    [SerializeField] private bool enableHoldJump = false;
    [SerializeField] private bool enableLowGravityJump = false;

    [Header("Hold Jump Settings")]
    [SerializeField] private float holdJumpForce = 10f;
    [SerializeField] private float maxJumpHoldTime = 0.2f;

    [Header("Low Gravity Jump Settings")]
    [SerializeField, Range(0f, 1f)] private float lowGravityMultiplier = 0.5f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 checkBox = new Vector2(0.2f, 0.2f);
    [SerializeField] private LayerMask darkGroundLayers;
    [SerializeField] private LayerMask lightGroundLayers;

    private bool holdJumpAvailable;
    private bool jumpInProgress;
    private bool hasStartedRising;
    private float jumpHoldTimer;

    private int currentPlayerLayer => gameObject.layer;
    private LayerMask groundLayer;

    private float normalGravityScale;

    private void Awake()
    {
        playerData = GetComponent<PlayerState>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        jumpAction = playerInput.actions["Jump"];

        normalGravityScale = rb.gravityScale;
        facingRootOriginalPosition = facingRoot.localPosition;

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
        if (value.isPressed && playerData.IsGrounded)
        {
            rb.AddForce(
                Vector2.up * jumpPower,
                ForceMode2D.Impulse
            );

            jumpInProgress = true;
            hasStartedRising = false;

            jumpHoldTimer = 0f;
            holdJumpAvailable = enableHoldJump;
        }
    }

    private void Update()
    {
        UpdateGroundLayers();
        
        bool grounded = Physics2D.OverlapBox(
            groundCheck.position,
            checkBox,
            0f,
            groundLayer
        );

        playerData.SetGrounded(grounded);
    }

    private void FixedUpdate()
    {
        // Horizontal movement
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );

        // Record when the jump has actually started rising.
        if (jumpInProgress && rb.linearVelocity.y > 0f)
        {
            hasStartedRising = true;
        }

        // TOGGLEABLE: Hold jump
        if (
            enableHoldJump &&
            holdJumpAvailable &&
            jumpAction.IsPressed() &&
            jumpInProgress &&
            rb.linearVelocity.y > 0f &&
            jumpHoldTimer < maxJumpHoldTime
        )
        {
            rb.AddForce(
                Vector2.up * holdJumpForce,
                ForceMode2D.Force
            );

            jumpHoldTimer += Time.fixedDeltaTime;

            if (jumpHoldTimer >= maxJumpHoldTime)
            {
                holdJumpAvailable = false;
            }
        }

        if (!jumpAction.IsPressed())
        {
            holdJumpAvailable = false;
        }

        // TOGGLEABLE: Reduced gravity while rising.
        if (
            enableLowGravityJump &&
            jumpInProgress &&
            rb.linearVelocity.y > 0f
        )
        {
            rb.gravityScale =
                normalGravityScale * lowGravityMultiplier;
        }
        else
        {
            rb.gravityScale = normalGravityScale;
        }

        // Apex reached.
        if (
            jumpInProgress &&
            hasStartedRising &&
            rb.linearVelocity.y <= 0f
        )
        {
            jumpInProgress = false;
            holdJumpAvailable = false;
            rb.gravityScale = normalGravityScale;
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
}