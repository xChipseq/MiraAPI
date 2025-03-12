using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;

namespace MiraAPI.Patches.Events;

<<<<<<<< HEAD:MiraAPI/Patches/Events/ReportBodyEventPatch.cs
[HarmonyPatch]
public static class ReportBodyEventPatch
========
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class StartMeetingEventPatch
>>>>>>>> dev:MiraAPI/Patches/Events/StartMeetingEventPatch.cs
{
    public static void Postfix(MeetingHud __instance)
    {
<<<<<<<< HEAD:MiraAPI/Patches/Events/ReportBodyEventPatch.cs
        var body = target != null ? Helpers.GetBodyById(target.PlayerId) : null;

        var @event = new ReportBodyEvent(__instance, target, body);
========
        var @event = new StartMeetingEvent(__instance);
>>>>>>>> dev:MiraAPI/Patches/Events/StartMeetingEventPatch.cs
        MiraEventManager.InvokeEvent(@event);
    }
}
