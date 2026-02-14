using UnityEngine;
using Unity.Netcode;

public class PCBallGrabber : MonoBehaviour
{
    public Transform holdPoint;
    public float grabDistance = 2f;
    public LayerMask ballLayer;
    public float throwForce = 10f;

    private Rigidbody grabbedBall;
    private bool isFollowingNetworked = false;
    private NetworkObject heldNetworkObject;

    void Update()
    {
        if (isFollowingNetworked && grabbedBall != null && holdPoint != null)
        {
            // smoothly follow hold point without changing parent to avoid NetworkObject parent change handling
            grabbedBall.transform.position = holdPoint.position;
            grabbedBall.transform.rotation = holdPoint.rotation;
        }
    }

    public void TryGrab(Transform cameraTransform)
    {
        if (grabbedBall != null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, ballLayer))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb == null) return;

            // If the object is a NetworkObject, enforce spawn/ownership only when networking is active
            NetworkObject netObj = rb.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    if (!netObj.IsSpawned)
                    {
                        Debug.LogWarning("Cannot grab networked ball that is not spawned.");
                        return;
                    }

                    if (netObj.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                    {
                        Debug.Log("Cannot grab networked ball we don't own.");
                        return;
                    }
                }
                // if NetworkManager is null or not listening (single-player), allow grabbing local NetworkObjects
            }

            grabbedBall = rb;
            grabbedBall.linearVelocity = Vector3.zero;
            grabbedBall.angularVelocity = Vector3.zero;

            var netObjParent = rb.GetComponent<NetworkObject>();
            if (netObjParent != null)
            {
                // For networked objects, avoid SetParent which triggers NetworkObject parent-change handling
                isFollowingNetworked = true;
                heldNetworkObject = netObjParent;

                // make physics kinematic while held
                if (grabbedBall != null) grabbedBall.isKinematic = true;
            }
            else
            {
                // local/non-network object: safe to parent
                grabbedBall.transform.SetParent(holdPoint);
                grabbedBall.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void Release(Transform cameraTransform)
    {
        if (grabbedBall == null) return;

        NetworkObject netObj = grabbedBall.GetComponent<NetworkObject>();

        bool networkActive = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        if (netObj != null && isFollowingNetworked)
        {
            // stop following, enable physics and set velocity
            isFollowingNetworked = false;
            heldNetworkObject = null;
            if (grabbedBall != null)
            {
                grabbedBall.isKinematic = false;
                grabbedBall.linearVelocity = cameraTransform.forward * throwForce;
            }
        }
        else
        {
            // non-networked or networked but not following: try to unparent if safe
            bool canUnparent = true;
            if (netObj != null && networkActive)
            {
                canUnparent = netObj.IsSpawned && netObj.OwnerClientId == NetworkManager.Singleton.LocalClientId;
            }

            if (canUnparent)
            {
                try { grabbedBall.transform.SetParent(null); } catch (System.Exception ex) { Debug.LogWarning($"Failed to unparent grabbed ball safely: {ex.Message}"); }
            }
            else
            {
                Debug.Log("Release: not unparenting networked ball we don't own or that isn't spawned.");
            }

            if (grabbedBall != null) grabbedBall.linearVelocity = cameraTransform.forward * throwForce;
        }

        grabbedBall = null;
    }

    public bool IsHoldingBall()
    {
        return grabbedBall != null;
    }
}
