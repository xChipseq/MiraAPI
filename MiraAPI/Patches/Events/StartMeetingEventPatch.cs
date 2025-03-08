using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class StartMeetingEventPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        var @event = new StartMeetingEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
    }
}
