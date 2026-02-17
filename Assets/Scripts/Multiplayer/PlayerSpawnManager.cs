using UnityEngine;
using Unity.Netcode;
using System.Collections;

// Simple server-side spawn position assignment for player objects.
// Place this in the MultiplayerScene on a GameObject and assign spawnPoints in the inspector.
public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    void Start()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        StartCoroutine(AssignPlayerPositionWhenReady(clientId));
    }

    IEnumerator AssignPlayerPositionWhenReady(ulong clientId)
    {
        // wait for PlayerObject to be available (poll a few frames)
        int tries = 0;
        const int maxTries = 30;
        while (tries < maxTries)
        {
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) yield break;
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            if (client.PlayerObject != null) break;
            tries++;
            yield return null;
        }

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) yield break;
        var c = NetworkManager.Singleton.ConnectedClients[clientId];
        var playerObj = c.PlayerObject;
        if (playerObj == null) yield break;

        if (spawnPoints == null || spawnPoints.Length == 0) yield break;

        int index = (int)(clientId % (ulong)spawnPoints.Length);
        playerObj.transform.position = spawnPoints[index].position;
        playerObj.transform.rotation = spawnPoints[index].rotation;

        Debug.Log($"PlayerSpawnManager: positioned client {clientId} at spawn {index}");
        // Also explicitly tell the owning client to set its local transform to the spawn position
        var netPlayer = playerObj.GetComponent<NETPlayer>();
        if (netPlayer != null)
        {
            var clientParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };
            netPlayer.SetInitialPositionClientRpc(spawnPoints[index].position, spawnPoints[index].rotation, clientParams);
        }
    }
}
