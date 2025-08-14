﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(LobbyViewSettingsPane))]
public static class LobbyViewPanePatches
{
    private static int SelectedModIdx { get; set; }

    private static MiraPluginInfo? SelectedMod => SelectedModIdx == 0
        ? null
        : MiraPluginManager.Instance.RegisteredPlugins[SelectedModIdx - 1];

    private static PassiveButton? ModifiersTabButton { get; set; }

    private static StringNames ModifiersTabName { get; } = CustomStringName.CreateAndRegister("ModifiersTab");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
    public static void AwakePatch(LobbyViewSettingsPane __instance)
    {
        __instance.gameModeText.transform.localPosition = new Vector3(-2.3f, 2.4f, -2);
        __instance.gameModeText.GetComponent<TextTranslatorTMP>().Destroy();

        // create modifiers button
        ModifiersTabButton = Object.Instantiate(__instance.rolesTabButton, __instance.rolesTabButton.transform.parent);
        ModifiersTabButton.buttonText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        ModifiersTabButton.name = "ModifiersTabButton";
        var pos = ModifiersTabButton.transform.localPosition;
        pos.x = 2.1f;
        ModifiersTabButton.transform.localPosition = pos;
        ModifiersTabButton.buttonText.text = "Modifiers";
        ModifiersTabButton.OnClick = new Button.ButtonClickedEvent();
        ModifiersTabButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(ModifiersTabName);
            }));
        ModifiersTabButton.gameObject.SetActive(SelectedModIdx!=0);

        // Create the next button
        var nextButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.3f);
        nextButton.transform.localPosition = new Vector3(-5.4f, 2.4f, -2f);
        nextButton.transform.localScale = new Vector3(3, 3, 2);
        nextButton.name = "RightArrowButton";

        var normal = nextButton.transform.FindChild("Normal").GetComponentInChildren<SpriteRenderer>();
        normal.transform.localPosition = new Vector3(0, 0f, 0.3f);
        normal.sprite = MiraAssets.NextButton.LoadAsset();

        var hover = nextButton.transform.FindChild("Hover").GetComponentInChildren<SpriteRenderer>();
        hover.transform.localPosition = new Vector3(0, 0f, 0.3f);
        hover.sprite = MiraAssets.NextButtonActive.LoadAsset();

        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                SelectedModIdx += 1;
                if (SelectedModIdx > MiraPluginManager.Instance.RegisteredPlugins.Length)
                {
                    SelectedModIdx = 0;
                }

                Refresh(__instance);
            }));

        // Create the back button
        var backButton = Object.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-6.3f, 2.4f, -2f);
        backButton.name = "LeftArrowButton";
        backButton.transform.FindChild("Normal").gameObject.GetComponentInChildren<SpriteRenderer>().flipX
            = backButton.transform.FindChild("Hover").gameObject.GetComponentInChildren<SpriteRenderer>().flipX
                = true;

        var passiveButton2 = backButton.gameObject.GetComponent<PassiveButton>();
        passiveButton2.OnClick = new Button.ButtonClickedEvent();
        passiveButton2.OnClick.AddListener(
            (UnityAction)(() =>
            {
                SelectedModIdx -= 1;
                if (SelectedModIdx < 0)
                {
                    SelectedModIdx = MiraPluginManager.Instance.RegisteredPlugins.Length;
                }

                Refresh(__instance);
            }));
    }

    private static void Refresh(LobbyViewSettingsPane menu)
    {
        ModifiersTabButton?.gameObject.SetActive(SelectedModIdx != 0);
        menu.gameModeText.text = SelectedMod?.PluginInfo.Metadata.Name ?? "Default";
        menu.RefreshTab();
        menu.scrollBar.ScrollToTop();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.ChangeTab))]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.RefreshTab))]
    // CHANGED BECAUSE OF INLINING
    public static void SetTabPatch(LobbyViewSettingsPane __instance)
    {
        if (__instance.currentTab != ModifiersTabName || SelectedMod == null)
        {
            ModifiersTabButton?.SelectButton(false);
            return;
        }

        __instance.taskTabButton.SelectButton(false);
        __instance.rolesTabButton.SelectButton(false);

        ModifiersTabButton?.SelectButton(true);

        var filteredGroups = SelectedMod.InternalOptionGroups
            .Where(x => x.GroupVisible() && (x.ShowInModifiersMenu || (x.OptionableType != null && x.OptionableType.IsAssignableTo(typeof(BaseModifier)))));

        DrawOptions(__instance, filteredGroups);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawNormalTab))]
    public static bool DrawNormalTabPatch(LobbyViewSettingsPane __instance)
    {
        if (SelectedModIdx == 0 || SelectedMod == null)
        {
            return true;
        }

        if (__instance.currentTab == ModifiersTabName)
        {
            return false;
        }

        var filteredGroups = SelectedMod.InternalOptionGroups
            .Where(x => x is { ShowInModifiersMenu: false, OptionableType: null } && x.GroupVisible());

        DrawOptions(__instance, filteredGroups);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawRolesTab))]
    public static bool DrawRolesTabPatch(LobbyViewSettingsPane __instance)
    {
        if (SelectedModIdx == 0)
        {
            return true;
        }

        if (__instance.currentTab == ModifiersTabName)
        {
            return false;
        }

        DrawRolesTab(__instance);
        return false;
    }

    private static void DrawOptions(LobbyViewSettingsPane menu, IEnumerable<AbstractOptionGroup> groups)
    {
        var num = 1.44f;

        var groupArray = groups.Where(x => x.GroupVisible() && x.Options.Any(y => y.Visible())).ToArray();

        foreach (var group in groupArray)
        {
            var categoryHeaderMasked = Object.Instantiate(
                menu.categoryHeaderOrigin,
                menu.settingsContainer,
                true);

            categoryHeaderMasked.SetHeader(StringNames.Name, 61);
            categoryHeaderMasked.Title.text = group.GroupName;
            categoryHeaderMasked.transform.localScale = Vector3.one;
            categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
            menu.settingsInfo.Add(categoryHeaderMasked.gameObject);
            num -= 1.05f;

            var i = 0;

            foreach (var option in group.Options)
            {
                if (!option.Visible())
                {
                    continue;
                }

                var viewSettingsInfoPanel = Object.Instantiate(
                    menu.infoPanelOrigin,
                    menu.settingsContainer,
                    true);

                viewSettingsInfoPanel.transform.localScale = Vector3.one;
                float num2;
                if (i % 2 == 0)
                {
                    num2 = -8.95f;
                    if (i > 0)
                    {
                        num -= 0.85f;
                    }
                }
                else
                {
                    num2 = -3f;
                }

                viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);

                var data = option.Data;

                if (data.Type == OptionTypes.Checkbox)
                {
                    viewSettingsInfoPanel.SetInfoCheckbox(
                        data.Title,
                        61,
                        Mathf.Approximately(option.GetFloatData(), 1));
                }
                else
                {
                    viewSettingsInfoPanel.SetInfo(data.Title, data.GetValueString(option.GetFloatData()), 61);
                }

                menu.settingsInfo.Add(viewSettingsInfoPanel.gameObject);
                i++;
            }

            num -= 0.85f;
        }

        menu.scrollBar.SetYBoundsMax(-num - 2);
    }

    private static void DrawRolesTab(LobbyViewSettingsPane instance)
    {
        if (SelectedMod == null)
        {
            return;
        }

        var num = 0.95f;
        var num2 = -6.53f;
        var categoryHeaderMasked =
            Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
        categoryHeaderMasked.SetHeader(StringNames.RoleQuotaLabel, 61);
        categoryHeaderMasked.transform.localScale = Vector3.one;
        categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, 1.26f, -2f);
        instance.settingsInfo.Add(categoryHeaderMasked.gameObject);

        var list = new List<Type>();

        var roleGroups = SelectedMod.InternalRoles.Values.OfType<ICustomRole>()
            .ToLookup(x => x.RoleOptionsGroup);

        // sort the groups by priority
        var sortedRoleGroups = roleGroups
            .OrderBy(x => x.Key.Priority)
            .ThenBy(x => x.Key.Name);

        foreach (var grouping in sortedRoleGroups)
        {
            if (!grouping.Any() || grouping.All(x => x.Configuration.HideSettings))
            {
                continue;
            }

            var group = grouping.Key;

            var name = group.Name switch
            {
                "Crewmate" => StringNames.CrewmateRolesHeader,
                "Impostor" => StringNames.ImpostorRolesHeader,
                _ => CustomStringName.CreateAndRegister(group.Name),
            };

            var categoryHeaderRoleVariant = Object.Instantiate(instance.categoryHeaderRoleOrigin, instance.settingsContainer, true);
            categoryHeaderRoleVariant.SetHeader(name, 61);

            if (name is not (StringNames.CrewmateRolesHeader or StringNames.ImpostorRolesHeader))
            {
                var veryDarkColor = group.Color.DarkenColor(.35f);
                categoryHeaderRoleVariant.Title.color = veryDarkColor;
                categoryHeaderRoleVariant.Background.color = group.Color;
            }

            categoryHeaderRoleVariant.transform.localScale = Vector3.one;
            categoryHeaderRoleVariant.transform.localPosition = new Vector3(0.09f, num, -2f);
            instance.settingsInfo.Add(categoryHeaderRoleVariant.gameObject);
            num -= 0.696f;

            foreach (var customRole in grouping)
            {
                if (customRole.Configuration.HideSettings)
                {
                    continue;
                }

                var roleBehaviour = customRole as RoleBehaviour;
                if (roleBehaviour == null)
                {
                    continue;
                }

                var chancePerGame =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role);
                var numPerGame =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleBehaviour.Role);

                var viewSettingsInfoPanelRoleVariant =
                    Object.Instantiate(
                        instance.infoPanelRoleOrigin,
                        instance.settingsContainer,
                        true);
                viewSettingsInfoPanelRoleVariant.transform.localScale = Vector3.one;
                viewSettingsInfoPanelRoleVariant.transform.localPosition = new Vector3(num2, num, -2f);

                var advancedRoleOptions = SelectedMod.InternalOptionGroups
                    .Where(x => x.OptionableType == customRole.GetType())
                    .SelectMany(x => x.Options)
                    .ToList();

                if (numPerGame > 0 && advancedRoleOptions.Count > 0)
                {
                    list.Add(customRole.GetType());
                }

                viewSettingsInfoPanelRoleVariant.SetInfo(
                    roleBehaviour.NiceName,
                    numPerGame,
                    chancePerGame,
                    61,
                    customRole.RoleColor,
                    customRole.Configuration.Icon?.LoadAsset() ?? MiraAssets.Empty.LoadAsset(),
                    true);
                viewSettingsInfoPanelRoleVariant.iconSprite.transform.localScale = new Vector3(0.365f, 0.365f, 1f);
                viewSettingsInfoPanelRoleVariant.iconSprite.transform.localPosition = new Vector3(0.7144f, -0.028f, -2);

                viewSettingsInfoPanelRoleVariant.titleText.color =
                    viewSettingsInfoPanelRoleVariant.chanceTitle.color =
                        viewSettingsInfoPanelRoleVariant.chanceBackground.color =
                            viewSettingsInfoPanelRoleVariant.background.color =
                                customRole.RoleColor.FindAlternateColor();
                instance.settingsInfo.Add(viewSettingsInfoPanelRoleVariant.gameObject);
                num -= 0.664f;
            }
        }

        if (list.Count > 0)
        {
            var categoryHeaderMasked2 =
                Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
            categoryHeaderMasked2.SetHeader(StringNames.RoleSettingsLabel, 61);
            categoryHeaderMasked2.transform.localScale = Vector3.one;
            categoryHeaderMasked2.transform.localPosition = new Vector3(-9.77f, num, -2f);
            instance.settingsInfo.Add(categoryHeaderMasked2.gameObject);
            num -= 1.7f;
            var num3 = 0f;
            for (var k = 0; k < list.Count; k++)
            {
                float num4;
                if (k % 2 == 0)
                {
                    num4 = -5.8f;
                    if (k > 0)
                    {
                        num -= num3 + 0.59f;
                        num3 = 0f;
                    }
                }
                else
                {
                    num4 = 0.14999962f;
                }

                var advancedRoleViewPanel =
                    Object.Instantiate(instance.advancedRolePanelOrigin, instance.settingsContainer, true);
                advancedRoleViewPanel.transform.localScale = Vector3.one;
                advancedRoleViewPanel.transform.localPosition = new Vector3(num4, num, -2f);
                var num5 = SetUpAdvancedRoleViewPanel(advancedRoleViewPanel, list[k], 0.59f, 61);

                if (num5 > num3)
                {
                    num3 = num5;
                }

                instance.settingsInfo.Add(advancedRoleViewPanel.gameObject);
            }
        }

        instance.scrollBar.SetYBoundsMax(-num);
    }

    private static float SetUpAdvancedRoleViewPanel(
        AdvancedRoleViewPanel viewPanel,
        Type roleType,
        float spacingY,
        int maskLayer)
    {
        if (SelectedMod == null)
        {
            return 0;
        }

        var role = SelectedMod.InternalRoles.Values.FirstOrDefault(x => x.GetType() == roleType);

        if (role == null)
        {
            return 0;
        }

        if (role is not ICustomRole customRole)
        {
            return 0;
        }

        viewPanel.header.SetHeader(
            role.StringName,
            maskLayer,
            role.TeamType == RoleTeamTypes.Crewmate,
            customRole.Configuration.Icon != null ? customRole.Configuration.Icon.LoadAsset() : MiraAssets.Empty.LoadAsset());
        viewPanel.header.icon.transform.localScale = new Vector3(0.465f, 0.465f, 1f);
        viewPanel.divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);

        var num = viewPanel.yPosStart;
        var num2 = 1.08f;

        var filteredOptions = SelectedMod.InternalOptionGroups
            .Where(x => x.OptionableType == roleType)
            .SelectMany(x=>x.Options)
            .ToList();

        for (var i = 0; i < filteredOptions.Count; i++)
        {
            var option = filteredOptions[i];
            var baseGameSetting = option.Data;
            var viewSettingsInfoPanel = Object.Instantiate(viewPanel.infoPanelOrigin, viewPanel.transform, true);
            viewSettingsInfoPanel.transform.localScale = Vector3.one;
            viewSettingsInfoPanel.transform.localPosition = new Vector3(viewPanel.xPosStart, num, -2f);

            var value = option.GetFloatData();

            if (baseGameSetting.Type == OptionTypes.Checkbox)
            {
                viewSettingsInfoPanel.SetInfoCheckbox(baseGameSetting.Title, maskLayer, value > 0f);
            }
            else
            {
                viewSettingsInfoPanel.SetInfo(baseGameSetting.Title, baseGameSetting.GetValueString(value), maskLayer);
            }

            num -= spacingY;
            if (i > 0)
            {
                num2 += 0.8f;
            }
        }

        return num2;
    }
}
