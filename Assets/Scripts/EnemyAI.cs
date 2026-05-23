using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float hp = 30f;
    public float speed = 3f;
    public float stoppingDistance = 3f; 

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2.5f;
    public float bulletSpeed = 10f;
    
    private float fireTimer;
    private Transform player;
    private Rigidbody2D rb;

    [Header("Visual Effects")]
    public GameObject explosionPrefab;
    private DamageFlash damageFlash;
    private Camera cam;

    // ---> NEW: Reference to the vision script
    private EnemyVision vision;

    [Header("Loot Drops")]
    public GameObject[] lootPrefabs; // Drag your Medikit/Powerup prefabs here!
    [Range(0f, 1f)] public float dropChance = 0.2f; // 20% chance to drop an item

    public EnemyHealthBar healthBar; // <--- ADD THIS
    private float maxHp;             // <--- ADD THIS

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        
        rb = GetComponent<Rigidbody2D>();
        fireTimer = fireRate;
        damageFlash = GetComponent<DamageFlash>();
        cam = Camera.main; 

        // ---> NEW: Get the vision script if it exists
        vision = GetComponent<EnemyVision>();

        maxHp = hp; // Remember what our starting health is
        if (healthBar != null) healthBar.Setup(maxHp); // Tell the bar to set up
    }

    void Update()
    {
        if (player == null) return;

        // ---> NEW: If this is a stealth enemy and hasn't seen the player, don't shoot!
        if (vision != null && vision.isPlayerSpotted == false) return;

        if (!IsOnScreen()) return; 

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    bool IsOnScreen()
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        return viewportPos.x >= -0.1f && viewportPos.x <= 1.1f &&
               viewportPos.y >= -0.1f && viewportPos.y <= 1.1f;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // ---> CHANGED: Squad Patrol Behavior (Straight Line)
        if (vision != null && vision.isPlayerSpotted == false) 
        {
            // Just move forward in formation based on where the spawner pointed them!
            rb.MovePosition(rb.position + (Vector2)transform.right * (speed * 0.4f) * Time.fixedDeltaTime);
            return; 
        }

        // ... (Keep the rest of your chase code here)
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stoppingDistance)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    // ... (Keep your Shoot and TakeDamage methods exactly the same below here)
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSound();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            Vector2 shootDir = (player.position - firePoint.position).normalized;
            bulletRb.linearVelocity = shootDir * bulletSpeed;
        }
    }

    public void TakeDamage(float amount)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHullHitSound();
        if (damageFlash != null) damageFlash.Flash();
        hp -= amount;
        if (healthBar != null) healthBar.UpdateHealth(hp); // Tell the bar to shrink!
        if (hp <= 0)
        {
            if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();
            
            // ---> THE FIX: Only roll the dice for loot if the GameManager says we are in Level 3!
            if (GameManager.Instance != null && GameManager.Instance.currentLevel == 3)
            {
                if (lootPrefabs.Length > 0 && Random.value <= dropChance)
                {
                    int randomItem = Random.Range(0, lootPrefabs.Length);
                    Instantiate(lootPrefabs[randomItem], transform.position, Quaternion.identity);
                }
            }

            if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated();
            Destroy(gameObject); 
        }
    }
}