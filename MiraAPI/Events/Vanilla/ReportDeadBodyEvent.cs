namespace MiraAPI.Events.Vanilla;

/// <summary>
/// The event that is invoked when a player reports a dead body.
/// </summary>
public class ReportDeadBodyEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the player that is reporting the body.
    /// </summary>
    public PlayerControl Reporter { get; }

    /// <summary>
    /// Gets the player that is being reported.
    /// </summary>
    public NetworkedPlayerInfo Target { get; }

    /// <summary>
    /// Gets the body that is being reported.
    /// </summary>
    public DeadBody? Body { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportDeadBodyEvent"/> class.
    /// </summary>
    /// <param name="reporter">The player who reported the body.</param>
    /// <param name="target">The player being reported.</param>
    /// <param name="body">The body being reported.</param>
    public ReportDeadBodyEvent(PlayerControl reporter, NetworkedPlayerInfo target, DeadBody? body)
    {
        Reporter = reporter;
        Target = target;
        Body = body;
    }
}
