using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance; // Singleton for easy access
    
    public GameObject bulletPrefab;
    public int poolSize = 30; // How many bullets to keep ready
    
    private List<GameObject> pool;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetBullet()
    {
        // Find an inactive bullet
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        // If all are in use, create a new one to prevent errors
        GameObject newObj = Instantiate(bulletPrefab);
        pool.Add(newObj);
        return newObj;
    }
}