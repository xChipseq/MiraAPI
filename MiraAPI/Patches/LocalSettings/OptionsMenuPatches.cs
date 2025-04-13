using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.LocalSettings;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Patches.LocalSettings;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuPatches
{
    private static Dictionary<ModSettingsTab, TabGroup> tabs = [];
    
    /// <summary>
    /// Creates the tabs and their content
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    public static void StartPostfix(OptionsMenuBehaviour __instance)
    {
        float yOffset = 0;
        var i = 0;
        foreach (var settings in LocalSettingsManager.AllTabs)
        {
            // Tab button and it's appearance
            var tabButton = GameObject.Instantiate(__instance.Tabs[0], __instance.transform);
            tabButton.name = $"{settings.Title} Button";
            tabButton.transform.localPosition = new Vector3(2.4f, 2.1f - yOffset, 5.5f);
            tabButton.transform.localScale = new Vector3(1.25f, 1.25f, 1);
            tabButton.Button.color = settings.TabColor;
            tabs.Add(settings, tabButton);

            var text = tabButton.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
            text.GetComponent<TextTranslatorTMP>().Destroy(); // i hate text translators
            text.transform.localPosition = new Vector3(0.078f, 0, 0);
            text.transform.localScale = new Vector3(0.9f, 0.9f);
            text.alignment = TextAlignmentOptions.Right;
            text.text = $"<b>{settings.ShortTitle}</b>";
            
            var button = tabButton.GetComponent<PassiveButton>();
            var rollover = tabButton.Rollover;
            rollover.OverColor = settings.TabHoverColor;
            rollover.OutColor = settings.TabColor;
            
            var sprite = new GameObject("sprite").AddComponent<SpriteRenderer>();
            sprite.gameObject.layer = 5; // ui layer
            sprite.transform.SetParent(tabButton.transform);
            sprite.transform.localPosition = new Vector3(0.5f, 0, -2);
            sprite.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            sprite.sprite = settings.Icon;
            
            text.gameObject.SetActive(settings.Icon == null);   // Hide the text and show the sprite if there's an icon
            sprite.gameObject.SetActive(settings.Icon != null);

            // The actual tab object
            var tab = GameObject.Instantiate(__instance.transform.FindChild("GeneralTab"), __instance.transform);
            tab.name = $"{settings.Title} Tab";
            tab.transform.DestroyChildren();
            tab.gameObject.SetActive(false);
            tabButton.Content = tab.gameObject;
            
            var generalLabel = __instance.transform.FindChild("GeneralTab").FindChild("ControlGroup").FindChild("ControlText_TMP").gameObject; // Wacky way to get objects we'll be using
            var generalToggle = __instance.transform.FindChild("GeneralTab").FindChild("ChatGroup").FindChild("CensorChatButton").gameObject;
            var sfxSlider = __instance.transform.FindChild("GeneralTab").FindChild("SoundGroup").FindChild("SFXSlider").gameObject;
            
            Dictionary<string, List<IConfigEntrySetting>> entriesByGroup = new();
            foreach (var entry in settings.ConfigEntries) // Group the settings by their section
            {   
                string group = entry.BaseEntry.Definition.Section;
                if (!entriesByGroup.ContainsKey(group))
                    entriesByGroup.Add(group, []);
                
                entriesByGroup[group].Add(entry);
            }
            
            float contentOffset = 0;
            foreach (var pair in entriesByGroup) // Create the settings for each section
            {
                CreateLabel(generalLabel, tab.transform, pair.Key, ref contentOffset); // Section label
                var contentOrder = 1;
                var contentIndex = 1;

                foreach (var setting in pair.Value)
                {
                    switch (setting)
                    {
                        case ConfigEntryBoolSetting boolSetting:
                            var toggle = CreateToggle(generalToggle, tab.transform, boolSetting, ref contentOffset, ref contentOrder, contentIndex == pair.Value.Count);
                            contentIndex++;
                            break;
                        case ConfigEntryFloatSetting floatSetting:
                            var slider = CreateSlider(sfxSlider, tab.transform, floatSetting, ref contentOffset, ref contentOrder);
                            contentIndex++;
                            break;
                        case ConfigEntryIntSetting intSetting:
                            var intButton = CreateIntSetting(generalToggle, tab.transform, intSetting, ref contentOffset, ref contentOrder, contentIndex == pair.Value.Count);
                            contentIndex++;
                            break;
                        default:
                            if (setting.GetType().IsGenericType &&
                                setting.GetType().GetGenericTypeDefinition() == typeof(ConfigEntryEnumSetting<>))
                            {
                                dynamic enumSetting = setting;
                                var enumButton = CreateEnumSetting(generalToggle, tab.transform, enumSetting, ref contentOffset, ref contentOrder, contentIndex == pair.Value.Count) as PassiveButton;
                                contentIndex++;
                            }
                            break;
                    }
                }
            }

            int tabIndex = i; // Local copies of the tab variables so we can use them in the lambdas
            float tabOffset = yOffset;
            
            button.OnClick.AddListener((UnityAction)(() =>
            {
                __instance.OpenTabGroup(tabIndex + 10);
            }));
            button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                tabButton.transform.localPosition = new Vector3(3.55f, 2.1f - tabOffset, 5.5f);

                sprite.gameObject.SetActive(false);
                text.gameObject.SetActive(true);
                text.transform.localPosition = new Vector3(0.045f, 0, 0);
                text.maxVisibleCharacters = 9999;
                text.text = settings.Title;
            }));
            button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                tabButton.transform.localPosition = new Vector3(2.4f, 2.1f - tabOffset, 5.5f);

                if (settings.Icon != null)
                    text.gameObject.SetActive(false);
                sprite.gameObject.SetActive(true);
                text.transform.localPosition = new Vector3(0.1f, 0, 0);
                text.maxVisibleCharacters = 4;
                text.text = $"<b>{settings.ShortTitle}</b>";
            }));

            yOffset += 0.6f;
            i++;
        }
    }

    /// <summary>
    /// Modifies how opening tabs is handled for the custom ones to work
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.OpenTabGroup))]
    public static bool OpenTabGroupPrefix(OptionsMenuBehaviour __instance, ref int index)
    {
        __instance.Tabs.ToList().ForEach(x => x.Close()); // Close all vanilla tabs
        tabs.ToList().ForEach(CustomClose); // Close all mod tabs
        
        if (index >= 10) // Tabs with index 10 and above are the mod settings tabs
        {
            CustomOpen(tabs.ToList()[index - 10]);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Makes the custom tabs close when opening the options menu
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Open))]
    public static void OpenPostfix(OptionsMenuBehaviour __instance)
    {
        tabs.ToList().ForEach(CustomClose);
    }

    private static void CustomOpen(KeyValuePair<ModSettingsTab, TabGroup> pair)
    {
        // For some reason this method kept throwing null reference errors randomly so...
        if (pair.Value.Button)
            pair.Value.Button.color = pair.Key.TabHoverColor;
        
        if (pair.Value.Rollover)
            pair.Value.Rollover.OutColor = pair.Key.TabHoverColor;
        
        if (pair.Value.Content)
            pair.Value.Content.SetActive(true);
    }
    private static void CustomClose(KeyValuePair<ModSettingsTab, TabGroup> pair)
    {
        // The same here
        if (pair.Value.Button)
            pair.Value.Button.color = pair.Key.TabColor;
        
        if (pair.Value.Rollover)
            pair.Value.Rollover.OutColor = pair.Key.TabColor;
        
        if (pair.Value.Content)
            pair.Value.Content.SetActive(false);
    }

    public static TextMeshPro CreateLabel(GameObject template, Transform parent, string text, ref float offset)
    {
        var label = GameObject.Instantiate(template, parent);
        label.transform.localPosition = new Vector3(0, 1.85f - offset);
        label.GetComponent<TextTranslatorTMP>().Destroy();
        label.name = text;

        var tmp = label.GetComponent<TextMeshPro>();
        tmp.text = $"<b>{text}</b>";
        tmp.alignment = TextAlignmentOptions.Center;

        offset += 0.5f;
        return tmp;
    }

    public static ToggleButtonBehaviour CreateToggle(GameObject template, Transform parent, ConfigEntryBoolSetting setting, ref float offset, ref int order, bool last)
    {
        var toggle = GameObject.Instantiate(template, parent).GetComponent<ToggleButtonBehaviour>();
        var tmp = toggle.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        var passiveButton = toggle.GetComponent<PassiveButton>();
        var rollover = toggle.GetComponent<ButtonRolloverHandler>();
        toggle.gameObject.SetActive(true);

        if (last && order == 1)
            toggle.transform.localPosition = new Vector3(0, 1.85f - offset);
        else
            toggle.transform.localPosition = new Vector3(order == 1 ? -1.35f : 1.28f, 1.85f - offset);

        toggle.BaseText = CustomStringName.CreateAndRegister(setting.Name);
        toggle.UpdateText((bool)setting.Entry.BoxedValue);
        toggle.name = setting.Name;
        toggle.Background.color = setting.Entry.Value ? setting.OnColor : setting.OffColor;
        passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        rollover.OverColor = setting.OnColor;

        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            setting.Entry.Value = !setting.Entry.Value;
            toggle.UpdateText(setting.Entry.Value);
            toggle.Background.color = setting.Entry.Value ? setting.OnColor : setting.OffColor;
        }));
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!setting.Description.IsNullOrWhiteSpace())
                tmp.text = setting.Description;
        }));
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            toggle.UpdateText(setting.Entry.Value);
            toggle.Background.color = setting.Entry.Value ? setting.OnColor : setting.OffColor;
        }));

        order++;
        if (order > 2 && !last)
        {
            offset += 0.5f;
            order = 1;
        }
        if (last)
            offset += 0.6f;
        return toggle;
    }
    
    public static SlideBar CreateSlider(GameObject template, Transform parent, ConfigEntryFloatSetting setting, ref float offset, ref int order)
    {
        var slider = GameObject.Instantiate(template, parent).GetComponent<SlideBar>();
        var rollover = slider.GetComponent<ButtonRolloverHandler>();
        slider.Title = slider.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>(); // Why the hell slider has a title property that is not even assigned???
        slider.Title.GetComponent<TextTranslatorTMP>().Destroy();
        slider.gameObject.SetActive(true);
        
        if (order == 2)
            offset += 0.5f;
        slider.transform.localPosition = new Vector3(-2.12f, 1.85f - offset);
        slider.name = setting.Name;
        slider.Range = new FloatRange(-1.5f, 1.5f);
        slider.SetValue(Mathf.InverseLerp(setting.SliderRange.min, setting.SliderRange.max, setting.Entry.Value));
        rollover.OverColor = setting.SliderColor;
        slider.Title.text = $"{setting.Name}: <b>{setting.Entry.Value}</b>";
        
        slider.OnValueChange.AddListener((UnityAction)(() =>
        {
            setting.Entry.Value = setting.RoundValue
                ? Mathf.Round(Mathf.Lerp(setting.SliderRange.min, setting.SliderRange.max, slider.Value))
                : Mathf.Lerp(setting.SliderRange.min, setting.SliderRange.max, slider.Value);
            
            slider.Title.text = $"{setting.Name}: <b>{setting.Entry.Value}</b>";
        }));

        order = 1;
        offset += 0.5f;
        return slider;
    }
    
    public static PassiveButton CreateIntSetting(GameObject template, Transform parent, ConfigEntryIntSetting setting, ref float offset, ref int order, bool last)
    {
        var button = GameObject.Instantiate(template, parent).GetComponent<PassiveButton>();
        var tmp = button.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        var rollover = button.GetComponent<ButtonRolloverHandler>();
        button.GetComponent<ToggleButtonBehaviour>().Destroy();
        tmp.GetComponent<TextTranslatorTMP>().Destroy();
        button.gameObject.SetActive(true);

        if (last && order == 1)
            button.transform.localPosition = new Vector3(0, 1.85f - offset);
        else
            button.transform.localPosition = new Vector3(order == 1 ? -1.35f : 1.28f, 1.85f - offset);

        tmp.text = $"{setting.Name}: {setting.Entry.Value}";
        button.name = setting.Name;
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        rollover.OverColor = setting.Color;

        button.OnClick.AddListener((UnityAction)(() =>
        {
            int value = setting.Entry.Value;
            value++;
            if (value > setting.Range.max)
                value = setting.Range.min;

            setting.Entry.Value = value;
            tmp.text = $"{setting.Name}: {setting.Entry.Value}";
        }));
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!setting.Description.IsNullOrWhiteSpace())
                tmp.text = setting.Description;
        }));
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            tmp.text = $"{setting.Name}: {setting.Entry.Value}";
        }));

        order++;
        if (order > 2 && !last)
        {
            offset += 0.5f;
            order = 1;
        }
        if (last)
            offset += 0.6f;
        return button;
    }

    public static PassiveButton CreateEnumSetting<T>(GameObject template, Transform parent, ConfigEntryEnumSetting<T> setting, ref float offset, ref int order, bool last) where T : System.Enum
    {
        var button = GameObject.Instantiate(template, parent).GetComponent<PassiveButton>();
        var tmp = button.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        var rollover = button.GetComponent<ButtonRolloverHandler>();
        button.GetComponent<ToggleButtonBehaviour>().Destroy();
        tmp.GetComponent<TextTranslatorTMP>().Destroy();
        button.gameObject.SetActive(true);

        if (last && order == 1)
            button.transform.localPosition = new Vector3(0, 1.85f - offset);
        else
            button.transform.localPosition = new Vector3(order == 1 ? -1.35f : 1.28f, 1.85f - offset);

        tmp.text = $"{setting.Name}: {setting.EnumNames[setting.ValueIndex]}";
        button.name = setting.Name;
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        rollover.OverColor = setting.Color;

        button.OnClick.AddListener((UnityAction)(() =>
        {
            int value = setting.ValueIndex;
            value++;
            if (value > setting.EnumNames.Length-1)
                value = 0;

            setting.ValueIndex = value;
            setting.Entry.BoxedValue = setting.ValueIndex;
            tmp.text = $"{setting.Name}: {setting.EnumNames[setting.ValueIndex]}";
        }));
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!setting.Description.IsNullOrWhiteSpace())
                tmp.text = setting.Description;
        }));
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            tmp.text = $"{setting.Name}: {setting.EnumNames[setting.ValueIndex]}";
        }));

        order++;
        if (order > 2 && !last)
        {
            offset += 0.5f;
            order = 1;
        }
        if (last)
            offset += 0.6f;
        return button;
    }
}
