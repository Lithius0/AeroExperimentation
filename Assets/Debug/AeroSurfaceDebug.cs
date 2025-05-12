using ImGuiNET;
using ImPlotNET;
using UnityEngine;

public class AeroSurfaceDebug : DebugModule
{
    public override string DefaultName => "Air Surface";

    public AeroSurface Target;
    private float flapAngle = 0;

    public override void Render()
    {
        if (ImGui.Begin("Test", ref ModuleEnabled))
        {
            ImGui.SliderAngle("Flap Angle", ref flapAngle, -50, 50);

            ImPlot.BeginPlot("Coefficients");
            const int count = 1000;
            float[] xData = new float[count];
            float[] liftCoefficients = new float[count];
            float[] drag = new float[count];

            for (int i = 0; i < count; i++)
            {
                float x = (float)i / count * 2 * Mathf.PI - Mathf.PI;
                xData[i] = Mathf.Rad2Deg * x;
                FlightCoefficients coefficients = AeroCoefficients.CalculateCoefficients(Target.Config, x, flapAngle);
                liftCoefficients[i] = float.IsFinite(coefficients.Lift) ? coefficients.Lift : 0;
                drag[i] = float.IsFinite(coefficients.Drag) ? coefficients.Drag : 0;
            }

            ImPlot.PlotLine("CL", ref xData[0], ref liftCoefficients[0], count, 0, 0);
            ImPlot.PlotLine("CD", ref xData[0], ref drag[0], count, 0, 0);

            ImPlot.EndPlot();
        }
        ImGui.End();
    }
}