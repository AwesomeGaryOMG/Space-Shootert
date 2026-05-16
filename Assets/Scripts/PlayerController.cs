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

    [Header("Visual Effects")]
    public GameObject explosionPrefab;
    private DamageFlash damageFlash;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        damageFlash = GetComponent<DamageFlash>();
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

        // ---> NEW: Screen Clamping Logic <---
        // Convert screen edges (0 to 1) into actual game world positions
        Vector2 minBounds = cam.ViewportToWorldPoint(new Vector2(0, 0)); // Bottom-Left
        Vector2 maxBounds = cam.ViewportToWorldPoint(new Vector2(1, 1)); // Top-Right

        // Get current position and clamp it between the bounds
        Vector2 clampedPos = rb.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minBounds.x + screenPadding, maxBounds.x - screenPadding);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minBounds.y + screenPadding, maxBounds.y - screenPadding);

        // Apply the clamped position back to the Rigidbody
        rb.position = clampedPos;
    }

    // --- EXACT POWER ROUTING LOGIC FROM HTML ---
    // Instantly shifts 10 power to target system, draining others proportionally
    void BoostPower(string system)
    {
        float amount = 10f;
        
        if (system == "weapons") powerWeapons = Mathf.Min(100f, powerWeapons + amount);
        if (system == "engines") powerEngines = Mathf.Min(100f, powerEngines + amount);
        if (system == "shields") powerShields = Mathf.Min(100f, powerShields + amount);

        // Calculate total routed power and check for overflow
        float overflow = (powerWeapons + powerEngines + powerShields) - 100f;

        if (overflow > 0)
        {
            if (system == "weapons") DistributeOverflow(ref powerEngines, ref powerShields, overflow);
            if (system == "engines") DistributeOverflow(ref powerWeapons, ref powerShields, overflow);
            if (system == "shields") DistributeOverflow(ref powerWeapons, ref powerEngines, overflow);
        }

        // Prevent power going below zero
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

        // ---> NEW: Trigger the white flash!
        if (damageFlash != null) damageFlash.Flash();

        // ---> NEW: Play player hit sound
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerHitSound();

        // Trigger a heavy camera shake!
        if (CameraShake.Instance != null)
        {
            StartCoroutine(CameraShake.Instance.Shake(0.3f, 0.4f));
        }

        // --- LOSE CONDITION CHECK ---
        if (hull <= 0)
        {
            Debug.Log("GAME OVER - Player Destroyed");

            // ---> NEW: Spawn explosion and hide the ship!
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            
            // ---> NEW: Play explosion sound for player death
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
}