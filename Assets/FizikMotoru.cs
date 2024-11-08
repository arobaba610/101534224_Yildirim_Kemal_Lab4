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

    // List to store all physics objects in the simulation
    public List<FizikObject> objects = new List<FizikObject>();
    public float dt = 0.02f; // Time step for physics simulation
    public Vector3 AccelerationGravity = new Vector3(0, -10, 0); // Gravity vector

    void FixedUpdate()
    {
        // Update each object if it is not static
        foreach (FizikObject objectA in objects)
        {
            if (!objectA.isStatic) // Only apply physics to non-static objects
            {
                Vector3 prevPos = objectA.transform.position;
                Vector3 newPos = objectA.transform.position + objectA.velocity * dt;

                // Update position based on calculated new position
                objectA.transform.position = newPos;

                // Apply gravity to the velocity
                Vector3 accelerationThisFrame = AccelerationGravity * objectA.gravityScale;
                objectA.velocity += accelerationThisFrame * dt;

                ApplyDrag(objectA); // Apply drag force to the object

                // Draw debug lines to visualize movement
                Debug.DrawLine(prevPos, newPos, Color.green, 10);
                Debug.DrawLine(objectA.transform.position, objectA.transform.position + objectA.velocity, Color.red, 10);
            }
        }

        // Reset colors of objects for collision visualization
        foreach (FizikObject obj in objects)
        {
            obj.GetComponent<Renderer>().material.color = Color.white;
        }

        // Check collisions between each pair of objects
        for (int iA = 0; iA < objects.Count; iA++)
        {
            FizikObject objectA = objects[iA];

            for (int iB = iA + 1; iB < objects.Count; iB++)
            {
                FizikObject objectB = objects[iB];

                if (objectA == objectB) continue; // Skip same object

                bool isOverlapping = false;

                // Determine collision type and handle accordingly
                if (objectA.shape.GetShape() == FizziksShape.Shape.Sphere && objectB.shape.GetShape() == FizziksShape.Shape.Sphere)
                {
                    isOverlapping = CollideSpheres(objectA, objectB);
                }
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Sphere && objectB.shape.GetShape() == FizziksShape.Shape.Plane && objectB.isStatic)
                {
                    isOverlapping = CollideSpherePlane((FizziksShapeSphere)objectA.shape, (FizziksShapePlane)objectB.shape, objectA);
                }
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Plane && objectB.shape.GetShape() == FizziksShape.Shape.Sphere && objectA.isStatic)
                {
                    isOverlapping = CollideSpherePlane((FizziksShapeSphere)objectB.shape, (FizziksShapePlane)objectA.shape, objectB);
                }
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Sphere && objectB.shape.GetShape() == FizziksShape.Shape.Halfspace && objectB.isStatic)
                {
                    isOverlapping = CollideSphereHalfspace((FizziksShapeSphere)objectA.shape, (FizziksShapeHalfspace)objectB.shape, objectA);
                }
                else if (objectA.shape.GetShape() == FizziksShape.Shape.Halfspace && objectB.shape.GetShape() == FizziksShape.Shape.Sphere && objectA.isStatic)
                {
                    isOverlapping = CollideSphereHalfspace((FizziksShapeSphere)objectB.shape, (FizziksShapeHalfspace)objectA.shape, objectB);
                }

                // if a collision is detected, change color to indicate collision
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

    // Sphere-Sphere Collision with translation adjustment for static interactions
    public static bool CollideSpheres(FizikObject objectA, FizikObject objectB)
    {
        Vector3 displacement = objectA.transform.position - objectB.transform.position;
        float distance = displacement.magnitude;
        float radiusA = ((FizziksShapeSphere)objectA.shape).radius;
        float radiusB = ((FizziksShapeSphere)objectB.shape).radius;

        float overlap = radiusA + radiusB - distance;

        if (overlap > 0.0f)
        {
            Vector3 collisionNormal = displacement / distance;

            if (objectA.isStatic || objectB.isStatic) // Handle static object collision
            {
                if (objectA.isStatic)
                {
                    objectB.transform.position -= collisionNormal * overlap;
                    objectB.velocity = Vector3.zero; // Stop movement for dynamic object on collision
                }
                else
                {
                    objectA.transform.position += collisionNormal * overlap;
                    objectA.velocity = Vector3.zero;
                }
            }
            else
            {
                // Handle collision response for two non-static objects
                Vector3 mtv = collisionNormal * overlap * 0.5f;
                objectA.transform.position += mtv;
                objectB.transform.position -= mtv;
            }

            return true;
        }
        return false;
    }

    // Sphere-Plane Collision with static interaction handling
    public bool CollideSpherePlane(FizziksShapeSphere sphere, FizziksShapePlane plane, FizikObject objectA)
    {
        Vector3 planeToSphere = sphere.transform.position - plane.transform.position;
        float positionAlongNormal = Vector3.Dot(planeToSphere, plane.Normal());
        float distanceToPlane = Mathf.Abs(positionAlongNormal);

        if (distanceToPlane < sphere.radius)
        {
            Vector3 mtv = plane.Normal() * (sphere.radius - distanceToPlane);
            objectA.transform.position += mtv;
            objectA.velocity = Vector3.zero;
            return true;
        }
        return false;
    }

    // Sphere-Halfspace Collision with static interaction handling
    public bool CollideSphereHalfspace(FizziksShapeSphere sphere, FizziksShapeHalfspace halfspace, FizikObject objectA)
    {
        Vector3 planeToSphere = sphere.transform.position - halfspace.transform.position;
        float positionAlongNormal = Vector3.Dot(planeToSphere, halfspace.Normal());

        if (positionAlongNormal < sphere.radius)
        {
            Vector3 mtv = halfspace.Normal() * (sphere.radius - positionAlongNormal);
            objectA.transform.position += mtv;
            objectA.velocity = Vector3.zero;
            return true;
        }
        return false;
    }
}
