using System;
using System.Collections.Generic;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace MiraAPI.Modifiers;

/// <summary>
/// Extension methods for modifiers.
/// </summary>
public static class ModifierExtensions
{
    internal static Dictionary<PlayerControl, ModifierComponent> ModifierComponents { get; } = [];

    /// <summary>
    /// Gets the ModifierComponent for a player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <returns>A ModifierComponent if there is one, null otherwise.</returns>
    public static ModifierComponent? GetModifierComponent(this PlayerControl player)
    {
        if (ModifierComponents.TryGetValue(player, out var component))
        {
            return component;
        }

        component = player.GetComponent<ModifierComponent>();
        if (component == null)
        {
            return null;
        }

        ModifierComponents[player] = component;
        return component;
    }

    /// <summary>
    /// Gets a modifier by its type, or null if the player doesn't have it.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>The Modifier if it is found, null otherwise.</returns>
    public static T? GetModifier<T>(this PlayerControl? player) where T : BaseModifier
    {
        return player?.GetModifierComponent()?.ActiveModifiers.Find(x => x is T) as T;
    }

    /// <summary>
    /// Checks if a player has a modifier.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    public static bool HasModifier<T>(this PlayerControl? player) where T : BaseModifier
    {
        return player?.GetModifierComponent() != null &&
               player.GetModifierComponent()!.HasModifier<T>();
    }

    /// <summary>
    /// Checks if a player has a modifier by its ID.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="id">The Modifier ID.</param>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    public static bool HasModifier(this PlayerControl? player, uint id)
    {
        return player?.GetModifierComponent() != null &&
               player.GetModifierComponent()!.HasModifier(id);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="target">The player to remove the modifier from.</param>
    /// <param name="modifierId">The ID of the modifier.</param>
    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(this PlayerControl target, uint modifierId)
    {
        target.GetModifierComponent()?.RemoveModifier(modifierId);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="player">The player to remove the modifier from.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    public static void RpcRemoveModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        var id = ModifierManager.GetModifierId(typeof(T));

        if (id == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }

        player.RpcRemoveModifier(id.Value);
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="target">The player to add the modifier to.</param>
    /// <param name="modifierId">The modifier ID.</param>
    [MethodRpc((uint)MiraRpc.AddModifier)]
    public static void RpcAddModifier(this PlayerControl target, uint modifierId, params object[] args)
    {
        var type = ModifierManager.GetModifierType(modifierId);
        if (type == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier with id {modifierId} because it is not registered.");
            return;
        }

        var modifier = ModifierFactory.CreateInstance(type, args);
        target.GetModifierComponent()?.AddModifier(modifier);
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="player">The player to add the modifier to.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <typeparam name="T">The modifier Type.</typeparam>
    public static void RpcAddModifier<T>(this PlayerControl player, params object[] args) where T : BaseModifier
    {
        var id = ModifierManager.GetModifierId(typeof(T));
        if (id == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }

        var modifier = ModifierFactory<T>.CreateInstance(args);
        player.GetModifierComponent()!.AddModifier(modifier);
    }
}
