using System;
using System.Reflection;
using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings.ConfigEntrySettings;

namespace MiraAPI.LocalSettings.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ClientSettingAttribute(ConfigEntryBase configEntry) : Attribute
{
    internal IClientSetting? HolderSetting { get; set; }

    /// <summary>
    /// Sets the value of the settomg.
    /// </summary>
    /// <param name="value">The new value as an object.</param>
    public abstract void SetValue(object value);

    /// <summary>
    /// Gets the value of the setting.
    /// </summary>
    /// <returns>The value of the setting as an object.</returns>
    public abstract object GetValue();

    internal abstract IClientSetting? CreateSetting(object? value, PropertyInfo property);
}