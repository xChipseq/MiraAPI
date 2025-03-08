using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
public static class ReportBodyEventPatch
{
    public static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo? target)
    {
        var body = target != null ? Helpers.GetBodyById(target.PlayerId) : null;

        var @event = new ReportBodyEvent(__instance, target, body);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
