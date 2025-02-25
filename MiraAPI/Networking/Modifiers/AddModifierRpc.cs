using System;
using Hazel;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Networking.Serialization;

namespace MiraAPI.Networking.Modifiers;

/// <summary>
/// Remote procedure call for adding a modifier.
/// </summary>
/// <param name="plugin">Mira plugin.</param>
/// <param name="id">RPC ID.</param>
[RegisterCustomRpc((uint)MiraRpc.AddModifier)]
public class AddModifierRpc(MiraApiPlugin plugin, uint id) : PlayerCustomRpc<MiraApiPlugin, ModifierData>(plugin, id)
{
    /// <inheritdoc />
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    /// <inheritdoc />
    public override void Write(MessageWriter writer, ModifierData data)
    {
        writer.WritePacked(data.Id);
        writer.WritePacked(data.Args.Length);
        foreach (var arg in data.Args)
        {
            writer.Write(arg.GetType().AssemblyQualifiedName);
            writer.Serialize(arg);
        }
    }

    /// <inheritdoc />
    public override ModifierData Read(MessageReader reader)
    {
        var modId = reader.ReadPackedUInt32();
        var argCount = reader.ReadPackedUInt32();
        var objects = new object[argCount];

        if (argCount > 0)
        {
            var types = new Type[argCount];
            for (var i = 0; i < argCount; i++)
            {
                var name = reader.ReadString();
                types[i] = Type.GetType(name) ?? throw new InvalidOperationException($"Type not found: {name}");
                objects[i] = reader.Deserialize(types[i]);
            }
        }

        return new ModifierData(modId, objects);
    }

    /// <inheritdoc />
    public override void Handle(PlayerControl player, ModifierData data)
    {
        player.AddModifier(data.Id, data.Args);
    }
}
