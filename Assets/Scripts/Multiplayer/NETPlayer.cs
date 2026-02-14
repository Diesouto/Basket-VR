using UnityEngine;
using Unity.Netcode;
// Prefer the new Input System when available
using UnityEngine.InputSystem;

public class NETPlayer : NetworkBehaviour
{
    [Header("References")]
    public Transform holdPoint;
    public Camera playerCamera;

    [Header("Grab")]
    public LayerMask ballLayer;
    public float grabDistance = 3f;
    [Header("Interact")]
    public LayerMask interactLayer;

    Rigidbody heldRb;
    InputSystem_Actions inputActions;

    void Update()
    {
        if (!IsOwner) return;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // enable camera only for the owner
        if (playerCamera != null)
        {
            playerCamera.enabled = IsOwner;
        }

        // Only the owner needs input handling
        if (IsOwner)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.Enable();
            inputActions.Player.Grab.performed += OnGrabPerformed;
            inputActions.Player.Grab.canceled += OnGrabCanceled;
        }
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
        try { heldRb.transform.SetParent(holdPoint); heldRb.transform.localPosition = Vector3.zero; } catch { }
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
            nobj.transform.SetParent(null);
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
}
