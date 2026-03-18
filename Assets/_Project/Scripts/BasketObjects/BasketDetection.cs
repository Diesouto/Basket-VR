using Unity.Netcode;
using UnityEngine;

public class BasketDetection : NetworkBehaviour
{
    [SerializeField] private ParticleSystem confettiParticles;

    private void OnTriggerEnter(Collider other)
    {
        bool networkActive = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        Basketball ball = other.GetComponent<Basketball>();
        if (ball == null)
            return;

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        // MULTIPLAYER: si no somos servidor, informamos al servidor para que valide y procese el scoring.
        if (networkActive && !IsServer)
        {
            // Solo solicitar score si la pelota aparentemente est� yendo hacia abajo y no ha scoreado a�n.
            if (!ball.GetHasScored() && rb != null && rb.linearVelocity.y < 0f)
            {
                // Obtener NetworkObject de la bola y enviar su NetworkObjectId al servidor
                var netObj = ball.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    RequestScoreServerRpc(netObj.NetworkObjectId);
                }
                else
                {
                    Debug.LogWarning("BasketDetection: Ball has no NetworkObject, cannot request score ServerRpc.");
                }
            }
            return;
        }

        // SERVER o singleplayer: el servidor procesa directamente
        if (!ball.GetHasScored() && rb != null && rb.linearVelocity.y < 0f)
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
                // Award points to the client that owns the ball (ball owner may differ from cart owner)
                var ballNetObj = ball.GetComponent<NetworkObject>();
                if (ballNetObj != null)
                {
                    PointsManager.Instance.AddPointsForClient(ballNetObj.OwnerClientId, PointsManager.Instance.PointsPerBasket);
                }
                else
                {
                    // Fallback: keep backwards compatibility and award to cart owner
                    PointsManager.Instance.AddBasketPoints(cart);
                }
            }
        }
    }

    // RPC que permite a clientes notificar al servidor que una pelota ha entrado en aro.
    // El servidor validar� existencia/estado y aplicar� puntos.
    [ServerRpc(RequireOwnership = false)]
    private void RequestScoreServerRpc(ulong ballNetId, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"Client requesting score for ball");
        if (!IsServer) return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ballNetId, out var nobj))
            return;

        var ball = nobj.GetComponent<Basketball>();
        if (ball == null) return;

        // Validaci�n server-side: no doble scoring y que la velocidad Y sea negativa (hacia abajo)
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (ball.GetHasScored())
            return;

        ball.SetHasScored(true);

        // Confetti a todos los clientes
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            PlayConfettiClientRpc();
        else
            PlayConfettiLocal();

        BasketballCart cart = ball.GetOwnerCart();

        if (cart != null && PointsManager.Instance != null)
        {
            var ballNetObj = ball.GetComponent<NetworkObject>();
            if (ballNetObj != null)
            {
                PointsManager.Instance.AddPointsForClient(ballNetObj.OwnerClientId, PointsManager.Instance.PointsPerBasket);
            }
            else
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