using UnityEngine;
using TMPro; // <-- Added TextMeshPro namespace
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level Settings")]
    public int currentLevel = 1;
    public bool isWeaponJammed = false;
    public float levelCompleteDelay = 1.5f;
    
    [Header("Wave Management")]
    public GameObject scoutEnemyPrefab;
    public GameObject kamikazeEnemyPrefab; 
    public GameObject heavyEnemyPrefab;    
    
    private int totalEnemiesToSpawn;
    private int enemiesAlive;
    private bool levelComplete = false;

    [Header("UI References")]
    public GameObject gameOverScreen;
    public GameObject levelCompleteScreen;
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI waveAnnouncementText;

    public GameObject pauseMenuScreen;
    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetupLevel(currentLevel);
    }

    void Update()
    {
        // Check for ESC key, but only if the game isn't already over
        if (Input.GetKeyDown(KeyCode.Escape) && !levelComplete && (gameOverScreen == null || !gameOverScreen.activeSelf))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void SetupLevel(int level)
    {
        currentLevel = level;
        levelComplete = false;
        
        switch(level)
        {
            case 1:
                objectiveText.text = "OBJECTIVE: SURVIVE 3 WAVES & REACH EXIT";
                isWeaponJammed = false;
                // Start the new 3-wave campaign spawner!
                StartCoroutine(CampaignWaveSpawner(scoutEnemyPrefab)); 
                break;
            case 2:
                objectiveText.text = "OBJECTIVE: WEAPONS JAMMED! SURVIVE & REACH EXIT";
                isWeaponJammed = true;
                // ---> CHANGED: Call a new spawner made specifically for Stealth mode
                StartCoroutine(StealthWaveSpawner()); 
                break;
            case 3:
                objectiveText.text = "FINAL OBJECTIVE: DEFEAT THE HEAVY FLEET";
                isWeaponJammed = false;
                StartCoroutine(FinalBossGauntlet()); // ---> NEW Level 3 Spawner
                break;
        }
    }

    IEnumerator CampaignWaveSpawner(GameObject enemyToSpawn)
    {
        int totalWaves = 3; // From your original Sprint 3 document

        for (int currentWave = 1; currentWave <= totalWaves; currentWave++)
        {
            // 1. THE COUNTDOWN (Ticket #1)
            if (waveAnnouncementText != null)
            {
                waveAnnouncementText.gameObject.SetActive(true);
                
                // Show which wave is starting
                waveAnnouncementText.text = "WAVE " + currentWave + " INCOMING!";
                yield return new WaitForSeconds(1.5f); 
                
                // The actual 3.. 2.. 1.. countdown
                for (int c = 3; c > 0; c--)
                {
                    waveAnnouncementText.text = c.ToString();
                    
                    // Optional: If you want a beep sound for each second, uncomment the line below!
                    // if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSound(); 
                    
                    yield return new WaitForSeconds(1f);
                }
                
                // The GO moment!
                waveAnnouncementText.text = "GO!";
                // Optional: Play a louder sound for GO!
                // if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();
                
                yield return new WaitForSeconds(0.5f);
                
                // Hide text once wave starts
                waveAnnouncementText.gameObject.SetActive(false);
            }

            // 2. SPAWN THE ENEMIES
            // Wave 1 = 4 enemies, Wave 2 = 6 enemies, Wave 3 = 8 enemies
            int enemiesThisWave = 2 + (currentWave * 2); 

            for (int i = 0; i < enemiesThisWave; i++)
            {
                if (levelComplete) yield break; // Stop spawning if they touch the portal

                float spawnY = Camera.main.transform.position.y + 22f; 
                float spawnX = Random.Range(-12f, 12f); 

                Vector2 dynamicSpawnPoint = new Vector2(spawnX, spawnY);
                Instantiate(enemyToSpawn, dynamicSpawnPoint, Quaternion.identity);
                
                yield return new WaitForSeconds(1.5f);
            }

            // 3. REST PERIOD
            // Wait 5 seconds before the next wave starts
            yield return new WaitForSeconds(5f); 
        }
        
        // After 3 waves, they just have to fly to the portal!
    }

    IEnumerator StealthWaveSpawner()
    {
        float spawnRate = 1.2f; // How fast new enemies appear. Lower = harder!

        // Keep spawning endlessly until the player touches the exit portal
        while (!levelComplete) 
        {
            // Always track where the camera currently is
            float camY = Camera.main.transform.position.y;
            
            // Pick a random side: 0 = Left, 1 = Right, 2 = Top
            int side = Random.Range(0, 3); 

            Vector2 spawnPos = Vector2.zero;
            float flightAngle = 0f;

            if (side == 0) // Spawn Left, patrol Right
            {
                // Spawn off-screen to the left, slightly above the player
                spawnPos = new Vector2(-16f, camY + Random.Range(5f, 15f));
                flightAngle = Random.Range(-15f, 15f); // Point nose roughly to the right
            }
            else if (side == 1) // Spawn Right, patrol Left
            {
                // Spawn off-screen to the right, slightly above the player
                spawnPos = new Vector2(16f, camY + Random.Range(5f, 15f));
                flightAngle = Random.Range(165f, 195f); // Point nose roughly to the left
            }
            else // Spawn Top, patrol Downwards
            {
                // Spawn high above the camera, anywhere along the X axis
                spawnPos = new Vector2(Random.Range(-12f, 12f), camY + 25f);
                flightAngle = Random.Range(240f, 300f); // Point nose roughly downwards
            }

            // Flip a coin to mix Scouts and Kamikazes
            GameObject enemyToSpawn = (Random.value > 0.5f) ? kamikazeEnemyPrefab : scoutEnemyPrefab;

            // Spawn the enemy facing the correct direction
            GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.Euler(0, 0, flightAngle));

            // ---> CRITICAL MEMORY FIX <---
            // Because this runs endlessly, we MUST automatically delete enemies 
            // after 20 seconds so your game doesn't lag from having 1,000 ships off-screen!
            Destroy(spawnedEnemy, 20f);

            // Wait a moment before spawning the next one
            yield return new WaitForSeconds(spawnRate);
        }
    }

    IEnumerator FinalBossGauntlet()
    {
        int currentWave = 1;

        while (currentWave <= 7)
        {
            if (waveAnnouncementText != null)
            {
                waveAnnouncementText.gameObject.SetActive(true);
                waveAnnouncementText.text = currentWave == 7 ? "FINAL BOSS INCOMING!" : "WAVE " + currentWave;
                yield return new WaitForSeconds(2f);
                waveAnnouncementText.gameObject.SetActive(false);
            }

            // Wave Logic
            if (currentWave <= 3) 
            {
                // WAVES 1-3: 360-Degree Kamikaze Swarm (Ultra Hard)
                int count = currentWave * 4; // More enemies every wave!
                for(int i = 0; i < count; i++)
                {
                    Instantiate(kamikazeEnemyPrefab, GetSurroundSpawnPoint(), Quaternion.identity);
                    yield return new WaitForSeconds(0.8f); // Faster spawning!
                }
            }
            else if (currentWave <= 6)
            {
                // WAVES 4-6: 360-Degree Mixed Fleet
                int count = 6 + currentWave; // Numbers get overwhelming here
                for(int i = 0; i < count; i++)
                {
                    GameObject enemy = (Random.value > 0.5f) ? kamikazeEnemyPrefab : scoutEnemyPrefab;
                    Instantiate(enemy, GetSurroundSpawnPoint(), Quaternion.identity);
                    yield return new WaitForSeconds(1f);
                }
            }

            // WAVE 7: THE BOSS
            if (currentWave == 7)
            {
                float spawnY = Camera.main.transform.position.y + 22f;
                // Spawn the Boss directly in front of the player
                Instantiate(heavyEnemyPrefab, new Vector2(0, spawnY), Quaternion.identity);
                
                // End the spawner loop entirely. The Boss script handles the Win condition!
                yield break; 
            }

            // Rest period between waves
            yield return new WaitForSeconds(5f);
            currentWave++;
        }
    }

    // ---> NEW METHOD: Drops enemies from every possible direction!
    Vector2 GetSurroundSpawnPoint()
    {
        float camX = Camera.main.transform.position.x;
        float camY = Camera.main.transform.position.y;
        
        int side = Random.Range(0, 4); // 0=Top, 1=Bottom, 2=Left, 3=Right

        if (side == 0) return new Vector2(camX + Random.Range(-12f, 12f), camY + 22f); // Top
        if (side == 1) return new Vector2(camX + Random.Range(-12f, 12f), camY - 15f); // Bottom (Sneaking up behind!)
        if (side == 2) return new Vector2(camX - 16f, camY + Random.Range(-10f, 15f)); // Left
        return new Vector2(camX + 16f, camY + Random.Range(-10f, 15f)); // Right
    }

    // Keep this here so Level 2 and 3 don't break the compiler!
    IEnumerator SpawnWave(int count, GameObject enemyToSpawn)
    {
        totalEnemiesToSpawn = count;
        enemiesAlive = count;

        for (int i = 0; i < count; i++)
        {
            float spawnY = Camera.main.transform.position.y + 22f; 
            float spawnX = Random.Range(-12f, 12f); 

            Vector2 dynamicSpawnPoint = new Vector2(spawnX, spawnY);
            Instantiate(enemyToSpawn, dynamicSpawnPoint, Quaternion.identity);
            
            yield return new WaitForSeconds(2.5f);
        }
    }

    public void EnemyDefeated()
    {
        enemiesAlive--;
        
        // We completely removed the old "kill all enemies to win" logic from here.
        // Now, the ONLY way to win is to touch the LevelGoal portal!
    }

    public void LevelComplete()
    {
        levelComplete = true;
        
        // ---> NEW: Play the portal sound here if you have it!
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPortalCompleteSound();

        // Start the delay timer instead of stopping time instantly
        StartCoroutine(LevelCompleteRoutine());
    }

    IEnumerator LevelCompleteRoutine()
    {
        // Wait for however many seconds you set in the Inspector
        yield return new WaitForSeconds(levelCompleteDelay);
        
        // NOW show the screen and pause the game
        if (levelCompleteScreen != null) levelCompleteScreen.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void LoadNextLevel()
    {
        // 1. Always unpause time before changing scenes!
        Time.timeScale = 1f; 

        // 2. Find out what scene we are currently on, and add 1 to get the next one
        int nextSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;

        // 3. Load it!
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
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

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuScreen != null) pauseMenuScreen.SetActive(true);
        Time.timeScale = 0f; // Freeze everything
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuScreen != null) pauseMenuScreen.SetActive(false);
        Time.timeScale = 1f; // Unfreeze everything
    }

    public void QuitToMainMenu()
    {
        // Always reset time to normal before switching scenes!
        Time.timeScale = 1f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void QuitToDesktop()
    {
        Debug.Log("Quitting Game to Desktop...");
        Application.Quit(); // Note: This only works in the compiled build, not the Unity Editor
    }
}