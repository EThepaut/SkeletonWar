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
    [SerializeField] bool isGrounded = false;
    private bool isJumpHeld = false;
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
    }

    void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJumpPerformed;
        inputActions.Player.Jump.canceled -= OnJumpCanceled;
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
        Vector3 targetVelocity = movement * playerData.moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
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