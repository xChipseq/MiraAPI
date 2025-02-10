using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example.Modifiers;

public class CaptainModifier : GameModifier
{
    public override string ModifierName => "Captain";

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override int GetAssignmentChance()
    {
        return 100;
    }
}
