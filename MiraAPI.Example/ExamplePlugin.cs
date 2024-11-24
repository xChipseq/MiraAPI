using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

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
        MiraEventManager.RegisterEventHandler<BeforeMurderEvent>(
            e =>
            {
                if (e.Source.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    e.Cancel();
                }
            });
        Harmony.PatchAll();
    }
}
