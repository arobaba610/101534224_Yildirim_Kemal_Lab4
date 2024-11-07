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
    public Vector3 AccelerationGravity = new Vector3(0, -10, 0); // Gravity direction and strength

    void FixedUpdate()
    {
        foreach (FizikObject objectA in objects)
        {
            Vector3 prevPos = objectA.transform.position;
            Vector3 newPos = objectA.transform.position + objectA.velocity * dt;

            // Update position
            objectA.transform.position = newPos;

            Vector3 accelerationThisFrame = AccelerationGravity * objectA.gravityScale;

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

            for (int iB = iA + 1; iB < objects.Count; iB++)
            {
                FizikObject objectB = objects[iB];

                if (objectA == objectB) continue;

                bool isOverlapping = false;

                // Sphere-Sphere Collision
                if (objectA.shape.GetShape() == FizziksShape.Shape.Sphere && objectB.shape.GetShape() == FizziksShape.Shape.Sphere)
                {
                    isOverlapping = IsOverlappingSpheres(objectA, objectB);
                }
                // Sphere-Plane Collision
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Sphere && objectB.shape.GetShape() == FizziksShape.Shape.Plane)
                {
                    isOverlapping = IsOverlappingSpherePlane((FizziksShapeSphere)objectA.shape, (FizziksShapePlane)objectB.shape);
                }
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Plane && objectB.shape.GetShape() == FizziksShape.Shape.Sphere)
                {
                    isOverlapping = IsOverlappingSpherePlane((FizziksShapeSphere)objectB.shape, (FizziksShapePlane)objectA.shape);
                }
                // Sphere-Halfspace Collision
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Sphere && objectB.shape.GetShape() == FizziksShape.Shape.Halfspace)
                {
                    isOverlapping = IsOverlappingSphereHalfspace((FizziksShapeSphere)objectA.shape, (FizziksShapeHalfspace)objectB.shape);
                }
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Halfspace && objectB.shape.GetShape() == FizziksShape.Shape.Sphere)
                {
                    isOverlapping = IsOverlappingSphereHalfspace((FizziksShapeSphere)objectB.shape, (FizziksShapeHalfspace)objectA.shape);
                }

                // If a collision is detected, change colors to visualize the collision
                if (isOverlapping)
                {
                    objectA.GetComponent<Renderer>().material.color = Color.red;
                    objectB.GetComponent<Renderer>().material.color = Color.red;
                    Debug.DrawLine(objectA.transform.position, objectB.transform.position, Color.red, 10);
                }
            }
        }
    }

    // Apply drag to slow down the object
    private void ApplyDrag(FizikObject objectA)
    {
        float dragCoefficient = objectA.drag;
        Vector3 dragForce = -dragCoefficient * objectA.velocity.sqrMagnitude * objectA.velocity.normalized;
        objectA.velocity += dragForce * dt;
    }

    // Check if two spheres are colliding
    public bool IsOverlappingSpheres(FizikObject objectA, FizikObject objectB)
    {
        Vector3 displacement = objectA.transform.position - objectB.transform.position;
        float distance = displacement.magnitude;
        float radiusA = ((FizziksShapeSphere)objectA.shape).radius;
        float radiusB = ((FizziksShapeSphere)objectB.shape).radius;

        return distance < radiusA + radiusB;
    }

    // Check if a sphere is colliding with a plane
    public bool IsOverlappingSpherePlane(FizziksShapeSphere sphere, FizziksShapePlane plane)
    {
        Vector3 planeToSphere = sphere.transform.position - plane.transform.position;
        float positionAlongNormal = Vector3.Dot(planeToSphere, plane.Normal());
        float distanceToPlane = Mathf.Abs(positionAlongNormal);
        return distanceToPlane < sphere.radius;
    }

    // Check if a sphere is colliding with a halfspace
    public bool IsOverlappingSphereHalfspace(FizziksShapeSphere sphere, FizziksShapeHalfspace halfspace)
    {
        Vector3 planeToSphere = sphere.transform.position - halfspace.transform.position;
        float positionAlongNormal = Vector3.Dot(planeToSphere, halfspace.Normal());

        // If the sphere is in the halfspace region (behind the plane), it's overlapping
        return positionAlongNormal < sphere.radius;
    }
}
