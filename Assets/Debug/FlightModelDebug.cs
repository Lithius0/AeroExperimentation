using ImGuiNET;
using ImPlotNET;
using UnityEngine;

public class FlightModelDebug : DebugModule
{
    public override string DefaultName => "Flight Model";

    public FlightModel Target;

    public override void Render()
    {
        if (ImGui.Begin("Flight Model", ref ModuleEnabled))
        {
            var rigidbody = Target.GetComponent<Rigidbody>();
            var forces = Target.Forces;
            ImGui.Text($"Speed: {rigidbody.linearVelocity.magnitude} m/s");
            ImGui.Text($"Lift: {forces.Lift.magnitude}");
            ImGui.Text($"Drag: {forces.Drag.magnitude}");
            ImGui.Text($"Lift/Drag: {forces.Lift.magnitude / forces.Drag.magnitude}");
            ImGui.Text($"Torque: {forces.Torque.magnitude}");
        }
        ImGui.End();
    }
}
