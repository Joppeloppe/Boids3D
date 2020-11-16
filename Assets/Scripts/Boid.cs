using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 direction = Vector3.zero;
    public float speed = 5f;
    
    // Used for behaviour calculations

    // Strength of the behaviour.
    public float alignmentStrength = 5f; 
    public float cohesionStrength = 1f; 
    public float separationStrength = 1.25f;
    public float containmentStrength = 5f;

    private SphereCollider sphere; // For radius and visual, can be changed to just radius if not using Unity.

    private Vector3 desired = Vector3.zero; // Desired direction, based on other boids.
    private int total = 0; // Total boids found

    private float containmentRadius = 0f; // Boundary so that the boids don't fly away forever.

    private Boid[] neighbours; // Array of closest boids to be considered in behaviour calculations.

    private void Start()
    {
        sphere = GetComponent<SphereCollider>();
        containmentRadius = transform.parent.GetComponent<SphereCollider>().radius;

        direction = Vector3.forward * speed;

        neighbours = Spawner.instance.objects.ToArray();

        InvokeRepeating("CheckForNeighbours", .5f, 1f); // Populate neighbours array once every second. Runs in a separate thread.
    }
    
    // Runs once every frame.
    private void Update()
    {
        BehaviourCalculations(neighbours);

        if(total > 0) // If we have found any boids to adjust direction to.
        {
            Alignment();
            Cohesion();
            Separation();
        }

        Move();

        Containment();
    }

    // Move this boid object.
    private void Move()
    {
        direction.Normalize(); // Creates a unit vector, a vector with the same direction but with a length of 1. (Math: vector / |vector| [sqrt(x^2 + y^2)])

        transform.position += direction * speed * Time.deltaTime; // Delta time is time since last frame in seconds.
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up); // Rotates the boid to be facing forwards and up(Y).
    }

    private void BehaviourCalculations(Boid[] others)
    {
        ResetCalculations();

        foreach (Boid other in others)
        {
            if (other == this) continue; // Skip itself

            Vector3 distance = other.transform.position - transform.position;

            if (distance.magnitude < sphere.radius) // Lenght of distance < perception radius
            {
                desired += other.direction; // Alignemnt
                desired += distance; // Cohesion
                //Separation
                Vector3 difference = transform.position - other.transform.position; // This vector points from other to this.
                difference /= distance.magnitude; // Inversly proportional to other (if other is close then move far away, if other is far then don't move as far away)
                desired += difference;
                
                total++;
            }
        }

        desired = desired / total;
    }

    // Align current direction with local boids direction.
    private void Alignment()
    {
        Vector3 steering = Vector3.Lerp(direction, desired, Time.deltaTime).normalized;
        direction += steering * alignmentStrength;
    }

    // Move towards the local average position of nearby boids.
    private void Cohesion()
    {
        Vector3 steering = Vector3.Lerp(Vector3.zero, desired, desired.magnitude / sphere.radius).normalized;
        direction += steering * cohesionStrength;
    }

    // Avoid crowding with local boids.
    private void Separation()
    {
        Vector3 steering = Vector3.Lerp(Vector3.zero, desired, desired.magnitude / sphere.radius).normalized;
        direction -= steering * separationStrength;
    }

    private void ResetCalculations()
    {
        desired = Vector3.zero;
        total = 0;
    }

    // Checks to see if the boid is outside of the containment area (spawner sphere by default),
    // if so a steer towards the center of the sphere.
    private void Containment()
    {
        if (this.transform.position.magnitude > containmentRadius)
        {
            direction += transform.position.normalized * (containmentRadius - transform.position.magnitude) * containmentStrength * Time.deltaTime;
        }
    }

    // We check every boid against every other boid to see if they are close.
    // This is slow when there are a lot of boids.
    // This can be optimised with a quadtree, and then check what cubes the boid is near
    // then loop through those boids instead.
    private void CheckForNeighbours()
    {
        List<Boid> temp = new List<Boid>();

        foreach (Boid item in Spawner.instance.objects)
        {
            if (Vector3.Distance(transform.position, item.transform.position) < sphere.radius)
                temp.Add(item);
        }

        neighbours = temp.ToArray();
    }
}