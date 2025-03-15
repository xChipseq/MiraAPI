using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Utilities to make handling roles in-game easier.
/// </summary>
public static class CustomRoleUtils
{
    /// <summary>
    /// Gets all active in-game roles.
    /// </summary>
    /// <returns>A list of roles.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<RoleBehaviour> GetActiveRoles() => PlayerControl.AllPlayerControls.ToArray().Select(x => x.Data.Role);

    /// <summary>
    /// Gets all active in-game roles in a certain team.
    /// </summary>
    /// <param name="team">The team you would like to check for.</param>
    /// <returns>A list of roles with the team.</returns>
    public static IEnumerable<RoleBehaviour> GetActiveRolesOfTeam(ModdedRoleTeams team) => GetActiveRoles().Where(x => x is ICustomRole customRole && customRole.Team == team);

    /// <summary>
    /// Gets all active in-game roles of a certain type.
    /// </summary>
    /// <typeparam name="T">The role Type you would like to check for. Must be a RoleBehaviour.</typeparam>
    /// <returns>A list of roles with that specific type.</returns>
    public static IEnumerable<T> GetActiveRolesOfType<T>() where T : RoleBehaviour => GetActiveRoles().OfType<T>();

    /// <summary>
    /// Creates a string builder for the Role Tab.
    /// </summary>
    /// <param name="role">The ICustomRole object.</param>
    /// <returns>A StringBuilder.</returns>
    public static StringBuilder CreateForRole(ICustomRole role)
    {
        var taskStringBuilder = new StringBuilder();
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleColor.ToTextColor()}You are a <b>{role.RoleName}.</b></color>");
        taskStringBuilder.Append("<size=70%>");
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");
        return taskStringBuilder;
    }
}
