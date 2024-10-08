using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMotion : MonoBehaviour
{
    public float angleDegrees = 30; // Launch angle in degrees
    public float speed = 20f; // Launch speed (units per second)
    public float startHeight = 1; // Starting height of the projectile

    public GameObject projectileToCopy; // Reference to the projectile prefab

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Calculate the initial launch velocity based on angle and speed
            float angleInRadians = angleDegrees * Mathf.Deg2Rad;
            Vector3 launchVelocity = new Vector3(speed * Mathf.Cos(angleInRadians), speed * Mathf.Sin(angleInRadians), 0);
            Vector3 startPosition = new Vector3(0, startHeight, 0);

            // Instantiate a new projectile and assign it the initial conditions
            GameObject newObject = Instantiate(projectileToCopy);
            FizikObject fizikObject = newObject.GetComponent<FizikObject>();

            // Set the projectile's initial velocity and position
            fizikObject.velocity = launchVelocity;
            fizikObject.transform.position = startPosition;

            Debug.Log("Launch!");
        }
    }
}
