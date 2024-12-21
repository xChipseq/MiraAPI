using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Events;

[HarmonyPatch]
public static class StartMeetingEventPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    public static bool PlayerControlCmdReportDeadBodyPrefix(PlayerControl __instance, NetworkedPlayerInfo? target)
    {
        var body = target != null ? Helpers.GetBodyById(target.PlayerId) : null;

        var @event = new StartMeetingEvent(__instance, target, body);
        MiraEventManager.InvokeEvent(@event);
        return !@event.IsCancelled;
    }
}
