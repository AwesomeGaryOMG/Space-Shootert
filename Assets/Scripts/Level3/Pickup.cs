using UnityEngine;

public class Pickup : MonoBehaviour
{
    // A dropdown menu in the Unity Inspector to choose what this item does!
    public enum PickupType { HullRepair, WeaponBoost, EngineBoost, ShieldCell }
    
    [Header("Pickup Settings")]
    public PickupType type;
    public float value = 25f; // How much health/power it gives
    public float lifetime = 12f; // Disappears if the player doesn't grab it
    public float floatSpeed = 1.5f;

    void Start()
    {
        // Don't let missed items float in the game forever
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Slowly drift down the screen towards the player
        transform.Translate(Vector3.down * floatSpeed * Time.deltaTime, Space.World);
        
        // Add a nice slow spin to make it look like a shiny collectible
        transform.Rotate(0, 0, 45f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null)
            {
                // Send the buff to the player!
                player.ApplyPickup(type, value);
            }
            
            // Optional: Play a nice ding sound here!
            // if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSound(); // (You can replace with a real pickup sound later)
            
            Destroy(gameObject);
        }
    }
}