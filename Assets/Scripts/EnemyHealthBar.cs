using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage; 
    
    [Header("Positioning")]
    // ---> NEW: Adjust this in the Inspector to make the bar hover higher or lower!
    public Vector3 offset = new Vector3(0f, 1.5f, 0f); 

    private float maxHp;
    private Transform target; // The ship we are following

    void Start()
    {
        // 1. Remember exactly who our parent ship is before we detach
        target = transform.parent;

        // 2. THE FIX: Detach from the parent! This stops the orbiting and spinning entirely.
        transform.SetParent(null); 
    }

    void LateUpdate()
    {
        // 3. If the ship is still alive, follow it!
        if (target != null)
        {
            // Hover exactly at the offset position, no matter which way the ship faces
            transform.position = target.position + offset;
            
            // Lock rotation completely flat
            transform.rotation = Quaternion.identity; 
        }
        else
        {
            // 4. If the ship was destroyed, destroy this floating health bar too
            Destroy(gameObject);
        }
    }

    public void Setup(float startingHp)
    {
        maxHp = startingHp;
        if (fillImage != null) fillImage.fillAmount = 1f; 
    }

    public void UpdateHealth(float currentHp)
    {
        if (maxHp <= 0 || fillImage == null) return;
        fillImage.fillAmount = currentHp / maxHp; 
    }
}