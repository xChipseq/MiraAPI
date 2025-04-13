using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Class for storing each mod's settings
/// </summary>
public class ModSettingsTab
{
    public readonly BasePlugin Plugin;
    public readonly List<IConfigEntrySetting> ConfigEntries = new();

    public readonly string Title;
    public readonly string ShortTitle;
    public readonly Color TabColor;
    public readonly Color TabHoverColor;
    public readonly Sprite Icon;
    
    public ModSettingsTab(BasePlugin plugin, string title = null, string shortTitle = null, Color? tabColor = null, Color? tabHoverColor = null, Sprite icon = null)
    {
        Plugin = plugin;

        var metadata = MetadataHelper.GetMetadata(plugin);
        Title = title ?? metadata.Name;
        ShortTitle = shortTitle ?? ShortParseName(Title);
        TabColor = tabColor ?? Color.white;
        TabHoverColor = tabHoverColor ?? Palette.AcceptedGreen;
        Icon = icon;
    }
    
    /// <summary>
    /// Creates a setting for your ConfigEntry.
    /// Use specific bind methods if you need to override more properties
    /// </summary>
    /// <param name="entry">The entry you want to bind.</param>
    /// <param name="name">Overriden name of your setting.</param>
    /// <param name="description">Overriden description of your setting.</param>
    /// <returns>The created setting. Null if entry type is not supported or something else went wrong.</returns>
    public IConfigEntrySetting? BindEntry(ConfigEntryBase entry, string name = null, string description = null)
    {
        IConfigEntrySetting setting = null;
        switch (entry)
        {
            case ConfigEntry<bool> boolEntry:
                setting = BindBoolEntry(boolEntry, name, description);
                break;
            case ConfigEntry<float> floatEntry:
                setting = BindFloatEntry(floatEntry, name, description);
                break;
            case ConfigEntry<int> intEntry:
                setting = BindIntEntry(intEntry, name, description);
                break;
            default:
                // If none of these types match, check if it's an enum
                if (entry.SettingType.IsEnum)
                {
                    MethodInfo method = GetType().GetMethod(nameof(BindEnumEntry));

                    // Make it generic using the enum type
                    MethodInfo genericMethod = method.MakeGenericMethod(entry.SettingType);
                    
                    setting = genericMethod.Invoke(this, new object[] { entry, entry.SettingType, name, description }) as IConfigEntrySetting;
                    break;
                }
                Logger<MiraApiPlugin>.Error($"Unsupported type of {entry.Definition} from {Plugin}: {entry.SettingType.Name}");
                break;
        }
        
        return setting;
    }

    public ConfigEntryBoolSetting BindBoolEntry(ConfigEntry<bool> entry, string name = null, string description = null,
        Color? onColor = null, Color? offColor = null)
    {
        ConfigEntryBoolSetting setting = new(Plugin, entry, name, description, onColor, offColor);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }

    public ConfigEntryFloatSetting BindFloatEntry(ConfigEntry<float> entry, string name = null, string description = null, Color? color = null, FloatRange sliderRange = null)
    {
        ConfigEntryFloatSetting setting = new(Plugin, entry, name, color, sliderRange);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }
    
    public ConfigEntryIntSetting BindIntEntry(ConfigEntry<int> entry, string name = null, string description = null, Color? color = null, IntRange range = null)
    {
        ConfigEntryIntSetting setting = new(Plugin, entry, name, description, color, range);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }
    
    public ConfigEntryEnumSetting<T> BindEnumEntry<T>(ConfigEntry<T> entry, Type enumType, string name = null, string description = null, Color? color = null, string[] customNames = null) where T : Enum
    {
        ConfigEntryEnumSetting<T> setting = new(Plugin, entry, enumType, name, description, color, customNames);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }
    
    private static string ShortParseName(string name)
    {
        var words = name.Split(' ');
        string fullString = "";
        words.ToList().ForEach(x => fullString += x[0]);
        return fullString;
    }
}