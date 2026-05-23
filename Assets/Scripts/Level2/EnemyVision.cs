using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 7f; 
    
    [Range(0, 360)]
    public float viewAngle = 90f; 
    public float proximityRadius = 1.5f; 

    [Header("Targeting Layers")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("State")]
    public bool isPlayerSpotted = false;
    
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    public int edgeResolution = 20; 

    void Start()
    {
        // Get the enemy's sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // ---> THE FIX: Create a brand new empty child object just for the cone!
        GameObject visionConeObj = new GameObject("VisionConeVisual");
        
        // Attach it to the enemy ship so it moves and rotates with it
        visionConeObj.transform.SetParent(transform);
        visionConeObj.transform.localPosition = Vector3.zero;
        visionConeObj.transform.localRotation = Quaternion.identity;

        // Put the Mesh components on the CHILD object, not the enemy!
        mesh = new Mesh();
        
        meshFilter = visionConeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        
        meshRenderer = visionConeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.sortingOrder = -1; // Keeps the cone behind your ships
    }

    void Update()
    {
        FindVisibleTargets();
        DrawFilledVisionCone(); 
    }

    void FindVisibleTargets()
    {
        isPlayerSpotted = false; 
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= proximityRadius)
        {
            isPlayerSpotted = true;
        }
        else if (distanceToPlayer <= viewRadius)
        {
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector2.Angle(transform.right, dirToPlayer); 

            if (angleToPlayer < viewAngle / 2f)
            {
                if (!Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleLayer))
                {
                    isPlayerSpotted = true; 
                }
            }
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPlayerSpotted ? Color.red : originalColor;
        }
    }

    void DrawFilledVisionCone()
    {
        int stepCount = Mathf.Max(1, Mathf.RoundToInt(viewAngle * edgeResolution / 360f));
        float stepAngleSize = viewAngle / stepCount;

        Vector3[] vertices = new Vector3[stepCount + 2];
        int[] triangles = new int[stepCount * 3];

        vertices[0] = Vector3.zero; 
        float currentAngle = -viewAngle / 2f; 

        for (int i = 0; i <= stepCount; i++)
        {
            float radian = currentAngle * Mathf.Deg2Rad;
            Vector3 localDir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0);
            Vector3 globalDir = transform.TransformDirection(localDir);

            float rayDistance = viewRadius;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, globalDir, viewRadius, obstacleLayer);
            
            if (hit.collider != null)
            {
                rayDistance = hit.distance; 
            }

            vertices[i + 1] = localDir * rayDistance;

            if (i < stepCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            currentAngle += stepAngleSize;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        Color coneColor = isPlayerSpotted ? new Color(1f, 0f, 0f, 0.25f) : new Color(1f, 1f, 0f, 0.15f);
        meshRenderer.material.color = coneColor;
    }
}