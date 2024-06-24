using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class JoshBoidManager : MonoBehaviour
{
    public enum EdgeBehavior
    {
        Wrap,
        Collide
    }

    [Header("Manager")]
    public static JoshBoidManager instance;

    [Header("World")]
    public Vector2      worldSize = new Vector2(100, 100);
    public EdgeBehavior edge = EdgeBehavior.Wrap;
    public uint         boidCount = 10;

    [Header("Boids")]
    public GameObject boidPrefab;
    public List<JoshBoid> boids;
    public float      boidSpeed = 20.0f;
    public float      boidSightRange = 10.0f;

    [Header("Rules")]
    public bool  boidEnableSeparation = false;
    public bool  boidEnableCohesion = false;
    public bool  boidEnableAlignment = false;
    public float boidStrengthSeparation = 1.0f;
    public float boidStrengthCohesion = 1.0f;
    public float boidStrengthAlignment = 1.0f;

    [Header("Debug")]
    public bool debugRanges = false;
    public bool debugNearby = false;
    public bool debugSelected = false;

    private void Awake()
    {
        // Store the singleton reference
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Draw the world boundaries
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(worldSize.x, 0, 0), Color.red);
        Debug.DrawLine(new Vector3(0, worldSize.y, 0), new Vector3(worldSize.x, worldSize.y, 0), Color.red);
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(0, worldSize.y, 0), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0, 0), new Vector3(worldSize.x, worldSize.y, 0), Color.red);

        long diff = boidCount - boids.Count; // The signed difference between how many boids we want and have.
        
        // Not enough boids, spawn more
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
            {
                Vector2 p = new Vector2(Random.Range(1.0f, worldSize.x - 1.0f), Random.Range(1.0f, worldSize.x - 1.0f));
                GameObject g = Instantiate(boidPrefab, p, Quaternion.identity); ;
                JoshBoid b = g.GetComponent<JoshBoid>();
                b.position = p;
                float angle = Random.Range(0, 2.0f * Mathf.PI);
                b.velocity.x = Mathf.Cos(angle)*boidSpeed;
                b.velocity.y = Mathf.Sin(angle)*boidSpeed;
                boids.Add(b);
            }
        }
        // Too many boids, remove some
        else if(diff < 0)
        {
            for (int i = 0; i < -diff; ++i)
            {
                Destroy(boids[boids.Count - 1].gameObject);
                boids.RemoveAt(boids.Count - 1);
            }
        }

        // Process each boid
        foreach (var b in boids)
        {
            // EUler integration
            b.velocity += b.force * Time.fixedDeltaTime;
            b.position += b.velocity * Time.fixedDeltaTime;
            b.force = Vector2.zero;

            // Apply edge wrapping
            if (edge == EdgeBehavior.Wrap)
            {
                if (b.position.x < 0)
                    b.position.x += worldSize.x;
                if (b.position.y < 0)
                    b.position.y += worldSize.y;
                if (b.position.x >= worldSize.x)
                    b.position.x -= worldSize.x;
                if (b.position.y >= worldSize.y)
                    b.position.y -= worldSize.y;
            }
            // Apply edge collision
            else if(edge == EdgeBehavior.Collide)
            {
                if (b.position.x < 0)
                    b.position.x = 0;
                if (b.position.y < 0)
                    b.position.y = 0;
                if (b.position.x >= worldSize.x)
                    b.position.x = worldSize.x;
                if (b.position.y >= worldSize.y)
                    b.position.y = worldSize.y;
            }
            // Apply the Boid's position to the transform
            b.transform.position = b.position;

            // Rotate the boid's sprite to face its velocityocity
            float angle = 0;
            if (b.velocity != Vector2.zero)
            {
                angle = Mathf.Atan2(-b.velocity.x, b.velocity.y) * Mathf.Rad2Deg;
                b.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    // Draw a debug circle
    static public void DrawCircle(Vector2 p, float radius, Color c)
    {
        Vector3 p3 = new Vector3(p.x, p.y, 0);
        Vector3 old = new Vector3(radius, 0, 0) + p3;
        for (int i = 0; i < 32; ++i)
        {
            float a = (i / 31.0f) * 2.0f * Mathf.PI;
            Vector3 current = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0) + p3;
            Debug.DrawLine(old, current, c);
            old = current;
        }
    }

    // Draw a debug line
    static public void DrawLine(Vector2 p1, Vector2 p2, Color c)
    {
        Debug.DrawLine(new Vector3(p1.x, p1.y, 0), new Vector3(p2.x, p2.y, 0), c);
    }

    // Draw a debug arrow
    static public void DrawArrow(Vector2 p1, Vector2 p2, Color c, float headSize = 0.3f)
    {
        Vector3 a = new Vector3(p1.x, p1.y, 0);
        Vector3 b = new Vector3(p2.x, p2.y, 0);
        Vector3 dir = (b - a).normalized;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0);
        Debug.DrawLine(a, b, c);
        Debug.DrawLine(b, b - dir * headSize + perp * headSize, c);
        Debug.DrawLine(b, b - dir * headSize - perp * headSize, c);
    }

    // Find all boids in range, skipping the self boid
    public List<JoshBoid> FindBoidsInRange(JoshBoid self, Vector2 p, float range)
    {
        if(debugRanges)
            DrawCircle(p, range, Color.white);
        List<JoshBoid> found = new List<JoshBoid>();
        foreach(var b in boids)
        {
            if(b!=self && Vector2.Distance(p,b.position)<=range)
            {
                found.Add(b);
                if(debugNearby)
                    DrawArrow(p, b.position, Color.green);
            }
        }
        return found;
    }
}
