using UnityEngine;
using System.Collections;

public class MenuSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] backgroundObjects; 
    public float spawnRate = 1.5f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 50% chance to spawn on the Left side, 50% chance for the Right side
            bool spawnOnLeft = Random.value > 0.5f;

            float spawnY = Random.Range(-8f, 8f); // Pick a random height on the screen
            float spawnX;
            float flightAngle;

            if (spawnOnLeft)
            {
                spawnX = -15f; // Off-screen to the left
                // Point it generally towards the right, with a slight random tilt up or down
                flightAngle = Random.Range(-30f, 30f); 
            }
            else
            {
                spawnX = 15f; // Off-screen to the right
                // Point it generally towards the left, with a slight random tilt up or down
                flightAngle = Random.Range(150f, 210f); 
            }

            Vector2 spawnPos = new Vector2(spawnX, spawnY);
            
            // Spawn the dummy ship
            GameObject prefabToSpawn = backgroundObjects[Random.Range(0, backgroundObjects.Length)];
            GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            // Rotate the ship so its nose (the right side) aims exactly where it is flying!
            obj.transform.rotation = Quaternion.Euler(0, 0, flightAngle);

            yield return new WaitForSeconds(spawnRate);
        }
    }
}