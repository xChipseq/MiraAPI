using BepInEx;
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
/// The main plugin class for Mira API.
/// </summary>
[BepInAutoPlugin("mira.api", "MiraAPI")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraApiPlugin : BasePlugin
{
    public static Color MiraColor { get; } = new Color32(238, 154, 112, 255);
    private static MiraPluginManager? PluginManager { get; set; }
    internal Harmony Harmony { get; } = new(Id);

    /// <inheritdoc />
    public override void Load()
    {
        Harmony.PatchAll();

        ReactorCredits.Register("Mira API", Version, true, ReactorCredits.AlwaysShow);

        PluginManager = new MiraPluginManager();
        PluginManager.Initialize();
    }
}
