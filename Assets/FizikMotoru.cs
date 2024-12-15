using System;
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



    public List<FizikObject> objects = new List<FizikObject>();
    public float dt = 0.02f;
    public Vector3 AccelerationGravity = new Vector3(0, -10, 0);

    public class SurfaceMaterial
    {
        public string materialName; 
        public float restitution;  

        public SurfaceMaterial(string name, float restitutionValue)
        {
            materialName = name;
            restitution = restitutionValue;
        }
    }


    public static class MaterialProperties
    {
        // A lookup table for material combinations
        private static Dictionary<(string, string), float> restitutionLookup = new Dictionary<(string, string), float>
    {
        { ("Steel", "Steel"), 0.6f },
        { ("Steel", "Rubber"), 0.8f },
        { ("Rubber", "Rubber"), 0.9f },
        { ("Wood", "Wood"), 0.4f },
        { ("Wood", "Steel"), 0.5f }
    };

        public static float GetCombinedRestitution(SurfaceMaterial matA, SurfaceMaterial matB)
        {
            // Check both key orders
            if (restitutionLookup.TryGetValue((matA.materialName, matB.materialName), out float restitution))
            {
                return restitution;
            }
            if (restitutionLookup.TryGetValue((matB.materialName, matA.materialName), out restitution))
            {
                return restitution;
            }

            // Default to the average if not in the table
            return (matA.restitution + matB.restitution) / 2f;
        }
    }


    private static float GetCombinedRestitution(FizikObject.SurfaceMaterial matA, FizikObject.SurfaceMaterial matB)
    {
        // Restitution lookup table
        Dictionary<(string, string), float> restitutionLookup = new Dictionary<(string, string), float>
    {
        { ("Steel", "Steel"), 0.6f },
        { ("Steel", "Rubber"), 0.8f },
        { ("Rubber", "Rubber"), 0.9f },
        { ("Wood", "Wood"), 0.4f },
        { ("Wood", "Steel"), 0.5f }
    };

        // Check for predefined restitution values
        if (restitutionLookup.TryGetValue((matA.materialName, matB.materialName), out float restitution))
            return restitution;

        if (restitutionLookup.TryGetValue((matB.materialName, matA.materialName), out restitution))
            return restitution;

        // Default to average if no predefined value is found
        return (matA.restitution + matB.restitution) / 2f;
    }




    void FixedUpdate()
    {
        dt = Time.fixedDeltaTime; // Ensure accurate timestep calculation
                                  // CleanUpObjects();
        UpdatePhysics();
        ResetVisuals();
        DetectCollisions();
    }

    //private void CleanUpObjects()
    //{
    //    objects.RemoveAll(obj => obj == null);
    //}

    private void UpdatePhysics()
    {
        foreach (FizikObject objectA in objects)
        {
            if (!objectA.isStatic)
            {
                Vector3 prevPos = objectA.transform.position;
                objectA.velocity += AccelerationGravity * objectA.gravityScale * dt;
                ApplyDrag(objectA);

                Vector3 newPos = objectA.transform.position + objectA.velocity * dt;
                objectA.transform.position = newPos;

                Debug.DrawLine(prevPos, newPos, Color.green, 0.1f);
                Debug.DrawLine(objectA.transform.position, objectA.transform.position + objectA.velocity, Color.red, 0.1f);
            }
        }
    }

    private void ResetVisuals()
    {
        foreach (FizikObject obj in objects)
        {
            if (obj != null && obj.GetComponent<Renderer>() != null)
            {
                obj.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    private void DetectCollisions()
    {
        for (int iA = 0; iA < objects.Count; iA++)
        {
            FizikObject objectA = objects[iA];
            if (objectA == null) continue;

            for (int iB = iA + 1; iB < objects.Count; iB++)
            {
                FizikObject objectB = objects[iB];
                if (objectB == null) continue;

                if (HandleCollision(objectA, objectB))
                {
                    objectA.GetComponent<Renderer>().material.color = Color.red;
                    objectB.GetComponent<Renderer>().material.color = Color.red;
                    Debug.DrawLine(objectA.transform.position, objectB.transform.position, Color.red, 0.1f);
                }
            }
        }
    }

    private bool HandleCollision(FizikObject objectA, FizikObject objectB)
    {
        bool isOverlapping = false;

        switch (objectA.shape.GetShape())
        {
            case FizziksShape.Shape.Sphere when objectB.shape.GetShape() == FizziksShape.Shape.Sphere:
                isOverlapping = CollideSpheres(objectA, objectB);
                break;

            case FizziksShape.Shape.Sphere when objectB.shape.GetShape() == FizziksShape.Shape.Plane:
                isOverlapping = CollideSpherePlane((FizziksShapeSphere)objectA.shape, (FizziksShapePlane)objectB.shape, objectA);
                break;

            case FizziksShape.Shape.Plane when objectB.shape.GetShape() == FizziksShape.Shape.Sphere:
                isOverlapping = CollideSpherePlane((FizziksShapeSphere)objectB.shape, (FizziksShapePlane)objectA.shape, objectB);
                break;

            case FizziksShape.Shape.Sphere when objectB.shape.GetShape() == FizziksShape.Shape.Halfspace:
                isOverlapping = CollideSphereHalfspace((FizziksShapeSphere)objectA.shape, (FizziksShapeHalfspace)objectB.shape, objectA);
                break;

            case FizziksShape.Shape.Halfspace when objectB.shape.GetShape() == FizziksShape.Shape.Sphere:
                isOverlapping = CollideSphereHalfspace((FizziksShapeSphere)objectB.shape, (FizziksShapeHalfspace)objectA.shape, objectB);
                break;

            case FizziksShape.Shape.AABB when objectB.shape.GetShape() == FizziksShape.Shape.AABB:
                isOverlapping = CollideAABBs((FizziksShapeAABB)objectA.shape, (FizziksShapeAABB)objectB.shape, objectA, objectB);
                break;

            case FizziksShape.Shape.AABB when objectB.shape.GetShape() == FizziksShape.Shape.Sphere:
                isOverlapping = CollideSphereAABB((FizziksShapeSphere)objectB.shape, (FizziksShapeAABB)objectA.shape, objectB);
                break;

            case FizziksShape.Shape.Sphere when objectB.shape.GetShape() == FizziksShape.Shape.AABB:
                isOverlapping = CollideSphereAABB((FizziksShapeSphere)objectA.shape, (FizziksShapeAABB)objectB.shape, objectA);
                break;

            case FizziksShape.Shape.AABB when objectB.shape.GetShape() == FizziksShape.Shape.Plane:
                isOverlapping = CollideAABBPlane((FizziksShapeAABB)objectA.shape, (FizziksShapePlane)objectB.shape, objectA);
                break;

            case FizziksShape.Shape.Plane when objectB.shape.GetShape() == FizziksShape.Shape.AABB:
                isOverlapping = CollideAABBPlane((FizziksShapeAABB)objectB.shape, (FizziksShapePlane)objectA.shape, objectB);
                break;
        }

        return isOverlapping;
    }

    private void ApplyDrag(FizikObject objectA)
    {
        float dragCoefficient = objectA.drag;
        Vector3 dragForce = -dragCoefficient * objectA.velocity.sqrMagnitude * objectA.velocity.normalized;
        objectA.velocity += dragForce * dt;
    }

    public static bool CollideAABBs(FizziksShapeAABB aabbA, FizziksShapeAABB aabbB, FizikObject objectA, FizikObject objectB)
    {
        // Precompute bounds once
        aabbA.RecalculateBounds();
        aabbB.RecalculateBounds();

        // Check overlap on each axis using SAT (Separating Axis Theorem)
        float overlapX = Mathf.Min(aabbA.max.x - aabbB.min.x, aabbB.max.x - aabbA.min.x);
        if (overlapX <= 0) return false;

        float overlapY = Mathf.Min(aabbA.max.y - aabbB.min.y, aabbB.max.y - aabbA.min.y);
        if (overlapY <= 0) return false;

        float overlapZ = Mathf.Min(aabbA.max.z - aabbB.min.z, aabbB.max.z - aabbA.min.z);
        if (overlapZ <= 0) return false;

        // Calculate Minimum Translation Vector (MTV)
        Vector3 mtv = CalculateMTV(overlapX, overlapY, overlapZ, aabbA, aabbB);

        // Resolve penetration
        ResolvePenetration(objectA, objectB, mtv);

        // Resolve velocity
        ResolveVelocity(objectA, objectB, mtv);

        return true;
    }

    private static Vector3 CalculateMTV(float overlapX, float overlapY, float overlapZ, FizziksShapeAABB aabbA, FizziksShapeAABB aabbB)
    {
        // Find the smallest overlap axis
        float[] overlaps = { overlapX, overlapY, overlapZ };
        int minAxis = Array.IndexOf(overlaps, Mathf.Min(overlaps));

        // Assign MTV based on the smallest overlap
        return minAxis switch
        {
            0 => new Vector3(overlapX * Mathf.Sign(aabbA.min.x - aabbB.min.x), 0, 0),
            1 => new Vector3(0, overlapY * Mathf.Sign(aabbA.min.y - aabbB.min.y), 0),
            _ => new Vector3(0, 0, overlapZ * Mathf.Sign(aabbA.min.z - aabbB.min.z))
        };
    }

    private static void ResolvePenetration(FizikObject objectA, FizikObject objectB, Vector3 mtv)
    {
        // Handle static and dynamic objects
        if (objectA.isStatic)
        {
            objectB.transform.position += mtv;
        }
        else if (objectB.isStatic)
        {
            objectA.transform.position -= mtv;
        }
        else
        {
            objectA.transform.position -= mtv / 2;
            objectB.transform.position += mtv / 2;
        }
    }

    private static void ResolveVelocity(FizikObject objectA, FizikObject objectB, Vector3 mtv)
    {
        Vector3 collisionNormal = mtv.normalized;
        Vector3 relativeVelocity = objectA.velocity - objectB.velocity;

        // Calculate restitution based on materials
        float restitution = GetCombinedRestitution(objectA.material, objectB.material);

        // Compute impulse magnitude
        float impulseMagnitude = (1 + restitution) * Vector3.Dot(relativeVelocity, collisionNormal) /
                                 (1 / objectA.mass + 1 / objectB.mass);

        // Apply impulse
        Vector3 impulse = collisionNormal * impulseMagnitude;
        if (!objectA.isStatic) objectA.velocity -= impulse / objectA.mass;
        if (!objectB.isStatic) objectB.velocity += impulse / objectB.mass;

        // Apply friction
        ApplyFriction(objectA, objectB, relativeVelocity, collisionNormal);
    }


    private static void ApplyFriction(FizikObject objectA, FizikObject objectB, Vector3 relativeVelocity, Vector3 collisionNormal)
    {
        // Compute tangent velocity
        Vector3 tangentVelocity = relativeVelocity - Vector3.Project(relativeVelocity, collisionNormal);

        // Friction force (proportional to relative velocity)
        float frictionCoefficient = 0.5f; // Example value; could vary per object
        Vector3 frictionForce = -frictionCoefficient * tangentVelocity;

        if (!objectA.isStatic) objectA.velocity += frictionForce / objectA.mass;
        if (!objectB.isStatic) objectB.velocity -= frictionForce / objectB.mass;
    }

    public static bool CollideSpheres(FizikObject objectA, FizikObject objectB)
    {
        Vector3 displacement = objectA.transform.position - objectB.transform.position;
        float distance = displacement.magnitude;
        float radiusA = ((FizziksShapeSphere)objectA.shape).radius;
        float radiusB = ((FizziksShapeSphere)objectB.shape).radius;

        float overlap = radiusA + radiusB - distance;

        if (overlap > 0.0f)
        {
            Vector3 collisionNormal = displacement.normalized;

            // Calculate restitution using materials
            float restitution = GetCombinedRestitution(objectA.material, objectB.material);

            Vector3 relativeVelocity = objectA.velocity - objectB.velocity;
            float impulseMagnitude = (1 + restitution) * Vector3.Dot(relativeVelocity, collisionNormal) /
                                     (1 / objectA.mass + 1 / objectB.mass);

            Vector3 impulse = collisionNormal * impulseMagnitude;

            if (!objectA.isStatic)
                objectA.velocity -= impulse / objectA.mass;

            if (!objectB.isStatic)
                objectB.velocity += impulse / objectB.mass;

            // Resolve penetration
            Vector3 mtv = collisionNormal * overlap * 0.5f;
            if (!objectA.isStatic) objectA.transform.position += mtv;
            if (!objectB.isStatic) objectB.transform.position -= mtv;

            return true;
        }

        return false;
    }



    //public static bool SweptAABB(FizziksShapeAABB aabbA, FizziksShapeAABB aabbB, Vector3 velocityA, Vector3 velocityB, out float collisionTime)
    //{
    //    collisionTime = float.MaxValue;

    //    // Relative velocity
    //    Vector3 relativeVelocity = velocityA - velocityB;

    //    // Define entry and exit times along each axis
    //    float tMin = 0.0f, tMax = 1.0f; // Time range within the current frame

    //    for (int i = 0; i < 3; i++) // X, Y, Z axes
    //    {
    //        float invVelocity = relativeVelocity[i] != 0.0f ? 1.0f / relativeVelocity[i] : float.PositiveInfinity;

    //        float entry = (aabbB.min[i] - aabbA.max[i]) * invVelocity;
    //        float exit = (aabbB.max[i] - aabbA.min[i]) * invVelocity;

    //        if (entry > exit) (entry, exit) = (exit, entry); // Swap if inverted

    //        tMin = Mathf.Max(tMin, entry);
    //        tMax = Mathf.Min(tMax, exit);

    //        if (tMin > tMax) return false; // No collision
    //    }

    //    collisionTime = tMin;
    //    return tMin <= 1.0f; // Collision occurs within the frame
    //}


    public static bool CollideAABBPlane(FizziksShapeAABB aabb, FizziksShapePlane plane, FizikObject objectA)
    {
        // Recalculate bounds to ensure they're up-to-date
        aabb.RecalculateBounds();

        // Get the plane normal and distance
        Vector3 planeNormal = plane.Normal().normalized;
        Vector3 planePosition = plane.transform.position;


        // Check each vertex of the AABB against the plane
        Vector3[] vertices = {
        new Vector3(aabb.min.x, aabb.min.y, aabb.min.z),
        new Vector3(aabb.min.x, aabb.min.y, aabb.max.z),
        new Vector3(aabb.min.x, aabb.max.y, aabb.min.z),
        new Vector3(aabb.min.x, aabb.max.y, aabb.max.z),
        new Vector3(aabb.max.x, aabb.min.y, aabb.min.z),
        new Vector3(aabb.max.x, aabb.min.y, aabb.max.z),
        new Vector3(aabb.max.x, aabb.max.y, aabb.min.z),
        new Vector3(aabb.max.x, aabb.max.y, aabb.max.z),
    };

        bool isOverlapping = false;
        float maxPenetration = 0.0f;

        foreach (var vertex in vertices)
        {
            // Calculate distance of the vertex from the plane
            float distanceToPlane = Vector3.Dot(vertex - planePosition, planeNormal);

            if (distanceToPlane < 0) // Vertex is below the plane
            {
                isOverlapping = true;
                maxPenetration = Mathf.Max(maxPenetration, -distanceToPlane);
            }
        }

        // Handle collision response if overlapping
        if (isOverlapping)
        {
            // Resolve penetration
            Vector3 mtv = planeNormal * maxPenetration;
            objectA.transform.position += mtv;

            // Apply collision response (reflect velocity)
            Vector3 relativeVelocity = objectA.velocity;
            float restitution = 0.8f; // Coefficient of restitution
            objectA.velocity -= (1 + restitution) * Vector3.Dot(relativeVelocity, planeNormal) * planeNormal;

            return true;
        }


        return false;
    }


    private static void HandleStaticCollision(FizikObject objectA, FizikObject objectB, Vector3 collisionNormal, float overlap)
    {
        // Check if A is static
        if (objectA.isStatic)
        {
            // Resolve penetration for objectB
            objectB.transform.position -= collisionNormal * overlap;

            // Apply velocity adjustment based on collision normal
            objectB.velocity -= Vector3.Project(objectB.velocity, collisionNormal);

            // Optionally apply friction if there's a tangent velocity
            Vector3 tangentVelocity = objectB.velocity - Vector3.Project(objectB.velocity, collisionNormal);
            float frictionCoefficient = 0.5f; // Adjustable coefficient
            objectB.velocity -= tangentVelocity.normalized * Mathf.Min(tangentVelocity.magnitude, frictionCoefficient);
        }
        // Check if B is static
        else if (objectB.isStatic)
        {
            // Resolve penetration for objectA
            objectA.transform.position += collisionNormal * overlap;

            // Apply velocity adjustment based on collision normal
            objectA.velocity -= Vector3.Project(objectA.velocity, collisionNormal);

            // Optionally apply friction if there's a tangent velocity
            Vector3 tangentVelocity = objectA.velocity - Vector3.Project(objectA.velocity, collisionNormal);
            float frictionCoefficient = 0.5f; // Adjustable coefficient
            objectA.velocity -= tangentVelocity.normalized * Mathf.Min(tangentVelocity.magnitude, frictionCoefficient);
        }
        // Both objects are dynamic (unlikely here but for extensibility)
        else
        {
            objectA.transform.position += collisionNormal * overlap * 0.5f;
            objectB.transform.position -= collisionNormal * overlap * 0.5f;

            // Stop velocities along the collision normal
            objectA.velocity -= Vector3.Project(objectA.velocity, collisionNormal);
            objectB.velocity -= Vector3.Project(objectB.velocity, collisionNormal);

            // Optionally apply friction to both objects
            Vector3 relativeVelocity = objectA.velocity - objectB.velocity;
            Vector3 tangentVelocity = relativeVelocity - Vector3.Project(relativeVelocity, collisionNormal);
            float frictionCoefficient = 0.5f; // Adjustable coefficient
            Vector3 frictionForce = -tangentVelocity.normalized * Mathf.Min(tangentVelocity.magnitude, frictionCoefficient);

            objectA.velocity += frictionForce / 2; // Split friction force
            objectB.velocity -= frictionForce / 2;
        }
    }


    public bool CollideSpherePlane(FizziksShapeSphere sphere, FizziksShapePlane plane, FizikObject objectA)
    {
        Vector3 planeNormal = plane.Normal().normalized;
        Vector3 planeToSphere = sphere.transform.position - plane.transform.position;
        float distanceToPlane = Vector3.Dot(planeToSphere, planeNormal);

        if (Mathf.Abs(distanceToPlane) < sphere.radius)
        {
            Vector3 mtv = planeNormal * (sphere.radius - Mathf.Abs(distanceToPlane));
            objectA.transform.position += mtv;

            Vector3 normalForce = -planeNormal * AccelerationGravity.magnitude * objectA.mass;
            Vector3 gravityPerpendicular = AccelerationGravity * objectA.gravityScale - Vector3.Project(AccelerationGravity, planeNormal);
            Vector3 frictionForce = -objectA.velocity.normalized * 0.5f * normalForce.magnitude;

            objectA.velocity += (gravityPerpendicular + frictionForce) * dt;
            objectA.velocity -= (1 + 0.8f) * Vector3.Dot(objectA.velocity, planeNormal) * planeNormal;

            return true;
        }

        return false;
    }







    public static bool CollideSphereAABB(FizziksShapeSphere sphere, FizziksShapeAABB aabb, FizikObject sphereObject)
    {
        aabb.RecalculateBounds();

        // Find the closest point on the AABB to the sphere's center
        Vector3 closestPoint = Vector3.Max(aabb.min, Vector3.Min(sphere.transform.position, aabb.max));

        // Calculate the distance from the sphere's center to the closest point
        Vector3 sphereToClosest = sphere.transform.position - closestPoint;
        float distanceSquared = sphereToClosest.sqrMagnitude;

        if (distanceSquared < sphere.radius * sphere.radius)
        {
            // Collision detected: resolve the collision
            float penetrationDepth = sphere.radius - Mathf.Sqrt(distanceSquared);
            Vector3 mtv = sphereToClosest.normalized * penetrationDepth;

            if (!sphereObject.isStatic)
            {
                sphereObject.transform.position += mtv;
                sphereObject.velocity = Vector3.zero;
            }

            return true;
        }

        return false;
    }






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



    public void DestroyFizikObject(FizikObject obj)
    {
        if (objects.Contains(obj))
        {
            objects.Remove(obj);
        }
        Destroy(obj.gameObject);
    }



}

