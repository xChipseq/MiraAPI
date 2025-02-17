using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class NeutralKillerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Outcast Killer";
    public string RoleDescription => "Outcast who can kill.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => Color.magenta;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        CanGetKilled = true,
        CanUseVent = true,
        RoleGroup = new RoleGroup("Outcast", Color.gray),
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // remove existing task header.
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return GameManager.Instance.DidHumansWin(gameOverReason);
    }
}
