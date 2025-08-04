using System;
using System.Reflection;
using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings.ConfigEntrySettings;
using UnityEngine;

namespace MiraAPI.LocalSettings.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ClientFloatSettingAttribute(ConfigEntry<bool> configEntry, Color? onColor = null, Color? offColor = null) : ClientSettingAttribute(configEntry)
{
    internal override IClientSetting? CreateSetting(object? value, PropertyInfo property)
    {
        var setting = new ClientBoolSetting(configEntry, onColor, offColor);
        return setting;
    }
}