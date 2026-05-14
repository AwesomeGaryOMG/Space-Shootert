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
        if (hitInfo.CompareTag("Enemy"))
        {
            hitInfo.GetComponent<EnemyAI>().TakeDamage(damage);
            gameObject.SetActive(false); // Return to pool on hit
        }
    }
}