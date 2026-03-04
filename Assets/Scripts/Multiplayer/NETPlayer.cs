using System;
using Unity.Netcode;
using UnityEngine;

public class NETPlayer : NetworkBehaviour
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

    [Header("References")]
    public Transform holdPoint;
    public Camera playerCamera;
    [Header("Local Components")]
    public SimpleFPSPlayer simpleFPSPlayer;
    public PCBallGrabber pcBallGrabber;

    [Header("Grab")]
    public LayerMask ballLayer;
    public float grabDistance = 3f;

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private Rigidbody heldRb;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float yVelocity;
    private float xRotation;

    private bool canPlay = false;

    void Update()
    {
        if (!IsOwner) return;
        if (!canPlay) return;
        if (inputActions == null) return;

        if (heldRb != null)
        {
            heldRb.transform.position = holdPoint.position;
            heldRb.transform.rotation = holdPoint.rotation;
        }

        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleMovement();
        HandleLook();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Desactiva la cámara del otro jugador para evitar que ambos jugadores vean a través de la misma cámara
        if (!IsOwner)
        {
            playerCamera.enabled = false;
            return;
        }

        if (playerCamera != null) playerCamera.enabled = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += Player_OnStateChanged;
            Player_OnStateChanged(this, System.EventArgs.Empty); 
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputActions = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();

        inputActions.Player.Enable();
        inputActions.Player.Grab.performed += OnGrabPerformed;
        inputActions.Player.Grab.canceled += OnGrabCanceled;

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        EnablePlayer();
    }

    private void Player_OnStateChanged(object sender, EventArgs e)
    {
        if (!IsOwner) return;

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

    void HandleMovement()
    {
        if (!IsOwner) return;

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
        if (!IsOwner) return;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void EnablePlayer()
    {
        if (!IsOwner) return;
        canPlay = true;
        controller.enabled = true;
    }

    public void DisablePlayerInput()
    {
        if (!IsOwner) return;
        canPlay = false;
        moveInput = Vector2.zero;
    }

    public void DisablePlayerCompletely()
    {
        if (!IsOwner) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        canPlay = false;
        moveInput = Vector2.zero;

        controller.enabled = false;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (playerCamera != null)
            playerCamera.enabled = false;

        if (inputActions != null)
        {
            inputActions.Player.Grab.performed -= OnGrabPerformed;
            inputActions.Player.Grab.canceled -= OnGrabCanceled;
            inputActions.Player.Disable();
            inputActions = null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= Player_OnStateChanged;
        }
    }

    void OnGrabPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        // First try to grab a ball. If that didn't hit anything, try interacting (e.g., spawn from cart).
        if (TryLocalGrab()) return;

        TryLocalInteract();
    }

    void OnGrabCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        TryLocalRelease();
    }

    bool TryLocalGrab()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, ballLayer))
        {
            // Ensure we hit a physics body (balls should have a Rigidbody)
            Rigidbody hitRb = hit.rigidbody != null ? hit.rigidbody : hit.collider.GetComponent<Rigidbody>();
            if (hitRb == null) return false;

            var netObj = hitRb.GetComponent<NetworkObject>();
            if (netObj == null) return false;
            RequestGrabServerRpc(netObj.NetworkObjectId, NetworkManager.Singleton.LocalClientId);
            return true;
        }

        return false;
    }

    void TryLocalInteract()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        bool didHit = false;
        if (interactLayer.value == 0)
        {
            didHit = Physics.Raycast(ray, out hit, grabDistance);
        }
        else
        {
            didHit = Physics.Raycast(ray, out hit, grabDistance, interactLayer);
        }

        if (!didHit)
        {
            Debug.Log("NETPlayer.TryLocalInteract: no hit");
            return;
        }

        Debug.Log($"NETPlayer.TryLocalInteract: hit {hit.collider.name} (layer {hit.collider.gameObject.layer})");
        var cart = hit.collider.GetComponentInParent<NETBasketballCart>();
        if (cart != null)
        {
            Debug.Log($"NETPlayer: requesting spawn from cart {cart.name}");
            cart.RequestSpawnServerRpc();
        }
    }

    void TryLocalRelease()
    {
        if (heldRb == null) return;
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        Vector3 relVel = cam.transform.forward * 8f;
        RequestReleaseServerRpc(heldRb.GetComponent<NetworkObject>().NetworkObjectId, relVel, NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void RequestGrabServerRpc(ulong ballNetId, ulong requestingClientId)
    {
        if (!IsServer) return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ballNetId)) return;
        var ballObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ballNetId].gameObject;

        var rb = ballObj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // transfer ownership to requester
        var nobj = ballObj.GetComponent<NetworkObject>();
        nobj.ChangeOwnership(requestingClientId);

        // Tell the owner client to parent locally
        var clientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { requestingClientId } } };
        GrabbedClientRpc(ballNetId, clientParams);
    }

    [ClientRpc]
    void GrabbedClientRpc(ulong ballNetId, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ballNetId)) return;
        var go = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ballNetId].gameObject;
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning($"GrabbedClientRpc: spawned object {go.name} has no Rigidbody; ignoring grab.");
            return;
        }

        heldRb = rb;
        heldRb.isKinematic = true;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void RequestReleaseServerRpc(ulong ballNetId, Vector3 linearVelocity, ulong requestingClientId)
    {
        if (!IsServer) return;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ballNetId)) return;

        var nobj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ballNetId];
        var rb = nobj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // unparent on server and enable physics
            rb.isKinematic = false;
            rb.linearVelocity = linearVelocity;
        }

        // return ownership to server
        nobj.ChangeOwnership(NetworkManager.ServerClientId);

        // clear held reference on owner client
        var clientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { requestingClientId } } };
        ReleasedClientRpc(ballNetId, clientParams);
    }

    [ClientRpc]
    void ReleasedClientRpc(ulong ballNetId, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        if (heldRb != null && heldRb.GetComponent<NetworkObject>().NetworkObjectId == ballNetId)
        {
            try { heldRb.transform.SetParent(null); } catch { }
            heldRb = null;
        }
    }

    [ClientRpc]
    public void SetInitialPositionClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        // directly set transform on the client for the player's owned object ???????
        try
        {
            transform.position = position;
            transform.rotation = rotation;
        }
        catch { }
    }
}
