using System.Linq;

namespace MiraAPI.GameOptions;

internal class ModifierOptionGroup : AbstractOptionGroup
{
    public override string GroupName { get; }

    public ModifierOptionGroup(string name, IModdedOption[] options, params AbstractOptionGroup[] others)
    {
        GroupName = name;
        Options.AddRange(options);
        Options.AddRange(others.SelectMany(x=>x.Options));
    }
}
