using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // Resets time just in case we quit to the menu from a paused game
        Time.timeScale = 1f; 
        
        // Loads Level 1 (Make sure Level 1 is exactly next in your Build Settings!)
        SceneManager.LoadScene("Level1"); 
    }

    public void QuitGame()
    {
        Debug.Log("Game is Exiting...");
        Application.Quit(); // This will close the compiled game
    }
}