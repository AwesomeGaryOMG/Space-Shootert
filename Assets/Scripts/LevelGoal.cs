using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player touches the portal...
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayPortalCompleteSound();
                // Trigger the Win Screen!
                GameManager.Instance.LevelComplete();
            }
        }
    }
}