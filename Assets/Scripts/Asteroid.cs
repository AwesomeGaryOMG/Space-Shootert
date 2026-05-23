using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public float speed = 4f;
    public float damage = 20f;
    public float lifetime = 12f;
    private float rotationSpeed;

    // ---> NEW: Slot for your asteroid explosion particle system
    public GameObject explosionPrefab;

    void Start()
    {
        // Destroy after a while so they don't clutter the game forever
        Destroy(gameObject, lifetime);
        
        // Give it a random tumble effect
        rotationSpeed = Random.Range(-100f, 100f);
    }

    void Update()
    {
        // Move forward constantly
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
        // Spin visually
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(damage);

            // ---> NEW: Spawn the explosion effect!
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Play explosion and destroy asteroid
            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();
            Destroy(gameObject);
        }
        else if (col.CompareTag("EnemyBullet") || col.CompareTag("Bullet"))
        {
            // ---> NEW: Spawn the explosion effect!
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Bullets destroy asteroids!
            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();
            Destroy(gameObject);
        }
    }
}