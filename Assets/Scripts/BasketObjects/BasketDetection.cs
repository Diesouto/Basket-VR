using UnityEngine;

public class BasketDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Basketball ball = other.GetComponent<Basketball>();
        if (ball == null) return;

        if (!ball.GetHasScored() && ball.GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            ball.SetHasScored(true);

            PointsManager.Instance.AddBasketPoints();
        }
    }
}