using System.IO;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Presets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

internal static class GamePresetsTabPatches
{
    private static GameObject _saveButton = null!;
    private static GameObject _refreshButton = null!;
    private static GameObject _presetHolder = null!;
    private static GridArrange _arrange = null!;

    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.Start))]
    public static class GamePresetsTabStartPatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(GamePresetsTab __instance)
        {
            Logger<MiraApiPlugin>.Error("GamePresetsTab Start called");
            var prefab = GameSettingMenu.Instance.GameSettingsButton;
            if (prefab == null)
            {
                Logger<MiraApiPlugin>.Error("GameSettingsButton prefab is null");
                return;
            }

            // create the save button
            var saveButton = Object.Instantiate(prefab, __instance.transform);
            saveButton.gameObject.name = "SaveButton";
            saveButton.gameObject.transform.localPosition = new Vector3(3.4f, 1.7f, -2);
            _saveButton = saveButton.gameObject;

            // set the button text and alignment
            var saveText = saveButton.buttonText;
            saveText.text = "Save";
            saveText.GetComponent<TextTranslatorTMP>().Destroy();
            saveText.alignment = TextAlignmentOptions.Center;
            saveText.transform.parent.localPosition = new Vector3(
                -.525f,
                saveText.transform.parent.localPosition.y,
                saveText.transform.parent.localPosition.z);

            // adjust the button size and colliders
            foreach (var collider in saveButton.Colliders)
            {
                if (collider.TryCast<BoxCollider2D>() is { } col)
                {
                    col.size = new Vector2(col.size.x / 2, col.size.y);
                }
            }

            foreach (var rend in saveButton.GetComponentsInChildren<SpriteRenderer>(true))
            {
                rend.size = new Vector2(rend.size.x / 2, rend.size.y);
            }

            var refreshButton = Object.Instantiate(saveButton, __instance.transform);
            _refreshButton = refreshButton.gameObject;
            refreshButton.gameObject.name = "RefreshButton";
            refreshButton.gameObject.transform.localPosition = new Vector3(3.4f, 1.1f, -2);
            var refreshText = refreshButton.buttonText;
            refreshText.text = "Refresh";

            // add the click event to save the preset
            saveButton.OnClick = new Button.ButtonClickedEvent();
            var input = saveButton.gameObject.AddComponent<InputBox>();
            saveButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    if (GameSettingMenuPatches.SelectedModIdx == 0)
                    {
                        return;
                    }

                    input.CreateDialog(
                        "Preset Name",
                        name =>
                        {
                            if (GameSettingMenuPatches.SelectedMod == null)
                            {
                                return;
                            }

                            var presetFile = new ConfigFile(
                                Path.Join(
                                    PresetManager.PresetDirectory,
                                    GameSettingMenuPatches.SelectedMod.PluginId,
                                    $"{name}.cfg"),
                                true);
                            foreach (var option in GameSettingMenuPatches.SelectedMod.InternalOptions.Where(
                                         x => x.IncludeInPreset))
                            {
                                option.SaveToPreset(presetFile, true);
                            }

                            presetFile.Save();
                        });
                }));

            // add the click event to refresh the presets
            refreshButton.OnClick = new Button.ButtonClickedEvent();
            refreshButton.OnClick.AddListener((UnityAction)Refresh);

            _saveButton.SetActive(false);
            _refreshButton.SetActive(false);

            _presetHolder = new GameObject("PresetHolder");
            _presetHolder.transform.SetParent(__instance.transform, false);
            _presetHolder.transform.localPosition = new Vector3(-1.7f, 1.7f, 0);
            _arrange = _presetHolder.AddComponent<GridArrange>();
            _arrange.Alignment = GridArrange.StartAlign.Right;
            _arrange.CellSize = new Vector2(2.5f, -.55f);
            _arrange.MaxColumns = 2;

            var watcher = new FileSystemWatcher
            {
                Path = PresetManager.PresetDirectory,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = "*.cfg",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };
            watcher.Changed += (_, _) => { Refresh(); };
        }

        private static void Refresh()
        {
            Logger<MiraApiPlugin>.Error("Refreshing presets");
            foreach (var mod in MiraPluginManager.Instance.RegisteredPlugins)
            {
                PresetManager.LoadPresets(mod);
            }

            GameSettingMenu.Instance.ChangeTab(0, false); // refresh the tab
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

            // Show the save and refresh buttons
            if (_saveButton != null)
            {
                _saveButton.SetActive(GameSettingMenuPatches.SelectedModIdx != 0);
            }
            if (_refreshButton != null)
            {
                _refreshButton.SetActive(GameSettingMenuPatches.SelectedModIdx != 0);
            }

            var prefab = GameSettingMenu.Instance.GameSettingsButton;

            foreach (var preset in MiraPluginManager.Instance.RegisteredPlugins.SelectMany(x => x.Presets))
            {
                var button = Object.Instantiate(prefab, _presetHolder.transform);
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
            _arrange.Start();
            _arrange.ArrangeChilds();
        }
    }

    [HarmonyPatch(typeof(GamePresetsTab), nameof(GamePresetsTab.OnDisable))]
    public static class GamePresetsOnDisablePatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix()
        {
            Logger<MiraApiPlugin>.Error("OnDisable called");

            // Hide the save and refresh buttons
            if (_saveButton != null)
            {
                _saveButton.SetActive(false);
            }
            if (_refreshButton != null)
            {
                _refreshButton.SetActive(false);
            }

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
