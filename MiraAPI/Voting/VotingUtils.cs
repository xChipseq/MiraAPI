﻿using System.Collections.Generic;
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
    /// Gets the exiled player from the list of votes. Returns null if no player is to be exiled.
    /// </summary>
    /// <param name="votes">>The list of votes to check.</param>
    /// <param name="isTie">Whether the vote is a tie.</param>
    /// <returns>The player to be exiled. Will be null if no player is to be exiled.</returns>
    public static NetworkedPlayerInfo? GetExiled(List<CustomVote> votes, out bool isTie)
    {
        var max = CalculateNumVotes(votes).MaxPair(out var tie);
        isTie = tie;
        var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key);

        if (exiled is null || exiled.IsDead || exiled.Disconnected)
        {
            exiled = null;
        }

        return exiled;
    }

    /// <summary>
    /// Handles when a vote is added and allows for other mods to override/modify.
    /// </summary>
    /// <param name="voteData">The player's vote data.</param>
    /// <param name="suspectIdx">Who the player voted for.</param>
    /// <param name="cancelVote">Whether you want the vote to commence or not.</param>
    private static void HandleVote(PlayerVoteData voteData, byte suspectIdx, out bool cancelVote)
    {
        var @event = new HandleVoteEvent(voteData, suspectIdx);
        MiraEventManager.InvokeEvent(@event);

        cancelVote = @event.PreventVote;

        if (@event.IsCancelled)
        {
            return;
        }

        if (voteData.VotesRemaining == 0 ||
            voteData.VotedFor(suspectIdx))
        {
            cancelVote = true;
            return;
        }

        voteData.DecreaseRemainingVotes(1);
        voteData.VoteForPlayer(suspectIdx);
    }

    /// <summary>
    /// Networks the removal of votes. Used to remove votes when a player disconnects.
    /// </summary>
    /// <param name="source">The player who is sending the RPC. Should be the host.</param>
    /// <param name="voterId">The player who voted.</param>
    /// <param name="votedFor">The player who the voter voted for.</param>
    [MethodRpc((uint)MiraRpc.RemoveVote, SendImmediately = true)]
    public static void RpcRemoveVote(PlayerControl source, byte voterId, byte votedFor)
    {
        if (!source.IsHost()) return;

        MeetingHud.Instance.playerStates.First(state => state.TargetPlayerId == voterId).UnsetVote();

        if (PlayerControl.LocalPlayer.PlayerId != voterId)
        {
            return;
        }

        MeetingHud.Instance.playerStates.First(state => state.TargetPlayerId == votedFor).ThumbsDown.enabled = false;

        if (!AmongUsClient.Instance.AmHost)
        {
            var voteData = PlayerControl.LocalPlayer.GetVoteData();
            voteData.DecreaseRemainingVotes(1);
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
    /// Networks the casting of a vote. We replace the vanilla solution with a custom version that works for the use case.
    /// </summary>
    /// <param name="source">The player who sent this RPC.</param>
    /// <param name="srcPlayerId">The id of the player who casted the vote.</param>
    /// <param name="suspectPlayerId">The voted player's id.</param>
    [MethodRpc((uint)MiraRpc.CastVote, SendImmediately = true)]
    public static void RpcCastVote(PlayerControl source, byte srcPlayerId, byte suspectPlayerId)
    {
        CustomCastVote(srcPlayerId, suspectPlayerId);
    }

    private static void CustomCastVote(byte srcPlayerId, byte suspectPlayerId)
    {
        var plr = GameData.Instance.GetPlayerById(srcPlayerId);
        if (!plr) return;

        var voteData = plr.Object.GetVoteData();
        if (!voteData) return;

        HandleVote(voteData, suspectPlayerId, out var cancelVote);

        // Handle local behaviour for the voter (for some reason checking AmOwner does not work for host)
        if (PlayerControl.LocalPlayer.PlayerId == srcPlayerId)
        {
            if (!cancelVote) SoundManager.Instance.PlaySound(MeetingHud.Instance.VoteLockinSound, false);

            foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
            {
                playerVoteArea.ClearButtons();
            }

            MeetingHud.Instance.SkipVoteButton.ClearButtons();

            var localVoteData = PlayerControl.LocalPlayer.GetVoteData();
            if (!localVoteData || localVoteData.VotesRemaining != 0) return;

            MeetingHud.Instance.SkipVoteButton.voteComplete = true;
            MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
        }

        if (cancelVote) return;

        // If player has no more votes, then make it show that the player has used all their votes.
        if (voteData.VotesRemaining == 0)
        {
            MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == srcPlayerId).SetVote(suspectPlayerId);
        }

        // If host, then check end voting, and set votes/send chat if applicable
        if (!PlayerControl.LocalPlayer.IsHost()) return;

        MeetingHud.Instance.SetDirtyBit(1U);
        MeetingHud.Instance.CheckForEndVoting();

        if (voteData.VotesRemaining != 0) return;
        PlayerControl.LocalPlayer.RpcSendChatNote(srcPlayerId, ChatNoteTypes.DidVote);
    }

    /// <summary>
    /// Calculates the total number of votes.
    /// </summary>
    /// <param name="votes">A list of calculated votes.</param>
    /// <returns>The total votes.</returns>
    public static Dictionary<byte, float> CalculateNumVotes(IEnumerable<CustomVote> votes)
    {
        var dictionary = new Dictionary<byte, float>();

        foreach (var vote in votes.Select(v=>v.Suspect))
        {
            if (!dictionary.TryAdd(vote, 1))
            {
                dictionary[vote] += 1;
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
        return
        [
            .. Helpers.GetAlivePlayers()
            .SelectMany(player => player.GetVoteData().Votes)
        ];
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
