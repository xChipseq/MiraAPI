namespace MiraAPI.Modifiers;

/// <summary>
/// Modifier data for networking.
/// </summary>
/// <param name="id">The ID of the modifier.</param>
/// <param name="args">Parameters for constructor.</param>
public readonly struct ModifierData(uint id, object[] args)
{
    /// <summary>
    /// Gets the type of the modifier.
    /// </summary>
    public uint Id { get; } = id;

    /// <summary>
    /// Gets the parameters for the constructor.
    /// </summary>
    public object[] Args { get; } = args;
}
