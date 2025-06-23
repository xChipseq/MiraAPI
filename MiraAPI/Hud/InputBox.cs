using System;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Hud;

/// <summary>
/// Simple input box for user input.
/// </summary>
[RegisterInIl2Cpp]
public class InputBox : MonoBehaviour
{
    public InputField inputField = null!;

    public void CreateDialog(string title, string placeholder, string submitText, Action<string> onSubmit)
    {
        var canvasObj = new GameObject("DialogCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.layer = LayerMask.NameToLayer("UI");

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000; // Ensure the canvas is on top of other UI elements

        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        var panelObj = new GameObject("DialogPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        var panelRect = panelObj.AddComponent<RectTransform>();
        var img = panelObj.AddComponent<Image>();

        img.color = new Color(1, 1, 1, 0.9f);
        img.raycastTarget = true;
        img.sprite = MiraAssets.RoundedBox.LoadAsset();
        img.type = Image.Type.Sliced;

        panelRect.sizeDelta = new Vector2(300, 200);
        panelRect.anchoredPosition = Vector2.zero;

        var titleText = CreateText(panelObj, title);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;

        var titleRect = titleText.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(280, 50);
        titleRect.anchoredPosition = new Vector2(0, 70);

        var inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(panelObj.transform);
        var inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(220, 40);
        inputRect.anchoredPosition = new Vector2(0, 20);

        var inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(1, 1, 1, 0.8f);

        inputField = inputObj.AddComponent<InputField>();
        inputField.textComponent = CreateText(inputObj, string.Empty, 18, Color.black);
        inputField.placeholder = CreateText(inputObj, placeholder, 18, new Color(0.5f, 0.5f, 0.5f));

        var buttonObj = new GameObject("SubmitButton");
        buttonObj.transform.SetParent(panelObj.transform);
        var button = buttonObj.AddComponent<Button>();
        var buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(150, 50);
        buttonRect.anchoredPosition = new Vector2(0, -50);

        var buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(1f, 1f, 1f, 1f);
        buttonImg.sprite = MiraAssets.RoundedBox.LoadAsset();
        buttonImg.type = Image.Type.Sliced;

        var buttonText = CreateText(buttonObj, submitText, 20, Color.white);
        buttonText.alignment = TextAnchor.MiddleCenter;

        var colors = button.colors;
        colors.highlightedColor = Color.green;
        colors.pressedColor = MiraAssets.AcceptedTeal;
        button.colors = colors;

        button.onClick.AddListener((UnityAction)(() =>
        {
            onSubmit(inputField.text);
            DestroyImmediate(canvasObj);
        }));

        inputField.ActivateInputField();
    }

    private static Text CreateText(GameObject parent, string textContent = "", int fontSize = 20, Color? textColor = null, Font? font = null)
    {
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform);

        var text = textObj.AddComponent<Text>();
        text.font = font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = textContent;
        text.color = textColor ?? Color.white;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;

        var rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);
        rect.anchoredPosition = Vector2.zero;
        return text;
    }
}
