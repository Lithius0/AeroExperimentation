using UnityEngine;

[CreateAssetMenu(fileName = "Air Surface Config", menuName = "Flight/Air Surface Config")]
public class AeroSurfaceConfig : ScriptableObject
{
    public float LiftSlope = 6.28f;
    public float SkinFriction = 0.02f;
    public float ZeroLiftAoADegrees = 0;
    public float StallAnglePositiveDegrees = 10;
    public float StallAngleNegativeDegrees = -10;
    public float Chord = 1;
    public float FlapFraction = 0.4f;
    public float Span = 2;
    public float AspectRatio = 2;
    public float TransitionWidthDegrees = 5;

    public float CorrectedLiftSlope => LiftSlope * (AspectRatio / (AspectRatio + 2 * (AspectRatio + 4) / (AspectRatio + 2)));
    public float StallAnglePositive => Mathf.Deg2Rad * StallAnglePositiveDegrees;
    public float StallAngleNegative => Mathf.Deg2Rad * StallAngleNegativeDegrees;
    public float ZeroLiftAoA => Mathf.Deg2Rad * ZeroLiftAoADegrees;
    public float TransitionWidth => Mathf.Deg2Rad * TransitionWidthDegrees;

    private void OnValidate()
    {
        if (FlapFraction > 0.4f)
            FlapFraction = 0.4f;
        if (FlapFraction < 0)
            FlapFraction = 0;

        if (StallAnglePositiveDegrees < 0)
            StallAnglePositiveDegrees = 0;
        if (StallAngleNegativeDegrees > 0)
            StallAngleNegativeDegrees = 0;

        if (Chord < 1e-3f)
            Chord = 1e-3f;
    }
}