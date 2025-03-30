using BepInEx.Unity.IL2CPP;

namespace MiraAPI;

/// <summary>
/// Mod compatibility tools.
/// </summary>
public static class ModCompatibility
{
    /// <summary>
    /// The ID for the Submerged mod.
    /// </summary>
    public const string SubmergedId = "Submerged";

    /// <summary>
    /// Gets a value indicating whether the Submerged mod is loaded.
    /// </summary>
    public static bool SubmergedLoaded => IL2CPPChainloader.Instance.Plugins.TryGetValue(SubmergedId, out _);
}
