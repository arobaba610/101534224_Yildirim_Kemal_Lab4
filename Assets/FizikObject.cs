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
    //public float radius = 1f;

    public float launchTime; // launchTime 

    void Start()
    {
        shape = GetComponent<FizziksShape>();
        FizikMotoru.Instance.objects.Add(this);
    }

}
