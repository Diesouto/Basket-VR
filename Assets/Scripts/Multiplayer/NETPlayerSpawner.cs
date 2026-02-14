using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NETPlayerSpawner : MonoBehaviour
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

        // Start coroutine to wait a frame so PlayerObject is available
        StartCoroutine(AssignSpawnCoroutine(clientId));
    }

    System.Collections.IEnumerator AssignSpawnCoroutine(ulong clientId)
    {
        yield return null; // wait one frame

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) yield break;
        var client = NetworkManager.Singleton.ConnectedClients[clientId];
        var playerObj = client.PlayerObject;
        if (playerObj == null) yield break;

        if (spawnPoints == null || spawnPoints.Length == 0) yield break;

        int index = (int)(clientId % (ulong)spawnPoints.Length);
        playerObj.transform.position = spawnPoints[index].position;
        playerObj.transform.rotation = spawnPoints[index].rotation;
    }
}
