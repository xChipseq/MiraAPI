using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hazel;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Networking.Serialization;

namespace MiraAPI.Networking;

/// <summary>
/// Remote procedure call for adding a modifier.
/// </summary>
/// <param name="plugin">Mira plugin.</param>
/// <param name="id">RPC ID.</param>
[RegisterCustomRpc((uint)MiraRpc.AddModifier)]
public class AddModifierRpc(MiraApiPlugin plugin, uint id) : PlayerCustomRpc<MiraApiPlugin, ModifierData>(plugin, id)
{
    private static readonly Dictionary<Type, ParameterInfo[]> ParameterCache = [];

    /// <inheritdoc />
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    /// <inheritdoc />
    public override void Write(MessageWriter writer, ModifierData data)
    {
        var modId = ModifierManager.GetModifierId(data.Type);
        writer.WritePacked((uint)modId!);
        MessageSerializer.Serialize(writer, data.Args);
    }

    /// <inheritdoc />
    public override ModifierData Read(MessageReader reader)
    {
        var modId = reader.ReadPackedUInt32();
        var modifier = ModifierManager.GetModifierType(modId)!;

        if (!ParameterCache.TryGetValue(modifier, out var paramTypes))
        {
            paramTypes = modifier.GetConstructors().OrderBy(x => x.GetParameters().Length).First().GetParameters();
            ParameterCache[modifier] = paramTypes;
        }

        var objects = new object[paramTypes.Length];
        foreach (var paramType in paramTypes)
        {
            objects[paramType.Position] = reader.Deserialize(paramType.ParameterType);
        }

        return new ModifierData(modifier, objects);
    }

    /// <inheritdoc />
    public override void Handle(PlayerControl innerNetObject, ModifierData data)
    {
        var modifier = ModifierFactory.CreateInstance(data.Type, data.Args);
        innerNetObject.GetModifierComponent()?.AddModifier(modifier);
    }
}
