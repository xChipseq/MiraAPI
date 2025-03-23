using System;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace MiraAPI.Hud.Tools;

[RegisterInIl2Cpp]
public class TextInput : MonoBehaviour
{
    public TextBoxTMP textBox;

    public Action<string>? OnTextChanged;

    public delegate bool ValidateInput(string input);
    public ValidateInput? Validate;

    public static void Create(Action<string>? onTextChanged = null, ValidateInput? validate = null)
    {
        var source = FindObjectOfType<NameTextBehaviour>(true);
        var input = Instantiate(source);
        var inputBox = input.gameObject.AddComponent<TextInput>();
        inputBox.textBox = source.nameSource;
        inputBox.OnTextChanged = onTextChanged;
        inputBox.Validate = validate;

        inputBox.textBox.OnChange = new Button.ButtonClickedEvent();
        inputBox.textBox.OnChange.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            var text = inputBox.textBox.text;
            if (inputBox.Validate != null && !inputBox.Validate(text))
            {
                return;
            }

            inputBox.OnTextChanged?.Invoke(text);
        }));
    }
}
