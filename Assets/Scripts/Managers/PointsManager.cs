using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class PointsManager : NetworkBehaviour
{
    public static PointsManager Instance { get; private set; }

    public event EventHandler OnPointsChanged;

    [SerializeField] private int pointsPerBasket = 2;

    // local cache of points per client
    private Dictionary<ulong, int> pointsPerClient = new Dictionary<ulong, int>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one PointsManager in scene!");
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only the server tracks connected clients and initializes their points
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // initialize current connected clients
            foreach (var kv in NetworkManager.Singleton.ConnectedClients)
            {
                ulong clientId = kv.Key;
                if (!pointsPerClient.ContainsKey(clientId))
                    pointsPerClient[clientId] = 0;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        if (!pointsPerClient.ContainsKey(clientId))
            pointsPerClient[clientId] = 0;

        // inform clients about the new (zero) points for this client
        UpdateClientPointsClientRpc(clientId, 0);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        // set disconnected client's points to 0 so clients don't show stale values
        if (pointsPerClient.ContainsKey(clientId))
            pointsPerClient[clientId] = 0;
        UpdateClientPointsClientRpc(clientId, 0);
    }

    public void AddBasketPointsForClient(ulong clientId)
    {
        // Allow single-player local updates when networking is not active.
        bool networkingActive = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        if (networkingActive && !IsServer)
        {
            Debug.LogWarning("AddBasketPointsForClient should be called on server.");
            return;
        }

        if (!pointsPerClient.ContainsKey(clientId))
            pointsPerClient[clientId] = 0;

        pointsPerClient[clientId] += pointsPerBasket;
        int newPoints = pointsPerClient[clientId];

        if (networkingActive)
        {
            // notify all clients about updated points for this client
            Debug.Log($"PointsManager: awarding {pointsPerBasket} to {clientId}, total {newPoints} (networked)");
            UpdateClientPointsClientRpc(clientId, newPoints);
        }
        else
        {
            // single-player: invoke local event so UI updates
            Debug.Log($"PointsManager: awarding {pointsPerBasket} to {clientId}, total {newPoints} (local)");
            OnPointsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Single-player fallback
    public void AddBasketPointsLocal()
    {
        // Treat local player as client 0
        AddBasketPointsForClient(0);
    }

    [ClientRpc]
    void UpdateClientPointsClientRpc(ulong clientId, int newPoints, ClientRpcParams clientRpcParams = default)
    {
        pointsPerClient[clientId] = newPoints;
        OnPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetPlayerPoints()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        if (pointsPerClient.TryGetValue(localId, out int val)) return val;
        return 0;
    }

    public int GetPlayerPoints(ulong clientId)
    {
        if (pointsPerClient.TryGetValue(clientId, out int val)) return val;
        return 0;
    }

    public int GetRivalPoints()
    {
        var all = Instance.GetAllPoints();
        ulong localId = NetworkManager.Singleton.LocalClientId;
        foreach (var kv in all)
        {
            if (kv.Key == localId) continue;
            return kv.Value;
        }

        return 0;
    }

    public Dictionary<ulong,int> GetAllPoints()
    {
        return new Dictionary<ulong,int>(pointsPerClient);
    }

    public void ResetAllPoints()
    {
        if (!IsServer) return;
        pointsPerClient.Clear();
        UpdateAllClientsResetClientRpc();
    }

    [ClientRpc]
    void UpdateAllClientsResetClientRpc(ClientRpcParams clientRpcParams = default)
    {
        pointsPerClient.Clear();
        OnPointsChanged?.Invoke(this, EventArgs.Empty);
    }
}