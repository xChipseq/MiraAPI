using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Patches for custom ejection messages.
/// </summary>
[HarmonyPatch(typeof(ExileController))]
public static class EjectionPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ExileController.Begin))]
    public static void BeginPostfix(ExileController __instance)
    {
        var @event = new EjectionEvent(__instance);
        MiraEventManager.InvokeEvent(@event);

        if (!__instance.initData.networkedPlayer || !__instance.initData.networkedPlayer.Role ||
            __instance.initData.networkedPlayer.Role is not ICustomRole role)
        {
            return;
        }

        if (role.GetCustomEjectionMessage(__instance.initData.networkedPlayer) == null)
        {
            return;
        }

        __instance.completeString = role.GetCustomEjectionMessage(__instance.initData.networkedPlayer);
    }

    [HarmonyPatch]
    internal static class WrapUpPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ExileController), nameof(ExileController.WrapUp));
            yield return AccessTools.Method(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn));
            if (ModCompatibility.SubmergedLoaded && AccessTools.TypeByName("Submerged.ExileCutscene.SubmergedExileController") is { } type)
            {
                yield return AccessTools.Method(type, "WrapUpAndSpawn");
            }
        }

        public static void Postfix()
        {
            var @event = new RoundStartEvent(false);
            MiraEventManager.InvokeEvent(@event);
        }
    }
}
