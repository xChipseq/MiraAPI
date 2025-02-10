using BepInEx.Unity.IL2CPP;
using MiraAPI.Colors;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiraAPI.PluginLoading;

/// <summary>
/// Mira Plugin manager.
/// </summary>
public sealed class MiraPluginManager
{
    private readonly Dictionary<Assembly, MiraPluginInfo> _registeredPlugins = [];

    internal MiraPluginInfo[] RegisteredPlugins() => [.. _registeredPlugins.Values];

    internal static MiraPluginManager Instance { get; private set; } = new();

    internal void Initialize()
    {
        Instance = this;
        IL2CPPChainloader.Instance.PluginLoad += (pluginInfo, assembly, plugin) =>
        {
            if (plugin is not IMiraPlugin miraPlugin)
            {
                return;
            }

            var info = new MiraPluginInfo(miraPlugin, pluginInfo);
            var roles = new List<Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<MiraDisableAttribute>() != null)
                {
                    continue;
                }

                RegisterModifier(type);
                RegisterOptions(type, info);

                if (RegisterRoleAttribute(type, info, out var role))
                {
                    roles.Add(role!);
                }

                RegisterButtonAttribute(type, info);
                RegisterColorClasses(type);
            }

            info.OptionGroups.Sort((x, y) => x.GroupPriority.CompareTo(y.GroupPriority));
            CustomRoleManager.RegisterRoleTypes(roles, info);

            _registeredPlugins.Add(assembly, info);
            Logger<MiraApiPlugin>.Info($"Registering mod {pluginInfo.Metadata.GUID} with Mira API.");
        };
        IL2CPPChainloader.Instance.Finished += PaletteManager.RegisterAllColors;
        IL2CPPChainloader.Instance.Finished += MiraEventManager.SortAllHandlers;
    }

    /// <summary>
    /// Get a mira plugin by its GUID.
    /// </summary>
    /// <param name="guid">The plugin GUID.</param>
    /// <returns>A MiraPluginInfo.</returns>
    public static MiraPluginInfo GetPluginByGuid(string guid)
    {
        return Instance._registeredPlugins.Values.First(plugin => plugin.PluginId == guid);
    }

    private static void RegisterOptions(Type type, MiraPluginInfo pluginInfo)
    {
        if (!type.IsAssignableTo(typeof(AbstractOptionGroup)))
        {
            return;
        }

        if (!ModdedOptionsManager.RegisterGroup(type, pluginInfo))
        {
            return;
        }

        foreach (var property in type.GetProperties())
        {
            if (property.PropertyType.IsAssignableTo(typeof(IModdedOption)))
            {
                ModdedOptionsManager.RegisterPropertyOption(type, property, pluginInfo);
                continue;
            }

            var attribute = property.GetCustomAttribute<ModdedOptionAttribute>();
            if (attribute == null)
            {
                continue;
            }

            ModdedOptionsManager.RegisterAttributeOption(type, attribute, property, pluginInfo);
        }
    }

    private static bool RegisterRoleAttribute(Type type, MiraPluginInfo pluginInfo, out Type? role)
    {
        role = null;

        if (!(typeof(RoleBehaviour).IsAssignableFrom(type) && typeof(ICustomRole).IsAssignableFrom(type)))
        {
            return false;
        }

        if (!ModList.GetById(pluginInfo.PluginId).IsRequiredOnAllClients)
        {
            Logger<MiraApiPlugin>.Error("Custom roles are only supported on all clients.");
            return false;
        }

        role = type;
        return true;
    }

    private static void RegisterColorClasses(Type type)
    {
        if (type.GetCustomAttribute<RegisterCustomColorsAttribute>() == null)
        {
            return;
        }

        if (!type.IsStatic())
        {
            Logger<MiraApiPlugin>.Error($"Color class {type.Name} must be static.");
            return;
        }

        foreach (var property in type.GetProperties())
        {
            if (property.PropertyType != typeof(CustomColor))
            {
                continue;
            }

            if (property.GetValue(null) is not CustomColor color)
            {
                Logger<MiraApiPlugin>.Error($"Color property {property.Name} in {type.Name} is not a CustomColor.");
                continue;
            }

            PaletteManager.CustomColors.Add(color);
        }
    }

    private static void RegisterModifier(Type type)
    {
        if (type.IsAssignableTo(typeof(BaseModifier)))
        {
            ModifierManager.RegisterModifier(type);
        }
    }

    private static void RegisterButtonAttribute(Type type, MiraPluginInfo pluginInfo)
    {
        if (type.IsAssignableTo(typeof(CustomActionButton)) || type.IsAssignableTo(typeof(CustomActionButton<>)))
        {
            CustomButtonManager.RegisterButton(type, pluginInfo);
        }
    }
}
