using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private bool hasAssignedSpawns = false;

    void Update()
    {
        if (hasAssignedSpawns) return;
        if (!NetworkManager.Singleton.IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count < GameManager.Instance.GetMinPlayersToStart()) return;

        // Ensure all player objects exist
        if (clients.Any(c => c.PlayerObject == null)) return;

        for (int i = 0; i < clients.Count && i < spawnPoints.Length; i++)
        {
            var playerObject = clients[i].PlayerObject;

            playerObject.transform.SetPositionAndRotation(
                spawnPoints[i].position,
                spawnPoints[i].rotation
            );
        }

        hasAssignedSpawns = true;
        enabled = false;
    }
}