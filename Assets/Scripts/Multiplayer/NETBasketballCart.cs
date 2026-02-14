using UnityEngine;
using Unity.Netcode;

public class NETBasketballCart : NetworkBehaviour, IBasketballOwner
{
    [Header("Ball")]
    [SerializeField] private NetworkObject basketballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnCooldown = 0.5f;

    float lastSpawnTime;

    // Clients can call this to request the server to spawn a ball
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestSpawnServerRpc()
    {
        if (!IsServer) return;
        Debug.Log($"NETBasketballCart.RequestSpawnServerRpc: spawn requested on server (cart={name})");
        TrySpawnBall();
    }

    // Only server actually spawns
    public void TrySpawnBall()
    {
        if (!IsServer) return;
        if (Time.time - lastSpawnTime < spawnCooldown)
        {
            Debug.Log("NETBasketballCart.TrySpawnBall: cooldown active");
            return;
        }
        lastSpawnTime = Time.time;
        Debug.Log("NETBasketballCart.TrySpawnBall: spawning ball now");
        SpawnBall();
    }

    void SpawnBall()
    {
        if (basketballPrefab == null)
        {
            Debug.LogWarning("NETBasketballCart: basketballPrefab not set");
            return;
        }

        Debug.Log($"NETBasketballCart.SpawnBall: instantiating prefab {basketballPrefab.name}");
        GameObject go = Instantiate(basketballPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);
        NetworkObject netObj = go.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("NETBasketballCart: prefab is not a NetworkObject");
            Destroy(go);
            return;
        }

        netObj.Spawn(true);
        Basketball ball = go.GetComponent<Basketball>();
        if (ball != null) ball.Initialize(this);
    }

    // IBasketballOwner implementation. Called by the Basketball when it wants to return/despawn.
    public void ReturnBall(Basketball ball)
    {
        if (!IsServer)
        {
            // forward to server if called on client
            var netObj = ball.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                RequestReturnServerRpc(netObj.NetworkObjectId);
            }
            return;
        }

        NetworkObject nb = ball.GetComponent<NetworkObject>();
        if (nb != null)
        {
            nb.Despawn(true);
        }
        else
        {
            Destroy(ball.gameObject);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestReturnServerRpc(ulong ballNetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ballNetId)) return;
        var nobj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ballNetId];
        nobj.Despawn(true);
    }
}
