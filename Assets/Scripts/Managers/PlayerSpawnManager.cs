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

        NETPlayer[] players = FindObjectsByType<NETPlayer>(FindObjectsSortMode.None);

        if (players.Length < GameManager.Instance.GetMinPlayersToStart()) return;

        foreach (var player in players)
        {
            int index = (int)player.OwnerClientId;

            if (index >= spawnPoints.Length) continue;

            player.transform.position = spawnPoints[index].position;
            player.transform.rotation = spawnPoints[index].rotation;
        }

        hasAssignedSpawns = true;
        enabled = false; // disable this script forever
    }
}
