using UnityEngine;

public class HeavyBossAI : MonoBehaviour
{
    [Header("Boss Stats")]
    public float hp = 500f; // Massive health pool!
    public float speed = 1.5f; // Slower than normal enemies
    public float stoppingDistance = 5f;

    [Header("Shooting Setup")]
    public GameObject bulletPrefab;
    public Transform[] firePoints; 
    public float fireRate = 1.5f; 
    public float bulletSpeed = 8f;

    // ==========================================
    // ---> NEW: Ramming / Smash Attack Setup
    // ==========================================
    [Header("Ramming / Smash Attack")]
    public float dashSpeed = 12f; // How fast it charges
    public float dashCooldown = 6f; // Time between dashes
    public float dashDuration = 0.6f; // How long the dash lasts
    public float dashDamage = 45f; // Massive damage!
    public float knockbackPower = 8f; // How hard it pushes the player

    private float fireTimer;
    private float dashTimer;
    private float currentDashTime;
    private bool isDashing = false;
    private Vector2 dashTargetDirection;
    // ==========================================

    private Transform player;
    private Rigidbody2D rb;

    [Header("Visual Effects")]
    public GameObject explosionPrefab;
    private DamageFlash damageFlash;
    private Camera cam;

    public EnemyHealthBar healthBar; 
    private float maxHp;             

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        
        rb = GetComponent<Rigidbody2D>();
        damageFlash = GetComponent<DamageFlash>();
        cam = Camera.main; 

        maxHp = hp; 
        if (healthBar != null) healthBar.Setup(maxHp); 

        fireTimer = fireRate;
        dashTimer = dashCooldown; // ---> NEW: Start the dash cooldown
    }

    void Update()
    {
        if (player == null) return;
        if (!IsOnScreen()) return; 

        // ---> CHANGED: Split logic between Dashing and Normal behavior
        if (!isDashing)
        {
            // 1. Tick down the Dash Timer
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                StartDash();
            }

            // 2. Tick down the Shooting Timer (Only shoots if NOT dashing)
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                FireAllGuns();
                fireTimer = fireRate;
            }
        }
        else
        {
            // 3. Tick down how long the dash lasts
            currentDashTime -= Time.deltaTime;
            if (currentDashTime <= 0f)
            {
                isDashing = false;
                dashTimer = dashCooldown; // Reset the dash cooldown
            }
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

        // ---> CHANGED: Handle Dash Movement vs Normal Movement
        if (isDashing)
        {
            // DASH MOVEMENT: Rocket forward blindly in the locked direction!
            rb.MovePosition(rb.position + dashTargetDirection * dashSpeed * Time.fixedDeltaTime);
            
            // Keep the sprite looking in the direction of the dash
            float dashAngle = Mathf.Atan2(dashTargetDirection.y, dashTargetDirection.x) * Mathf.Rad2Deg;
            rb.rotation = dashAngle + 180f;
            return; // Skip normal movement
        }

        // NORMAL MOVEMENT: Slowly creep towards the player
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stoppingDistance)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle + 180f; 
    }

    // ---> NEW: Prepares the Boss to launch forward
    void StartDash()
    {
        isDashing = true;
        currentDashTime = dashDuration;
        
        // Lock onto where the player is RIGHT NOW
        dashTargetDirection = (player.position - transform.position).normalized;
    }

    // ---> NEW: Detect if the Boss physically hits the player
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController p = collision.gameObject.GetComponent<PlayerController>();
            if (p != null)
            {
                // Deal massive damage if dashing, or minor scraping damage if just bumping
                p.TakeDamage(isDashing ? dashDamage : 15f);

                // Calculate which way to push the player
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;

                // Apply the knockback!
                p.ApplyKnockback(pushDirection, knockbackPower);
            }
        }
    }

    void FireAllGuns()
    {
        if (bulletPrefab == null || firePoints.Length == 0) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSound();

        Vector2 shootDir = (player.position - transform.position).normalized;

        foreach (Transform fp in firePoints)
        {
            if (fp == null) continue; 

            GameObject bullet = Instantiate(bulletPrefab, fp.position, transform.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = shootDir * bulletSpeed;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHullHitSound();
        if (damageFlash != null) damageFlash.Flash();
        
        hp -= amount;
        if (healthBar != null) healthBar.UpdateHealth(hp); 
        
        if (hp <= 0)
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Instantiate(explosionPrefab, transform.position + new Vector3(1.5f, 1.5f, 0), Quaternion.identity);
                Instantiate(explosionPrefab, transform.position + new Vector3(-1.5f, -1.5f, 0), Quaternion.identity);
                Instantiate(explosionPrefab, transform.position + new Vector3(1.5f, -1.5f, 0), Quaternion.identity);
                Instantiate(explosionPrefab, transform.position + new Vector3(-1.5f, 1.5f, 0), Quaternion.identity);
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();
            
            if (GameManager.Instance != null) GameManager.Instance.LevelComplete();
            
            Destroy(gameObject); 
        }
    }
}