using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Example.Buttons.Freezer;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

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
        MiraEventManager.RegisterEventHandler<MiraButtonClickEvent<FreezeButton>>(e=> 
        {
            Logger<ExamplePlugin>.Warning("Freeze button clicked!");
            e.Cancel();
            e.Button.SetTimer(5f);
        });

        MiraEventManager.RegisterEventHandler<MiraButtonCancelledEvent<FreezeButton>>(e=>
        {
            Logger<ExamplePlugin>.Warning("Freeze button cancelled!");
            e.Button.OverrideName("Freeze Canceled");
        });
        Harmony.PatchAll();
    }
}
