using AmongUs.GameOptions;
using MiraAPI.Roles;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class ChameloenRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Chamelon";
    public string RoleLongDescription => "Stay invisible while not moving.";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.AcceptedGreen;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = ExampleAssets.Banner,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Shapeshifter),
    };

    public override void Initialize(PlayerControl player)
    {
        Player = player;
        Logger<ExamplePlugin>.Info("Initializing ChamelonRole for player: " + player.PlayerId);
    }

    public void FixedUpdate()
    {
        if (!Player)
        {
            return;
        }

        if (Player.MyPhysics.Velocity.magnitude > 0)
        {
            var rend = Player.cosmetics.currentBodySprite.BodySprite;
            var tmp = Player.cosmetics.nameText;
            tmp.color = Color.Lerp(tmp.color, new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1), Time.deltaTime * 4f);
            rend.color = Color.Lerp(rend.color, new Color(1, 1, 1, 1), Time.deltaTime * 4f);

            foreach (var cosmetic in Player.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                cosmetic.color = Color.Lerp(cosmetic.color, new Color(1, 1, 1, 1), Time.deltaTime * 4f);
            }
        }
        else
        {
            SpriteRenderer rend = Player.cosmetics.currentBodySprite.BodySprite;
            TextMeshPro tmp = Player.cosmetics.nameText;
            tmp.color = Color.Lerp(tmp.color, new Color(tmp.color.r, tmp.color.g, tmp.color.b, Player.AmOwner ? 0.3f : 0), Time.deltaTime * 4f);
            rend.color = Color.Lerp(rend.color, new Color(1, 1, 1, Player.AmOwner ? 0.3f : 0), Time.deltaTime * 4f);

            foreach (var cosmetic in Player.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                cosmetic.color = Color.Lerp(cosmetic.color, new Color(1, 1, 1, Player.AmOwner ? 0.3f : 0), Time.deltaTime * 4f);
            }
        }
    }
}
