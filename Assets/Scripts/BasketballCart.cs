using UnityEngine;

public class BasketballCart : MonoBehaviour
{
    [Header("Ball")]
    public GameObject basketballPrefab;
    public Transform spawnPoint;
    public float spawnCooldown = 0.5f;

    float lastSpawnTime;

    public void SpawnBall()
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        lastSpawnTime = Time.time;

        Instantiate(basketballPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
