using AmongUs.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Used to configure the specific settings of a role.
/// </summary>
public struct CustomRoleConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRoleConfiguration"/> struct.
    /// </summary>
    /// <param name="role">The role in which you are configuring.</param>
    public CustomRoleConfiguration(ICustomRole role)
    {
        var roleBehaviour = role as RoleBehaviour;

        MaxRoleCount = 15;
        DefaultRoleCount = 0;
        DefaultChance = 0;

        OptionsScreenshot = Icon = MiraAssets.Empty;
        AffectedByLightOnAirship = role.Team == ModdedRoleTeams.Crewmate;
        KillButtonOutlineColor = role.Team switch
        {
            ModdedRoleTeams.Impostor => Palette.ImpostorRed,
            ModdedRoleTeams.Crewmate => Palette.CrewmateBlue,
            _ => role.RoleColor,
        };
        RoleHintType = RoleHintType.RoleTab;
        GhostRole = role.Team is ModdedRoleTeams.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
        CanGetKilled = roleBehaviour?.IsDead == false && role.Team is not ModdedRoleTeams.Impostor;
        UseVanillaKillButton = role.Team is ModdedRoleTeams.Impostor;
        CanUseVent = role.Team is ModdedRoleTeams.Impostor;
        CanUseSabotage = role.Team is ModdedRoleTeams.Impostor;
        TasksCountForProgress = role.Team is ModdedRoleTeams.Crewmate;
        HideSettings = roleBehaviour?.IsDead == true;
        CanModifyChance = true;
        IntroTeamColor = role.Team switch
        {
            ModdedRoleTeams.Custom => Color.gray,
            _ => null,
        };
        IntroTeamTitle = role.Team switch
        {
            ModdedRoleTeams.Custom => "NEUTRAL",
            _ => null,
        };
        IntroTeamDescription = role.Team switch
        {
            ModdedRoleTeams.Custom => "You are Neutral. You do not have a team.",
            _ => null,
        };
        RoleGroup = role.Team switch
        {
            ModdedRoleTeams.Crewmate => RoleGroup.Crewmate,
            ModdedRoleTeams.Impostor => RoleGroup.Impostor,
            ModdedRoleTeams.Custom => RoleGroup.Neutral,
        };
    }

    public RoleGroup RoleGroup;

    /// <summary>
    /// Gets the hard limit of players that can have this role. This property is used to set a limit in the Role Options menu. If set to 0, the role will not be assigned at start.
    /// </summary>
    public int MaxRoleCount;

    /// <summary>
    /// Gets the default role count.
    /// </summary>
    public int DefaultRoleCount;

    /// <summary>
    /// Gets the default role chance.
    /// </summary>
    public int DefaultChance;

    /// <summary>
    /// Whether the chance option can be changed or not.
    /// </summary>
    public bool CanModifyChance;

    /// <summary>
    /// Gets the Sprite used for the Role Options menu screenshot.
    /// </summary>
    public LoadableAsset<Sprite> OptionsScreenshot;

    /// <summary>
    /// Gets the Sprite used for the Role Icon.
    /// </summary>
    public LoadableAsset<Sprite> Icon;

    /// <summary>
    /// Gets a value indicating whether the role is affected by light affectors on Airship.
    /// </summary>
    public bool AffectedByLightOnAirship;

    /// <summary>
    /// Gets a value indicating whether the role can be killed by vanilla murder system.
    /// </summary>
    public bool CanGetKilled;

    /// <summary>
    /// Gets a value indicating whether the role should use the vanilla kill button.
    /// </summary>
    public bool UseVanillaKillButton;

    /// <summary>
    /// Gets a value indicating whether the role can use vents.
    /// </summary>
    public bool CanUseVent;

    /// <summary>
    /// Gets a value indicating whether the role can use the sabotage button.
    /// </summary>
    public bool CanUseSabotage;

    /// <summary>
    /// Gets a value indicating whether the role's tasks count towards task progress.
    /// </summary>
    public bool TasksCountForProgress;

    /// <summary>
    /// Gets a value indicating whether the role should show up in the Role Options menu.
    /// </summary>
    public bool HideSettings;

    /// <summary>
    /// Gets the color to show during the "Team" part of the intro cutscene.
    /// </summary>
    public Color? IntroTeamColor;

    /// <summary>
    /// Gets the title text to show during the "Team" part of the intro cutscene.
    /// </summary>
    public string? IntroTeamTitle;

    /// <summary>
    /// Gets the description text to show during the "Team" part of the intro cutscene.
    /// </summary>
    public string? IntroTeamDescription;

    /// <summary>
    /// Gets the outline color for the KillButton if <see cref="UseVanillaKillButton"/> is true.
    /// </summary>
    public Color KillButtonOutlineColor;

    /// <summary>
    /// Gets the role hint style. See <see cref="RoleHintType"/> enum for all options.
    /// </summary>
    public RoleHintType RoleHintType;

    /// <summary>
    /// Gets the Ghost role that is applied when the player is killed.
    /// </summary>
    public RoleTypes GhostRole;
}
