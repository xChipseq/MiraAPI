using BepInEx.Configuration;

namespace MiraAPI.LocalSettings.ConfigEntrySettings;

public interface IClientSetting
{
    ConfigEntryBase BaseEntry { get; }
}