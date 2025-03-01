using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace MiraAPI.Voting;

/// <summary>
/// Utilities used for the Mira voting system.
/// </summary>
public static class VotingUtils
{
    private const byte SkipVoteId = 253;

    /// <summary>
    /// Handles when a vote is added and allows for other mods to override/modify.
    /// </summary>
    /// <param name="voteData">The player's vote data.</param>
    /// <param name="suspectIdx">Who the player voted for.</param>
    public static void HandleVote(PlayerVoteData voteData, byte suspectIdx)
    {
        HandleVoteEvent @event = new HandleVoteEvent(voteData, suspectIdx);
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            return;
        }

        voteData.RemoveVotes(1);
        voteData.VoteForPlayer(suspectIdx);
    }

    /// <summary>
    /// Networks the removal of votes. Used to remove votes when a player disconnects.
    /// </summary>
    /// <param name="source">The player who is sending the RPC. Should be the host.</param>
    /// <param name="voterId">The player who voted.</param>
    /// <param name="votedFor">The player who the voter voted for.</param>
    [MethodRpc((uint)MiraRpc.RemoveVote)]
    public static void RpcRemoveVote(PlayerControl source, byte voterId, byte votedFor)
    {
        if (source.OwnerId != AmongUsClient.Instance.HostId)
        {
            return;
        }

        MeetingHud.Instance.playerStates.First(state => state.TargetPlayerId == voterId).UnsetVote();

        if (PlayerControl.LocalPlayer.PlayerId != voterId)
        {
            return;
        }

        MeetingHud.Instance.playerStates.First(state => state.TargetPlayerId == votedFor).ThumbsDown.enabled = false;

        if (!AmongUsClient.Instance.AmHost)
        {
            var voteData = PlayerControl.LocalPlayer.GetVoteData();
            voteData.RemoveVotes(1);
            voteData.RemovePlayerVote(votedFor);
        }

        foreach (var t in MeetingHud.Instance.playerStates)
        {
            t.voteComplete = false;
        }

        MeetingHud.Instance.SkipVoteButton.voteComplete = false;
        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Calculates the total number of votes.
    /// </summary>
    /// <param name="votes">A list of calculated votes.</param>
    /// <returns>The total votes.</returns>
    public static Dictionary<byte, int> CalculateNumVotes(IEnumerable<CustomVote> votes)
    {
        var dictionary = new Dictionary<byte, int>();

        foreach (var vote in votes)
        {
            if (!dictionary.TryAdd(vote.Suspect, 1))
            {
                dictionary[vote.Suspect] += 1;
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Calculates votes to check if all players have voted.
    /// </summary>
    /// <returns>The list of votes.</returns>
    public static List<CustomVote> CalculateVotes()
    {
        return (from player in Helpers.GetAlivePlayers()
            from vote in player.GetVoteData()!.VotedPlayers
            select new CustomVote(player.PlayerId, vote)).ToList();
    }

    /// <summary>
    /// Handles the populating of results locally. Called by the PopulateResultsRpc.
    /// </summary>
    /// <param name="votes">The list of networked votes.</param>
    public static void HandlePopulateResults(List<CustomVote> votes)
    {
        PopulateResultsEvent @event = new PopulateResultsEvent(votes);
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            return;
        }

        MeetingHud.Instance.TitleText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(
                StringNames.MeetingVotingResults,
                Il2CppSystem.Array.Empty<Il2CppSystem.Object>());


        var delays = new Dictionary<byte, int>();
        var num = 0;

        for (var i = 0; i < MeetingHud.Instance.playerStates.Length; i++)
        {
            var playerVoteArea = MeetingHud.Instance.playerStates[i];
            playerVoteArea.ClearForResults();
            foreach (var vote in votes)
            {
                var playerById = GameData.Instance.GetPlayerById(vote.Voter);
                if (playerById == null)
                {
                    Logger<MiraApiPlugin>.Error($"Couldn't find player info for voter: {vote.Voter}");
                }
                else if (i == 0 && vote.Suspect == SkipVoteId)
                {
                    MeetingHud.Instance.BloopAVoteIcon(playerById, num, MeetingHud.Instance.SkippedVoting.transform);
                    num++;
                }
                else if (vote.Suspect == playerVoteArea.TargetPlayerId)
                {
                    if (!delays.TryAdd(vote.Suspect, 0))
                    {
                        delays[vote.Suspect]++;
                    }

                    MeetingHud.Instance.BloopAVoteIcon(playerById, delays[vote.Suspect], playerVoteArea.transform);
                }
            }
        }
    }
}
