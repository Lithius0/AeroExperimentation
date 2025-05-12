using EchoImGui;
using ImGuiNET;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class is responsible for the main debug menu UI.
/// To make a new debug menu module, make a new class that inherits from <see cref="DebugModule"/>.
/// </summary>
public class DebugMenu : MonoBehaviour
{
    public static readonly List<DebugModule> Modules = new();
    private static float scale = 1;

    // Debug menu should not be enabled by default, so don't even expose it in editor.
    [NonSerialized]
    public bool MenuEnabled = false;
    // This is a debug menu and doesn't need to be rebinded, so just a simple InputAction will suffice.
    [SerializeField]
    private InputAction menuToggle;

    private bool showAboutWindow = false;
    private bool showMetricsWindow = false;
    private bool showDebugLogWindow = false;
    private bool showIdStackToolWindow = false;

    // Start is called before the first frame update
    private void OnEnable()
    {
        menuToggle.Enable();
        menuToggle.performed += ToggleEnabled;
        ImGuiController.OnLayout += OnLayout;
    }

    private void OnDisable()
    {
        menuToggle.Disable();
        menuToggle.performed -= ToggleEnabled;
        ImGuiController.OnLayout -= OnLayout;
    }

    private void ToggleEnabled(InputAction.CallbackContext context)
    {
        MenuEnabled = !MenuEnabled;
    }

    private void OnLayout()
    {
        if (!MenuEnabled)
            return;

        // Modules should render even if the debug menu is collapsed,
        // so they go out here.
        foreach (var module in Modules)
        {
            if (module.ModuleEnabled)
            {
                module.Render();
            }
        }

        if (ImGui.Begin("Debug Menu", ref MenuEnabled, ImGuiWindowFlags.MenuBar))
        {
            // Menu bar for all the ImGui debug tools
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("ImGui"))
                {
                    ImGui.MenuItem("About", null, ref showAboutWindow);
                    ImGui.MenuItem("Metrics/Debugger", null, ref showMetricsWindow);
                    ImGui.MenuItem("Debug Log", null, ref showDebugLogWindow);
                    ImGui.MenuItem("ID Stack Tool", null, ref showIdStackToolWindow);
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (showAboutWindow)
                ImGui.ShowAboutWindow(ref showAboutWindow);
            if (showMetricsWindow)
                ImGui.ShowMetricsWindow(ref showMetricsWindow);
            if (showDebugLogWindow)
                ImGui.ShowDebugLogWindow(ref showDebugLogWindow);
            if (showIdStackToolWindow)
                ImGui.ShowIDStackToolWindow(ref showIdStackToolWindow);


            if (ImGui.SliderFloat("Scale", ref scale, 0.1f, 3f))
            {
                ImGui.GetIO().FontGlobalScale = scale;
            }

            // Checkboxes for all the modules
            foreach (var module in Modules)
            {
                ImGui.Checkbox(module.Label, ref module.ModuleEnabled);
                ImGui.SetItemTooltip(module.Tooltip);
            }
        }
        ImGui.End();
    }
}
