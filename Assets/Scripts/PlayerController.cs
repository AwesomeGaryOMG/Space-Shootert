using UnityEngine;

// --- CRITICAL INTEGRATION STEP ---
// Make sure your PlayerShip GameObject in Unity is assigned the Tag "Player"!
// This script assumes that tag is set for enemy bullet collisions.
// ---------------------------------

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float hull = 100f;
    public float shield = 100f;

    [Header("Power System (0-100)")]
    public float powerWeapons = 33f;
    public float powerEngines = 33f;
    public float powerShields = 34f;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector2 mousePos;
    
    // We use a custom velocity variable to perfectly match your HTML friction math
    private Vector2 customVelocity; 

    // ---> NEW: Padding so the ship doesn't hang halfway off screen
    private float screenPadding = 0.5f;

    [Header("Level Boundaries")]
    public float minX = -15f; // Left wall
    public float maxX = 15f;  // Right wall
    public float minY = -5f;  // Bottom wall (Start line)
    public float maxY = 200f; // Top wall (End of the level)

    [Header("Visual Effects")]
    public GameObject explosionPrefab;
    public DamageFlash hullFlash;   // For the spaceship
    public DamageFlash shieldFlash; // For the shield bubble

    public float currentTotalPowerLimit = 100f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (GameManager.Instance != null && GameManager.Instance.isWeaponJammed)
        {
            // Level 2 Stealth: Weapons offline, 50/50 split!
            powerWeapons = 0f;
            powerEngines = 50f;
            powerShields = 50f;
        }
        else
        {
            // Standard Levels: Normal 3-way split (100 / 3)
            powerWeapons = 33.33f;
            powerEngines = 33.33f;
            powerShields = 33.33f;
        }
    }

    void Update()
    {
        // 1. Mouse Aiming Input
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // 2. Power Routing Inputs (1, 2, 3)
        if (Input.GetKeyDown(KeyCode.Alpha1)) BoostPower("weapons");
        if (Input.GetKeyDown(KeyCode.Alpha2)) BoostPower("engines");
        if (Input.GetKeyDown(KeyCode.Alpha3)) BoostPower("shields");

        // 3. Exact Shield Regen Math from HTML
        // Recharges proportionally based on the percentage of power routed to Shields
        shield = Mathf.Min(100f, shield + (powerShields / 100f) * 12f * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // --- MOVEMENT MATH (Matched to HTML) ---
        // Using GetAxisRaw for snappy, instant-on input response
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(inputX, inputY).normalized;

        if (inputDir.sqrMagnitude > 0)
        {
            float accel = 0.6f; // Exactly matching HTML acceleration
            customVelocity += inputDir * accel;
        }

        // --- SPEED CAP (Matched to HTML) ---
        // Base speed 2, plus scaled boost up to 7, based on Engine power
        float maxSpeed = 2f + (powerEngines / 100f) * 5f;
        float currentSpeed = customVelocity.magnitude;

        if (currentSpeed > maxSpeed)
        {
            customVelocity = (customVelocity / currentSpeed) * maxSpeed;
        }

        // --- FRICTION (Matched to HTML) ---
        // Mimics the 'drifty' feel of the JS game
        customVelocity *= 0.92f; 

        // --- APPLY TO RIGIDBODY ---
        // Multiplied by 50 to scale HTML pixels to Unity Units, plus FixedDeltaTime
        rb.linearVelocity = customVelocity * 50f * Time.fixedDeltaTime;

        // --- ROTATION MATH ---
        // Points the ship at the mouse cursor
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;

        // ---> NEW: Massive World Clamping Logic <---
        Vector2 clampedPos = rb.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minY, maxY);

        // Apply the clamped position back to the Rigidbody
        rb.position = clampedPos;
    }

    // --- EXACT POWER ROUTING LOGIC FROM HTML ---
    // Instantly shifts 10 power to target system, draining others proportionally
    // --- EXACT POWER ROUTING LOGIC FROM HTML ---
    // Instantly shifts 10 power to target system, draining others proportionally
    void BoostPower(string system)
    {
        if (system == "weapons" && GameManager.Instance != null && GameManager.Instance.isWeaponJammed)
        {
            return; 
        }

        float amount = 10f;
        
        if (system == "weapons") powerWeapons = Mathf.Min(100f, powerWeapons + amount);
        if (system == "engines") powerEngines = Mathf.Min(100f, powerEngines + amount);
        if (system == "shields") powerShields = Mathf.Min(100f, powerShields + amount);

        // ---> CHANGED: Instead of a hard 100f, we use your new growing limit! <---
        float overflow = (powerWeapons + powerEngines + powerShields) - currentTotalPowerLimit;

        if (overflow > 0)
        {
            if (system == "weapons") DistributeOverflow(ref powerEngines, ref powerShields, overflow);
            if (system == "engines") DistributeOverflow(ref powerWeapons, ref powerShields, overflow);
            if (system == "shields") DistributeOverflow(ref powerWeapons, ref powerEngines, overflow);
        }

        powerWeapons = Mathf.Max(0, powerWeapons);
        powerEngines = Mathf.Max(0, powerEngines);
        powerShields = Mathf.Max(0, powerShields);
    }

    void DistributeOverflow(ref float other1, ref float other2, float overflow)
    {
        float totalOthers = other1 + other2;
        if (totalOthers > 0)
        {
            other1 -= overflow * (other1 / totalOthers);
            other2 -= overflow * (other2 / totalOthers);
        }
    }

    // Call this from the shooting script to get current weapon damage
    public float GetShipDamage()
    {
        // Base 4, plus scaled boost up to 20, based on Weapon power
        return 4f + (powerWeapons / 100f) * 16f; // Exact HTML Math
    }

    // =========================================================================
    //  PHASE 4 ADDITION: Receive Damage & Game Over Logic
    // =========================================================================
    // This function must be public so enemy bullets can call it.
    public void TakeDamage(float amount)
    {
        // Remember if we had a shield BEFORE taking damage
        bool hitShield = shield > 0;

        // 1. Exact HTML logic: Shield absorbs damage first
        if (shield > 0)
        {
            shield -= amount;
            if (shield < 0)
            {
                hull += shield; 
                shield = 0;
            }
        }
        else
        {
            hull -= amount;
        }

        // ---> NEW: Flash the correct sprite AND play the correct sound!
        if (hitShield)
        {
            if (shieldFlash != null) shieldFlash.Flash();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShieldHitSound();
        }
        else
        {
            if (hullFlash != null) hullFlash.Flash();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHullHitSound();
        }

        // [TEMPORARILY DISABLED] Trigger a heavy camera shake!
        // if (CameraShake.Instance != null)
        // {
        //     StartCoroutine(CameraShake.Instance.Shake(0.15f, 0.1f));
        // }

        // --- LOSE CONDITION CHECK ---
        if (hull <= 0)
        {
            Debug.Log("GAME OVER - Player Destroyed");

            // Spawn explosion and hide the ship!
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            
            // ---> PLAY EXPLOSION SOUND ON DEATH
            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();
            
            GetComponent<SpriteRenderer>().enabled = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Time.timeScale = 0f; 
            }
        }
    }

    // =========================================================================
    //  FINAL ADDITION: Loot Drop & Upgrade System (Rogue-Lite Progression)
    // =========================================================================
    public void ApplyPickup(Pickup.PickupType type, float amount)
    {
        switch (type)
        {
            case Pickup.PickupType.HullRepair:
                // Health heals the ship instantly
                hull = Mathf.Min(100f, hull + amount); 
                break;
            
            case Pickup.PickupType.ShieldCell:
                // Permanently increases the total power pool and the Shield power!
                currentTotalPowerLimit = Mathf.Min(300f, currentTotalPowerLimit + amount);
                powerShields = Mathf.Min(100f, powerShields + amount);
                break;
            
            case Pickup.PickupType.WeaponBoost:
                // Permanently increases the total power pool and Weapon power!
                currentTotalPowerLimit = Mathf.Min(300f, currentTotalPowerLimit + amount);
                powerWeapons = Mathf.Min(100f, powerWeapons + amount);
                break;
            
            case Pickup.PickupType.EngineBoost:
                // Permanently increases the total power pool and Engine power!
                currentTotalPowerLimit = Mathf.Min(300f, currentTotalPowerLimit + amount);
                powerEngines = Mathf.Min(100f, powerEngines + amount);
                break;
        }
    }

    // =========================================================================
    //  KNOCKBACK SYSTEM
    // =========================================================================
    public void ApplyKnockback(Vector2 knockbackDirection, float force)
    {
        // Instantly spikes the player's custom velocity, pushing them backward!
        customVelocity += knockbackDirection * force;
    }
}