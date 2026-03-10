using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BasketballCart : NetworkBehaviour, IBasketballOwner
{
    [Header("Ball")]
    [SerializeField] GameObject basketballPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float spawnCooldown = 0.5f;

    [Header("Pool")]
    [SerializeField] int initialPoolSize = 10;

    Queue<Basketball> ballPool = new Queue<Basketball>();
    float lastSpawnTime;

    void Start()
    {
        if (IsServer)
        {
            CreatePool();
        }
    }

    void CreatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject ballObj = Instantiate(basketballPrefab, transform);

            var netObj = ballObj.GetComponent<NetworkObject>();
            netObj.Spawn(true);

            ballObj.SetActive(false);

            Basketball ball = ballObj.GetComponent<Basketball>();
            ball.Initialize(this);

            ballPool.Enqueue(ball);
        }
    }

    // Called when VR player grabs from cart
    public void SpawnBall(SelectEnterEventArgs args)
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        lastSpawnTime = Time.time;

        ulong senderClientId = NetworkManager.Singleton.LocalClientId;

        SpawnBallRpc(senderClientId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SpawnBallRpc(ulong clientId)
    {
        if (ballPool.Count == 0)
            return;

        Basketball ball = ballPool.Dequeue();

        ball.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        ball.ResetBall();
        ball.gameObject.SetActive(true);

        ulong netId = ball.GetComponent<NetworkObject>().NetworkObjectId;

        GiveBallRpc(netId, clientId);
    }

    [Rpc(SendTo.NotOwner)]
    void GiveBallRpc(ulong ballNetId, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ballNetId, out NetworkObject netObj))
            return;

        XRGrabInteractable grab = netObj.GetComponent<XRGrabInteractable>();

        XRBaseInteractor interactor = FindFirstObjectByType<XRBaseInteractor>();

        if (interactor != null && grab != null)
        {
            grab.interactionManager.SelectEnter(
                (IXRSelectInteractor)interactor,
                (IXRSelectInteractable)grab
            );
        }
    }

    public void ReturnBall(Basketball ball)
    {
        if (!IsServer)
            return;

        ball.gameObject.SetActive(false);
        ballPool.Enqueue(ball);
    }
}