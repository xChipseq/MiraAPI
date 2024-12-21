using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example.Modifiers;

[RegisterModifier]
public class HighPriorityModifier : GameModifier
{
    public override string ModifierName => "High Priority";
    public override int GetAssignmentChance()
    {
        return 100;
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override int Priority()
    {
        return 100;
    }
}
