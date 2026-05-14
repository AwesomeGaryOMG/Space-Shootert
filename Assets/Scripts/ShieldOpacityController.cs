using UnityEngine;

// Assumes this is attached to the "ShipShield_Visual" object which has the SpriteRenderer (the knob)
[RequireComponent(typeof(SpriteRenderer))]
public class ShieldOpacityController : MonoBehaviour
{
    [Header("Dependencies")]
    // Drag the main PlayerShip object (holding the PlayerController script) here in Inspector
    public PlayerController playerController;

    [Header("Settings")]
    // The previous range was 0.0 to 1.0. 
    // We constrain the physical visual alpha here.
    // Setting default to exactly 25/255 scale (approx 0.098f) as requested.
    [Range(0f, 1f)]
    public float maxAlphaAllowed = 0.098f; 

    private SpriteRenderer shieldSpriteRenderer;

    void Start()
    {
        shieldSpriteRenderer = GetComponent<SpriteRenderer>();

        // Set initial state to invisible
        Color currentColor = shieldSpriteRenderer.color;
        currentColor.a = 0f;
        shieldSpriteRenderer.color = currentColor;
    }

    void Update()
    {
        if (playerController == null) return;

        // 1. Calculate the raw percentage (0.0f to 1.0f) based on health
        float currentShieldValue = playerController.shield;
        float maxShieldValue = 100f; // Assumed max from blueprint

        float shieldPercentage = currentShieldValue / maxShieldValue;
        shieldPercentage = Mathf.Clamp01(shieldPercentage); // Ensures 0-1 range

        // 2. Map the 0-1 percentage to the 0.0 to 0.098 range.
        // Multiply the health percentage by our specific max allowed alpha.
        // Example: If shield is at 100% (1.0), raw percentage is 1.0 * 0.098 = 0.098 (faint).

        float finalAlphaVisual = shieldPercentage * maxAlphaAllowed;

        // 3. Apply the constrained alpha to the SpriteRenderer (the green knob)
        // We preserve the existing RGB color of the knob.
        Color nextColor = shieldSpriteRenderer.color;
        nextColor.a = finalAlphaVisual;
        shieldSpriteRenderer.color = nextColor;
    }
}