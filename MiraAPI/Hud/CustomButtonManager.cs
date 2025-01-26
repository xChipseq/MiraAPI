using System;
using System.Collections.Generic;
using System.Reflection;
using MiraAPI.Events.Mira;
using MiraAPI.PluginLoading;
using Reactor.Utilities;

namespace MiraAPI.Hud;

/// <summary>
/// Custom button manager for handling custom buttons.
/// </summary>
public static class CustomButtonManager
{
    internal static readonly List<CustomActionButton> CustomButtons = [];
    internal static readonly Dictionary<Type, Type> ButtonEventTypes = [];
    internal static readonly Dictionary<Type, Type> ButtonCancelledEventTypes = [];

    internal static void RegisterButton(Type buttonType, MiraPluginInfo pluginInfo)
    {
        if (!typeof(CustomActionButton).IsAssignableFrom(buttonType))
        {
            Logger<MiraApiPlugin>.Error($"Skipping button {buttonType.Name}. Does not inherit CustomActionButton!");
            return;
        }

        if (Activator.CreateInstance(buttonType) is not CustomActionButton button)
        {
            Logger<MiraApiPlugin>.Error($"Failed to create button from {buttonType.Name}");
            return;
        }

        CustomButtons.Add(button);
        pluginInfo.Buttons.Add(button);
        typeof(CustomButtonSingleton<>).MakeGenericType(buttonType)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, button);

        ButtonEventTypes.Add(buttonType, typeof(MiraButtonClickEvent<>).MakeGenericType(buttonType));
        ButtonCancelledEventTypes.Add(buttonType, typeof(MiraButtonCancelledEvent<>).MakeGenericType(buttonType));
    }
}
