using System.Linq;
using HarmonyLib;
using Il2CppSystem;
using MiraAPI.Roles;
using Reactor.Utilities;
using UnityEngine;
using Action = System.Action;
using Object = Il2CppSystem.Object;

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
        var role = CustomRoleManager.CustomRoles.Values.FirstOrDefault(x => x.StringName == key);
        if (!role || role is not ICustomRole)
        {
            return true;
        }

        Logger<MiraApiPlugin>.Error("UPDATING ROLE TEXT");

        var text = teamType == RoleTeamTypes.Crewmate
            ? Palette.CrewmateSettingChangeText.ToTextColor()
            : Palette.ImpostorRed.ToTextColor();

        var item = TranslationController.Instance.GetString(
            StringNames.LobbyChangeSettingNotificationRole,
            string.Concat(
                $"<sprite name=\"{role.NiceName}\">",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
                text,
                TranslationController.Instance.GetString(key, Array.Empty<Object>()),
                "</color></font>"
            ),
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleCount + "</font>",
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleChance + "%"
        );

        CustomMessageLogic(__instance, role, item, playSound);
        return false;
    }
    private static void CustomMessageLogic(NotificationPopper __instance, RoleBehaviour role, string item, bool playSound)
    {
        if (__instance.lastMessageKey == (int)role.StringName && __instance.activeMessages.Count > 0)
        {
            __instance.activeMessages.ToArray()[__instance.activeMessages.Count - 1].UpdateMessage(item);
        }
        else
        {
            __instance.lastMessageKey = (int)role.StringName;
            var newMessage = UnityEngine.Object.Instantiate(__instance.notificationMessageOrigin, Vector3.zero, Quaternion.identity, __instance.transform);
            newMessage.transform.localPosition = new Vector3(0f, 0f, -2f);
            newMessage.SetUp(item, __instance.settingsChangeSprite, __instance.settingsChangeColor, new Action(delegate
            {
                __instance.OnMessageDestroy(newMessage);
            }));
            __instance.ShiftMessages();
            __instance.AddMessageToQueue(newMessage);
        }
        if (playSound)
        {
            SoundManager.Instance.PlaySoundImmediate(__instance.settingsChangeSound, false, 1f, 1f, null);
        }
    }
}

