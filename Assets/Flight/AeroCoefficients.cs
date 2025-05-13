using UnityEngine;

public struct FlightCoefficients
{
    public float Lift;
    public float Drag;
    public float Moment;
}

public static class AeroCoefficients
{
    private static float DeflectedZeroLiftAoa(AeroSurfaceConfig config, float flapAngle)
    {
        float theta = Mathf.Acos(2 * config.FlapFraction - 1);
        // Marked as tau in paper
        float flapEffectiveness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;
        // Marked as nu in paper
        float viscosityFactor = ViscosityFactor(flapAngle);

        // Marks as C_L_alpha in paper
        float correctedLiftSlope = config.CorrectedLiftSlope;
        float liftSlopeShift = correctedLiftSlope * flapEffectiveness * viscosityFactor * flapAngle;
        return config.ZeroLiftAoA - liftSlopeShift / correctedLiftSlope;
    }

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


    public static FlightCoefficients CalculateCoefficients(AeroSurfaceConfig config, float aoa, float flapAngle)
    {

        float correctedLiftSlope = config.CorrectedLiftSlope;
        float theta = Mathf.Acos(2 * config.FlapFraction - 1);
        float flapEffectivness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;
        float deltaLift = correctedLiftSlope * flapEffectivness * ViscosityFactor(flapAngle) * flapAngle;

        float zeroLiftAoaBase = config.ZeroLiftAoA;
        float zeroLiftAoA = zeroLiftAoaBase - deltaLift / correctedLiftSlope;

        float clMaxHigh = correctedLiftSlope * (config.StallAnglePositive - config.ZeroLiftAoA) + deltaLift * LiftCoefficientMaxFraction(config.FlapFraction);
        float clMaxLow = correctedLiftSlope * (config.StallAngleNegative - config.ZeroLiftAoA) + deltaLift * LiftCoefficientMaxFraction(config.FlapFraction);

        float stallAnglePositive = zeroLiftAoA + clMaxHigh / correctedLiftSlope;
        float stallAngleNegative = zeroLiftAoA + clMaxLow / correctedLiftSlope;

        if (aoa <= stallAnglePositive && aoa >= stallAngleNegative)
        {
            return CalculateLowAngleCoefficients(config, aoa, flapAngle);
        }
        else
        {
            return CalculateHighAngleCoefficients(config, aoa, flapAngle, stallAnglePositive, stallAngleNegative);
        }
    }

    private static FlightCoefficients CalculateLowAngleCoefficients(AeroSurfaceConfig config, float aoa, float flapAngle)
    {
        float zeroLiftAoa = DeflectedZeroLiftAoa(config, flapAngle);
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

    private static FlightCoefficients CalculateHighAngleCoefficients(AeroSurfaceConfig config, float aoa, float flapAngle, float stallAnglePositive, float stallAngleNegative)
    {
        // Need to calculate lift coefficient at low aoa for the induced aoa value.
        float zeroLiftAoa = DeflectedZeroLiftAoa(config, flapAngle);
        float inducedAoa;

        if (aoa > stallAnglePositive)
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