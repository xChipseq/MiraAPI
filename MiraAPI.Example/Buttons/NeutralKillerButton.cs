using MiraAPI.Example.GameOver;
using MiraAPI.Example.Roles;
using MiraAPI.GameEnd;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Rewired;
using UnityEngine;

namespace MiraAPI.Example.Buttons;

public class NeutralKillerButton : CustomActionButton
{
    public override string Name => "Win Game";
    public override float Cooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;
    public override KeyboardKeyCode CurrentKeybind => KeyboardKeyCode.K;
    protected override void OnClick()
    {
        CustomGameOver.Trigger<NeutralKillerGameOver>([PlayerControl.LocalPlayer.Data]);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is NeutralKillerRole;
    }
}
