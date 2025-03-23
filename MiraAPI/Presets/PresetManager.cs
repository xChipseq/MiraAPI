using System.IO;
using System.Linq;
using BepInEx.Configuration;
using MiraAPI.PluginLoading;
using UnityEngine;

namespace MiraAPI.Presets;

/// <summary>
/// Provides functionality to manage and load game option presets for plugins.
/// </summary>
public static class PresetManager
{
    /// <summary>
    /// Gets the directory where the presets are stored.
    /// </summary>
    public static string PresetDirectory { get; } = Path.Join(Application.persistentDataPath, "mira_presets");

    internal static void CreateDefaultPreset(MiraPluginInfo plugin)
    {
        var presetPath = Path.Join(PresetManager.PresetDirectory, plugin.PluginId);
        if (!Directory.Exists(presetPath))
        {
            Directory.CreateDirectory(presetPath);
        }

        var presetConfig = new ConfigFile(Path.Join(presetPath, "Default.cfg"), true);

        foreach (var option in plugin.InternalOptions.Where(x => x.IncludeInPreset))
        {
            option.SaveToPreset(presetConfig, true);
        }

        presetConfig.Save();
    }

    /// <summary>
    /// Loads the presets for the specified plugin by reading the configuration files from the preset directory.
    /// </summary>
    /// <param name="plugin">>The plugin for which the presets should be loaded.</param>
    public static void LoadPresets(MiraPluginInfo plugin)
    {
        foreach (var btn in plugin.InternalPresets.Select(x=>x.PresetButton))
        {
            if (btn != null)
            {
                Object.DestroyImmediate(btn);
            }
        }

        plugin.InternalPresets.Clear();
        if (!Directory.Exists(PresetDirectory))
        {
            Directory.CreateDirectory(PresetDirectory);
        }

        var pluginPresetPath = Path.Join(PresetDirectory, plugin.PluginId);
        if (!Directory.Exists(pluginPresetPath))
        {
            Directory.CreateDirectory(pluginPresetPath);
        }

        foreach (var file in Directory.GetFiles(pluginPresetPath, "*.cfg"))
        {
            var presetName = Path.GetFileNameWithoutExtension(file);
            var presetConfig = new ConfigFile(file, true);
            plugin.InternalPresets.Add(new OptionPreset(presetName, plugin, presetConfig));
        }
        plugin.Presets = [.. plugin.InternalPresets];
    }
}
