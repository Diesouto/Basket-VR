using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BasketballCart : NetworkBehaviour, IBasketballOwner
{
    [Header("Ball")]
    [SerializeField] private GameObject basketballPrefab;
    [SerializeField] private float spawnCooldown = 0.5f;

    float lastSpawnTime;

    // Cache XR interaction data for grab after spawn
    private IXRSelectInteractor _lastInteractor;
    private XRInteractionManager _lastInteractionManager;

    // =========================
    // ENTRY POINT (VR GRAB)
    // =========================
    public void SpawnBall(SelectEnterEventArgs args)
    {
        _lastInteractor = args.interactorObject;
        _lastInteractionManager = args.manager;

        // 🟢 SINGLE PLAYER (no network running)
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            SpawnBallLocal();
            return;
        }

        // 🔵 MULTIPLAYER
        RequestSpawnServerRpc();
    }

    // =========================
    // SERVER RPC (CLIENT → SERVER)
    // =========================
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        lastSpawnTime = Time.time;

        SpawnBallServer(clientId);
    }

    // =========================
    // SERVER SPAWN LOGIC
    // =========================
    private void SpawnBallServer(ulong clientId)
    {
        GameObject ballObj = Instantiate(basketballPrefab);

        var netObj = ballObj.GetComponent<NetworkObject>();

        netObj.Spawn(true);
        netObj.ChangeOwnership(clientId);

        var ball = ballObj.GetComponent<Basketball>();
        ball.Initialize(this);
        ball.ResetBall();

        // Tell only that client to grab
        GrabBallClientRpc(netObj.NetworkObjectId, clientId);
    }

    private void SpawnBallLocal()
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        lastSpawnTime = Time.time;

        GameObject ballObj = Instantiate(basketballPrefab);

        var ball = ballObj.GetComponent<Basketball>();
        ball.Initialize(this);
        ball.ResetBall();

        var grabInteractable = ballObj.GetComponent<XRGrabInteractable>();

        if (_lastInteractor != null && _lastInteractionManager != null)
        {
            _lastInteractionManager.SelectEnter(_lastInteractor, grabInteractable);
        }
    }

    // =========================
    // CLIENT RPC (SERVER → CLIENT)
    // =========================
    [ClientRpc]
    private void GrabBallClientRpc(ulong netId, ulong targetClientId)
    {
        // Only execute on the intended client
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out var netObj))
            return;

        var grabInteractable = netObj.GetComponent<XRGrabInteractable>();

        if (_lastInteractor != null && _lastInteractionManager != null)
        {
            _lastInteractionManager.SelectEnter(_lastInteractor, grabInteractable);
        }
    }

    // =========================
    // RETURN / DESPAWN
    // =========================
    public void ReturnBall(Basketball ball)
    {
        // 🟢 SINGLE PLAYER
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Destroy(ball.gameObject);
            return;
        }

        // 🔵 MULTIPLAYER
        if (!IsServer)
        {
            RequestReturnServerRpc(ball.GetComponent<NetworkObject>().NetworkObjectId);
            return;
        }

        ball.GetComponent<NetworkObject>().Despawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestReturnServerRpc(ulong ballNetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ballNetId))
            return;

        var nobj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ballNetId];
        nobj.Despawn(true);
    }
}