using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FizikObject : MonoBehaviour
{
    public FizziksShape shape = null;
    public Vector3 velocity = Vector3.zero;
    public float drag = 0.1f;
    public float mass = 1f;
    public float gravityScale = 1;
    public bool isStatic = false; // Definition for static objects

    public FizikMotoru.SurfaceMaterial material; // Use the shared SurfaceMaterial class

    void Start()
    {
        shape = GetComponent<FizziksShape>();

        // Default material setup if not assigned
        if (material == null)
        {
            material = new FizikMotoru.SurfaceMaterial("Default", 0.8f);
        }

        // Add this object to the physics engine
        FizikMotoru.Instance.objects.Add(this);
    }
}
