using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMotion : MonoBehaviour
{
    [Header("Launch Parameters")]
    public float angleHorizontalDegrees = 30; // Horizontal launch angle in degrees (XZ plane)
    public float angleVerticalDegrees = 45; // Vertical launch angle in degrees (YZ plane)
    public float speed = 20f; // Launch speed (units per second)
    public float startHeight = 1; // Starting height of the projectile

    [Header("Projectile Settings")]
    public GameObject projectileToCopy; // Reference to the projectile prefab

    [Header("Debug Settings")]
    public bool enableDebug = true; // Enable debug visualization

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchProjectile();
        }
    }

    void LaunchProjectile()
    {
        // Convert angles to radians
        float horizontalAngleRad = angleHorizontalDegrees * Mathf.Deg2Rad;
        float verticalAngleRad = angleVerticalDegrees * Mathf.Deg2Rad;

        // Calculate the initial launch velocity
        float horizontalSpeed = speed * Mathf.Cos(verticalAngleRad);
        Vector3 launchVelocity = new Vector3(
            horizontalSpeed * Mathf.Cos(horizontalAngleRad),
            speed * Mathf.Sin(verticalAngleRad),
            horizontalSpeed * Mathf.Sin(horizontalAngleRad)
        );

        Vector3 startPosition = new Vector3(0, startHeight, 0);

        // Instantiate a new projectile and assign it the initial conditions
        GameObject newObject = Instantiate(projectileToCopy);
        FizikObject fizikObject = newObject.GetComponent<FizikObject>();

        // Set the projectile's initial velocity and position
        fizikObject.velocity = launchVelocity;
        fizikObject.transform.position = startPosition;

        if (enableDebug)
        {
            Debug.Log($"Launch! Velocity: {launchVelocity}, Start Position: {startPosition}");
        }
    }
}
