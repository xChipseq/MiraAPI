using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Patches.Voting;

[HarmonyPatch(typeof(DummyBehaviour))]
internal static class DummyBehaviourPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DummyBehaviour.Update))]
    public static bool DummyUpdatePatch(DummyBehaviour __instance)
    {
        NetworkedPlayerInfo data = __instance.myPlayer.Data;
        if (data == null || data.IsDead) return false;

        if (MeetingHud.Instance && !__instance.DidVote())
        {
            Coroutines.Start(CoDoVote(__instance));
        }

        return false;
    }

    private static IEnumerator CoDoVote(DummyBehaviour dummy)
    {
        dummy.voted = true;
        yield return new WaitForSeconds(dummy.voteTime.Next());
        if (DidVote(dummy)) yield break;

        var dummyVoteEvent = new DummyVoteEvent(dummy);
        MiraEventManager.InvokeEvent(dummyVoteEvent);

        if (dummyVoteEvent.IsCancelled)
        {
            dummy.voted = false;
            yield break;
        }

        List<byte> potentialSuspects = new();
        potentialSuspects.AddRange(PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => p != dummy.myPlayer)
            .Where(p => dummyVoteEvent.PlayerIsValid(p))
            .Select(p => p.PlayerId));

        if (dummyVoteEvent.CanSkip)
        {
            potentialSuspects.Add(253);
        }

        if (dummy.myPlayer.GetVoteData().VotesRemaining > 0)
        {
            VotingUtils.RpcCastVote(PlayerControl.LocalPlayer, dummy.myPlayer.PlayerId, potentialSuspects.Random());
        }
    }

    private static bool DidVote(this DummyBehaviour dummy)
    {
        var dummyVoteData = dummy.myPlayer.GetVoteData();
        return dummyVoteData.VotesRemaining > 0;
    }
}
