using UnityEngine;
using Unity.Netcode;

public class BasketDetection : NetworkBehaviour
{
    [SerializeField] private ParticleSystem confettiParticles;

    private void OnTriggerEnter(Collider other)
    {
        // Only the server should handle scoring in multiplayer. In single-player, allow local scoring.
        bool networkActive = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        if (networkActive && !IsServer) return;

        Basketball ball = other.GetComponent<Basketball>();
        if (ball == null) return;

        if (!ball.GetHasScored() && ball.GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            ball.SetHasScored(true);

            PlayConfettiClientRpc();

            // Determine the owner of the ball if networked
            var netObj = ball.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                ulong owner = netObj.OwnerClientId;
                Debug.Log($"BasketDetection: scored by owner {owner}");
                if (PointsManager.Instance != null)
                    PointsManager.Instance.AddBasketPointsForClient(owner);
            }
            else
            {
                // local singleplayer
                Debug.Log("BasketDetection: scored in single-player");
                if (PointsManager.Instance != null)
                    PointsManager.Instance.AddBasketPointsLocal();
            }
        }
    }

    [ClientRpc]
    private void PlayConfettiClientRpc()
    {
        if (confettiParticles != null)
        {
            confettiParticles.Play();
        }
    }
}