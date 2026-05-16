using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip shootSound;
    public AudioClip explosionSound;
    public AudioClip playerHitSound;
    public AudioClip UIHoverSound; // Optional

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
        if (shootSound != null) sfxSource.PlayOneShot(shootSound, 0.5f); // 0.5f is volume
    }

    public void PlayExplosionSound()
    {
        if (explosionSound != null) sfxSource.PlayOneShot(explosionSound, 0.8f);
    }

    public void PlayPlayerHitSound()
    {
        if (playerHitSound != null) sfxSource.PlayOneShot(playerHitSound, 1f);
    }
}