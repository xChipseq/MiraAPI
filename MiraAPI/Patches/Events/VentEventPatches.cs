using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla;
using MiraAPI.Utilities;
using Reactor.Utilities;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Patches to invoke vent related MiraEvents.
/// </summary>
[HarmonyPatch]
public static class VentEventPatches
{
    private static bool _showButtons;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    public static void VentUsePostfix(VentButton __instance)
    {
        __instance.currentTarget?.SetButtons(_showButtons);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
    public static bool EnterVentPrefix(PlayerPhysics __instance, int id, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        var pc = __instance.myPlayer;
        var vent = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == id);

        var @event = new EnterVentEvent(pc, vent);
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            Logger<MiraApiPlugin>.Error("Enter vent event cancelled!");
            __result = Helpers.EmptyCoroutine().WrapToIl2Cpp();
        }

        if (pc.AmOwner)
        {
            _showButtons = !@event.IsCancelled;
        }
        return !@event.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
    public static bool ExitVentPrefix(PlayerPhysics __instance, int id, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        var pc = __instance.myPlayer;
        var vent = ShipStatus.Instance.AllVents.First(v => v.Id == id);

        var @event = new ExitVentEvent(pc, vent);
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            Logger<MiraApiPlugin>.Error("Exit vent event cancelled!");
            __result = Helpers.EmptyCoroutine().WrapToIl2Cpp();
        }

        if (pc.AmOwner)
        {
            _showButtons = @event.IsCancelled;
        }
        return !@event.IsCancelled;
    }
}
