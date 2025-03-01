namespace MiraAPI.Voting;

/// <summary>
/// A custom implementation to keep track of votes.
/// </summary>
/// <param name="voter">The player who voted.</param>
/// <param name="suspect">The suspect.</param>
public readonly struct CustomVote(byte voter, byte suspect)
{
    /// <summary>
    /// Gets the playerId of the Voter.
    /// </summary>
    public byte Voter { get; } = voter;

    /// <summary>
    /// Gets the playerId of the Suspect.
    /// </summary>
    public byte Suspect { get; } = suspect;
}
