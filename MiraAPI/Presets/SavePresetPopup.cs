using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RegisterInIl2Cpp]
public class SavePresetPopup(nint ptr) : Minigame(ptr)
{
    private PassiveButton saveButton;
    private PassiveButton closeButton;
    private TextMeshPro textBoxText;
    private TextBoxTMP textBox;

    private Action<string> onSave;

    // Cleanup holder object
    public void OnDestroy()
    {
        transform.parent.gameObject.Destroy();
    }

    public override void Close()
    {
    }

    private void Awake()
    {
        var textboxHolder = transform.GetChild(1).GetChild(1);
        saveButton = transform.FindChild("SaveButton").GetComponent<PassiveButton>();
        closeButton = transform.FindChild("CloseButton").GetComponent<PassiveButton>();
        textBox = textboxHolder.GetChild(0).GetComponent<TextBoxTMP>();
        textBoxText = textBox.transform.GetChild(0).GetComponent<TextMeshPro>();

        saveButton.OnClick = new Button.ButtonClickedEvent();
        saveButton.OnClick.AddListener((UnityAction)(() =>
        {
            onSave.Invoke(textBoxText.text);
            this.BaseClose();
        }));

        closeButton.OnClick = new Button.ButtonClickedEvent();
        closeButton.OnClick.AddListener((UnityAction)(() =>
        {
            this.BaseClose();
        }));

        textBox.OnChange = new Button.ButtonClickedEvent();
        textBox.OnChange.AddListener((UnityAction)(() =>
        {
            textBoxText.text = textBoxText.text.Replace(" ", string.Empty);
        }));

        textBox.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            textBox.GiveFocus();
        }));

        foreach (var tmp in gameObject.GetComponentsInChildren<TextMeshPro>())
        {
            if (tmp.gameObject.transform.parent.name == "Textbox") continue;
            var text = tmp.text;
            tmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF RadialMenu Material\">{text}</font>";
        }
    }

    public static void CreatePopup(Action<string> saveAction)
    {
        // Because innerscuff doesnt account for Z value in Close animation
        var holder = new GameObject("PopupHolder")
        {
            transform =
            {
                parent = Camera.main!.transform,
            },
        };
        holder.transform.localPosition = new Vector3(0, -1f, -500);

        var gameObject = Instantiate(MiraAssets.PresetSavePopup.LoadAsset(), holder.transform);
        var popup = gameObject.GetComponent<SavePresetPopup>();
        popup.onSave = saveAction;
        popup.Begin(null);
    }
}

