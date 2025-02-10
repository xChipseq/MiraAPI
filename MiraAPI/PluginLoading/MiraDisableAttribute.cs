using System;

namespace MiraAPI.PluginLoading;

/// <summary>
/// Skip an element during plugin loading.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class MiraDisableAttribute : Attribute;
