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
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), typeof(SystemTypes), typeof(byte))]
    public static bool ShipStatusRpcUpdateSystemPrefix(ShipStatus __instance, SystemTypes systemType, byte amount)
    {
        var @event = new UpdateSystemEvent(systemType, amount);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
