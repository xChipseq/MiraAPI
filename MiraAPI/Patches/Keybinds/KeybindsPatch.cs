using HarmonyLib;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using Rewired;

namespace MiraAPI.Patches.Keybinds
{
    [HarmonyPatch(typeof(InputManager_Base), nameof(InputManager_Base.Awake))]
    public static class KeybindMenuPatch
    {
        [HarmonyPrefix]
        private static void StartPrefix(InputManager_Base __instance)
        {
            foreach (var entry in KeybindManager.GetEntries())
            {
                __instance.userData.RegisterModBind(entry.Id, entry.Description, entry.Key, modifier1: entry.Modifier1, modifier2: entry.Modifier2, modifier3: entry.Modifier3);
            }
        }
    }
}
