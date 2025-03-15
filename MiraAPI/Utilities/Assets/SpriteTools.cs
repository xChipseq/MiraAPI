using System;
using System.IO;
using System.Reflection;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for various sprite-related operations.
/// </summary>
public static class SpriteTools
{
    /// <summary>
    /// Load a sprite from a resource path.
    /// </summary>
    /// <param name="resourcePath">The path to the resource.</param>
    /// <returns>A sprite made from the resource.</returns>
    /// <exception cref="Exception">The resource cannot be found.</exception>
    public static Sprite LoadSpriteFromPath(string resourcePath, Assembly assembly, float pixelsPerUnit)
    {
        var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        var myStream = assembly.GetManifestResourceStream(resourcePath);
        if (myStream != null)
        {
            var buttonTexture = myStream.ReadFully();
            tex.LoadImage(buttonTexture, false);
        }
        else
        {
            throw new ArgumentException($"Resource not found: {resourcePath}");
        }

        tex.name = resourcePath;
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        sprite.name = resourcePath;
        return sprite;
    }
    public static void Dump(this Sprite sprite, string path) => File.WriteAllBytes(path, sprite.texture.Decompress().EncodeToPNG());

    public static Texture2D Decompress(this Texture2D source)
    {
        var renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTex);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        var readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        readableText.name = source.name;
        return readableText;
    }
}
