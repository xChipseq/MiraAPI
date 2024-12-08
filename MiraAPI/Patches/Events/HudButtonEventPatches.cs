using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Patch for button related MiraEvents.
/// </summary>
[HarmonyPatch]
public static class HudButtonEventPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AdminButton), nameof(AdminButton.DoClick))]
    public static bool AdminButtonDoClickPrefix(AdminButton __instance)
    {
        var @event = new AdminButtonClickEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    public static bool SabotageButtonDoClickPrefix(SabotageButton __instance)
    {
        var @event = new SabotageButtonClickEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    public static bool UseButtonDoClickPrefix(UseButton __instance)
    {
        var @event = new UseButtonClickEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    public static bool VentButtonDoClickPrefix(VentButton __instance)
    {
        var @event = new VentButtonClickEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
