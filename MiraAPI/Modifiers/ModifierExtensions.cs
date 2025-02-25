using System;
using System.Collections.Generic;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace MiraAPI.Modifiers;

/// <summary>
/// Extensions for modifiers.
/// </summary>
public static class ModifierExtensions
{
    /// <summary>
    /// Gets the dictionary to cache ModifierComponents.
    /// </summary>
    public static Dictionary<PlayerControl, ModifierComponent> ModifierComponents { get; } = [];

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="target">The player to add the modifier to.</param>
    /// <param name="modifierId">The modifier ID.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    public static void RpcAddModifier(this PlayerControl target, uint modifierId, params object[] args)
    {
        _ = ModifierManager.GetModifierType(modifierId) ?? throw new InvalidOperationException(
            $"Modifier with ID {modifierId} is not registered.");

        Rpc<AddModifierRpc>.Instance.Send(target, new ModifierData(modifierId, args));
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="player">The player to add the modifier to.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    public static void RpcAddModifier(this PlayerControl player, Type type, params object[] args)
    {
        var id = ModifierManager.GetModifierId(type) ?? throw new InvalidOperationException(
            $"Modifier {type.Name} is not registered.");

        player.RpcAddModifier(id, args);
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="player">The player to add the modifier to.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <typeparam name="T">The modifier Type.</typeparam>
    public static void RpcAddModifier<T>(this PlayerControl player, params object[] args) where T : BaseModifier
    {
        player.RpcAddModifier(typeof(T), args);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="target">The player to remove the modifier from.</param>
    /// <param name="modifierId">The ID of the modifier.</param>
    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(this PlayerControl target, uint modifierId)
    {
        target.RemoveModifier(modifierId);
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
    /// Checks if the player has a specific modifier.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>True if the player has the modifier, false otherwise.</returns>
    public static bool HasModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent()!.HasModifier(predicate);
    }

    /// <summary>
    /// Checks if the player has a specific modifier by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>True if the player has the modifier, false otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, Type type, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent()!.HasModifier(type, predicate);
    }

    /// <summary>
    /// Checks if the player has a specific modifier by ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="id">The ID of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>True if the player has the modifier, false otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, uint id, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent()!.HasModifier(id, predicate);
    }

    /// <summary>
    /// Gets a specific modifier from the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>The modifier if found, null otherwise.</returns>
    public static T? GetModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent()!.GetModifier(predicate);
    }

    /// <summary>
    /// Gets a specific modifier from the player by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>The modifier if found, null otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent()!.GetModifier(type, predicate);
    }

    /// <summary>
    /// Gets a specific modifier from the player by ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="id">The ID of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>The modifier if found, null otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        uint id,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent()!.GetModifier(id, predicate);
    }

    /// <summary>
    /// Gets all modifiers of a specific type from the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifiers.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>An enumerable of modifiers.</returns>
    public static IEnumerable<T> GetModifiersByType<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : BaseModifier
    {
        return player.GetModifierComponent()!.GetModifiersByType(predicate);
    }

    /// <summary>
    /// Gets all modifiers of a specific type from the player.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifiers.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>An enumerable of modifiers.</returns>
    public static IEnumerable<BaseModifier> GetModifiersByType(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent()!.GetModifiersByType(type, predicate);
    }

    /// <summary>
    /// Gets all modifiers of a specific type from the player by ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="id">The ID of the modifiers.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    /// <returns>An enumerable of modifiers.</returns>
    public static IEnumerable<BaseModifier> GetModifiersByType(
        this PlayerControl player,
        uint id,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent()!.GetModifiersByType(id, predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RemoveModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : BaseModifier
    {
        player.GetModifierComponent()!.RemoveModifier(predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RemoveModifier(this PlayerControl player, Type type, Func<BaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent()!.RemoveModifier(type, predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player by ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="modifierId">The ID of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the modifiers.</param>
    public static void RemoveModifier(
        this PlayerControl player,
        uint modifierId,
        Func<BaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent()!.RemoveModifier(modifierId, predicate);
    }

    /// <summary>
    /// Removes a specific modifier from the player.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="modifier">The modifier to remove.</param>
    public static void RemoveModifier(this PlayerControl player, BaseModifier modifier)
    {
        player.GetModifierComponent()!.RemoveModifier(modifier);
    }

    /// <summary>
    /// Adds a specific modifier to the player.
    /// </summary>
    /// <typeparam name="T">The type of the modifier.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The added modifier.</returns>
    public static T? AddModifier<T>(this PlayerControl player, params object[] args) where T : BaseModifier
    {
        return player.GetModifierComponent()!.AddModifier<T>(args);
    }

    /// <summary>
    /// Adds a specific modifier to the player.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The added modifier.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, BaseModifier modifier)
    {
        return player.GetModifierComponent()!.AddModifier(modifier);
    }

    /// <summary>
    /// Adds a specific modifier to the player by type.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The added modifier.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, Type type, params object[] args)
    {
        return player.GetModifierComponent()!.AddModifier(type, args);
    }

    /// <summary>
    /// Adds a specific modifier to the player by ID.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="id">The ID of the modifier.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The added modifier.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, uint id, params object[] args)
    {
        return player.GetModifierComponent()!.AddModifier(id, args);
    }
}
