using System;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace MiraAPI.LocalSettings.ConfigEntrySettings;

/// <summary>
/// Base class for Config Entries Settings
/// </summary>
public abstract class ClientSetting<T>(ConfigEntry<T> entry) : IClientSetting
{
    public ConfigEntry<T> Entry { get; } = entry;
    public ConfigEntryBase BaseEntry { get; } = entry as ConfigEntryBase;
}

