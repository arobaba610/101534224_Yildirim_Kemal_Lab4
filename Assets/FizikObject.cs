using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FizikObject : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;
    public float drag = 0.1f;
    public float mass = 1f;
    public float radius = 1f;

    void Start()
    {
        FizikMotoru.Instance.objects.Add(this);
    }

}
