using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Stop once gameplay begins
        if (GameManager.Instance.IsGamePlaying()) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count < GameManager.Instance.GetMinPlayersToStart()) return;

        if (clients.Any(c => c.PlayerObject == null)) return;

        for (int i = 0; i < clients.Count && i < spawnPoints.Length; i++)
        {
            var playerObject = clients[i].PlayerObject;
            var player = playerObject.GetComponent<NETPlayer>();

            if (player == null) continue;

            var targetClient = clients[i].ClientId;

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetClient }
                }
            };

            player.SetInitialPositionClientRpc(
                spawnPoints[i].position,
                spawnPoints[i].rotation,
                clientRpcParams
            );
        }
    }
}