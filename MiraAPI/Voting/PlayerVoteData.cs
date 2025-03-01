using System;
using System.Collections.Generic;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.Voting;

/// <summary>
/// Handles player votes, and removing/adding additional votes.
/// </summary>
[RegisterInIl2Cpp]
public class PlayerVoteData(nint ptr) : MonoBehaviour(ptr)
{
    /// <summary>
    /// Gets the owner of this component.
    /// </summary>
    public PlayerControl? Owner { get; private set; }

    /// <summary>
    /// Gets the players which the owner has voted for.
    /// </summary>
    public List<byte> VotedPlayers { get; private set; } = [];

    /// <summary>
    /// Gets or sets the amount of votes the owner has left.
    /// </summary>
    public int VotesRemaining
    {
        get => _votesRemaining;
        set => _votesRemaining = Math.Max(0, value);
    }

    private int _votesRemaining = 1;

    private void Awake()
    {
        Owner = GetComponent<PlayerControl>();

        if (Owner == null)
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Adds the voted player to the list.
    /// </summary>
    /// <param name="playerId">The target's playerId.</param>
    public void VoteForPlayer(byte playerId)
    {
        VotedPlayers.Add(playerId);
    }

    /// <summary>
    /// Removes the voted player from the list.
    /// </summary>
    /// <param name="playerId">The target's playerId.</param>
    public void RemovePlayerVote(byte playerId)
    {
        if (!VotedPlayers.Contains(playerId)) return;
        VotedPlayers.Remove(playerId);
    }

    /// <summary>
    /// Sets the player's votes remaining.
    /// </summary>
    /// <param name="votesRemaining">The amount of votes you would like to set it to.</param>
    public void SetVotesRemaining(int votesRemaining)
    {
        VotesRemaining = votesRemaining;
    }

    /// <summary>
    /// Adds votes to the owner.
    /// </summary>
    /// <param name="amount">The amount of votes you would like to add.</param>
    public void AddVotes(int amount)
    {
        SetVotesRemaining(VotesRemaining + amount);
    }

    /// <summary>
    /// Removes votes from the owner.
    /// </summary>
    /// <param name="amount">The amount of votes you would like to remove.</param>
    public void RemoveVotes(int amount)
    {
        SetVotesRemaining(VotesRemaining - amount);
    }
}
