using HarmonyLib;
using MiraAPI.Keybinds;
using UnityEngine;

namespace MiraAPI.Patches;

/// <summary>
/// Checks keybind conflicts in the lobby and disables secondary ones.
/// </summary>
[HarmonyPatch(typeof(GameStartManager))]
public static class GameStartManagerPatch
{
    public static bool conflictShown = false;

    [HarmonyPatch(nameof(GameStartManager.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(GameStartManager __instance)
    {
        if (conflictShown) return;

        var conflicts = KeybindManager.GetConflicts();
        if (conflicts.Count == 0) return;

        conflictShown = true;

        var message = "The following keybinds are disabled due to conflicts:\n\n";

        foreach (var (a, b) in conflicts)
        {
            message += $"• <color=#FF5555>{b.Description}</color> (conflicts with {a.Description})\n";
            b.Handler = null;
        }

        HudManager.Instance.ShowPopUp(message.TrimEnd());
    }
}
