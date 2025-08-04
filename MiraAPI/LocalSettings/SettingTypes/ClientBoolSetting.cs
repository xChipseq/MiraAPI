using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace MiraAPI.LocalSettings.ConfigEntrySettings;

/// <summary>
/// Class for Config Entry Bool Settings
/// </summary>
public class ClientBoolSetting( 
    ConfigEntry<bool> entry,
    Color? onColor = null,
    Color? offColor = null)
    : ClientSetting<bool>(entry)
{
    public Color OnColor { get; } = onColor ?? Palette.AcceptedGreen;
    public Color OffColor { get; } = offColor ?? Color.white;
}