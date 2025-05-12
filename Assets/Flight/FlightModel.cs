using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlightModel : MonoBehaviour
{
    private Rigidbody rigidBody;
    private AeroSurface[] airSurfaces;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        airSurfaces = GetComponentsInChildren<AeroSurface>();
    }

    private void FixedUpdate()
    {
        foreach (var airSurface in airSurfaces)
        {
            Vector3 position = airSurface.Position;
            Vector3 force = airSurface.GetForce(rigidBody.GetPointVelocity(position));
            rigidBody.AddForceAtPosition(force, position);
        }
    }
}
