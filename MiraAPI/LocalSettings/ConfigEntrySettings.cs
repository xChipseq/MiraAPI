using System;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace MiraAPI.LocalSettings;

public interface IConfigEntrySetting
{
    string Name { get; }
    string Description { get; }
    BasePlugin Plugin { get; }
    ConfigEntryBase BaseEntry { get; }
}

/// <summary>
/// Base class for Config Entries Settings
/// </summary>
public abstract class ConfigEntrySetting<T>(
    BasePlugin plugin,
    ConfigEntry<T> entry,
    string name = null,
    string description = null) : IConfigEntrySetting
{
    public string Name { get; } = name ?? entry.Definition.Key;
    public string Description { get; } = description ?? entry.Description.Description;
    
    public BasePlugin Plugin { get; } = plugin;
    public ConfigEntry<T> Entry { get; } = entry;
    public ConfigEntryBase BaseEntry { get; } = entry as ConfigEntryBase;
}

/// <summary>
/// Class for Config Entry Bool Settings
/// </summary>
public class ConfigEntryBoolSetting(
    BasePlugin plugin,
    ConfigEntry<bool> entry,
    string name = null,
    string description = null,
    Color? onColor = null,
    Color? offColor = null)
    : ConfigEntrySetting<bool>(plugin, entry, name, description)
{
    public readonly Color OnColor = onColor ?? Palette.AcceptedGreen;
    public readonly Color OffColor = offColor ?? Color.white;
}

/// <summary>
/// Class for Config Entry Float Settings
/// </summary>
public class ConfigEntryFloatSetting(
    BasePlugin plugin,
    ConfigEntry<float> entry,
    string name = null,
    Color? sliderColor = null,
    FloatRange sliderRange = null,
    bool roundValue = true)
    : ConfigEntrySetting<float>(plugin, entry, name, "")
{
    public readonly Color SliderColor = sliderColor ?? Palette.AcceptedGreen;
    public readonly FloatRange SliderRange = sliderRange ?? new(0, 100);
    public readonly bool RoundValue = roundValue;
}

/// <summary>
/// Class for Config Entry Int Settings
/// </summary>
public class ConfigEntryIntSetting(
    BasePlugin plugin,
    ConfigEntry<int> entry,
    string name = null,
    string description = null,
    Color? color = null,
    IntRange range = null)
    : ConfigEntrySetting<int>(plugin, entry, name, description)
{
    public readonly Color Color = color ?? Palette.AcceptedGreen;
    public readonly IntRange Range = range ?? new(0, 3);
}

/// <summary>
/// Class for Config Entry Enum Settings
/// </summary>
public class ConfigEntryEnumSetting<T>(
    BasePlugin plugin,
    ConfigEntry<T> entry,
    Type enumType,
    string name = null,
    string description = null,
    Color? color = null,
    string[] customNames = null)
    : ConfigEntrySetting<T>(plugin, entry, name, description) where T : Enum
{
    public readonly Color Color = color ?? Palette.AcceptedGreen;
    public readonly Type EnumType = enumType;
    public int ValueIndex = Enum.GetNames(enumType).ToList().IndexOf(entry.Value.ToString());
    public readonly string[] EnumNames = customNames ?? Enum.GetNames(enumType);
}