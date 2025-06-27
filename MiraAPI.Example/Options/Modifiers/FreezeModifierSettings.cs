using MiraAPI.Example.Modifiers.Freezer;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example.Options.Modifiers;

public class FreezeModifierSettings : AbstractOptionGroup<FreezeModifier>
{
    public override string GroupName => "Freeze";

    [ModdedToggleOption("Use Color")]
    public bool UseColor { get; set; } = true;
}
