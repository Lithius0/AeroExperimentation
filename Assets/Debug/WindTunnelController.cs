using ImGuiNET;
using UnityEngine;

public class WindTunnelController : DebugModule
{
    public override string DefaultName => "Wind Tunnel Controller";

    private float windSpeed = 0;

    public FlightModel FlightModel;
    public ParticleSystem Windparticles;

    public override void Render()
    {
        if (ImGui.Begin("Wind Tunnel", ref ModuleEnabled))
        {
            if (ImGui.SliderFloat("Wind Speed", ref windSpeed, 0, 300))
            {
                FlightModel.Wind = new Vector3(0, 0, -windSpeed);
                var velocityOverLiftime = Windparticles.velocityOverLifetime;
                velocityOverLiftime.speedModifier = windSpeed;
            }
        }
        ImGui.End();
    }
}
