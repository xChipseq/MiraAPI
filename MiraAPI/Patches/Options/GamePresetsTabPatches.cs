using System.Linq;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

internal static class GamePresetsTabPatches
{
    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.Start))]
    public static class GamePresetsTabStartPatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(GamePresetsTab __instance)
        {
            var prefab = GameSettingMenu.Instance.GameSettingsButton;
            if (prefab == null)
            {
                Logger<MiraApiPlugin>.Error("GameSettingsButton prefab is null");
                return;
            }

            var saveButton = Object.Instantiate(prefab, __instance.transform);
            saveButton.buttonText.text = "Save Settings";
            if (saveButton.buttonText.TryGetComponent<TextTranslatorTMP>(out var textTranslator))
            {
                textTranslator.DestroyImmediate();
            }
            saveButton.OnClick = new Button.ButtonClickedEvent();
            saveButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    
                }));

            var holder = new GameObject("PresetHolder");
            holder.transform.SetParent(__instance.transform, false);
            holder.transform.localPosition = new Vector3(-1.7f, 1.7f, 0);
            var arrange = holder.AddComponent<GridArrange>();

            foreach (var preset in MiraPluginManager.Instance.RegisteredPlugins.SelectMany(x=>x.Presets))
            {
                var button = Object.Instantiate(prefab, holder.transform);
                button.buttonText.text = preset.Name;
                if (button.buttonText.TryGetComponent<TextTranslatorTMP>(out var tmp))
                {
                    tmp.DestroyImmediate();
                }

                button.OnClick = new Button.ButtonClickedEvent();
                button.OnClick.AddListener(
                    (UnityAction)(() =>
                    {
                        Logger<MiraApiPlugin>.Error($"Loading preset {preset.Name}");
                        preset.LoadPreset();
                    }));
                button.gameObject.SetActive(false);
                preset.PresetButton = button.gameObject;
            }

            arrange.Alignment = GridArrange.StartAlign.Right;
            arrange.CellSize = new Vector2(2.5f, -.55f);
            arrange.Start();
            arrange.ArrangeChilds();
        }
    }

    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.OnEnable))]
    public static class GamePresetsOnEnablePatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(GamePresetsTab __instance)
        {
            Logger<MiraApiPlugin>.Error("OnEnable called");
            __instance.StandardPresetButton.gameObject.SetActive(GameSettingMenuPatches.SelectedModIdx == 0);
            __instance.SecondPresetButton.gameObject.SetActive(GameSettingMenuPatches.SelectedModIdx == 0);

            foreach (var mod in MiraPluginManager.Instance.RegisteredPlugins)
            {
                foreach (var button in mod.Presets.Select(x=>x.PresetButton))
                {
                    if (button != null)
                    {
                        button.SetActive(mod == GameSettingMenuPatches.SelectedMod);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.OnDisable))]
    public static class GamePresetsOnDisablePatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix()
        {
            Logger<MiraApiPlugin>.Error("OnDisable called");

            foreach (var mod in MiraPluginManager.Instance.RegisteredPlugins)
            {
                foreach (var button in mod.Presets.Select(x=>x.PresetButton))
                {
                    if (button != null)
                    {
                        button.SetActive(false);
                    }
                }
            }
        }
    }
}
