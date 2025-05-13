using UnityEngine;

public class AeroSurface : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public AeroSurfaceConfig Config;
    public float FlapDeployRatio = 0;
    public float MaxFlapAngleDegrees = 30;
    public float MaxFlapAngle => Mathf.Deg2Rad * MaxFlapAngleDegrees;
    public float FlapAngle => FlapDeployRatio * MaxFlapAngle;

    public bool IsStalling { get; private set; } = false;
    public AeroForces Forces { get; private set; } = new();

    public AeroForces CalculateForces(Vector3 velocity, Vector3 relativePosition)
    {
        const float airDensity = 1.225f;

        // Ignoring spanwise-flow.
        velocity = Vector3.ProjectOnPlane(velocity, transform.right);
        float aoa = GetAoa(velocity);
        FlightCoefficients coefficients = AeroCoefficients.CalculateCoefficients(Config, aoa, FlapAngle);
        float area = Config.Chord * Config.Span;

        Vector3 dragDirection = -velocity.normalized;
        Vector3 liftDirection = transform.up;

        float dynamicPressure = 0.5f * airDensity * velocity.sqrMagnitude;

        AeroForces forces;
        forces.Lift = area * coefficients.Lift * dynamicPressure * liftDirection;
        forces.Drag = area * coefficients.Drag * dynamicPressure * dragDirection;
        //forces.Torque = area * coefficients.Moment * Config.Chord * dynamicPressure * -transform.right;

        forces.Torque = Vector3.Cross(relativePosition, forces.Lift + forces.Drag);

        Forces = forces;

        return forces;
    }

    private float GetAoa(Vector3 velocity)
    {
        if (Mathf.Approximately(velocity.sqrMagnitude, 0))
            return 0;
        return Mathf.Deg2Rad * Vector3.SignedAngle(transform.forward, velocity, transform.right);
    }
}
