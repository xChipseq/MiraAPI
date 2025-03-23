using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MiraAPI.Example;

[BepInAutoPlugin("mira.example", "MiraExampleMod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ExamplePlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText => "Mira API\nExample Mod";
    public ConfigFile GetConfigFile() => Config;
    public override void Load()
    {
        ExampleEventHandlers.Initialize();
        Harmony.PatchAll();

        var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExamplePlugin))!.Location) + "\\touhats.catalog";
        AddressablesLoader.RegisterCatalog(path);
        AddressablesLoader.RegisterHats("touhats");

        AddressablesLoader.RegisterCatalog("https://raw.githubusercontent.com/MyDragonBreath/MyDragonBreath/refs/heads/main/DONTUSEGITHUBASACDN/catalog.json");
    }
}
