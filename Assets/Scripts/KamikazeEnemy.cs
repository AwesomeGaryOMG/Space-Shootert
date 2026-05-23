using UnityEngine;

public class KamikazeEnemy : MonoBehaviour
{
    public float hp = 20f;
    public float speed = 4.5f; 
    public float explosionDamage = 30f;

    private Transform player;
    private Rigidbody2D rb;

    [Header("Visual Effects")]
    public GameObject explosionPrefab;
    private DamageFlash damageFlash;

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
        damageFlash = GetComponent<DamageFlash>();

        // ---> NEW: Get the vision script if it exists
        vision = GetComponent<EnemyVision>();

        maxHp = hp; // Remember what our starting health is
        if (healthBar != null) healthBar.Setup(maxHp); // Tell the bar to set up
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

        // Relentlessly chase the player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    // ... (Keep your OnCollisionEnter2D and TakeDamage methods exactly the same below here)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null) playerScript.TakeDamage(explosionDamage);
            if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated();
            Destroy(gameObject);
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