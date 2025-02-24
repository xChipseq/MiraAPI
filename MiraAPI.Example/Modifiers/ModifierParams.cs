using MiraAPI.Modifiers;
using Reactor.Utilities;

namespace MiraAPI.Example.Modifiers;

public class ModifierParams : BaseModifier
{
    public override string ModifierName => "ModifierParams";

    public ModifierParams(string param1, int param2, PlayerControl playerControl)
    {
        Logger<ExamplePlugin>.Error($"Param1: {param1}, Param2: {param2}");
        Logger<ExamplePlugin>.Error($"Player: {playerControl.PlayerId}");
    }
}
