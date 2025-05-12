
using System;
using UnityEngine;

public abstract class DebugModule : MonoBehaviour
{
    private static int instanceCounter = 0;

    [NonSerialized] // Debug modules should be always be disabled by default, don't expose.
    public bool ModuleEnabled = false;

    /// <summary>
    /// Override this string in child classes to change the default name. 
    /// This will show if <see cref="MenuName"/> is null or empty.
    /// </summary>
    public virtual string DefaultName => "DebugMenu";

    /// <summary>
    /// Name of the debug menu. 
    /// </summary>
    public string MenuName = "";
    /// <summary>
    /// Label used for ImGui. Each menu needs a unique label, so there's an instance number attached.
    /// </summary>
    public string Label => $"{(string.IsNullOrEmpty(MenuName) ? DefaultName : MenuName)}##DebugModule{instanceNumber}";
    public string Tooltip => GetType().Name;

    private int instanceNumber = 1;

    private void Awake()
    {
        instanceCounter++;
        instanceNumber = instanceCounter;
    }

    private void OnEnable()
    {
        DebugMenu.Modules.Add(this);
    }

    private void OnDisable()
    {
        DebugMenu.Modules.Remove(this);
    }

    public abstract void Render();
}
