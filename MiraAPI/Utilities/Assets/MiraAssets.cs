using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// Various assets used throughout Mira API.
/// </summary>
public static class MiraAssets
{
    static MiraAssets()
    {
        var boxTex = SpriteTools.LoadTextureFromResourcePath(
            "MiraAPI.Resources.RoundedBox.png",
            System.Reflection.Assembly.GetCallingAssembly());
        var boxSprite = Sprite.Create(
            boxTex,
            new Rect(0, 0, boxTex.width, boxTex.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0U,
            SpriteMeshType.Tight,
            new Vector4(20, 20, 20, 20));

        RoundedBox = new LoadableAssetWrapper<Sprite>(boxSprite);
    }

    /// <summary>
    /// Gets the accepted teal color used for various selections in the UI.
    /// </summary>
    public static Color32 AcceptedTeal { get; } = new(43, 233, 198, 255);

    /// <summary>
    /// Gets the empty sprite.
    /// </summary>
    public static LoadableAsset<Sprite> Empty { get; } = new LoadableResourceAsset("MiraAPI.Resources.Empty.png");

    /// <summary>
    /// Gets the RoundedBox sprite, which is a rounded rectangle used for UI elements.
    /// </summary>
    public static LoadableAsset<Sprite> RoundedBox { get; }

    /// <summary>
    /// Gets the NextButton sprite.
    /// </summary>
    public static LoadableAsset<Sprite> NextButton { get; } = new LoadableResourceAsset("MiraAPI.Resources.NextButton.png");

    /// <summary>
    /// Gets the active variant of the NextButton sprite.
    /// </summary>
    public static LoadableAsset<Sprite> NextButtonActive { get; } = new LoadableResourceAsset("MiraAPI.Resources.NextButtonActive.png");

    /// <summary>
    /// Gets the Cog icon sprite.
    /// </summary>
    public static LoadableAsset<Sprite> Cog { get; } = new LoadableResourceAsset("MiraAPI.Resources.Cog.png");

    /// <summary>
    /// Gets the Checkmark icon sprite.
    /// </summary>
    public static LoadableAsset<Sprite> Checkmark { get; } = new LoadableResourceAsset("MiraAPI.Resources.Checkmark.png");

    /// <summary>
    /// Gets the Checkmark Box sprite.
    /// </summary>
    public static LoadableAsset<Sprite> CheckmarkBox { get; } = new LoadableResourceAsset("MiraAPI.Resources.CheckMarkBox.png");

    /// <summary>
    /// Gets the custom category header sprite used in the options menu.
    /// </summary>
    public static LoadableAsset<Sprite> CategoryHeader { get; } = new LoadableResourceAsset("MiraAPI.Resources.CategoryHeader.png");
}
