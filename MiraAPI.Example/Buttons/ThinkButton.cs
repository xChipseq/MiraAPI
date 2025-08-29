using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Buttons;

public class ThinkButton : CustomActionButton
{
    public override string Name => "Think";
    public override float Cooldown => 10f;
    public override float EffectDuration => 10f;
    public override bool EffectCancelable => true;
    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;
    protected override void OnClick()
    {
        // This button does absolutely nothing besides changing its text
        Button!.OverrideText("Thinking...");
    }

    public override void OnEffectEnd()
    {
        Button!.OverrideText("Think");
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role != null && role.IsImpostor;
    }
}
