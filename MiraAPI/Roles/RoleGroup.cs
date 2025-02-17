using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Represents a group of roles.
/// </summary>
/// <param name="Name">The group's name.</param>
/// <param name="Color">The group's color.</param>
/// <param name="Priority">The priority to sort by on role setting screen. Negative means first.</param>
public record struct RoleGroup(string Name, Color Color, int Priority = 0)
{
    public static RoleGroup Crewmate { get; } = new("Crewmate", Palette.CrewmateBlue, -2);
    public static RoleGroup Impostor { get; } = new("Impostor", Palette.ImpostorRed, -1);
    public static RoleGroup Neutral { get; } = new("Neutral Roles", Color.gray);
}
