using System.Linq;
using HarmonyLib;
using MiraAPI.Presets;

namespace MiraAPI.Patches.Options;

internal static class GamePresetsTabPatches
{
    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.OpenMenu))]
    public static class GamePresetsTabOnEnablePatch
    {
        public static void Postfix(GamePresetsTab __instance)
        {
            if (GameSettingMenuPatches.SelectedModIdx == 0)
            {
                return;
            }

            __instance.StandardPresetButton.gameObject.SetActive(false);
            __instance.SecondPresetButton.gameObject.SetActive(false);

            foreach (var preset in PresetManager.Presets.Where(x=>x.Plugin==GameSettingMenuPatches.SelectedMod))
            {
            }
        }
    }
}
