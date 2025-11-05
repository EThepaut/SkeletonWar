using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerCubeController : NetworkTransform
{
#if UNITY_EDITOR
    // These bool properties ensure that any expanded or collapsed property views
    // within the inspector view will be saved and restored the next time the
    // asset/prefab is viewed.
    public bool PlayerCubeControllerPropertiesVisible;
#endif

    [Header("Movement Settings")]
    public float Speed = 10f;
    public bool ApplyVerticalInputToZAxis;

    [Header("Camera Settings")]
    public float mouseSensitivity = 0.1f;
    public Camera playerCamera;

    [Header("Jump & Gravity Settings")]
    public float gravity = 9.81f * 2;
    public float jumpHeight = 2f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundLayerMask;

    [Header("Visual")]
    [SerializeField] public GameObject cube;

    // Private variables
    private Vector3 m_Motion;
    private CharacterController controller;
    private float xRotation = 0;
    private float yRotation = 0;
    private bool isCameraLocked = false;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 lastPosition = new Vector3(0, 0, 0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            InitializeForOwner();
        }
    }

    private void InitializeForOwner()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cube != null)
            cube.GetComponent<Renderer>().material.color = Color.red;
    }

    private void Update()
    {
        // If not spawned or we don't have authority, then don't update
        if (!IsSpawned || !IsOwner)
        {
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        HandleCameraLockToggle();
        HandleMouseLook();
        HandleShoot();
        HandleMovement();
        HandleGravityAndJump();
        CheckMovement();
    }

    private void HandleCameraLockToggle()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            isCameraLocked = !isCameraLocked;
            Cursor.lockState = isCameraLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isCameraLocked;
        }
    }

    private void HandleMouseLook()
    {
        Vector2 look = Mouse.current.delta.ReadValue() * mouseSensitivity;

        yRotation += look.x;
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        xRotation -= look.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleShoot()
    {
        if (playerCamera == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Weapon weapon = GetComponentInChildren<Weapon>();
            if (weapon != null)
            {
                weapon.Fire();
            }
            else
            {
                Debug.LogWarning("Aucun composant Weapon trouvé sur le joueur");
            }
        }
    }

    private void HandleMovement()
    {
        // Scene switching logic
        if (Keyboard.current.nKey.isPressed)
        {
            int sceneLvl = SceneManager.GetActiveScene().buildIndex;
            if (sceneLvl != 3)
                SceneManager.LoadScene("Stage3p", LoadSceneMode.Single);
            else
                SceneManager.LoadScene("Stage2p", LoadSceneMode.Single);
        }

        // Movement input
        m_Motion = Vector3.zero;

        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

        if (moveInput.magnitude > 1)
            moveInput.Normalize();

        // Apply movement based on configuration
        if (!ApplyVerticalInputToZAxis)
        {
            // Standard FPS movement (WASD controls X and Z)
            Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y);
            if (controller != null)
            {
                controller.Move(move * Speed * Time.deltaTime);
            }
            else
            {
                // Fallback to direct transform movement if no CharacterController
                transform.position += move * Speed * Time.deltaTime;
            }
        }
        else
        {
            // Alternative movement (WASD controls X and Y)
            m_Motion.x = moveInput.x;
            m_Motion.y = moveInput.y;

            if (m_Motion.magnitude > 0)
            {
                if (controller != null)
                {
                    Vector3 move = transform.right * m_Motion.x + transform.up * m_Motion.y;
                    controller.Move(move * Speed * Time.deltaTime);
                }
                else
                {
                    transform.position += m_Motion * Speed * Time.deltaTime;
                }
            }
        }
    }

    private void HandleGravityAndJump()
    {
        if (controller == null) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayerMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckMovement()
    {
        if (lastPosition != gameObject.transform.position && isGrounded)
        {
            lastPosition = gameObject.transform.position;
        }
    }
}