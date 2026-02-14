using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NETPlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [Header("Fallback Player Prefab (assign same as NetworkManager Player Prefab)")]
    [SerializeField] private NetworkObject fallbackPlayerPrefab;

    void Start()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        // handle already-connected clients (host runs) after one frame
        StartCoroutine(AssignExistingClientsNextFrame());
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Start coroutine to wait a frame so PlayerObject is available
        StartCoroutine(AssignSpawnCoroutine(clientId));
    }

    System.Collections.IEnumerator AssignSpawnCoroutine(ulong clientId)
    {
        yield return null; // wait one frame

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) yield break;
        var client = NetworkManager.Singleton.ConnectedClients[clientId];
        var playerObj = client.PlayerObject;
        Debug.Log($"NETPlayerSpawner: AssignSpawnCoroutine client {clientId}, playerObj={(playerObj==null?"null":"exists")}, connectedClients={NetworkManager.Singleton.ConnectedClients.Count}");
        if (playerObj == null)
        {
            Debug.LogError($"NETPlayerSpawner: PlayerObject is null for client {clientId}. Ensure NetworkManager.Player Prefab is assigned or set a fallbackPlayerPrefab on NETPlayerSpawner.");
            yield break;
        }

        if (spawnPoints == null || spawnPoints.Length == 0) yield break;

        int index = (int)(clientId % (ulong)spawnPoints.Length);
        playerObj.transform.position = spawnPoints[index].position;
        playerObj.transform.rotation = spawnPoints[index].rotation;
    }

    System.Collections.IEnumerator AssignExistingClientsNextFrame()
    {
        yield return null; // wait one frame so PlayerObjects are created

        if (NetworkManager.Singleton == null) yield break;

        foreach (var kv in NetworkManager.Singleton.ConnectedClients)
        {
            var clientId = kv.Key;
            var client = kv.Value;
            var playerObj = client.PlayerObject;
            Debug.Log($"NETPlayerSpawner: existing client {clientId}, playerObj={(playerObj==null?"null":"exists")}");

            if (playerObj == null && fallbackPlayerPrefab != null && NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"NETPlayerSpawner: spawning fallback player prefab for client {clientId}");
                var go = Instantiate(fallbackPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
                var netObj = go.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.SpawnAsPlayerObject(clientId, true);
                    playerObj = netObj;
                }
            }

            if (playerObj == null && fallbackPlayerPrefab == null)
            {
                Debug.LogError($"NETPlayerSpawner: No PlayerObject for client {clientId} and no fallbackPlayerPrefab assigned. Assign the Player Prefab in NetworkManager or set a fallback on NETPlayerSpawner.");
            }

            if (playerObj == null) continue;

            if (spawnPoints == null || spawnPoints.Length == 0) continue;

            int index = (int)(clientId % (ulong)spawnPoints.Length);
            playerObj.transform.position = spawnPoints[index].position;
            playerObj.transform.rotation = spawnPoints[index].rotation;
        }
    }
}
