using UnityEngine;

public struct AeroForces
{
    public Vector3 Lift;
    public Vector3 Drag;
    public Vector3 Torque;

    public static AeroForces operator +(AeroForces a, AeroForces b)
    {
        AeroForces result;
        result.Lift = a.Lift + b.Lift;
        result.Drag = a.Drag + b.Drag;
        result.Torque = a.Torque + b.Torque;
        return result;
    }

}
