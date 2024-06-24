using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoshBoid : MonoBehaviour
{
    public Vector2 velocity;
    public Vector2 position;
    public Vector2 force;
    
    public void AddForce(Vector2 f)
    {
        force += force;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Store the Boid's transform position into it's pos
        position = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Find all nearby Boids
        var nearby = BoidManager.instance.FindBoidsInRange(this, position, BoidManager.instance.boidSightRange);
        // If there are nearby Boids
        if (nearby.Count > 0)
        {
            // Do flocking processing here
        }

    }
}
