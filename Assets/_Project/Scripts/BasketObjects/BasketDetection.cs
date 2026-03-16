using UnityEngine;
using Unity.Netcode;

public class BasketDetection : NetworkBehaviour
{
    [SerializeField] private ParticleSystem confettiParticles;

    private void OnTriggerEnter(Collider other)
    {
        bool networkActive = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        // Only the server should handle scoring in multiplayer
        if (networkActive && !IsServer)
            return;

        Basketball ball = other.GetComponent<Basketball>();
        if (ball == null)
            return;

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (!ball.GetHasScored() && rb.linearVelocity.y < 0)
        {
            ball.SetHasScored(true);

            // Play confetti
            if (networkActive)
            {
                // Multiplayer: notify clients
                PlayConfettiClientRpc();
            }
            else
            {
                // Singleplayer: fallback
                PlayConfettiLocal();
            }

            BasketballCart cart = ball.GetOwnerCart();

            if (cart != null && PointsManager.Instance != null)
            {
                PointsManager.Instance.AddBasketPoints(cart);
            }
        }
    }

    [ClientRpc]
    private void PlayConfettiClientRpc()
    {
        PlayConfettiLocal();
    }

    // Local fallback function
    private void PlayConfettiLocal()
    {
        if (confettiParticles != null)
        {
            confettiParticles.Play();
        }
    }
}