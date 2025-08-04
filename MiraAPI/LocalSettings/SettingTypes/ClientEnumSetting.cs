using System;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace MiraAPI.LocalSettings.ConfigEntrySettings;

/// <summary>
/// Class for Config Entry Enum Settings
/// </summary>
public class ClientEnumSetting<T>(
    ConfigEntry<T> entry,
    Type enumType,
    Color? color = null,
    string[] customNames = null)
    : ClientSetting<T>(entry) where T : Enum
{
    public Color Color { get; } = color ?? Palette.AcceptedGreen;
    public Type EnumType { get; } = enumType;
    public string[] EnumNames { get; set; } = customNames ?? Enum.GetNames(enumType);
    
    public int ValueIndex = Enum.GetNames(enumType).ToList().IndexOf(entry.Value.ToString());
}