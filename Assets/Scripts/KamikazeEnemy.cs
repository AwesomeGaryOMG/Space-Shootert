using UnityEngine;

public class KamikazeEnemy : MonoBehaviour
{
    public float hp = 20f;
    public float speed = 4.5f; // Faster than normal enemies
    public float explosionDamage = 30f;

    private Transform player;
    private Rigidbody2D rb;

    [Header("Visual Effects")]
    public GameObject explosionPrefab;
    private DamageFlash damageFlash;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        rb = GetComponent<Rigidbody2D>();

        damageFlash = GetComponent<DamageFlash>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Relentlessly chase the player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(explosionDamage);
            }
            
            // ---> NEW: Spawn the explosion before destroying the kamikaze ship
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            
            if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated();
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        
        // Trigger the white flash
        if (damageFlash != null) damageFlash.Flash();
        
        if (hp <= 0)
        {
            // Spawn explosion exactly where the enemy is
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // ---> NEW: Play explosion sound
            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();

            if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated();
            Destroy(gameObject); 
        }
    }
}