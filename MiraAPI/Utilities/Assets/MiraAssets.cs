using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A static class that contains various assets used in the Mira API.
/// </summary>
public static class MiraAssets
{
    /// <summary>
    /// Gets the color used for teal highlighting in UI.
    /// </summary>
    public static Color32 AcceptedTeal { get; } = new(43, 233, 198, 255);

    /// <summary>
    /// Gets the Mira API asset bundle.
    /// </summary>
    public static AssetBundle MiraAssetBundle { get; } = AssetBundleManager.Load("mirabundle");

    /// <summary>
    /// Gets the ModifierDisplay prefab.
    /// </summary>
    public static LoadableAsset<GameObject> ModifierDisplay { get; } = new LoadableBundleAsset<GameObject>("Modifiers", MiraAssetBundle);

    /// <summary>
    /// Gets the empty sprite asset.
    /// </summary>
    public static LoadableResourceAsset Empty { get; } = new("MiraAPI.Resources.Empty.png");

    /// <summary>
    /// Gets the Next Button sprite.
    /// </summary>
    public static LoadableResourceAsset NextButton { get; } = new("MiraAPI.Resources.NextButton.png");

    /// <summary>
    /// Gets the highlighted Next Button sprite.
    /// </summary>
    public static LoadableResourceAsset NextButtonActive { get; } = new("MiraAPI.Resources.NextButtonActive.png");

    /// <summary>
    /// Gets the Cog icon used in Role Settings Menu.
    /// </summary>
    public static LoadableResourceAsset Cog { get; } = new("MiraAPI.Resources.Cog.png");

    /// <summary>
    /// Gets the Checkmark Box used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset CheckmarkBox { get; } = new("MiraAPI.Resources.CheckMarkBox.png");

    /// <summary>
    /// Gets the Checkmark used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset Checkmark { get; } = new("MiraAPI.Resources.Checkmark.png");

    /// <summary>
    /// Gets the white CategoryHeader used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset CategoryHeader { get; } = new("MiraAPI.Resources.CategoryHeader.png");
}
