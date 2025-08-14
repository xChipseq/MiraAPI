﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.Modifiers;

/// <summary>
/// The component for handling modifiers.
/// </summary>
[RegisterInIl2Cpp]
public class ModifierComponent(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the active modifiers on the player.
    /// </summary>
    [HideFromIl2Cpp]
    public ImmutableList<BaseModifier> ActiveModifiers { get; private set; } = ImmutableList<BaseModifier>.Empty;

    private ModifierDisplayComponent? ModifierDisplay { get; set; }

    [HideFromIl2Cpp]
    private List<BaseModifier> Modifiers { get; set; } = [];

    private PlayerControl _player = null!;

    private readonly List<BaseModifier> _toRemove = [];

    private readonly List<BaseModifier> _toAdd = [];

    internal void ClearModifiers()
    {
        _toRemove.AddRange(Modifiers);
    }

    private void Awake()
    {
        _player = GetComponent<PlayerControl>();
        if (_player == null)
        {
            Logger<MiraApiPlugin>.Error("ModifierComponent could not find PlayerControl on the same GameObject.");
            Destroy(this);
            return;
        }

        ModifierExtensions.ModifierComponents.Add(_player, this);

        Modifiers = [];

        if (!_player.AmOwner)
        {
            return;
        }

        ModifierDisplay = ModifierDisplayComponent.CreateDisplay();
        ModifierDisplay?.UpdateModifiersList(Modifiers);
    }

    private void FixedUpdate()
    {
        var removed = _toRemove.Count > 0;
        foreach (var modifier in _toRemove.ToArray())
        {
            _toRemove.Remove(modifier);
            try
            {
                modifier.OnDeactivate();
            }
            catch (Exception e)
            {
                Logger<MiraApiPlugin>.Error($"Error while deactivating modifier {modifier.ModifierName}: {e.ToString()}");
            }

            Modifiers.Remove(modifier);
        }

        var added = _toAdd.Count > 0;
        foreach (var modifier in _toAdd.ToArray())
        {
            _toAdd.Remove(modifier);
            Modifiers.Add(modifier);
            modifier.Initialized = true;
            try
            {
                modifier.OnActivate();
            }
            catch (Exception e)
            {
                Logger<MiraApiPlugin>.Error($"Error while activating modifier {modifier.ModifierName}: {e.ToString()}");
            }

            if (modifier is TimedModifier { AutoStart: true } timer)
            {
                timer.StartTimer();
            }
        }

        if (removed || added)
        {
            if (_player.AmOwner && HudManager.InstanceExists)
            {
                ModifierDisplay?.UpdateModifiersList(Modifiers);
            }

            ActiveModifiers = Modifiers.ToImmutableList();
        }

        foreach (var modifier in ActiveModifiers)
        {
            try
            {
                modifier.FixedUpdate();
            }
            catch (Exception e)
            {
                Logger<MiraApiPlugin>.Error($"Error while (fixed) updating modifier {modifier.ModifierName}: {e.ToString()}");
            }
        }

        if (_player.AmOwner && ModifierDisplay && HudManager.InstanceExists)
        {
            ModifierDisplay!.gameObject.SetActive(MeetingHud.Instance || HudManager.Instance.TaskPanel.isActiveAndEnabled);
        }
    }

    private void Update()
    {
        foreach (var modifier in ActiveModifiers)
        {
            try
            {
                modifier.Update();
            }
            catch (Exception e)
            {
                Logger<MiraApiPlugin>.Error($"Error while updating modifier {modifier.ModifierName}: {e.ToString()}");
            }
        }
    }

    private void OnDestroy()
    {
        // Remove this component from the global list when destroyed
        ModifierExtensions.ModifierComponents.Remove(_player);
    }

    /// <summary>
    /// Gets a collection of modifiers by type.
    /// </summary>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>A collection of modifiers.</returns>
    [HideFromIl2Cpp]
    public IEnumerable<T> GetModifiers<T>(Func<T, bool>? predicate=null) where T : BaseModifier
    {
        return ActiveModifiers.OfType<T>().Where(x => predicate == null || predicate(x));
    }

    /// <summary>
    /// Gets a collection of modifiers by type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>A collection of modifiers.</returns>
    [HideFromIl2Cpp]
    public IEnumerable<BaseModifier> GetModifiers(Type type, Func<BaseModifier, bool>? predicate=null)
    {
        return ActiveModifiers.Where(x => x.GetType() == type && (predicate == null || predicate(x)));
    }

    /// <summary>
    /// Gets a collection of modifiers by type ID.
    /// </summary>
    /// <param name="id">The modifier's type ID.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>A collection of modifiers.</returns>
    [HideFromIl2Cpp]
    public IEnumerable<BaseModifier> GetModifiers(uint id, Func<BaseModifier, bool>? predicate=null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return GetModifiers(type, predicate);
    }

    /// <summary>
    /// Tries to get a modifier by its type.
    /// </summary>
    /// <param name="modifier">The modifier or null.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier<T>([NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        modifier = GetModifier(predicate);
        return modifier != null;
    }

    /// <summary>
    /// Tries to get a modifier by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier(Type type, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        modifier = GetModifier(type, predicate);
        return modifier != null;
    }

    /// <summary>
    /// Tries to get a modifier by its type ID.
    /// </summary>
    /// <param name="id">The modifier type ID.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier(uint id, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        modifier = GetModifier(id, predicate);
        return modifier != null;
    }

    /// <summary>
    /// Tries to get a modifier by its unique ID.
    /// </summary>
    /// <param name="modifierGuid">The modifier unique ID.</param>
    /// <param name="modifier">The modifier or null.</param>
    /// <returns>True if the modifier was found, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier(Guid modifierGuid, [NotNullWhen(true)] out BaseModifier? modifier)
    {
        modifier = GetModifier(modifierGuid);
        return modifier != null;
    }

    /// <summary>
    /// Gets a modifier by its type.
    /// </summary>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>The Modifier if it is found, null otherwise.</returns>
    [HideFromIl2Cpp]
    public T? GetModifier<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return GetModifiers(predicate).FirstOrDefault();
    }

    /// <summary>
    /// Gets a modifier by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>The Modifier if it is found, null otherwise.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? GetModifier(Type type, Func<BaseModifier, bool>? predicate = null)
    {
        return GetModifiers(type).FirstOrDefault(predicate ?? (_ => true));
    }

    /// <summary>
    /// Gets a modifier by its type ID.
    /// </summary>
    /// <param name="id">The modifier ID.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns>The Modifier if it is found, null otherwise.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? GetModifier(uint id, Func<BaseModifier, bool>? predicate = null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return GetModifier(type, predicate);
    }

    /// <summary>
    /// Gets a modifier by unique ID.
    /// </summary>
    /// <param name="modifierGuid">The modifier's unique ID.</param>
    /// <returns>The modifier if it is found, or null.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? GetModifier(Guid modifierGuid)
    {
        return ActiveModifiers.Find(x => x.UniqueId == modifierGuid);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <typeparam name="T">The modifier type.</typeparam>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        RemoveModifier(typeof(T), x => predicate == null || predicate((T)x));
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(Type type, Func<BaseModifier, bool>? predicate = null)
    {
        var modifiers = ActiveModifiers.Where(x => x.GetType() == type && (predicate == null || predicate(x))).ToList();
        if (modifiers.Count > 1)
        {
            throw new InvalidOperationException($"Cannot remove modifier {type.Name} because there are multiple instances of that modifier.");
        }

        var modifier = modifiers.FirstOrDefault();
        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {type.Name} because it is not active.");
            return;
        }

        RemoveModifier(modifier);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="modifier">The modifier object.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(BaseModifier modifier)
    {
        if (!ActiveModifiers.Contains(modifier))
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {modifier.ModifierName} because it is not active on this player.");
            return;
        }

        _toRemove.Add(modifier);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="typeId">The modifier's type ID.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(uint typeId, Func<BaseModifier, bool>? predicate = null)
    {
        var type = ModifierManager.GetModifierType(typeId) ?? throw new InvalidOperationException(
            $"Cannot remove modifier with id {typeId} because it is not registered.");
        RemoveModifier(type, predicate);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="uniqueId">The modifier's unique ID.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(Guid uniqueId)
    {
        var modifier = GetModifier(uniqueId);
        if (modifier == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier with unique id {uniqueId} because it is not active.");
            return;
        }

        RemoveModifier(modifier);
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <typeparam name="T">The Type of the modifier.</typeparam>
    /// <returns>The new modifier.</returns>
    [HideFromIl2Cpp]
    public T? AddModifier<T>(params object[] args) where T : BaseModifier
    {
        return AddModifier(typeof(T), args) as T;
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The modifier that was added.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(BaseModifier modifier)
    {
        // TODO: Make a proper synchronization system.
        if (LobbyBehaviour.Instance)
        {
            Logger<MiraApiPlugin>.Warning($"Modifiers added in the lobby won't sync to new players!");
        }

        var id = modifier.TypeId;
        if (modifier.Unique && ActiveModifiers.Find(x => x.TypeId == id) != null)
        {
            Logger<MiraApiPlugin>.Error($"Player already has modifier with id {id}!");
            return null;
        }

        if (ActiveModifiers.Contains(modifier))
        {
            Logger<MiraApiPlugin>.Error($"Player already has this modifier!");
            return null;
        }

        _toAdd.Add(modifier);
        modifier.Player = _player;
        modifier.ModifierComponent = this;
        if (modifier.UniqueId == Guid.Empty)
        {
            modifier.UniqueId = Guid.NewGuid();
        }
        return modifier;
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The modifier that was added.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(Type type, params object[] args)
    {
        BaseModifier? modifier;
        if (args.Length > 0)
        {
            modifier = ModifierFactory.CreateInstance(type, args);
        }
        else
        {
            modifier = Activator.CreateInstance(type) as BaseModifier;
            if (modifier == null)
            {
                throw new InvalidOperationException($"Cannot add modifier {type.Name} because it is not a valid modifier.");
            }
        }

        return AddModifier(modifier);
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="id">The ID of the modifier.</param>
    /// <param name="args">The arguments to initialize the modifier constructor with.</param>
    /// <returns>The modifier if it was created, or null if it failed.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(uint id, params object[] args)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot add modifier with id {id} because it is not registered.");

        return AddModifier(type, args);
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type.
    /// </summary>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier<T>(Func<T, bool>? predicate=null) where T : BaseModifier
    {
        return ActiveModifiers.Exists(x => x is T modifier && (predicate == null || predicate(modifier)));
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Type type, Func<BaseModifier, bool>? predicate=null)
    {
        return ActiveModifiers.Exists(x => x.GetType() == type && (predicate == null || predicate(x)));
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type ID.
    /// </summary>
    /// <param name="id">The modifier's type ID.</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <returns>True if the modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(uint id, Func<BaseModifier, bool>? predicate=null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return HasModifier(type, predicate);
    }

    /// <summary>
    /// Checks if a player has an active modifier by its unique ID.
    /// </summary>
    /// <param name="id">The modifier's guid.</param>
    /// <returns>True if the modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Guid id)
    {
        return ActiveModifiers.Exists(x => x.UniqueId == id);
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type.
    /// </summary>
    /// <param name="checkInactive">Whether to check inactive modifiers (those pending to be added).</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier<T>(bool checkInactive, Func<T, bool>? predicate=null) where T : BaseModifier
    {
        return ActiveModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
        bool MatchExpr(BaseModifier bm) => bm is T modifier && (predicate == null || predicate(modifier));
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="checkInactive">Whether to check inactive modifiers (those pending to be added).</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Type type, bool checkInactive, Func<BaseModifier, bool>? predicate=null)
    {
        return ActiveModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
        bool MatchExpr(BaseModifier bm) => bm.GetType() == type && (predicate == null || predicate(bm));
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type ID.
    /// </summary>
    /// <param name="id">The modifier's type ID.</param>
    /// <param name="checkInactive">Whether to check inactive modifiers (those pending to be added).</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <returns>True if the modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(uint id, bool checkInactive, Func<BaseModifier, bool>? predicate=null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return HasModifier(type, checkInactive, predicate);
    }

    /// <summary>
    /// Checks if a player has an active modifier by its unique ID.
    /// </summary>
    /// <param name="id">The modifier's guid.</param>
    /// <param name="checkInactive">Whether to check inactive modifiers (those pending to be added).</param>
    /// <returns>True if the modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Guid id, bool checkInactive)
    {
        return ActiveModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
        bool MatchExpr(BaseModifier bm) => bm.UniqueId == id;
    }
}
