using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Patch for trigging the GameEndEvent.
/// </summary>
[HarmonyPatch]
public static class EndGamePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static void SetEverythingUpPatch(EndGameManager __instance)
    {
        var @event = new GameEndEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
    }
}
