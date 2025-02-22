using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using UnityEngine.UI;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(TaskAddButton))]
public static class TaskAddButtonPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaskAddButton.Start))]
    public static bool StartPrefix(TaskAddButton __instance)
    {
        if (uint.TryParse(__instance.name, out var result))
        {
            __instance.Overlay.enabled = false;
            if (PlayerControl.LocalPlayer.HasModifier(result))
            {
                __instance.Overlay.enabled = true;
                __instance.Overlay.sprite = __instance.CheckImage;
            }

            __instance.Button.OnClick = new Button.ButtonClickedEvent();
            __instance.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                if (PlayerControl.LocalPlayer.HasModifier(result))
                {
                    PlayerControl.LocalPlayer.GetModifierComponent()!.RemoveModifier(result);
                    __instance.Overlay.enabled = false;
                }
                else
                {
                    PlayerControl.LocalPlayer.GetModifierComponent()!.AddModifier(ModifierManager.GetModifierType(result)!);
                    __instance.Overlay.enabled = true;
                }
            }));
            return false;
        }

        return true;
    }
}
