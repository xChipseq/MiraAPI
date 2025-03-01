using MiraAPI.Example.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example.Options.Modifiers;

public class CaptainModifierSettings : AbstractOptionGroup<CaptainModifier>
{
    public override string GroupName => "Captain Settings";

    [ModdedNumberOption("Uses", 1, 5)]
    public float NumUses { get; set; } = 3;
}
