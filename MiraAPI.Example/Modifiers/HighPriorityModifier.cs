using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example.Modifiers;

public class HighPriorityModifier : GameModifier
{
    public override string ModifierName => "High Priority";

    public override int Priority()
    {
        return 100;
    }
}
