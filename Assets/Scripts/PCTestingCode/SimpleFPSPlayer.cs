using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 0.5f;
    public Transform cameraTransform;
    public float maxLookAngle = 80f;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("PC Grab")]
    public PCBallGrabber ballGrabber;

    private CharacterController controller;
    private InputSystem_Actions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float yVelocity;
    private float xRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        inputActions.Player.Grab.performed += _ => OnGrabPressed();
        inputActions.Player.Grab.canceled += _ => OnGrabReleased();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void OnGrabPressed()
    {
        // Si ya tengo pelota no spawneo ni hago raycast a carro
        if (ballGrabber != null && ballGrabber.IsHoldingBall())
            return;

        // Intentar agarrar pelota
        ballGrabber.TryGrab(cameraTransform);

        // Si no hay pelota, intentar interactuar con el carro
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            BasketballCart cart = hit.collider.GetComponent<BasketballCart>();
            if (cart != null)
                cart.SpawnBall();
        }
    }

    void OnGrabReleased()
    {
        ballGrabber.Release(cameraTransform);
    }
}
