using UnityEngine;
using Unity.Netcode;

public class NETPlayer : NetworkBehaviour
{
    [Header("References")]
    public Transform holdPoint;

    [Header("Grab")]
    public LayerMask ballLayer;
    public float grabDistance = 3f;

    Rigidbody heldRb;

    void Update()
    {
        if (!IsOwner) return;

        // Simple PC input for testing
        if (Input.GetKeyDown(KeyCode.E))
            TryLocalGrab();

        if (Input.GetKeyDown(KeyCode.Q))
            TryLocalRelease();
    }

    void TryLocalGrab()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, ballLayer))
        {
            var netObj = hit.collider.GetComponent<NetworkObject>();
            if (netObj == null) return;
            RequestGrabServerRpc(netObj.NetworkObjectId, NetworkManager.Singleton.LocalClientId);
        }
    }

    void TryLocalRelease()
    {
        if (heldRb == null) return;
        Vector3 relVel = Camera.main.transform.forward * 8f;
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
        heldRb = go.GetComponent<Rigidbody>();
        heldRb.transform.SetParent(holdPoint);
        heldRb.transform.localPosition = Vector3.zero;
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
