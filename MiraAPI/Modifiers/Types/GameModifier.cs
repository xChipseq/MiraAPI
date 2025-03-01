using System;
using BepInEx.Configuration;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using Reactor.Utilities;

namespace MiraAPI.Modifiers.Types;

/// <summary>
/// The base class for a game modifier. Game modifiers are applied at the start of the game on top of the player's role.
/// </summary>
public abstract class GameModifier : BaseModifier
{
    /// <inheritdoc />
    public override bool ShowInFreeplay => true;

    /// <summary>
    /// Gets the assignment configuration for the modifier.
    /// </summary>
    public virtual AssignmentConfiguration AssignmentConfiguration => new(0, 0, true, true);

    internal ConfigDefinition AmountDefinition => new("Modifiers", $"Amount {GetType().FullName}");

    internal ConfigDefinition ChanceDefinition => new("Modifiers", $"Chance {GetType().FullName}");

    internal ModdedNumberOption AmountOption { get; } = new("Amount", 0, 0, 15, 1, MiraNumberSuffixes.None);

    internal ModdedNumberOption ChanceOption { get; } = new("Chance", 0, 0, 100, 1, MiraNumberSuffixes.Percent);

    /// <summary>
    /// Gets the chance of the modifier being assigned to a player.
    /// </summary>
    /// <returns>An int value between 0 and 100 representing percent.</returns>
    public virtual int GetAssignmentChance()
    {
        return AssignmentConfiguration.CreateChanceOption &&
               ParentMod.PluginConfig.TryGetEntry(ChanceDefinition, out ConfigEntry<int> entry)
            ? entry.Value
            : AssignmentConfiguration.DefaultChance;
    }

    /// <summary>
    /// Gets the amount of players that can have this modifier in a game.
    /// </summary>
    /// <returns>An int value greater than or equal to zero.</returns>
    public virtual int GetAmountPerGame()
    {
        return AssignmentConfiguration.CreateAmountOption &&
               ParentMod.PluginConfig.TryGetEntry(AmountDefinition, out ConfigEntry<int> entry)
            ? entry.Value
            : AssignmentConfiguration.DefaultAmount;
    }

    /// <summary>
    /// Sets the chance of the modifier being assigned to a player.
    /// </summary>
    /// <param name="chance">The chance between 0 and 100.</param>
    public virtual void SetAssignmentChance(int chance)
    {
        if (ParentMod.PluginConfig.TryGetEntry(ChanceDefinition, out ConfigEntry<int> entry))
        {
            entry.Value = Math.Clamp(chance, 0, 100);
            return;
        }

        Logger<MiraApiPlugin>.Error($"Error getting chance configuration for modifier: {ModifierName}");
    }

    /// <summary>
    /// Sets the amount of players that can have this modifier in a game.
    /// </summary>
    /// <param name="amount">The amount of players between zero and the max player count.</param>
    public virtual void SetAmountPerGame(int amount)
    {
        if (ParentMod.PluginConfig.TryGetEntry(AmountDefinition, out ConfigEntry<int> entry))
        {
            entry.Value = Math.Max(amount, 0);
            return;
        }

        Logger<MiraApiPlugin>.Error($"Error getting amount configuration for modifier: {ModifierName}");
    }

    /// <summary>
    /// Gets the priority at which the modifier will spawn. The higher the value, the higher up on the assignment list.
    /// </summary>
    /// <returns>An int value greater than or equal to -1.</returns>
    public virtual int Priority() => -1;

    /// <summary>
    /// Determines whether the modifier is valid on a role.
    /// </summary>
    /// <param name="role">The role to be checked.</param>
    /// <returns>True if the modifier is valid on the role, otherwise false.</returns>
    public virtual bool IsModifierValidOn(RoleBehaviour role) => true;

    /// <summary>
    /// Determines whether the player won the game with this modifier.
    /// </summary>
    /// <param name="reason">The reason why the game ended.</param>
    /// <returns>True if the player won, false if they lost. Return null to use the player's default win condition.</returns>
    public virtual bool? DidWin(GameOverReason reason) => null;
}
