using System;

namespace MiraAPI.Modifiers;

/// <summary>
/// Modifier data for networking.
/// </summary>
/// <param name="type">The Type of the modifier.</param>
/// <param name="args">Parameters for constructor.</param>
public readonly struct ModifierData(Type type, object[] args)
{
    /// <summary>
    /// Gets the type of the modifier.
    /// </summary>
    public Type Type { get; } = type;

    /// <summary>
    /// Gets the parameters for the constructor.
    /// </summary>
    public object[] Args { get; } = args;
}
