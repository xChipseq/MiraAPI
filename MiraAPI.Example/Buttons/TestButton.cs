using MiraAPI.Example.Modifiers;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Example.Buttons;

public class TestButton : CustomActionButton
{
    public override string Name => "Test Button";
    public override float Cooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;

    protected override void OnClick()
    {
        var randomPlayer = PlayerControl.AllPlayerControls.ToArray().Random();
        PlayerControl.LocalPlayer.RpcAddModifier<ModifierParams>("test", 1, randomPlayer);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return true;
    }
}
