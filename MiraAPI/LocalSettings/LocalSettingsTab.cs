using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using MiraAPI.LocalSettings.ConfigEntrySettings;
using Reactor.Utilities;
using UnityEngine;
using xCloud;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Class for storing each mod's settings
/// </summary>
public abstract class LocalSettingsTab
{
    internal readonly List<IClientSetting> ConfigEntries = new();
    
    /// <summary>
    /// Gets the name of your tab.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Gets the short version of your name. Showed when there is no <see cref="TabIcon"/>.
    /// </summary>
    public virtual string ShortName => ShortParseName(Name);
    
    /// <summary>
    /// Indicates whether there should be a button for your tab.
    /// If false, the tab needs to be shown manually with <see cref="Open"/>.
    /// </summary>
    public abstract bool TabButtonEnabled { get; }
    
    /// <summary>
    /// Gets the color of the tab.
    /// </summary>
    public virtual Color TabColor => Color.white;
    
    /// <summary>
    /// Gets the color of the tab in hovered state.
    /// </summary>
    public virtual Color TabHoverColor => Palette.AcceptedGreen;
    
    /// <summary>
    /// Gets the icon of the tab.
    /// Replaces <see cref="ShortName"/>
    /// </summary>
    public virtual Sprite TabIcon { get; }
    
    /// <summary>
    /// Creates a setting for your ConfigEntry.
    /// Use specific bind methods if you need to override more properties
    /// </summary>
    /// <param name="entry">The entry you want to bind.</param>
    /// <param name="name">Overriden name of your setting.</param>
    /// <param name="description">Overriden description of your setting.</param>
    /// <returns>The created setting. Null if entry type is not supported or something else went wrong.</returns>
    public IClientSetting? BindEntry(ConfigEntryBase entry, string name = null, string description = null)
    {
        IClientSetting setting = null;
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
                    
                    setting = genericMethod.Invoke(this, new object[] { entry, entry.SettingType, name, description, null, null }) as IClientSetting;
                    break;
                }
                Logger<MiraApiPlugin>.Error($"Unsupported type of {entry.Definition} from {Plugin}: {entry.SettingType.Name}");
                break;
        }
        
        return setting;
    }

    public ClientBoolSetting BindBoolEntry(ConfigEntry<bool> entry, string name = null, string description = null,
        Color? onColor = null, Color? offColor = null)
    {
        ClientBoolSetting setting = new(Plugin, entry, name, description, onColor, offColor);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }

    public ClientFloatSetting BindFloatEntry(ConfigEntry<float> entry, string name = null, string description = null, Color? color = null, FloatRange sliderRange = null)
    {
        ClientFloatSetting setting = new(Plugin, entry, name, color, sliderRange);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }
    
    public ClientIntSetting BindIntEntry(ConfigEntry<int> entry, string name = null, string description = null, Color? color = null, IntRange range = null)
    {
        ClientIntSetting setting = new(Plugin, entry, name, description, color, range);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }
    
    public ClientEnumSetting<T> BindEnumEntry<T>(ConfigEntry<T> entry, Type enumType, string name = null, string description = null, Color? color = null, string[] customNames = null) where T : Enum
    {
        ClientEnumSetting<T> setting = new(Plugin, entry, enumType, name, description, color, customNames);
        ConfigEntries.Add(setting);
        Logger<MiraApiPlugin>.Info($"{setting.GetType().Name} created for {entry.Definition} from {Plugin}");
        return setting;
    }
    
    /// <summary>
    /// Opens the tab
    /// </summary>
    public void Open()
    {
        
    }
    
    private static string ShortParseName(string name)
    {
        var words = name.Split(' ');
        string fullString = "";
        words.ToList().ForEach(x => fullString += x[0]);
        return fullString;
    }
}