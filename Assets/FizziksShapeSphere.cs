using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FizziksShapeSphere : FizziksShape
{
    public override Shape GetShape()
    {
        return Shape.Sphere;
    }

    public float radius = 1;

    public void UpdateScale()
    {
        transform.localScale = new Vector3(radius, radius, radius) * 2f;
    }

    public void OnValidate()
    {
        UpdateScale();
    }

    private void Update()
    {
        UpdateScale();
    }

     
}
