using UnityEngine;

public class MenuDrifter : MonoBehaviour
{
    [Header("Drift Settings")]
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
    public float lifetime = 15f;
    
    [Header("Scale Settings")]
    public float minScale = 0.5f; // ---> NEW: Exposes scale to Inspector
    public float maxScale = 0.8f; // ---> NEW: Exposes scale to Inspector

    private float actualSpeed;

    void Start()
    {
        Destroy(gameObject, lifetime);
        actualSpeed = Random.Range(minSpeed, maxSpeed);

        // Uses your custom Inspector settings for the fake 3D depth!
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);
    }

    void Update()
    {
        transform.Translate(Vector3.right * actualSpeed * Time.deltaTime, Space.Self);
    }
}