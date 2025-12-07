using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Class configuration")]
    [SerializeField] private PlayerClassData playerData;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    //Components
    private Rigidbody rb;
    private PlayerInputActions inputActions;

    //State
    private int currentJumpCount = 0;
    [SerializeField] bool isGrounded = false; //to debug
    private bool isJumpHeld = false;
    bool isRunning = false; 
     bool isDashing = false;
    bool canDash = true;
    private float nextDashTime = 0f;    
    private Vector2 moveInput;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Jump.canceled += OnJumpCanceled;
        inputActions.Player.Run.performed += OnRunPerformed; 
        inputActions.Player.Run.canceled += OnRunCancelled;
        inputActions.Player.Dash.performed += OnDashPerformed;
        inputActions.Player.Dash.canceled += OnDashCancelled;
    }

    void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJumpPerformed;
        inputActions.Player.Jump.canceled -= OnJumpCanceled;
        inputActions.Player.Run.performed -= OnRunPerformed;
        inputActions.Player.Run.canceled -= OnRunCancelled;
        inputActions.Player.Dash.performed -= OnDashCancelled;
        inputActions.Player.Dash.canceled -= OnDashCancelled;
        inputActions.Player.Disable();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (playerData != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();
    }

    void HandleInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;

        Vector3 checkPosition = groundCheck != null ? groundCheck.position : transform.position;
        isGrounded = Physics.CheckSphere(checkPosition, groundCheckRadius, groundLayer);
        if (isGrounded && !wasGrounded)
        {
            currentJumpCount = 0;
        }
    }

    void HandleMovement()
    {
        if (playerData == null) return;
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        float speed = isRunning ? playerData.runSpeed : playerData.moveSpeed;
        Vector3 targetVelocity = movement * speed;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 velocityXZ = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        Vector3 newVelocity = Vector3.MoveTowards(velocityXZ, targetVelocity, playerData.acceleration * Time.deltaTime);

        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
    }

    void TryJump()
    {
        if (playerData == null) return;

        if (currentJumpCount < playerData.maxJumps)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * playerData.jumpForce, ForceMode.Impulse);
            currentJumpCount++;
        }
    }

    void HandleGravity()
    {
        if (playerData == null) return;

        if (playerData.canSlowFall && isJumpHeld && rb.linearVelocity.y < 0 && !isGrounded)
        {
            float gravityReduction = Physics.gravity.y * (1f - playerData.slowFallGravityScale);
            rb.AddForce(Vector3.up * -gravityReduction * rb.mass, ForceMode.Force);
        }
    }

    void HandleDash()
    {
        if(playerData == null) return;
        if (isDashing && canDash)
        {
        Vector3 dashDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if (dashDirection == Vector3.zero)
            {
                dashDirection = Vector3.zero;
            }
        rb.AddForce(dashDirection * playerData.dashForce, ForceMode.Impulse);
        }
    }
    void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (canDash && Time.time >= nextDashTime) 
        { 
            isDashing = true;
            HandleDash();       
            nextDashTime = Time.time + playerData.dashCooldown;
        }
    }
    void OnDashCancelled(InputAction.CallbackContext context)
    {
        isDashing = false;
    }
    void OnRunPerformed(InputAction.CallbackContext context)
    {
        isRunning = true;
    }

    void OnRunCancelled(InputAction.CallbackContext context)
    {
        isRunning = false;
    }
    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        isJumpHeld = true;
        TryJump();
    }

    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isJumpHeld = false;
    }

    //gizmos pour la ground check
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 checkPos = groundCheck != null ? groundCheck.position : transform.position;
        Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
    }
}