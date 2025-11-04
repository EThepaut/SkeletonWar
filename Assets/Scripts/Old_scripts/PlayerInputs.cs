using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputs : NetworkBehaviour
{
    public float mouseSensitivity = 0.1f;
    float xRotation = 0;
    float yRotation = 0;

    bool isCameraLocked = false;

    [SerializeField] private GameObject cylinder;
    private CharacterController controller;
    public Camera playerCamera;


    public float speed = 5f;
    public float gravity = 9.81f * 2;
    public float jumpHeight = 2f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundLayerMask;

    Vector3 velocity;
    public bool isGrounded;

    private Vector3 lastPosition = new Vector3(0, 0, 0);

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (IsOwner)
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (cylinder != null)
                cylinder.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    void Update()
    {

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            isCameraLocked = !isCameraLocked;
            Cursor.lockState = isCameraLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = isCameraLocked ? false : true;
        }
        /*if (!IsOwner) return;*/

        HandleMouseLook();
        HandleShoot();
        HandleMovement();
        HandleGravityAndJump();
        CheckMovement();
    }

    void HandleMouseLook()
    {
        Vector2 look = Mouse.current.delta.ReadValue() * mouseSensitivity;

        yRotation += look.x;
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        xRotation -= look.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleShoot()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
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

    void HandleMovement()
    {

        if (Keyboard.current.nKey.isPressed)
        {
            int sceneLvl = SceneManager.GetActiveScene().buildIndex;
            if (sceneLvl != 3)
                SceneManager.LoadScene("Stage3p", LoadSceneMode.Single);
            else
                SceneManager.LoadScene("Stage2p", LoadSceneMode.Single);
        }

        if (controller == null)
        {
            Debug.LogError("CharacterController non trouv� !");
            return;
        }
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) moveInput.y += 1; // Z
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1; // S
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1; // Q
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1; // D

        if (moveInput.magnitude > 1)
            moveInput.Normalize();

        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y);
        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
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

    void CheckMovement()
    {
        if (lastPosition != gameObject.transform.position && isGrounded)
        {
            lastPosition = gameObject.transform.position;
        }
    }
}