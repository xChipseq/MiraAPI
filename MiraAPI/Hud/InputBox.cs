using System;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Hud;

[RegisterInIl2Cpp]
public class InputBox : MonoBehaviour
{
    private GameObject dialog;
    private InputField inputField;

    public void CreateDialog(string title, Action<string> onSubmit)
    {
        // Create Canvas
        var canvasObj = new GameObject("DialogCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.layer = LayerMask.NameToLayer("UI");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Panel
        var panelObj = new GameObject("DialogPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        var panelRect = panelObj.AddComponent<RectTransform>();
        var img = panelObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.6f); // Semi-transparent black
        img.raycastTarget = true;

        panelRect.sizeDelta = new Vector2(200, 200);
        panelRect.anchoredPosition = Vector2.zero;

        // Create Title Text
        var titleText = CreateText(panelObj, title);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontSize = 24;
        titleText.color = Color.white;
        var titleRect = titleText.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(200, 40);
        titleRect.anchoredPosition = new Vector2(0, 70);

        // Create InputField
        var inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(panelObj.transform);
        var inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(150, 50);
        inputRect.anchoredPosition = new Vector2(0, 30);

        inputField = inputObj.AddComponent<InputField>();
        inputField.textComponent = CreateText(inputObj);
        inputField.placeholder = CreateText(inputObj, "Enter text here...");

        // Create Submit Button
        var buttonObj = new GameObject("SubmitButton");
        buttonObj.transform.SetParent(panelObj.transform);
        var button = buttonObj.AddComponent<Button>();
        var buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(100, 40);
        buttonRect.anchoredPosition = new Vector2(0, -50);

        var buttonText = CreateText(buttonObj, "Submit");
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        button.onClick.AddListener(
            (UnityAction)(() =>
            {
                onSubmit(inputField.text);
                DestroyImmediate(canvasObj);
            }));
    }

    private static Text CreateText(GameObject parent, string textContent="")
    {
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform);
        var text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = textContent;
        text.color = Color.white;
        text.fontSize = 20;

        var rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);
        rect.anchoredPosition = Vector2.zero;
        return text;
    }
}
