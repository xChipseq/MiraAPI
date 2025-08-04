using System;
using System.Reflection;
using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings.ConfigEntrySettings;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.LocalSettings.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ClientEnumSettingAttribute(ConfigEntryBase configEntry, Type enumType, Color? color = null, string[] customNames = null) : ClientSettingAttribute(configEntry)
{
    internal override IClientSetting? CreateSetting(object? value, PropertyInfo property)
    {
        if (configEntry.SettingType.IsEnum)
        {
            Type genericType = typeof(ClientEnumSetting<>).MakeGenericType(enumType);
            var setting = Activator.CreateInstance(genericType, [configEntry, enumType, color, customNames]);
            return setting as IClientSetting;
        }
        Logger<MiraApiPlugin>.Error();
    }
}