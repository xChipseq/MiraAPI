using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(TutorialManager._RunTutorial_d__3), nameof(TutorialManager._RunTutorial_d__3.MoveNext))]
public static class FreeplayRoundStartPatch
{
    public static void Postfix(TutorialManager._RunTutorial_d__3 __instance, ref bool __result)
    {
        if (!__result)
        {
            MiraEventManager.InvokeEvent(new RoundStartEvent(true));
        }
    }
}
