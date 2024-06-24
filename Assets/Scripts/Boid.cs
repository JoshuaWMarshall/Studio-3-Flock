using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour
{
    // Basic properties of a Boid, used for Euler Integration
    public Vector2 vel;
    public Vector2 pos;
    public Vector2 force;

    // Accumulate force
    // Every update the BoidManager uses the Boid's force then wipes it to zero
    public void AddForce(Vector2 f)
    {
        force += f;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Store the Boid's transform position into it's pos
        pos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Find all nearby Boids
        var nearby = BoidManager.instance.FindBoidsInRange(this, pos, BoidManager.instance.boidSightRange);
        // If there are nearby Boids
        if (nearby.Count > 0)
        {
            Vector2 alignment = Vector2.zero;
            Vector2 cohesion = Vector2.zero;
            Vector2 separation = Vector2.zero;
            // Do flocking processing here
            if (BoidManager.instance.boidEnableAlignment)
            {
                alignment = Align(this, BoidManager.instance.boidSightRange, BoidManager.instance.boidStrengthAlignment);
            }
            if (BoidManager.instance.boidEnableCohesion)
            {
                cohesion = Flock(this, BoidManager.instance.boidSightRange, BoidManager.instance.boidStrengthCohesion);
            }

            if (BoidManager.instance.boidEnableSeparation)
            {
                separation = Avoid(this, BoidManager.instance.boidSightRange, BoidManager.instance.boidStrengthSeparation);
            }
            // Combine the forces
            var combinedForce = cohesion + alignment + separation;

            AddForce(combinedForce);
        }

        // Apply the accumulated force to velocity and position
        vel += force * Time.fixedDeltaTime;
        // Normalize the velocity if it exceeds the maximum speed
        float maxSpeed = BoidManager.instance.boidSpeed;
        if (vel.magnitude > maxSpeed)
        {
            vel = vel.normalized * maxSpeed;
        }
        pos += vel * Time.fixedDeltaTime;
        transform.position = pos;

        // Reset the force for the next update
        force = Vector2.zero;
    }

    private Vector2 Flock(Boid boid, float distance, float power)
    {
        var nearby = BoidManager.instance.FindBoidsInRange(boid, boid.pos, distance);
        if (nearby.Count == 0) return Vector2.zero;

        float meanX = nearby.Sum(x => x.pos.x) / nearby.Count;
        float meanY = nearby.Sum(x => x.pos.y) / nearby.Count;
        float deltaCenterX = meanX - boid.pos.x;
        float deltaCenterY = meanY - boid.pos.y;
        return new Vector2(deltaCenterX * power, deltaCenterY * power);
    }

    private Vector2 Align(Boid boid, float distance, float power)
    {
        var nearby = BoidManager.instance.FindBoidsInRange(boid, boid.pos, distance);
        if (nearby.Count == 0) return Vector2.zero;

        float meanXvel = nearby.Sum(x => x.vel.x) / nearby.Count;
        float meanYvel = nearby.Sum(x => x.vel.y) / nearby.Count;
        float dXvel = meanXvel - boid.vel.x;
        float dYvel = meanYvel - boid.vel.y;
        return new Vector2(dXvel * power, dYvel * power);
    }

    private Vector2 Avoid(Boid boid, float distance, float power)
    {
        var nearby = BoidManager.instance.FindBoidsInRange(boid, boid.pos, distance);
        if (nearby.Count == 0) return Vector2.zero;

        Vector2 sumCloseness = Vector2.zero;
        foreach (var nearbyBois in nearby)
        {
            float closeness = distance - Vector2.Distance(boid.pos, nearbyBois.pos);
            sumCloseness += (boid.pos - nearbyBois.pos) * closeness;
        }
        return sumCloseness * power;
    }
}
