using System.Collections.Generic;
using MiraAPI.Networking;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace MiraAPI.GameEnd;

/// <summary>
/// Custom Game Over.
/// </summary>
public abstract class CustomGameOver
{
    /// <summary>
    /// Gets the current CustomGameOver.
    /// </summary>
    public static CustomGameOver? Instance { get; internal set; }

    /// <summary>
    /// Run before setting up the game over.
    /// </summary>
    /// <param name="endGameManager">The EndGameManager instance.</param>
    /// <returns>Return True to use the default implementation, return False to skip it.</returns>
    public virtual bool BeforeSetEverythingUp(EndGameManager endGameManager)
    {
        return true;
    }

    /// <summary>
    /// Run after setting up the game over.
    /// </summary>
    /// <param name="endGameManager">The EndGameManager instance.</param>
    public virtual void AfterSetEverythingUp(EndGameManager endGameManager)
    {
    }

    /// <summary>
    /// Send a custom game over.
    /// </summary>
    /// <param name="winners">A collection of winners.</param>
    /// <typeparam name="T">Type of the custom game over.</typeparam>
    public static void Send<T>(IEnumerable<NetworkedPlayerInfo> winners) where T : CustomGameOver
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            Logger<MiraApiPlugin>.Error("Only the host can send a custom game over.");
            return;
        }

        var reason = GameOverManager.GetGameOverId<T>();
        var data = new GameOverData(reason, [.. winners]);

        Rpc<CustomGameOverRpc>.Instance.Send(PlayerControl.LocalPlayer, data, true);
    }

    /// <summary>
    /// Implicitly convert a CustomGameOver to a GameOverReason.
    /// </summary>
    /// <param name="gameOver">The CustomGameOver instance.</param>
    /// <returns>The GameOverReason associated with the CustomGameOver.</returns>
    public static implicit operator GameOverReason(CustomGameOver gameOver)
    {
        return (GameOverReason)GameOverManager.GetGameOverId(gameOver.GetType());
    }
}
