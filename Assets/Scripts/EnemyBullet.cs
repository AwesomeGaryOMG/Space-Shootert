using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float lifeTime = 2.5f; // Matches HTML life: 2.5
    public float damage = 10f;

    void Start()
    {
        // Automatically destroy the bullet after 'lifeTime' seconds
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Ignore other enemies
        if (hitInfo.CompareTag("Enemy")) return;

        // If it hits the player, deal damage
        if (hitInfo.CompareTag("Player"))
        {
            PlayerController player = hitInfo.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            // Destroy the bullet on impact
            Destroy(gameObject);
        }
    }
}