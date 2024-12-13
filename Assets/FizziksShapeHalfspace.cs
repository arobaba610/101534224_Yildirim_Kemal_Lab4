using UnityEngine;
public class FizziksShapeHalfspace : FizziksShape
{
    public override Shape GetShape()
    {
        return Shape.Halfspace;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 Normal()
    {
        return transform.up;
    }
}
