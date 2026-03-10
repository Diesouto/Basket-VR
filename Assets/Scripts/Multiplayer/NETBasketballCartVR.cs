using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class NETBasketballCartVR : NetworkBehaviour, IBasketballOwner
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

        SpawnBallServerRpc(senderClientId);
    }

    [Rpc(SendTo.Server)]
    void SpawnBallServerRpc(ulong clientId)
    {
        if (ballPool.Count == 0)
            return;

        Basketball ball = ballPool.Dequeue();

        ball.ResetBall();

        ball.transform.position = spawnPoint.position;
        ball.transform.rotation = spawnPoint.rotation; 
        ball.GetComponent<NetworkObject>().ChangeOwnership(clientId); //// ????

        ball.gameObject.SetActive(true);

        ulong netId = ball.GetComponent<NetworkObject>().NetworkObjectId;

        var clientParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        GiveBallClientRpc(netId, clientParams);
    }

    [ClientRpc]
    void GiveBallClientRpc(ulong ballNetId, ClientRpcParams clientRpcParams = default)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ballNetId, out NetworkObject netObj))
            return;

        XRGrabInteractable grab = netObj.GetComponent<XRGrabInteractable>();
        XRBaseInteractor interactor = FindLocalHandInteractor();

        if (grab == null || interactor == null)
            return;

        interactor.interactionManager.SelectEnter(
            (IXRSelectInteractor)interactor,
            (IXRSelectInteractable)grab
        );
    }

    XRBaseInteractor FindLocalHandInteractor()
    {
        XRBaseInteractor[] interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);

        foreach (var interactor in interactors)
        {
            if (interactor.gameObject.activeInHierarchy)
            {
                return interactor;
            }
        }

        return null;
    }

    public void ReturnBall(Basketball ball)
    {
        if (!IsServer)
            return;

        ball.gameObject.SetActive(false);
        ballPool.Enqueue(ball);
    }
}