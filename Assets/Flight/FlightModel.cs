using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlightModel : MonoBehaviour
{
    /// <summary>
    /// Cached forces values. Updated whenever the forces are calculated internally.
    /// </summary>
    public AeroForces Forces { get; private set; } = new();
    public Vector3 Velocity => rigidBody.linearVelocity;

    // It would be better to place this in an external object keeping track of atmospheric conditions.
    // But as it is right now there's no point to do so.
    public Vector3 Wind = Vector3.zero;

    private Rigidbody rigidBody;
    private AeroSurface[] surfaces;


    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        surfaces = GetComponentsInChildren<AeroSurface>();
    }

    private void Start()
    {
        rigidBody.AddForce(transform.forward * rigidBody.mass * 100, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        CalculateAerodynamicForces();
        foreach (var surfaces in surfaces)
        {
            Vector3 position = surfaces.Position;
            rigidBody.AddForceAtPosition(surfaces.Forces.Lift + surfaces.Forces.Drag, position);
        }
        rigidBody.AddForce(transform.forward * 14000);
    }

    private AeroForces CalculateAerodynamicForces()
    {
        AeroForces forces = new();
        foreach (var surfaces in surfaces)
        {
            Vector3 position = surfaces.Position;
            forces += surfaces.CalculateForces(rigidBody.GetPointVelocity(position) - Wind, position - rigidBody.worldCenterOfMass);
        }
        Forces = forces;
        return forces;
    }

    public void ApplyControl(Vector3 controlVector)
    {
        foreach (var surface in surfaces)
        {
            surface.ApplyControl(controlVector);
        }
    }

#if UNITY_EDITOR
    // For gizmos drawing.
    public void CalculateCenterOfLift(out Vector3 center, out Vector3 force, Vector3 displayAirVelocity, float displayAirDensity)
    {
        Vector3 com;
        AeroForces forceAndTorque;
        if (surfaces == null)
        {
            center = Vector3.zero;
            force = Vector3.zero;
            return;
        }

        if (rigidBody == null)
        {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            forceAndTorque = CalculateAerodynamicForces();
        }
        else
        {
            com = rigidBody.worldCenterOfMass;
            forceAndTorque = Forces;
        }

        Vector3 linearForce = forceAndTorque.Lift + forceAndTorque.Drag;
        force = linearForce;
        center = com + Vector3.Cross(linearForce, forceAndTorque.Torque) / linearForce.sqrMagnitude;
    }
#endif
}
