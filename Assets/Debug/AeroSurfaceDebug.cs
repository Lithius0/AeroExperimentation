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

            const int count = 1000;
            float[] xData = new float[count];
            float[] lift = new float[count];
            float[] liftNormal = new float[count];
            float[] liftStall = new float[count];
            float[] drag = new float[count];
            float[] dragNormal = new float[count];
            float[] dragStall = new float[count];
            float[] liftDragRatio = new float[count];

            if (ImPlot.BeginPlot("Coefficients"))
            {
                for (int i = 0; i < count; i++)
                {
                    float x = (float)i / count * 2 * Mathf.PI - Mathf.PI;
                    xData[i] = Mathf.Rad2Deg * x;
                    AeroIntermediates intermediates = AeroCoefficients.CalculateIntermediates(Target.Config, Target.FlapAngle);
                    FlightCoefficients coefficients = AeroCoefficients.CalculateCoefficients(Target.Config, x, Target.FlapAngle);
                    FlightCoefficients normalCoefficients = AeroCoefficients.CalculateLowAngleCoefficients(Target.Config, x, intermediates);
                    FlightCoefficients stallCoefficients = AeroCoefficients.CalculateHighAngleCoefficients(Target.Config, x, intermediates);
                    lift[i] = coefficients.Lift;
                    liftNormal[i] = normalCoefficients.Lift;
                    liftStall[i] = stallCoefficients.Lift;
                    drag[i] = coefficients.Drag;
                    dragNormal[i] = normalCoefficients.Drag;
                    dragStall[i] = stallCoefficients.Drag;
                    liftDragRatio[i] = coefficients.Lift / coefficients.Drag;
                }

                ImPlot.SetupAxisLimits(ImAxis.Y1, -2, 2, ImPlotCond.Once);

                ImPlot.SetNextLineStyle(new Vector4(0.3f, 0.5f, 0.7f, 1f));
                ImPlot.PlotLine("CL", ref xData[0], ref lift[0], count, 0, 0);
                ImPlot.SetNextLineStyle(new Vector4(0.3f, 0.5f, 0.7f, 0.3f));
                ImPlot.PlotLine("CL_Normal", ref xData[0], ref liftNormal[0], count, 0, 0);
                ImPlot.SetNextLineStyle(new Vector4(0.3f, 0.5f, 0.7f, 0.3f));
                ImPlot.PlotLine("CL_Stall", ref xData[0], ref liftStall[0], count, 0, 0);
                ImPlot.SetNextLineStyle(new Vector4(0.8f, 0.5f, 0.3f, 1f));
                ImPlot.PlotLine("CD", ref xData[0], ref drag[0], count, 0, 0);
                ImPlot.SetNextLineStyle(new Vector4(0.8f, 0.5f, 0.3f, 0.3f));
                ImPlot.PlotLine("CD_Normal", ref xData[0], ref dragNormal[0], count, 0, 0);
                ImPlot.SetNextLineStyle(new Vector4(0.8f, 0.5f, 0.3f, 0.3f));
                ImPlot.PlotLine("CD_Stall", ref xData[0], ref dragStall[0], count, 0, 0);

                ImPlot.EndPlot();
            }

            // CL/CD plots are on a different order of magnitude than the other coefficients.
            if (ImPlot.BeginPlot("Lift to Drag Ratio"))
            {
                ImPlot.SetNextLineStyle(new Vector4(0.8f, 0.5f, 0.8f, 1f));
                ImPlot.PlotLine("CL/CD", ref xData[0], ref liftDragRatio[0], count, 0, 0);
                ImPlot.EndPlot();
            }

        }
        ImGui.End();
    }
}