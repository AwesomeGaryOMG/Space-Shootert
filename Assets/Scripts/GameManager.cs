using UnityEngine;
using TMPro; // <-- Added TextMeshPro namespace
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level Settings")]
    public int currentLevel = 1;
    public bool isWeaponJammed = false;

    [Header("Wave Management")]
    public Transform[] spawnPoints;
    public GameObject scoutEnemyPrefab;
    public GameObject kamikazeEnemyPrefab;
    public GameObject heavyEnemyPrefab;

    private int totalEnemiesToSpawn;
    private int enemiesAlive;
    private bool levelComplete = false;

    [Header("UI References")]
    public GameObject gameOverScreen;
    public GameObject levelCompleteScreen;
    public GameObject pauseMenu;

    // <-- Changed from Text to TextMeshProUGUI
    public TextMeshProUGUI objectiveText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetupLevel(currentLevel);
    }

    void SetupLevel(int level)
    {
        currentLevel = level;
        levelComplete = false;

        switch (level)
        {
            case 1:
                objectiveText.text = "OBJECTIVE: DESTROY ALL ENEMIES";
                isWeaponJammed = false;
                StartCoroutine(SpawnWave(5, scoutEnemyPrefab));
                break;
            case 2:
                objectiveText.text = "OBJECTIVE: WEAPONS JAMMED! SURVIVE & REACH EXIT";
                isWeaponJammed = true;
                StartCoroutine(SpawnWave(8, kamikazeEnemyPrefab));
                break;
            case 3:
                objectiveText.text = "OBJECTIVE: DEFEAT THE HEAVY FLEET";
                isWeaponJammed = false;
                StartCoroutine(SpawnWave(10, heavyEnemyPrefab));
                break;
        }
    }

    IEnumerator SpawnWave(int count, GameObject enemyToSpawn)
    {
        totalEnemiesToSpawn = count;
        enemiesAlive = count;

        for (int i = 0; i < count; i++)
        {
            // Pick a random spawn point
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyToSpawn, sp.position, Quaternion.identity);

            // Wait a few seconds before spawning the next one
            yield return new WaitForSeconds(2.5f);
        }
    }

    public void EnemyDefeated()
    {
        enemiesAlive--;

        // Level 1 Win Condition
        if (currentLevel == 1 && enemiesAlive <= 0 && !levelComplete)
        {
            LevelComplete();
        }
    }

    public void LevelComplete()
    {
        levelComplete = true;
        levelCompleteScreen.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Unpause the game
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void Pause() // Unused, function duplicate in PlayerController.cs
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    public void Continue()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // Unpause the game
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        UnityEngine.Application.Quit();
    }
}