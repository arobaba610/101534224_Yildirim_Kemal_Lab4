using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FizikMotoru : MonoBehaviour
{
    static FizikMotoru instance = null;
    public static FizikMotoru Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FizikMotoru>();
            }
            return instance;
        }
    }

    public List<FizikObject> objects = new List<FizikObject>(); // List of all physics objects
    public float dt = 0.02f; // Delta time for the physics simulation
    public Vector3 AccelerationGravity = new Vector3(0, -10, 0);

    void FixedUpdate()
    {
        foreach (FizikObject objectA in objects)
        {
            Vector3 prevPos = objectA.transform.position;
            Vector3 newPos = objectA.transform.position + objectA.velocity * dt;

            // Update position
            objectA.transform.position = newPos;

            Vector3 accelerationThisFrame = AccelerationGravity;

            // Update velocity according to gravity
            objectA.velocity += accelerationThisFrame * dt;

            
            ApplyDrag(objectA);

            
            Debug.DrawLine(prevPos, newPos, Color.green, 10);
            Debug.DrawLine(objectA.transform.position, objectA.transform.position + objectA.velocity, Color.red, 10);
        }

        // Reset the color of all objects before collision checks
        foreach (FizikObject obj in objects)
        {
            obj.GetComponent<Renderer>().material.color = Color.white;
        }

        // Check for collisions between all objects
        for (int iA = 0; iA < objects.Count; iA++)
        {
            FizikObject objectA = objects[iA];

            for (int iB = iA +1; iB < objects.Count; iB++)
            {
                FizikObject objectB = objects[iB];

                if (objectA == objectB) continue;

                // Check for collision between spheres
                if (IsOverlappingSpheres(objectA, objectB))
                {
                    // Collision detected, change the color of the spheres to red
                    Debug.DrawLine(objectA.transform.position, objectB.transform.position, Color.red, 10);
                    objectA.GetComponent<Renderer>().material.color = Color.red;
                    objectB.GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }
    }

    // to slow down the object
    private void ApplyDrag(FizikObject objectA)
    {
        float dragCoefficient = objectA.drag;
        Vector3 dragForce = -dragCoefficient * objectA.velocity.sqrMagnitude * objectA.velocity.normalized;
        objectA.velocity += dragForce * dt;
    }

    //  to check if two spheres are colliding
    public bool IsOverlappingSpheres(FizikObject objectA, FizikObject objectB)
    {
        // the distance between the centers of the two objects
        Vector3 displacement = objectA.transform.position - objectB.transform.position;
        float distance = displacement.magnitude;
        // If the distance is less than the sum of their radius, they are overlapping
        Debug.Log("Checking collision between: " + objectA.name + " and " + objectB.name);
        return distance < (objectA.radius + objectB.radius);
    }
}
