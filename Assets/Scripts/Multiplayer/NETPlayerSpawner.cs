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
        // wait several frames to allow the PlayerObject to be created by Netcode
        int attempts = 0;
        const int maxAttempts = 30;
        while (attempts < maxAttempts)
        {
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) yield break;
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            var playerObj = client.PlayerObject;
            if (playerObj != null)
            {
                Debug.Log($"NETPlayerSpawner: AssignSpawnCoroutine client {clientId}, playerObj exists after {attempts} frames, connectedClients={NetworkManager.Singleton.ConnectedClients.Count}");
                break;
            }
            attempts++;
            yield return null;
        }

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) yield break;
        var clientAfter = NetworkManager.Singleton.ConnectedClients[clientId];
        var playerObjAfter = clientAfter.PlayerObject;
        if (playerObjAfter == null)
        {
            Debug.LogError($"NETPlayerSpawner: PlayerObject is still null for client {clientId} after waiting. Ensure NetworkManager.Player Prefab is assigned or set a fallbackPlayerPrefab on NETPlayerSpawner.");
            yield break;
        }

        if (spawnPoints == null || spawnPoints.Length == 0) yield break;

        int index = (int)(clientId % (ulong)spawnPoints.Length);
        playerObjAfter.transform.position = spawnPoints[index].position;
        playerObjAfter.transform.rotation = spawnPoints[index].rotation;
    }

    System.Collections.IEnumerator AssignExistingClientsNextFrame()
    {
        // wait a few frames so PlayerObjects are created
        const int maxAttempts = 30;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (NetworkManager.Singleton == null) yield break;

            bool allHave = true;
            foreach (var kv in NetworkManager.Singleton.ConnectedClients)
            {
                var client = kv.Value;
                if (client.PlayerObject == null)
                {
                    allHave = false;
                    break;
                }
            }

            if (allHave) break;
            yield return null;
        }

        if (NetworkManager.Singleton == null) yield break;

        foreach (var kv in NetworkManager.Singleton.ConnectedClients)
        {
            var clientId = kv.Key;
            var client = kv.Value;
            var playerObj = client.PlayerObject;
            Debug.Log($"NETPlayerSpawner: existing client {clientId}, playerObj={(playerObj==null?"null":"exists")}");

            if (playerObj == null && NetworkManager.Singleton.IsServer)
            {
                // Try explicit fallback prefab on spawner first
                if (fallbackPlayerPrefab != null)
                {
                    Debug.Log($"NETPlayerSpawner: spawning fallbackPlayerPrefab for client {clientId}");
                    var go = Instantiate(fallbackPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
                    var netObj = go.GetComponent<NetworkObject>();
                    if (netObj != null)
                    {
                        netObj.SpawnAsPlayerObject(clientId, true);
                        playerObj = netObj;
                    }
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
