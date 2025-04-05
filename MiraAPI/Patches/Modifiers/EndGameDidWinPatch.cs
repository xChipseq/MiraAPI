using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities;

namespace MiraAPI.Patches.Modifiers;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class EndGameDidWinPatch
{
    public static void Postfix()
    {
        var gameOverReason = EndGameResult.CachedGameOverReason;
        EndGameResult.CachedWinners.Clear();

        var players = GameData.Instance.AllPlayers.ToArray();
        for (var i = 0; i < GameData.Instance.PlayerCount; i++)
        {
            var networkedPlayerInfo = players[i];
            if (!networkedPlayerInfo)
            {
                continue;
            }

            var didWin = networkedPlayerInfo.Role.DidWin(gameOverReason);

            bool? modifierDidWin = null;
            foreach (var modifier in networkedPlayerInfo.Object.GetModifiers<GameModifier>().OrderByDescending(x => x.Priority()))
            {
                var result = modifier.DidWin(gameOverReason);
                if (!result.HasValue) continue;

                modifierDidWin = result.Value;
                break;
            }

            if (modifierDidWin.HasValue)
            {
                didWin = modifierDidWin.Value;
            }

            if (didWin)
            {
                EndGameResult.CachedWinners.Add(new CachedPlayerData(networkedPlayerInfo));
            }
        }
    }
}
