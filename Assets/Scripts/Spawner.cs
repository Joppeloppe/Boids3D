using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject boidPrefab;
    public int numberToSpawn = 50;

    private SphereCollider sphere;

    public List<Boid> objects = new List<Boid>();

    public static Spawner instance; // Singleton so that we easily can access the list of all boids without a reference in each boid. See Boid.CheckForNeighbours to see optimisation option.

    // Creates a number of Boid's randomly inside the sphere/radius, this sphere/radius is also the containment sphere by default.
    // The random rotation gives the boid it's initial direction as well, because of 'direction = Vector3.forward * speed;' in boid.Start
    private void Start()
    {
        instance = this;

        sphere = GetComponent<SphereCollider>();

        for (int i = 0; i < numberToSpawn; i++)
        {
            GameObject newObject = Instantiate(boidPrefab, this.transform.position + Random.insideUnitSphere * sphere.radius, Random.rotation, this.transform);
            objects.Add(newObject.GetComponent<Boid>());
        }
    }
}
