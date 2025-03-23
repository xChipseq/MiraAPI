using System;
using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// Represents a modded option.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public abstract class ModdedOption<T> : IModdedOption
{
    private IMiraPlugin? _parentMod;

    /// <inheritdoc />
    public uint Id { get; }

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public StringNames StringName { get; }

    /// <inheritdoc />
    public BaseGameSetting Data { get; protected init; } = null!;

    /// <inheritdoc />
    public IMiraPlugin? ParentMod
    {
        get => _parentMod;
        set
        {
            if (_parentMod != null || value == null) return;
            _parentMod = value;
            var entry = _parentMod.GetConfigFile().Bind(ConfigDefinition, DefaultValue);
            Value = entry.Value;
        }
    }

    /// <summary>
    /// Gets or sets the value of the option.
    /// </summary>
    public T Value { get; protected set; }

    /// <summary>
    /// Gets the default value of the option.
    /// </summary>
    public T DefaultValue { get; }

    /// <summary>
    /// Gets or sets the event that is invoked when the value of the option changes.
    /// </summary>
    public Action<T>? ChangedEvent { get; set; }

    /// <inheritdoc />
    public Func<bool> Visible { get; set; }

    /// <inheritdoc />
    public bool IncludeInPreset { get; set; }

    /// <inheritdoc />
    public OptionBehaviour? OptionBehaviour { get; protected set; }

    /// <inheritdoc />
    public ConfigDefinition? ConfigDefinition
    {
        get => _configDefinition;
        set
        {
            if (_configDefinition is not null) return;
            _configDefinition = value;
        }
    }

    private ConfigDefinition? _configDefinition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedOption{T}"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="includeInPreset">Whether to include the option in the preset.</param>
    protected ModdedOption(string title, T defaultValue, bool includeInPreset = true)
    {
        Id = ModdedOptionsManager.NextId;
        Title = title;
        DefaultValue = defaultValue;
        Value = defaultValue;
        StringName = CustomStringName.CreateAndRegister(Title);
        Visible = () => true;
        IncludeInPreset = includeInPreset;
    }

    internal void ValueChanged(OptionBehaviour optionBehaviour)
    {
        SetValue(GetValueFromOptionBehaviour(optionBehaviour));
    }

    /// <summary>
    /// Sets the value of the option.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    public void SetValue(T newValue)
    {
        var oldVal = Value;
        Value = newValue;

        if (Value?.Equals(oldVal) == false)
        {
            ChangedEvent?.Invoke(Value);
        }

        if (AmongUsClient.Instance.AmHost)
        {
            if (ParentMod?.GetConfigFile().TryGetEntry<T>(ConfigDefinition, out var entry) == true)
            {
                entry.Value = Value;
            }

            Rpc<SyncOptionsRpc>.Instance.Send(PlayerControl.LocalPlayer, [GetNetData()], true);
        }

        OnValueChanged(newValue);
    }

    /// <inheritdoc />
    public void SaveToPreset(ConfigFile presetConfig, bool saveDefault=false)
    {
        if (ConfigDefinition is null)
        {
            Logger<MiraApiPlugin>.Error($"Attempted to save {Title} to preset, but ConfigDefinition is null.");
            return;
        }
        var value = saveDefault ? DefaultValue : Value;

        if (!presetConfig.ContainsKey(ConfigDefinition))
        {
            presetConfig.Bind(ConfigDefinition, DefaultValue);
        }

        presetConfig[ConfigDefinition].BoxedValue = value;
    }

    /// <inheritdoc />
    public abstract float GetFloatData();

    /// <inheritdoc />
    public abstract NetData GetNetData();

    /// <inheritdoc />
    public abstract void HandleNetData(byte[] data);

    /// <summary>
    /// Handles the value changed event.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    protected abstract void OnValueChanged(T newValue);

    /// <summary>
    /// Gets the value from the option behaviour.
    /// </summary>
    /// <param name="optionBehaviour">The OptionBehaviour.</param>
    /// <returns>The value.</returns>
    public abstract T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour);

    /// <inheritdoc />
    public abstract OptionBehaviour CreateOption(
        ToggleOption toggleOpt,
        NumberOption numberOpt,
        StringOption stringOpt,
        Transform container);

    /// <summary>
    /// Implicitly converts the option to type of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <returns>Value of type <typeparamref name="T"/>.</returns>
    public static implicit operator T(ModdedOption<T> option)
    {
        return option.Value;
    }
}
