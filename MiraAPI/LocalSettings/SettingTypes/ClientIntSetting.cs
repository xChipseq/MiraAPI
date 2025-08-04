using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace MiraAPI.LocalSettings.ConfigEntrySettings;

/// <summary>
/// Class for Config Entry Int Settings
/// </summary>
public class ClientIntSetting(
    ConfigEntry<int> entry,
    Color? color = null,
    IntRange range = null)
    : ClientSetting<int>(entry)
{
    public Color Color { get; } = color ?? Palette.AcceptedGreen;
    public IntRange Range { get; } = range ?? new(0, 3);
}