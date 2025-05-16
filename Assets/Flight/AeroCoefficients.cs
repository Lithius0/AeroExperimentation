using UnityEngine;

public struct FlightCoefficients
{
    public float Lift;
    public float Drag;
    public float Moment;

    public static FlightCoefficients Lerp(FlightCoefficients a, FlightCoefficients b, float t)
    {
        FlightCoefficients coefficients;
        coefficients.Lift = Mathf.Lerp(a.Lift, b.Lift, t);
        coefficients.Drag = Mathf.Lerp(a.Drag, b.Drag, t);
        coefficients.Moment = Mathf.Lerp(a.Moment, b.Moment, t);
        return coefficients;
    }
}

/// <summary>
/// A collection of intermediate values used in both the normal and stall flight regimes.
/// The original purpose is to allow a debug menu to peer into these values 
/// but also allows for running the coefficient calculations for the two regimes separately.
/// </summary>
public struct AeroIntermediates
{
    // Technically not an intermediate, but many of the coefficients are functions of flap angle so including may help.
    public float FlapAngle;

    public float CorrectedLiftSlope;
    public float FlapEffectiveness;
    public float DeltaLift;
    public float ZeroLiftAoa;

    public float StallAnglePositive;
    public float StallAngleNegative;
}

public static class AeroCoefficients
{
    private static float ViscosityFactor(float flapAngle)
    {
        return Mathf.Lerp(0.8f, 0.4f, (Mathf.Abs(flapAngle * Mathf.Rad2Deg) - 10) / 50);
    }

    private static float LiftCoefficientMaxFraction(float flapFraction)
    {
        return Mathf.Clamp01(1 - 0.5f * (flapFraction - 0.1f) / 0.3f);
    }

    private static float TorqCoefficientProportion(float effectiveAngle)
    {
        return 0.25f - 0.175f * (1 - 2 * Mathf.Abs(effectiveAngle) / Mathf.PI);
    }

    public static AeroIntermediates CalculateIntermediates(AeroSurfaceConfig config, float flapAngle)
    {
        AeroIntermediates intermediates;
        float correctedLiftSlope = config.CorrectedLiftSlope;
        float theta = Mathf.Acos(2 * config.FlapFraction - 1);
        float flapEffectiveness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;
        float deltaLift = correctedLiftSlope * flapEffectiveness * ViscosityFactor(flapAngle) * flapAngle;
        float zeroLiftAoa = config.ZeroLiftAoABase - deltaLift / correctedLiftSlope;

        float clMaxHigh = correctedLiftSlope * (config.StallAnglePositive - config.ZeroLiftAoABase) + deltaLift * LiftCoefficientMaxFraction(config.FlapFraction);
        float clMaxLow = correctedLiftSlope * (config.StallAngleNegative - config.ZeroLiftAoABase) + deltaLift * LiftCoefficientMaxFraction(config.FlapFraction);

        float stallAnglePositive = zeroLiftAoa + clMaxHigh / correctedLiftSlope;
        float stallAngleNegative = zeroLiftAoa + clMaxLow / correctedLiftSlope;

        intermediates.FlapAngle = flapAngle;

        intermediates.CorrectedLiftSlope = correctedLiftSlope;
        intermediates.FlapEffectiveness = flapEffectiveness;
        intermediates.DeltaLift = deltaLift;
        intermediates.ZeroLiftAoa = zeroLiftAoa;

        intermediates.StallAnglePositive = stallAnglePositive;
        intermediates.StallAngleNegative = stallAngleNegative;

        return intermediates;
    }

    public static FlightCoefficients CalculateCoefficients(AeroSurfaceConfig config, float aoa, float flapAngle)
    {
        AeroIntermediates intermediates = CalculateIntermediates(config, flapAngle);

        float transitionRadius = config.TransitionWidth / 2;
        float stallAnglePositive = intermediates.StallAnglePositive;
        float stallAngleNegative = intermediates.StallAngleNegative;
        if (Mathf.Abs(aoa - stallAnglePositive) <= transitionRadius)
        {
            var lowCoefficients = CalculateLowAngleCoefficients(config, aoa, intermediates);
            var highCoefficients = CalculateHighAngleCoefficients(config, aoa, intermediates);
            float lerpT = Mathf.InverseLerp(stallAnglePositive - transitionRadius, stallAnglePositive + transitionRadius, aoa);
            return FlightCoefficients.Lerp(lowCoefficients, highCoefficients, lerpT);
        }
        else if (Mathf.Abs(aoa - stallAngleNegative) <= transitionRadius)
        {
            var lowCoefficients = CalculateLowAngleCoefficients(config, aoa, intermediates);
            var highCoefficients = CalculateHighAngleCoefficients(config, aoa, intermediates);
            float lerpT = Mathf.InverseLerp(stallAngleNegative - transitionRadius, stallAngleNegative + transitionRadius, aoa);
            return FlightCoefficients.Lerp(highCoefficients, highCoefficients, lerpT);
        }
        else if (aoa <= stallAnglePositive && aoa >= stallAngleNegative)
        {
            return CalculateLowAngleCoefficients(config, aoa, intermediates);
        }
        else
        {
            return CalculateHighAngleCoefficients(config, aoa, intermediates);
        }
    }

    public static FlightCoefficients CalculateLowAngleCoefficients(AeroSurfaceConfig config, float aoa, AeroIntermediates intermediates)
    {
        float zeroLiftAoa = intermediates.ZeroLiftAoa;
        float liftCoefficient = config.CorrectedLiftSlope * (aoa - zeroLiftAoa);
        float effectiveAngleOfAttack = aoa - zeroLiftAoa - liftCoefficient / (Mathf.PI * config.AspectRatio);
        float sinAngleOfAttack = Mathf.Sin(effectiveAngleOfAttack);
        float cosAngleOfAttack = Mathf.Cos(effectiveAngleOfAttack);
        float tangentLiftCoefficient = config.SkinFriction * Mathf.Cos(effectiveAngleOfAttack);
        float normalLiftCoefficient = (liftCoefficient + tangentLiftCoefficient * sinAngleOfAttack) / cosAngleOfAttack;
        float dragCoefficient = normalLiftCoefficient * sinAngleOfAttack + tangentLiftCoefficient * cosAngleOfAttack;
        float torqueCoefficient = -normalLiftCoefficient * TorqCoefficientProportion(effectiveAngleOfAttack);

        return new FlightCoefficients()
        {
            Lift = liftCoefficient,
            Drag = dragCoefficient,
            Moment = torqueCoefficient,
        };
    }

    public static FlightCoefficients CalculateHighAngleCoefficients(AeroSurfaceConfig config, float aoa, AeroIntermediates intermediates)
    {
        float zeroLiftAoa = intermediates.ZeroLiftAoa;
        float stallAnglePositive = intermediates.StallAnglePositive;
        float stallAngleNegative = intermediates.StallAngleNegative;
        float flapAngle = intermediates.FlapAngle;

        float inducedAoa;

        if (aoa > 0)
        {
            float lowAoaLiftCoefficient = config.CorrectedLiftSlope * (stallAnglePositive - zeroLiftAoa);
            float inducedAoaAtStall = lowAoaLiftCoefficient / (Mathf.PI * config.AspectRatio);
            inducedAoa = Mathf.Lerp(inducedAoaAtStall, 0, Mathf.InverseLerp(stallAnglePositive, Mathf.PI / 2, aoa));
        }
        else
        {
            float lowAoaLiftCoefficient = config.CorrectedLiftSlope * (stallAngleNegative - zeroLiftAoa);
            float inducedAoaAtStall = lowAoaLiftCoefficient / (Mathf.PI * config.AspectRatio);
            inducedAoa = Mathf.Lerp(inducedAoaAtStall, 0, Mathf.InverseLerp(stallAngleNegative, -Mathf.PI / 2, aoa));
        }

        float dragCoefficient90 = -4.26e-2f * flapAngle * flapAngle + 2.1e-1f * flapAngle + 1.98f;
        float effectiveAngleOfAttack = aoa - zeroLiftAoa - inducedAoa;
        float sinAngleOfAttack = Mathf.Sin(effectiveAngleOfAttack);
        float cosAngleOfAttack = Mathf.Cos(effectiveAngleOfAttack);
        // TODO: Find out where that abs comes from. It is not in the original paper.
        float normalLiftCoefficient = dragCoefficient90 * sinAngleOfAttack * (1 / (0.56f + 0.44f * Mathf.Abs(sinAngleOfAttack)) - 0.41f * (1 - Mathf.Exp(-17 / config.AspectRatio)));
        float tangentLiftCoefficient = 0.5f * config.SkinFriction * cosAngleOfAttack;
        float liftCoefficient = normalLiftCoefficient * cosAngleOfAttack - tangentLiftCoefficient * sinAngleOfAttack;
        float dragCoefficient = normalLiftCoefficient * sinAngleOfAttack + tangentLiftCoefficient * cosAngleOfAttack;
        float torqueCoefficient = -normalLiftCoefficient * TorqCoefficientProportion(effectiveAngleOfAttack);

        return new FlightCoefficients()
        {
            Lift = liftCoefficient,
            Drag = dragCoefficient,
            Moment = torqueCoefficient,
        };
    }
}