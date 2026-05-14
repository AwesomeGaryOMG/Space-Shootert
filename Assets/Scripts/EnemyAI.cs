using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float hp = 30f;
    public float speed = 3f;
    public float stoppingDistance = 3f; // The distance at which the enemy stops chasing and just shoots

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2.5f;
    public float bulletSpeed = 10f;
    
    private float fireTimer;
    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        // Find the player automatically
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        
        rb = GetComponent<Rigidbody2D>();
        
        // Initialize the shoot timer
        fireTimer = fireRate;
    }

    void Update()
    {
        if (player == null) return;

        // Shooting timer logic
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Calculate direction and distance to the player
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        // Chase the player if they are further away than the stopping distance
        if (distance > stoppingDistance)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }

        // Always rotate to look at the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Missing Bullet Prefab or Fire Point!");
            return;
        }

        // Spawn the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        
        if (bulletRb != null)
        {
            // Calculate the exact direction to the player from the fire point and shoot
            Vector2 shootDir = (player.position - firePoint.position).normalized;
            bulletRb.linearVelocity = shootDir * bulletSpeed;
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        
        // Add particle effects here later!
        
        if (hp <= 0)
        {
            Destroy(gameObject); 
        }
    }
}