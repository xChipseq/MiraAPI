using System.Collections.Generic;
using BepInEx.Unity.IL2CPP;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Manages the setting tabs
/// </summary>
public static class LocalSettingsManager
{
    internal static readonly List<ModSettingsTab> AllTabs = new();

    /// <summary>
    /// Creates a settings tab for your mod
    /// </summary>
    /// <param name="plugin">Your mod's plugin.</param>
    /// <param name="title">Title of your tab. This will show when the button is hovered.</param>
    /// <param name="shortTitle">Short version of title. Showed when button is not hovered. Replaced by Icon if it's set.</param>
    /// <param name="tabColor">Color of the tab button.</param>
    /// <param name="tabHoverColor">Color of the tab button when it's hovered.</param>
    /// <param name="icon">Icon of your tab button.</param>
    /// <returns>The created tab.</returns>
    public static ModSettingsTab CreateSettingsTab(BasePlugin plugin, string title = null, string shortTitle = null, Color? tabColor = null, Color? tabHoverColor = null, Sprite icon = null)
    {
        ModSettingsTab settings = new(plugin, title, shortTitle, tabColor, tabHoverColor, icon);
        AllTabs.Add(settings);
        Logger<MiraApiPlugin>.Info($"Settings tab created for {plugin}");
        return settings;
    }
}