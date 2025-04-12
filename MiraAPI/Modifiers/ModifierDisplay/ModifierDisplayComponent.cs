using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Modifiers.ModifierDisplay;

/// <summary>
/// The code used to display Mira modifiers.
/// </summary>
[RegisterInIl2Cpp]
public class ModifierDisplayComponent(nint ptr) : MonoBehaviour(ptr)
{
    /// <summary>
    /// Gets the instance of the Modifier Display.
    /// </summary>
    public static ModifierDisplayComponent? Instance { get; private set; }

    private GameObject _children = null!;
    private GameObject _modTemplate = null!;
    private PassiveButton _toggleButton = null!;
    private TextMeshPro _toggleBtnText = null!;

    private readonly Dictionary<BaseModifier, ModifierUiComponent> _modifiers = [];

    /// <summary>
    /// Gets a read only dictionary of the created modifier UI components.
    /// </summary>
    [HideFromIl2Cpp]
    public ReadOnlyDictionary<BaseModifier, ModifierUiComponent> Modifiers => new(_modifiers);

    /// <summary>
    /// Gets a value indicating whether the modifier display hud is currently open.
    /// </summary>
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        Instance = this;

        _children = transform.GetChild(0).gameObject;
        _modTemplate = transform.GetChild(2).gameObject;

        _toggleButton = transform.GetChild(1).GetComponent<PassiveButton>();
        _toggleButton.OnClick = new Button.ButtonClickedEvent();
        _toggleButton.OnClick.AddListener((UnityAction)(() =>
        {
            IsOpen = !IsOpen;
            _children.gameObject.SetActive(!_children.gameObject.activeSelf);
        }));

        _toggleButton.ClickSound = HudManager.Instance.GameMenu.StreamerModeButton.GetComponent<PassiveButton>().ClickSound;
        _toggleButton.HoverSound = HudManager.Instance.GameMenu.StreamerModeButton.GetComponent<PassiveButton>().HoverSound;
        _toggleButton.inactiveSprites = _toggleButton.transform.GetChild(1).gameObject;
        _toggleBtnText = _toggleButton.transform.GetChild(0).GetComponent<TextMeshPro>();
        _toggleBtnText.font = HudManager.Instance.TaskPanel.taskText.font;
        _toggleBtnText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;

        IsOpen = false;
        _children.gameObject.SetActive(false);
    }

    [HideFromIl2Cpp]
    internal static ModifierDisplayComponent CreateDisplay()
    {
        var gameObject = Instantiate(MiraAssets.ModifierDisplay.LoadAsset(), HudManager.Instance.transform);
        var minigame = gameObject.AddComponent<ModifierDisplayComponent>();
        return minigame;
    }

    [HideFromIl2Cpp]
    private ModifierUiComponent CreateForModifier(BaseModifier modifier)
    {
        var newMod = Instantiate(_modTemplate, _children.transform);
        newMod.transform.name = modifier.ModifierName;
        var comp = newMod.AddComponent<ModifierUiComponent>();
        comp.Modifier = modifier;
        newMod.gameObject.SetActive(true);
        return comp;
    }

    [HideFromIl2Cpp]
    internal void UpdateModifiersList(List<BaseModifier> modifiers)
    {
        var filteredModifiers = modifiers.Where(x => !x.HideOnUi && x.GetDescription() != string.Empty).ToList();

        foreach (var mod in _modifiers.ToArray())
        {
            if (filteredModifiers.Contains(mod.Key))
            {
                filteredModifiers.Remove(mod.Key);
                continue;
            }

            mod.Value.gameObject.Destroy();
            _modifiers.Remove(mod.Key);
        }

        _toggleBtnText.text = $"Modifiers ({modifiers.Count})";

        if (modifiers.Count == 0)
        {
            _toggleButton.gameObject.SetActive(false);
            return;
        }

        _toggleButton.gameObject.SetActive(true);

        foreach (var mod in filteredModifiers)
        {
            _modifiers.Add(mod, CreateForModifier(mod));
        }
    }
}
