namespace MiraAPI.Voting;

/// <summary>
/// Represents a custom vote.
/// </summary>
/// <param name="Voter">The player that voted.</param>
/// <param name="Suspect">The suspect being voted for.</param>
/// <param name="Weight">The weight of the vote for calculations.</param>
public record struct CustomVote(byte Voter, byte Suspect, float Weight = 1f);
