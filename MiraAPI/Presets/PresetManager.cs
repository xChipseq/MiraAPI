using System.Collections.Generic;
using System.IO;
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

    /// <summary>
    /// Gets the list of available presets for the game options.
    /// </summary>
    public static List<OptionPreset> Presets { get; } = [];

    /// <summary>
    /// Loads the presets for the specified plugin by reading the configuration files from the preset directory.
    /// </summary>
    /// <param name="plugin">>The plugin for which the presets should be loaded.</param>
    public static void LoadPresets(MiraPluginInfo plugin)
    {
        Presets.Clear();
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
            Presets.Add(new OptionPreset(presetName, plugin, presetConfig));
        }
    }
}
