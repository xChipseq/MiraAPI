using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Used for patching sabotage/system related MiraEvents.
/// </summary>
[HarmonyPatch]
public static class SabotageEventPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(byte))]
    public static bool ShipStatusUpdateSystemPrefix(ShipStatus __instance, SystemTypes systemType, PlayerControl player, byte amount)
    {
        var @event = new UpdateSystemEvent(systemType, player, amount);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
