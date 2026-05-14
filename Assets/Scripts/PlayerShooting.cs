using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Transform[] firePoints;
    public float fireRate = 0.18f; // Exact cooldown from your HTML
    private float nextFireTime = 0f;
    
    private PlayerController player;

    void Start()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        // Change to "Fire1" (Left Click) or keep it as KeyCode.X based on your preference
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        float currentDamage = player.GetShipDamage(); // Pulls the scaled damage from Phase 1

        foreach (Transform firePoint in firePoints)
        {
            // Grab a bullet from the pool instead of Instantiating
            GameObject bullet = BulletPool.Instance.GetBullet();
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            
            // Fire it
            bullet.GetComponent<Bullet>().Setup(firePoint.right, currentDamage);
        }
    }
}