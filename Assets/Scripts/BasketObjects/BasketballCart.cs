using System.Collections.Generic;
using UnityEngine;

public class BasketballCart : MonoBehaviour
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
        {
            Debug.Log("TODAVIA NO HAY PELOTAS");
            return; // @TODO: opcional expandir pool dinámicamente
        }

        lastSpawnTime = Time.time;

        Basketball ball = ballPool.Dequeue();

        ball.transform.position = spawnPoint.position;
        ball.transform.rotation = spawnPoint.rotation;

        ball.ResetBall();
        ball.gameObject.SetActive(true);
    }

    public void ReturnBall(Basketball ball)
    {
        ball.gameObject.SetActive(false);
        ballPool.Enqueue(ball);
    }
}
