using UnityEngine;
using UnityEngine.UI; // <-- This handles both Standard Text and Images!

public class JammingAlarmUI : MonoBehaviour
{
    [Header("UI References")]
    public Text warningText; // <-- Changed from TextMeshProUGUI to standard Text
    public Image redScreenOverlay; 

    [Header("Pulse Settings")]
    public float pulseSpeed = 4f;

    void Update()
    {
        // Only pulse if the GameManager says weapons are jammed
        if (GameManager.Instance != null && GameManager.Instance.isWeaponJammed)
        {
            // Calculate a smooth pulsing effect using a Sine wave
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; 

            if (warningText != null)
            {
                warningText.gameObject.SetActive(true);
                // Pulses between 30% and 100% visibility
                float textAlpha = Mathf.Lerp(0.3f, 1f, pulse); 
                warningText.color = new Color(1f, 0f, 0f, textAlpha); // Bright Red
            }

            if (redScreenOverlay != null)
            {
                redScreenOverlay.gameObject.SetActive(true);
                // Pulses between 0% and 25% visibility
                float overlayAlpha = Mathf.Lerp(0f, 0.01f, pulse); 
                redScreenOverlay.color = new Color(1f, 0f, 0f, overlayAlpha); 
            }
        }
        else
        {
            // Hide everything if weapons are working normally
            if (warningText != null) warningText.gameObject.SetActive(false);
            if (redScreenOverlay != null) redScreenOverlay.gameObject.SetActive(false);
        }
    }
}