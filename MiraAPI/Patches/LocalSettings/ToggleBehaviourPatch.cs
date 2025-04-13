using HarmonyLib;

namespace MiraAPI.Patches.LocalSettings;

[HarmonyPatch(typeof(ToggleButtonBehaviour))]
public static class ToggleBehaviourPatch
{
    /// <summary>
    /// Makes the ToggleButtonBehaviour On/Off text bolded because it looks good.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ToggleButtonBehaviour.ResetText))]
    public static void ResetTextPrefix(ToggleButtonBehaviour __instance)
    {
        __instance.Text.text = $"{DestroyableSingleton<TranslationController>.Instance.GetString(__instance.BaseText)}: <b>{DestroyableSingleton<TranslationController>.Instance.GetString(__instance.onState ? StringNames.SettingsOn : StringNames.SettingsOff)}</b>";
    }
}