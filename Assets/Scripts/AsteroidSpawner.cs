using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;
    public float spawnRate = 2f;
    public float spawnRadius = 18f; // Spawns just outside the camera

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);
            if (player == null) break;

            // Pick a random point in a circle around the player
            float angle = Random.Range(0f, 360f);
            Vector2 spawnPos = (Vector2)player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;

            GameObject asteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);

            // Point the asteroid roughly toward the player, but add some randomness so it drifts 
            Vector2 directionToPlayer = ((Vector2)player.position - spawnPos).normalized;
            float randomDrift = Random.Range(-45f, 45f); // 45 degree random drift
            
            asteroid.transform.up = Quaternion.Euler(0, 0, randomDrift) * directionToPlayer;
        }
    }
}