using UnityEngine;

public class RadarScanner : MonoBehaviour
{
    [Header("Settings")]
    public float scanSpeed = -150f; // Negative number makes it spin clockwise!

    void Update()
    {
        // Continuously rotate the image around the Z axis (perfect for 2D UI)
        transform.Rotate(0, 0, scanSpeed * Time.deltaTime);
    }
}