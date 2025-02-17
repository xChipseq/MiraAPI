using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RoleSettingMenuPatches
{
    private static Dictionary<RoleGroup, bool> RoleGroupHidden { get; } = [];
    private static List<CategoryHeaderEditRole> CategoryHeaderEditRoles { get; } = [];
    private static List<RoleOptionSetting> RoleOptionSettings { get; } = [];

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.SetQuotaTab))]
    public static bool PatchStart(RolesSettingsMenu __instance)
    {
        __instance.roleChances = new Il2CppSystem.Collections.Generic.List<RoleOptionSetting>();
        __instance.advancedSettingChildren = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();

        var maskBg = __instance.scrollBar.transform.FindChild("MaskBg");
        var hitbox = __instance.scrollBar.transform.FindChild("Hitbox");

        if (GameSettingMenuPatches.SelectedModIdx == 0)
        {
            __instance.AllButton.transform.parent.gameObject.SetActive(true);
            __instance.AllButton.gameObject.SetActive(true);
            __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 0.657f, -4);
            maskBg.localPosition = new Vector3(1.5353f, -.5734f, -.1f);
            maskBg.localScale = new Vector3(6.6811f, 3.3563f, 0.5598f);
            hitbox.localPosition = new Vector3(0.3297f, -.2333f, 4f);
            hitbox.localScale = new Vector3(1, 1, 1);
            return true;
        }

        var num = 0.522f;

        __instance.AllButton.transform.parent.gameObject.SetActive(false);
        __instance.AllButton.gameObject.SetActive(false);
        __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 1.5261f, -4);
        maskBg.localPosition = new Vector3(1.5353f, -1.0607f, -.1f);
        maskBg.localScale = new Vector3(6.6811f, 4.1563f, 0.5598f);
        hitbox.localPosition = new Vector3(0.3297f, -.6333f, 4f);
        hitbox.localScale = new Vector3(1, 1.2f, 1);

        var num3 = 0;

        var roleGroups = GameSettingMenuPatches.SelectedMod?.CustomRoles.Values.OfType<ICustomRole>()
            .ToLookup(x => x.Configuration.RoleGroup);

        if (roleGroups is null)
        {
            return true;
        }

        // sort the groups by priority
        var sortedRoleGroups = roleGroups
            .OrderBy(x => x.Key.Priority)
            .ThenBy(x => x.Key.Name);

        foreach (var grouping in sortedRoleGroups)
        {
            if (!grouping.Any())
            {
                continue;
            }

            RoleGroupHidden.TryAdd(grouping.Key, false);

            var name = grouping.Key.Name switch
            {
                "Crewmate" => StringNames.CrewmateRolesHeader,
                "Impostor" => StringNames.ImpostorRolesHeader,
                _ => CustomStringName.CreateAndRegister(grouping.Key.Name),
            };

            var categoryHeaderEditRole = Object.Instantiate(
                __instance.categoryHeaderEditRoleOrigin,
                Vector3.zero,
                Quaternion.identity,
                __instance.RoleChancesSettings.transform);
            categoryHeaderEditRole.SetHeader(name, 20);
            categoryHeaderEditRole.transform.localPosition = new Vector3(4.986f, num, -2f);
            categoryHeaderEditRole.transform.Find("LabelSprite").transform.localScale = new Vector3(1.3f, 1, 0.5529f);
            categoryHeaderEditRole.transform.Find("QuotaHeader").gameObject.SetActive(!RoleGroupHidden[grouping.Key]);

            CategoryHeaderEditRoles.Add(categoryHeaderEditRole);

            num -= 0.522f;

            var label = RoleGroupHidden[grouping.Key]
                ? "(Click to open)"
                : "(Click to close)";
            var newText = Object.Instantiate(categoryHeaderEditRole.Title, categoryHeaderEditRole.transform);
            newText.text = $"<size=50%>{label}</size>";
            newText.transform.localPosition = new Vector3(-3.3425f, -0.1706f, -1);
            newText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();

            if (!RoleGroupHidden[grouping.Key])
            {
                foreach (var role in grouping)
                {
                    if (role is not RoleBehaviour roleBehaviour)
                    {
                        continue;
                    }

                    var option = CreateQuotaOption(__instance, roleBehaviour, ref num, num3);
                    if (option is not null)
                    {
                        RoleOptionSettings.Add(option);
                        num3++;
                    }
                }
            }

            var boxCol = categoryHeaderEditRole.gameObject.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(3, 0.25f);
            boxCol.offset = new Vector2(-2.05f, -.175f);

            var headerBtn = categoryHeaderEditRole.gameObject.AddComponent<PassiveButton>();
            headerBtn.ClickSound = __instance.BackButton.GetComponent<PassiveButton>().ClickSound;
            headerBtn.OnMouseOver = new UnityEvent();
            headerBtn.OnMouseOut = new UnityEvent();
            headerBtn.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    RoleGroupHidden[grouping.Key] = !RoleGroupHidden[grouping.Key];
                    foreach (var header in CategoryHeaderEditRoles)
                    {
                        header.gameObject.Destroy();
                    }
                    CategoryHeaderEditRoles.Clear();
                    foreach (var option in RoleOptionSettings)
                    {
                        option.gameObject.Destroy();
                    }
                    RoleOptionSettings.Clear();
                    __instance.SetQuotaTab();
                }));
            headerBtn.SetButtonEnableState(true);

            if (!RoleGroupHidden[grouping.Key])
            {
                num -= 0.4f;
            }
        }
        __instance.scrollBar.CalculateAndSetYBounds(__instance.roleChances.Count + 5, 1f, 6f, 0.43f);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.Update))]
    public static bool UpdatePatch()
    {
        return GameSettingMenuPatches.SelectedModIdx == 0;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RolesSettingsMenu.OpenChancesTab))]
    public static void OpenChancesTabPostfix(RolesSettingsMenu __instance)
    {
        __instance.scrollBar.CalculateAndSetYBounds(__instance.roleChances.Count + 5, 1f, 6f, 0.43f);
        __instance.scrollBar.ScrollToTop();
    }

    private static void ValueChanged(OptionBehaviour obj)
    {
        var roleSetting = obj.Cast<RoleOptionSetting>();
        var role = roleSetting.Role as ICustomRole;
        if (role is null or { Configuration.HideSettings: true })
        {
            return;
        }

        try
        {
            if (role.Configuration.MaxRoleCount != 0)
            {
                role.SetCount(roleSetting.RoleMaxCount);
            }

            if (role.Configuration.CanModifyChance)
            {
                role.SetChance(roleSetting.RoleChance);
            }
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Warning(e);
        }

        roleSetting.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);
        HudManager.Instance.Notifier.AddRoleSettingsChangeMessage(
            roleSetting.Role.StringName,
            roleSetting.RoleMaxCount,
            roleSetting.RoleChance,
            roleSetting.Role.TeamType,
            false);

        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<SyncRoleOptionsRpc>.Instance.Send(PlayerControl.LocalPlayer, [role.GetNetData()], true);
        }

        GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
    }

    private static void CreateAdvancedSettings(RolesSettingsMenu __instance, RoleBehaviour role)
    {
        foreach (var option in __instance.advancedSettingChildren)
        {
            Object.Destroy(option.gameObject);
        }

        __instance.advancedSettingChildren.Clear();

        var num = -0.872f;

        var filteredOptions = GameSettingMenuPatches.SelectedMod?.Options.Where(x => x.AdvancedRole == role.GetType()) ?? [];

        foreach (var option in filteredOptions)
        {
            if (option.AdvancedRole is not null && option.AdvancedRole != role.GetType())
            {
                continue;
            }

            var newOpt = option.CreateOption(
                __instance.checkboxOrigin,
                __instance.numberOptionOrigin,
                __instance.stringOptionOrigin,
                __instance.AdvancedRolesSettings.transform);

            newOpt.transform.localPosition = new Vector3(2.17f, num, -2f);
            newOpt.SetClickMask(__instance.ButtonClickMask);

            SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in componentsInChildren)
            {
                renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
            }

            foreach (var fontMat in newOpt.GetComponentsInChildren<TextMeshPro>(true).Select(x => x.fontMaterial))
            {
                fontMat.SetFloat(ShaderID.StencilComp, 3f);
                fontMat.SetFloat(ShaderID.Stencil, 20);
            }

            newOpt.LabelBackground.enabled = false;
            __instance.advancedSettingChildren.Add(newOpt);

            num += -0.45f;
            newOpt.Initialize();
        }

        __instance.scrollBar.CalculateAndSetYBounds(__instance.advancedSettingChildren.Count + 3, 1f, 6f, 0.45f);
        __instance.scrollBar.ScrollToTop();
    }

    private static void ChangeTab(RoleBehaviour role, RolesSettingsMenu __instance)
    {
        if (role is not ICustomRole customRole)
        {
            Logger<MiraApiPlugin>.Error($"Role {role.NiceName} is not a custom role.");
            return;
        }

        __instance.roleDescriptionText.text = customRole.RoleLongDescription;
        __instance.roleTitleText.text = TranslationController.Instance.GetString(
            role.StringName,
            new Il2CppReferenceArray<Il2CppSystem.Object>(0));
        __instance.roleScreenshot.sprite = Sprite.Create(
            customRole.Configuration.OptionsScreenshot.LoadAsset().texture,
            new Rect(0, 0, 370, 230),
            Vector2.one / 2,
            100);
        __instance.roleScreenshot.drawMode = SpriteDrawMode.Sliced;
        __instance.roleHeaderSprite.color = customRole.RoleColor;
        __instance.roleHeaderText.color = customRole.RoleColor.GetAlternateColor();

        var bg = __instance.AdvancedRolesSettings.transform.Find("Background");
        bg.localPosition = new Vector3(1.4041f, -7.08f, 0);
        bg.GetComponent<SpriteRenderer>().size = new Vector2(89.4628f, 100);

        CreateAdvancedSettings(__instance, role);

        foreach (var optionBehaviour in __instance.advancedSettingChildren)
        {
            if (optionBehaviour.IsCustom())
            {
                continue;
            }

            optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                optionBehaviour.SetAsPlayer();
            }
        }

        __instance.RoleChancesSettings.SetActive(false);
        __instance.AdvancedRolesSettings.SetActive(true);
        __instance.RefreshChildren();
    }

    private static RoleOptionSetting? CreateQuotaOption(RolesSettingsMenu __instance, RoleBehaviour role, ref float yPos, int index)
    {
        if (role is not ICustomRole customRole)
        {
            Logger<MiraApiPlugin>.Error($"Role {role.NiceName} is not a custom role.");
            return null;
        }

        var roleOptionSetting = Object.Instantiate(
            __instance.roleOptionSettingOrigin,
            Vector3.zero,
            Quaternion.identity,
            __instance.RoleChancesSettings.transform);
        roleOptionSetting.transform.localPosition = new Vector3(-0.15f, yPos, -2f);

        roleOptionSetting.SetRole(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions, role, 20);
        roleOptionSetting.labelSprite.color = customRole.RoleColor;
        roleOptionSetting.OnValueChanged = new Action<OptionBehaviour>(ValueChanged);
        roleOptionSetting.SetClickMask(__instance.ButtonClickMask);
        __instance.roleChances.Add(roleOptionSetting);

        roleOptionSetting.titleText.transform.localPosition = new Vector3(-0.5376f, -0.2923f, 0f);
        roleOptionSetting.titleText.color = customRole.RoleColor.GetAlternateColor();
        roleOptionSetting.titleText.horizontalAlignment = HorizontalAlignmentOptions.Left;

        if (GameSettingMenuPatches.SelectedMod is null ||
            GameSettingMenuPatches.SelectedMod.Options.Exists(
                x => x.AdvancedRole != null && x.AdvancedRole.IsInstanceOfType(role)))
        {
            var newButton = Object.Instantiate(roleOptionSetting.buttons[0], roleOptionSetting.transform);
            newButton.name = "ConfigButton";
            newButton.transform.localPosition = new Vector3(0.4473f, -0.3f, -2f);
            newButton.transform.FindChild("Text_TMP").gameObject.DestroyImmediate();
            newButton.activeSprites.Destroy();

            var btnRend = newButton.transform.FindChild("ButtonSprite").GetComponent<SpriteRenderer>();
            btnRend.sprite = MiraAssets.Cog.LoadAsset();

            var passiveButton = newButton.GetComponent<GameOptionButton>();
            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.interactableColor = btnRend.color = customRole.RoleColor.GetAlternateColor();
            passiveButton.interactableHoveredColor = Color.white;

            passiveButton.OnClick.AddListener((UnityAction)(() => { ChangeTab(role, __instance); }));
        }

        if (customRole.Configuration is { MaxRoleCount: 0 })
        {
            roleOptionSetting.CountMinusBtn.gameObject.SetActive(false);
            roleOptionSetting.CountPlusBtn.gameObject.SetActive(false);
        }

        if (!customRole.Configuration.CanModifyChance)
        {
            roleOptionSetting.ChanceMinusBtn.gameObject.SetActive(false);
            roleOptionSetting.ChancePlusBtn.gameObject.SetActive(false);
        }

        if (index < GameSettingMenuPatches.SelectedMod?.CustomRoles.Count - 1)
        {
            yPos += -0.43f;
        }

        return roleOptionSetting;
    }
}
