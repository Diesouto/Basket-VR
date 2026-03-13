using UnityEngine;
using System;

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

    private bool canPlay = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (inputActions != null)
        {
            inputActions.Player.Enable();

            inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

            inputActions.Player.Grab.performed += _ => OnGrabPressed();
            inputActions.Player.Grab.canceled += _ => OnGrabReleased();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += Player_OnStateChanged;
        }
    }

    private void Player_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            EnablePlayer();
        }
        else if (GameManager.Instance.IsGameOver())
        {
            DisablePlayerCompletely();
        }
        else
        {
            DisablePlayerInput();
        }
    }

    void OnDisable()
    {
        if (inputActions != null)
            inputActions.Player.Disable();

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= Player_OnStateChanged;
    }

    void Update()
    {
        if (!canPlay) return;

        if (inputActions == null) return;
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
        if (!canPlay) return;

        if (ballGrabber != null && ballGrabber.IsHoldingBall())
            return;

        ballGrabber.TryGrab(cameraTransform);

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            BasketballCart cart = hit.collider.GetComponent<BasketballCart>();
            //if (cart != null)
                //cart.SpawnBall();
        }
    }

    void OnGrabReleased()
    {
        if (!canPlay) return;

        ballGrabber.Release(cameraTransform);
    }

    public void EnablePlayer()
    {
        canPlay = true;
        controller.enabled = true;
    }

    public void DisablePlayerInput()
    {
        canPlay = false;
        moveInput = Vector2.zero;
    }

    public void DisablePlayerCompletely()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        canPlay = false;
        moveInput = Vector2.zero;

        controller.enabled = false;

        if (ballGrabber != null && ballGrabber.IsHoldingBall())
            ballGrabber.Release(cameraTransform);
    }
}