using UnityEngine;
using Unity.Netcode;

public class PCBallGrabber : MonoBehaviour
{
    public Transform holdPoint;
    public float grabDistance = 2f;
    public LayerMask ballLayer;
    public float throwForce = 10f;

    private Rigidbody grabbedBall;

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

            grabbedBall.transform.SetParent(holdPoint);
            grabbedBall.transform.localPosition = Vector3.zero;
        }
    }

    public void Release(Transform cameraTransform)
    {
        if (grabbedBall == null) return;

        NetworkObject netObj = grabbedBall.GetComponent<NetworkObject>();

        bool canUnparent = true;
        if (netObj != null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                // only unparent if this client owns the networked object and it's spawned
                canUnparent = netObj.IsSpawned && netObj.OwnerClientId == NetworkManager.Singleton.LocalClientId;
            }
            else
            {
                // networking not active (single-player), allow unparent
                canUnparent = true;
            }
        }

        if (canUnparent)
        {
            try
            {
                grabbedBall.transform.SetParent(null);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to unparent grabbed ball safely: {ex.Message}");
            }
        }
        else
        {
            Debug.Log("Release: not unparenting networked ball we don't own or that isn't spawned.");
        }

        grabbedBall.linearVelocity = cameraTransform.forward * throwForce;

        grabbedBall = null;
    }

    public bool IsHoldingBall()
    {
        return grabbedBall != null;
    }
}
