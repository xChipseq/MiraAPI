﻿using HarmonyLib;
using Il2CppSystem;
using MiraAPI.Roles;
using System.Linq;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(NotificationPopper))]
public static class NotificationPopperPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(NotificationPopper.AddRoleSettingsChangeMessage))]
    public static bool RoleChangeMsgPatch(
        NotificationPopper __instance,
        [HarmonyArgument(0)] StringNames key,
        [HarmonyArgument(1)] int roleCount,
        [HarmonyArgument(2)] int roleChance,
        [HarmonyArgument(3)] RoleTeamTypes teamType,
        [HarmonyArgument(4)] bool playSound)
    {
        if (CustomRoleManager.CustomRoles.Values.All(role => role.StringName != key))
        {
            return true;
        }

        var text = teamType == RoleTeamTypes.Crewmate
            ? Palette.CrewmateSettingChangeText.ToTextColor()
            : Palette.ImpostorRed.ToTextColor();

        var item = DestroyableSingleton<TranslationController>.Instance.GetString(
            StringNames.LobbyChangeSettingNotificationRole,
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
            text,
            DestroyableSingleton<TranslationController>.Instance.GetString(key, Array.Empty<Object>()),
            "</color></font>",
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleCount + "</font>",
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleChance + "%"
        );

        __instance.SettingsChangeMessageLogic(key, item, playSound);
        return false;
    }
}
