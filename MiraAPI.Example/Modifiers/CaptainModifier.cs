using MiraAPI.Example.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example.Modifiers;

public class CaptainModifier : GameModifier
{
    public override string ModifierName => "Captain";

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CaptainModifierSettings>.Instance.Chance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CaptainModifierSettings>.Instance.Amount;
    }
}
