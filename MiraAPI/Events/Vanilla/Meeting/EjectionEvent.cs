namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when a player is ejected.
/// </summary>
public class EjectionEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the player that is being ejected.
    /// </summary>
    public PlayerControl? Player { get; }

    /// <summary>
    /// Gets the instance of the ExileController.
    /// </summary>
    public ExileController ExileController { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EjectionEvent"/> class.
    /// </summary>
    /// <param name="player">The player who is being ejected.</param>
    /// <param name="controller">The exile controller.</param>
    public EjectionEvent(PlayerControl player, ExileController controller)
    {
        Player = player;
        ExileController = controller;
    }
}
