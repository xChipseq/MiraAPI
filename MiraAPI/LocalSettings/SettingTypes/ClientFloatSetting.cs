using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace MiraAPI.LocalSettings.ConfigEntrySettings;

/// <summary>
/// Class for Config Entry Float Settings
/// </summary>
public class ClientFloatSetting(
    ConfigEntry<float> entry,
    Color? sliderColor = null,
    FloatRange sliderRange = null,
    bool roundValue = true)
    : ClientSetting<float>(entry)
{
    public Color SliderColor { get; } = sliderColor ?? Palette.AcceptedGreen;
    public FloatRange SliderRange { get; } = sliderRange ?? new(0, 100);
    public bool RoundValue { get; } = roundValue;
}