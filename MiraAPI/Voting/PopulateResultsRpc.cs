using System.Linq;
using Hazel;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace MiraAPI.Voting;

/// <summary>
/// Handles the networking for populating results of a meeting. Custom implementation to work with Mira voting API.
/// </summary>
[RegisterCustomRpc((uint)MiraRpc.PopulateResults)]
public class PopulateResultsRpc(MiraApiPlugin plugin, uint id) : PlayerCustomRpc<MiraApiPlugin, CustomVote[]>(plugin, id)
{
    /// <inheritdoc />
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    /// <inheritdoc />
    public override void Write(MessageWriter writer, CustomVote[]? data)
    {
        if (data == null)
        {
            writer.WritePacked(0);
            return;
        }

        writer.WritePacked(data.Length);
        foreach (var t in data)
        {
            writer.Write(t.Voter);
            writer.Write(t.Suspect);
        }
    }

    /// <inheritdoc />
    public override CustomVote[] Read(MessageReader reader)
    {
        var votes = new CustomVote[reader.ReadPackedInt32()];

        for (var i = 0; i < votes.Length; i++)
        {
            votes[i] = new CustomVote(reader.ReadByte(), reader.ReadByte());
        }

        return votes;
    }

    /// <inheritdoc />
    public override void Handle(PlayerControl innerNetObject, CustomVote[]? data)
    {
        if (AmongUsClient.Instance.HostId != innerNetObject.OwnerId)
        {
            return;
        }

        if (data == null)
        {
            return;
        }

        VotingUtils.HandlePopulateResults([.. data]);
    }
}
