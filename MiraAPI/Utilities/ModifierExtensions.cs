using System;
using System.Collections.Generic;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace MiraAPI.Utilities;

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
    [MethodRpc((uint)MiraRpc.AddModifier)]
    public static void RpcAddModifier(this PlayerControl target, uint modifierId)
    {
        var type = ModifierManager.GetModifierType(modifierId);
        if (type == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier with id {modifierId} because it is not registered.");
            return;
        }

        target.AddModifier(type);
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="player">The player to add the modifier to.</param>
    /// <typeparam name="T">The modifier Type.</typeparam>
    public static void RpcAddModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        var id = ModifierManager.GetModifierId(typeof(T));
        if (id == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }

        player.RpcAddModifier(id.Value);
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
        if (!component)
        {
            return null;
        }

        ModifierComponents[player] = component;
        return component;
    }

    /// <summary>
    /// Gets the ModifierComponent for a player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="component">The ModifierComponent if it is found.</param>
    /// <returns>A ModifierComponent if there is one, null otherwise.</returns>
    public static bool TryGetModifierComponent(this PlayerControl player, out ModifierComponent? component)
    {
        if (ModifierComponents.TryGetValue(player, out component))
        {
            return component;
        }

        component = player.GetComponent<ModifierComponent>();
        return component;
    }

    /// <summary>
    /// Gets a collection of modifiers by their type, or null if the player doesn't have one.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>The Modifier if it is found, null otherwise.</returns>
    public static IEnumerable<T> GetModifiersByType<T>(this PlayerControl player) where T : BaseModifier
    {
        return player.GetModifierComponent()!.GetModifiersByType<T>();
    }

    /// <summary>
    /// Gets a modifier by its type, or null if the player doesn't have it.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>The Modifier if it is found, null otherwise.</returns>
    public static T? GetModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        return player.GetModifierComponent()!.GetModifier<T>();
    }

    /// <summary>
    /// Tries to get a modifier by its type.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    public static bool TryGetModifier<T>(this PlayerControl player, out T? modifier) where T : BaseModifier
    {
        return player.GetModifierComponent()!.TryGetModifier(out modifier);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="type">The modifier type.</param>
    public static void RemoveModifier(this PlayerControl player, Type type)
    {
        player.GetModifierComponent()!.RemoveModifier(type);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The modifier type.</typeparam>
    public static void RemoveModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        player.GetModifierComponent()!.RemoveModifier<T>();
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="modifierId">The modifier ID.</param>
    public static void RemoveModifier(this PlayerControl player, uint modifierId)
    {
        player.GetModifierComponent()!.RemoveModifier(modifierId);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="modifier">The modifier object.</param>
    public static void RemoveModifier(this PlayerControl player, BaseModifier modifier)
    {
        player.GetModifierComponent()!.RemoveModifier(modifier);
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The modifier that was added.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, BaseModifier modifier)
    {
        return player.GetModifierComponent()!.AddModifier(modifier);
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="type">The modifier type.</param>
    /// <returns>The modifier that was added.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, Type type)
    {
        return player.GetModifierComponent()!.AddModifier(type);
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The Type of the modifier.</typeparam>
    /// <returns>The new modifier.</returns>
    public static T? AddModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        return player.GetModifierComponent()!.AddModifier<T>();
    }

    /// <summary>
    /// Checks if a player has an active or queued modifier by its ID.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="id">The Modifier ID.</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, uint id, Func<BaseModifier, bool>? predicate=null)
    {
        return player.GetModifierComponent()!.HasModifier(id, predicate);
    }

    /// <summary>
    /// Checks if a player has an active or queued modifier by its type.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    public static bool HasModifier<T>(this PlayerControl player, Func<T, bool>? predicate=null) where T : BaseModifier
    {
        return player.GetModifierComponent()!.HasModifier(predicate);
    }
}
