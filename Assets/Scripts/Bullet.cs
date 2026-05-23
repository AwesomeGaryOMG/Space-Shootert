using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    private float damage;
    private float lifetime = 1.5f; // From your HTML
    private float lifeTimer;

    public void Setup(Vector2 direction, float currentDamage)
    {
        damage = currentDamage;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        lifeTimer = lifetime;
    }

    void Update()
    {
        // Instead of Destroy(), we disable the bullet to return it to the pool
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0) gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. Did we hit an asteroid/obstacle? 
        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            gameObject.SetActive(false); // Return to pool so it doesn't fly through rocks
            return;
        }

        // 2. Did we hit an Enemy?
        if (hitInfo.CompareTag("Enemy"))
        {
            // ---> THE FIX: Safely check for every possible enemy script! <---
            
            // Check if it's a normal Scout
            EnemyAI scout = hitInfo.GetComponent<EnemyAI>();
            if (scout != null) 
            {
                scout.TakeDamage(damage); 
            }

            // Check if it's a Kamikaze
            KamikazeEnemy kamikaze = hitInfo.GetComponent<KamikazeEnemy>();
            if (kamikaze != null) 
            {
                kamikaze.TakeDamage(damage); 
            }

            // Check if it's the Final Boss
            HeavyBossAI boss = hitInfo.GetComponent<HeavyBossAI>();
            if (boss != null) 
            {
                boss.TakeDamage(damage); 
            }

            // Return to pool on hit
            gameObject.SetActive(false); 
        }
    }
}