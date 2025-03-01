using System;

namespace MiraAPI.GameOptions;

/// <summary>
/// Interface for groups that have some type of "Optionable" class.
/// </summary>
public interface IOptionableGroup
{
    /// <summary>
    /// Gets the type of the Optionable class.
    /// </summary>
    Type OptionableType { get; }
}
