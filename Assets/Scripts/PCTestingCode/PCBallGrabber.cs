using UnityEngine;

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

        grabbedBall.transform.SetParent(null);
        grabbedBall.linearVelocity = cameraTransform.forward * throwForce;

        grabbedBall = null;
    }

    public bool IsHoldingBall()
    {
        return grabbedBall != null;
    }
}
