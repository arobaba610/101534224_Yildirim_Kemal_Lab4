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

            // Update velocity according to gravity
            objectA.velocity += AccelerationGravity * dt;

            // Apply drag (velocity damping effect)
            ApplyDrag(objectA);

            // Debug lines for visualization
            Debug.DrawLine(prevPos, newPos, Color.green, 10);
            Debug.DrawLine(objectA.transform.position, objectA.transform.position + objectA.velocity, Color.red, 10);
        }
    }

    // Apply drag (velocity damping) to slow down the object
    private void ApplyDrag(FizikObject objectA)
    {
        float dragCoefficient = objectA.drag;
        Vector3 dragForce = -dragCoefficient * objectA.velocity.sqrMagnitude * objectA.velocity.normalized;
        objectA.velocity += dragForce * dt;
    }
}
