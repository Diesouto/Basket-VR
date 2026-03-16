using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BasketballCart : NetworkBehaviour, IBasketballOwner
{
    [Header("Ball")]
    [SerializeField] private GameObject basketballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnCooldown = 0.5f;

    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 10;

    Queue<Basketball> ballPool = new Queue<Basketball>();
    float lastSpawnTime;

    void Awake()
    {
        CreatePool();
    }

    void CreatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject ballObj = Instantiate(basketballPrefab, transform);

            ballObj.SetActive(false);

            Basketball ball = ballObj.GetComponent<Basketball>();
            ball.Initialize(this);

            ballPool.Enqueue(ball);
        }
    }

    public void SpawnBall()
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        if (ballPool.Count == 0)
            return;

        lastSpawnTime = Time.time;

        Basketball ball = ballPool.Dequeue();

        ball.Initialize(this);

        ball.transform.position = spawnPoint.position;
        ball.transform.rotation = spawnPoint.rotation;

        ball.ResetBall();
        ball.gameObject.SetActive(true);
    }

    // Spawn ball directly into VR hand
    public void SpawnBall(SelectEnterEventArgs args)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (!IsOwner) return;
        }

        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        if (ballPool.Count == 0)
            return;

        lastSpawnTime = Time.time;

        Basketball ball = ballPool.Dequeue();

        ball.transform.position = spawnPoint.position;
        ball.transform.rotation = spawnPoint.rotation;

        ball.ResetBall();
        ball.gameObject.SetActive(true);

        var grabInteractable = ball.GetComponent<XRGrabInteractable>();

        args.manager.SelectEnter(
            args.interactorObject,
            grabInteractable
        );
    }

    public void ReturnBall(Basketball ball)
    {
        ball.gameObject.SetActive(false);
        ballPool.Enqueue(ball);
    }


    // PC MULTIPLAYER CODE
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestSpawnServerRpc()
    {
        if (!IsServer) return;
        Debug.Log($"NETBasketballCart.RequestSpawnServerRpc: spawn requested on server (cart={name})");
        TrySpawnBall();
    }

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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestReturnServerRpc(ulong ballNetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ballNetId)) return;
        var nobj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ballNetId];
        nobj.Despawn(true);
    }
}