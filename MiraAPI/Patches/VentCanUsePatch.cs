﻿using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches;

/// <summary>
/// Used to change vent behaviour for the event system, custom roles, and modifiers.
/// </summary>
[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
internal static class VentCanUsePatch
{
    // ReSharper disable InconsistentNaming
    public static void Postfix(Vent __instance, ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse)
    {
        var @event = new PlayerCanUseEvent(__instance.Cast<IUsable>());
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            canUse = couldUse = false;
            return;
        }

        var @object = pc.Object;
        var role = @object.Data.Role;

        var canVent = role is ICustomRole customRole ? customRole.Configuration.CanUseVent : role.CanVent;
        couldUse = canVent;

        var modifiers = @object.GetModifierComponent().ActiveModifiers;
        if (modifiers.Count > 0)
        {
            switch (canVent)
            {
                case true when modifiers.Exists(x => x.CanVent().HasValue && x.CanVent() == false):
                    couldUse = canUse = false;
                    return;
                case false when modifiers.Exists(x => x.CanVent().HasValue && x.CanVent() == true):
                    couldUse = true;
                    break;
            }
        }

        var num = float.MaxValue;

        canUse = couldUse;
        if (canUse)
        {
            var center = @object.Collider.bounds.center;
            var position = __instance.transform.position;
            num = Vector2.Distance(center, position);
            canUse &= num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
        }

        __result = num;
    }
}
