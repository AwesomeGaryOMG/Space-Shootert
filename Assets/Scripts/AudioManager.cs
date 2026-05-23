using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip shootSound;
    public AudioClip explosionSound;
    // Split the old "playerHitSound" into two distinct sounds
    public AudioClip hullHitSound; 
    public AudioClip shieldHitSound;
    // New level complete sound
    public AudioClip portalCompleteSound; 

    void Awake()
    {
        // Simple Singleton so we can call this from anywhere
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayShootSound()
    {
        if (shootSound != null) sfxSource.PlayOneShot(shootSound, 0.4f);
    }

    public void PlayExplosionSound()
    {
        if (explosionSound != null) sfxSource.PlayOneShot(explosionSound, 0.7f);
    }

    public void PlayHullHitSound()
    {
        if (hullHitSound != null) sfxSource.PlayOneShot(hullHitSound, 0.8f);
    }

    public void PlayShieldHitSound()
    {
        if (shieldHitSound != null) sfxSource.PlayOneShot(shieldHitSound, 1.0f);
    }

    public void PlayPortalCompleteSound()
    {
        if (portalCompleteSound != null) sfxSource.PlayOneShot(portalCompleteSound, 0.9f);
    }
}