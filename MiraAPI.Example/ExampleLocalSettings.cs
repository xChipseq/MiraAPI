using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Example;

public static class ExampleLocalSettings
{
    public static ModSettingsTab ExampleTab;
    
    public static ConfigEntry<bool> ExampleBoolEntry;
    public static ConfigEntry<bool> ExampleColoredEntry;
    public static ConfigEntry<int> ExampleIntEntry;
    public static ConfigEntry<float> ExampleFloatEntry;
    public static ConfigEntry<ExampleSettingEnum> ExampleEnumEntry;
    
    public enum ExampleSettingEnum
    {
        Cheeseburger,
        Fries,
        Pizza,
        ChickenNuggets,
    }

    public static void Example()
    {
        var plugin = PluginSingleton<ExamplePlugin>.Instance;
        var config = plugin.Config;
        
        ExampleBoolEntry = config.Bind("General", "Example Bool Setting", true, "You can toggle this on and off.");
        ExampleColoredEntry = config.Bind("General", "Example Colored Setting", false);
        ExampleIntEntry = config.Bind("General", "Example Int Setting", 3, "You can switch between numbers.");
        ExampleFloatEntry = config.Bind("Misc", "Example Float Setting", 50f);
        ExampleEnumEntry = config.Bind("Misc", "Example Enum Setting", ExampleSettingEnum.Cheeseburger, "This one has values from an enum :o");

        ExampleTab = LocalSettingsManager.CreateSettingsTab(plugin);
        ExampleTab.BindEntry(ExampleBoolEntry);
        ExampleTab.BindBoolEntry(ExampleColoredEntry, onColor: Color.green, offColor: Color.red);
        ExampleTab.BindEntry(ExampleIntEntry);
        ExampleTab.BindEntry(ExampleFloatEntry);
        ExampleTab.BindEnumEntry(ExampleEnumEntry, enumType: typeof(ExampleSettingEnum), customNames: ["Cheeseburger", "Fries", "Pizza", "Chicken Nuggets"]);
    }
}