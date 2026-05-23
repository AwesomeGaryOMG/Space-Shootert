using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // Drag your player ship here in the Inspector!

    [Header("Settings")]
    public float smoothSpeed = 3f; // Reduced from 5f for a smoother glide
    // Updated vertical offset so the player is lower on the screen, giving more forward view
    public Vector3 offset = new Vector3(0f, 2f, -10f); 

    void LateUpdate()
    {
        if (player == null) return;

        // Calculate where the camera should be
        Vector3 desiredPosition = player.position + offset;
        
        // Smoothly glide towards that position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }
}