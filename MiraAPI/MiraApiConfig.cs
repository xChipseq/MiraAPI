using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI;

/// <summary>
/// Mira API Config File Handler
/// </summary>
public class MiraApiConfig(ConfigFile config)
{
    /// <summary>
    /// Gets whether the modifiers hud should be on the left side of the screen (under roles/task tab). Recommended for streamers.
    /// </summary>
    public ConfigEntry<bool> ModifiersHudLeftSide { get; private set; } = config.Bind("Displays", "Show Modifiers HUD on Left Side", false);
}
