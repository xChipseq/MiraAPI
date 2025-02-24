using HarmonyLib;
using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;

namespace MiraAPI.Patches;

/// <summary>
/// Sync all options, role settings, and modifiers to the player when they join the game.
/// </summary>
[HarmonyPatch(typeof(AmongUsClient))]
public static class AmongUsClientSyncPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    public static void CreatePlayerPatch(ClientData clientData)
    {
        var joinEvent = new PlayerJoinEvent(clientData);
        MiraEventManager.InvokeEvent(joinEvent);

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (clientData.Id == AmongUsClient.Instance.HostId)
        {
            return;
        }

        ModdedOptionsManager.SyncAllOptions(clientData.Id);
        CustomRoleManager.SyncAllRoleSettings(clientData.Id);
        ModifierManager.SyncAllModifiers(clientData.Id);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    public static void PlayerLeftPatch(ClientData data, DisconnectReasons reason)
    {
        var leftEvent = new PlayerLeaveEvent(data, reason);
        MiraEventManager.InvokeEvent(leftEvent);
    }
}
