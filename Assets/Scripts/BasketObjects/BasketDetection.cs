using UnityEngine;

public class BasketDetection : MonoBehaviour
{
    [Header("Scoring")]
    public int points = 2; // puntos por encestar

    private void OnTriggerEnter(Collider other)
    {
        Basketball ball = other.GetComponent<Basketball>();
        if (ball == null) return;

        // Solo contar si no se ha anotado aún
        if (!ball.GetHasScored())
        {
            ball.SetHasScored(true);
            Debug.Log("+2 puntos");

            // GameManager.Instance.AddPoints(2);
        }
    }
}
