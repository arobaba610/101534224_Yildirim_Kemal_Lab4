using UnityEngine;

public class PhysicsSimulationInitializer : MonoBehaviour
{
    void Start()
    {
        // Create materials
        FizikMotoru.SurfaceMaterial steel = new FizikMotoru.SurfaceMaterial("Steel", 0.6f);
        FizikMotoru.SurfaceMaterial rubber = new FizikMotoru.SurfaceMaterial("Rubber", 0.9f);
        FizikMotoru.SurfaceMaterial wood = new FizikMotoru.SurfaceMaterial("Wood", 0.4f);

        // Add these materials to objects
        GameObject objA = new GameObject("ObjectA");
        FizikObject objectA = objA.AddComponent<FizikObject>();
        objectA.material = steel; // Assign steel material

        GameObject objB = new GameObject("ObjectB");
        FizikObject objectB = objB.AddComponent<FizikObject>();
        objectB.material = rubber; // Assign rubber material

        GameObject objC = new GameObject("ObjectC");
        FizikObject objectC = objC.AddComponent<FizikObject>();
        objectC.material = wood; // Assign wood material

        // Add objects to the physics engine
        FizikMotoru.Instance.objects.Add(objectA);
        FizikMotoru.Instance.objects.Add(objectB);
        FizikMotoru.Instance.objects.Add(objectC);

        // Set object positions, masses, and velocities
        objectA.transform.position = new Vector3(-1, 0, 0);
        objectA.mass = 5f;
        objectA.velocity = new Vector3(2, 0, 0);

        objectB.transform.position = new Vector3(1, 0, 0);
        objectB.mass = 3f;
        objectB.velocity = new Vector3(-1, 0, 0);

        objectC.transform.position = new Vector3(0, 2, 0);
        objectC.mass = 2f;
        objectC.velocity = new Vector3(0, -3, 0);
    }
}
