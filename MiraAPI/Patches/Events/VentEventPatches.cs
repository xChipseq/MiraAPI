using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Patches to invoke vent related MiraEvents.
/// </summary>
[HarmonyPatch]
public static class VentEventPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    public static void EnterVentPrefix(Vent __instance, PlayerControl pc)
    {
        var @event = new EnterVentEvent(pc, __instance);
        MiraEventManager.InvokeEvent(@event);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    public static void ExitVentPrefix(Vent __instance, PlayerControl pc)
    {
        var @event = new ExitVentEvent(pc, __instance);
        MiraEventManager.InvokeEvent(@event);
    }
}
