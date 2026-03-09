using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // size = 2

    private bool hasAssignedSpawns = false;

    void Update()
    {
        if (hasAssignedSpawns) return;

        if (!NetworkManager.Singleton.IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count < GameManager.Instance.GetMinPlayersToStart()) return;

        for (int i = 0; i < clients.Count; i++)
        {
            var playerObject = clients[i].PlayerObject;

            if (playerObject == null) continue;

            if (i >= spawnPoints.Length) continue;

            playerObject.transform.position = spawnPoints[i].position;
            playerObject.transform.rotation = spawnPoints[i].rotation;
        }

        hasAssignedSpawns = true;
        enabled = false;
    }
}
