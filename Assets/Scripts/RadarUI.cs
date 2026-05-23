using UnityEngine;
using System.Collections.Generic; // <--- NEED THIS to use Lists!

public class RadarUI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RectTransform radarCenter; 

    [Header("UI Dots")]
    public RectTransform portalDot; 
    
    // ---> CHANGED: Instead of a fixed array, we just need ONE prefab to copy!
    public GameObject enemyDotPrefab; 

    [Header("Radar Settings")]
    public float radarDisplayRadius = 75f; 
    public float worldScanRadius = 30f; 

    private Transform portal;
    
    // ---> NEW: A dynamic list that can grow and shrink as needed
    private List<RectTransform> activeEnemyDots = new List<RectTransform>(); 

    void Start()
    {
        GameObject p = GameObject.Find("LevelGoal"); 
        if (p != null) portal = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 1. Track Portal
        if (portal != null && portalDot != null)
        {
            UpdateDotPosition(portal.position, portalDot);
        }

        // 2. Track Enemies Dynamically!
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        // ---> THE MAGIC: If there are more enemies than dots, create new dots instantly!
        while (activeEnemyDots.Count < enemies.Length)
        {
            GameObject newDot = Instantiate(enemyDotPrefab, radarCenter);
            activeEnemyDots.Add(newDot.GetComponent<RectTransform>());
        }

        // Update the positions of all the dots
        for (int i = 0; i < activeEnemyDots.Count; i++)
        {
            if (i < enemies.Length)
            {
                activeEnemyDots[i].gameObject.SetActive(true);
                UpdateDotPosition(enemies[i].transform.position, activeEnemyDots[i]);
            }
            else
            {
                // Hide any extra dots if enemies were destroyed
                activeEnemyDots[i].gameObject.SetActive(false); 
            }
        }
    }

    void UpdateDotPosition(Vector3 targetWorldPos, RectTransform dot)
    {
        Vector3 offset = targetWorldPos - player.position;
        Vector2 uiPos = new Vector2(offset.x, offset.y) * (radarDisplayRadius / worldScanRadius);

        if (uiPos.magnitude > radarDisplayRadius)
        {
            uiPos = uiPos.normalized * radarDisplayRadius;
        }

        dot.anchoredPosition = uiPos;
    }
}