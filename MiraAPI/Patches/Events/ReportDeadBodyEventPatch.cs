using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Events;

[HarmonyPatch]
public static class ReportDeadBodyEventPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    public static bool PlayerControlReportDeadBodyPrefix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        var body = Helpers.GetBodyById(target.PlayerId);
        var @event = new ReportDeadBodyEvent(__instance, target, body);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
