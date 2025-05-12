using UnityEngine;

public class AeroSurface : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public AeroSurfaceConfig Config;
    public float FlapDeployRatio = 0;
    public float MaxFlapAngleDegrees = 30;
    public float MaxFlapAngle => Mathf.Deg2Rad * MaxFlapAngleDegrees;
    public float FlapAngle => FlapDeployRatio * MaxFlapAngle;

    public Vector3 GetForce(Vector3 velocity)
    {
        const float airDensity = 1.225f;

        Vector3 airVelocity = velocity;
        Vector3 normal = transform.up;
        float aoa = GetAoa(airVelocity);
        FlightCoefficients coefficients = AeroCoefficients.CalculateCoefficients(Config, aoa, FlapAngle);
        float area = Config.Chord * Config.Span;
        return 0.5f * coefficients.Lift * airDensity * area * normal;
    }

    private float GetAoa(Vector3 velocity)
    {
        if (Mathf.Approximately(velocity.sqrMagnitude, 0))
            return 0;
        return Vector3.SignedAngle(transform.forward, velocity, transform.right);
    }
}
