using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JoshBoid : MonoBehaviour
{
    // Basic properties of a Boid, used for Euler Integration
    public Vector2 velocity;
    public Vector2 position;
    public Vector2 force;

    // Accumulate force
    // Every update the JoshBoidManager uses the Boid's force then wipes it to zero
    public void AddForce(Vector2 f)
    {
        force += f;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Store the Boid's transform position into it's position
        position = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Find all nearby Boids
        var nearby = JoshBoidManager.instance.FindBoidsInRange(this, position, JoshBoidManager.instance.boidSightRange);
        // If there are nearby Boids then make an if statement regarding whether the boids should align, group or separate
        if (nearby.Count > 0)
        {
            Vector2 alignment = Vector2.zero;
            Vector2 cohesion = Vector2.zero;
            Vector2 separation = Vector2.zero;
            // Do flocking processing here
            if (JoshBoidManager.instance.boidEnableAlignment)
            {
                alignment = Align(this, JoshBoidManager.instance.boidSightRange, JoshBoidManager.instance.boidStrengthAlignment);
            }
            if (JoshBoidManager.instance.boidEnableCohesion)
            {
                cohesion = Grouping(this, JoshBoidManager.instance.boidSightRange, JoshBoidManager.instance.boidStrengthCohesion);
            }

            if (JoshBoidManager.instance.boidEnableSeparation)
            {
                separation = Separate(this, JoshBoidManager.instance.boidSightRange, JoshBoidManager.instance.boidStrengthSeparation);
            }
            // Combine the forces
            var combinedForce = cohesion + alignment + separation;

            AddForce(combinedForce);
        }

        // Apply the accumulated force to velocity and position
        velocity += force * Time.fixedDeltaTime;
        position += velocity * Time.fixedDeltaTime;
        transform.position = position;

        // Reset the force for the next update
        force = Vector2.zero;
    }

    private Vector2 Grouping(JoshBoid boid, float distance, float power)
    {
        var nearby = JoshBoidManager.instance.FindBoidsInRange(boid, boid.position, distance);
        if (nearby.Count == 0)
        {
            return Vector2.zero;
        }

        float meanX = nearby.Sum(x => x.position.x) / nearby.Count;
        float meanY = nearby.Sum(x => x.position.y) / nearby.Count;
        float deltaCenterX = meanX - boid.position.x;
        float deltaCenterY = meanY - boid.position.y;
        return new Vector2(deltaCenterX * power, deltaCenterY * power);
    }

    private Vector2 Align(JoshBoid boid, float distance, float power)
    {
        var nearby = JoshBoidManager.instance.FindBoidsInRange(boid, boid.position, distance);
        if (nearby.Count == 0)
        {
            return Vector2.zero;
        }

        float meanXvelocity = nearby.Sum(x => x.velocity.x) / nearby.Count;
        float meanYvelocity = nearby.Sum(x => x.velocity.y) / nearby.Count;
        float dXvelocity = meanXvelocity - boid.velocity.x;
        float dYvelocity = meanYvelocity - boid.velocity.y;
        return new Vector2(dXvelocity * power, dYvelocity * power);
    }

    private Vector2 Separate(JoshBoid boid, float distance, float power)
    {
        var nearby = JoshBoidManager.instance.FindBoidsInRange(boid, boid.position, distance);
        if (nearby.Count == 0)
        {
            return Vector2.zero;
        }

        Vector2 sumCloseness = Vector2.zero;
        foreach (var neighbor in nearby)
        {
            float closeness = distance - Vector2.Distance(boid.position, neighbor.position);
            sumCloseness += (boid.position - neighbor.position) * closeness;
        }
        return sumCloseness * power;
    }
}
