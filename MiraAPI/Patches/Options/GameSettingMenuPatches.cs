using System;
using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
internal static class GameSettingMenuPatches
{
    public static int SelectedModIdx { get; private set; }

    public static MiraPluginInfo? SelectedMod { get; private set; }

    private static TextMeshPro? _text;

    private static Vector3 _roleBtnOgPos;

    private static GameOptionsMenu? _modifiersTab;
    private static PassiveButton? _modifiersButton;
    private static PassiveButton? _smallRolesButton;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    public static void ChangeTabPostfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            _modifiersTab?.gameObject.SetActive(tabNum == 3);
            _modifiersButton?.SelectButton(tabNum == 3);
            _smallRolesButton?.SelectButton(tabNum == 2);
        }

        if (previewOnly)
        {
            return;
        }

        _modifiersButton?.SelectButton(tabNum == 3);
        _smallRolesButton?.SelectButton(tabNum == 2);
    }

    /// <summary>
    /// Prefix for the <see cref="GameSettingMenu.Start"/> method. Sets up the custom options.
    /// </summary>
    /// <param name="__instance">The GameSettingMenu instance.</param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        _roleBtnOgPos = __instance.RoleSettingsButton.transform.localPosition;
        __instance.transform.FindChild("GameSettingsLabel").gameObject.SetActive(false);

        var helpThing = __instance.transform.FindChild("What Is This?");
        var tmpText = Object.Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        tmpText.GetComponent<TextTranslatorTMP>().Destroy();
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, 1.5399f, -2);

        _text = tmpText.GetComponent<TextMeshPro>();
        _text.fontSizeMax = 3.2f;
        _text.overflowMode = TextOverflowModes.Overflow;

        _text.alignment = TextAlignmentOptions.Center;

        var nextButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.transform.localPosition = new Vector3(-2.2663f, 1.5272f, -25f);
        nextButton.name = "RightArrowButton";
        nextButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButton.LoadAsset();
        nextButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButtonActive.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                SelectedModIdx += 1;
                if (SelectedModIdx > MiraPluginManager.Instance.RegisteredPlugins().Length)
                {
                    SelectedModIdx = 0;
                }

                UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
            }));

        var backButton = Object.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-4.4209f, 1.5272f, -25f);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX =
            backButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener(
            (UnityAction)(() =>
            {
                SelectedModIdx -= 1;
                if (SelectedModIdx < 0)
                {
                    SelectedModIdx = MiraPluginManager.Instance.RegisteredPlugins().Length;
                }

                UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
            }));

        // clone game settings tab for modifiers
        _modifiersTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        _modifiersTab.name = "MODIFIERS TAB";

        // create button for modifiers
        __instance.RoleSettingsButton.gameObject.SetActive(false);
        _smallRolesButton = Object.Instantiate(
            __instance.RoleSettingsButton,
            __instance.RoleSettingsButton.transform.parent);
        var pos = new Vector3(
            -3.65f,
            _smallRolesButton.transform.localPosition.y,
            _smallRolesButton.transform.localPosition.z);
        _smallRolesButton.transform.localPosition = pos;
        _smallRolesButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(2, false);
            }));
        _smallRolesButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(2, true);
            }));

        var roleText = _smallRolesButton.buttonText;
        roleText.text = "Roles";
        roleText.GetComponent<TextTranslatorTMP>().Destroy();
        roleText.alignment = TextAlignmentOptions.Center;
        roleText.transform.parent.localPosition = new Vector3(
            -.525f,
            roleText.transform.parent.localPosition.y,
            roleText.transform.parent.localPosition.z);

        foreach (var collider in _smallRolesButton.Colliders)
        {
            if (collider.TryCast<BoxCollider2D>() is { } col)
            {
                col.size = new Vector2(col.size.x / 2, col.size.y);
            }
        }

        foreach (var rend in _smallRolesButton.GetComponentsInChildren<SpriteRenderer>(true))
        {
            rend.size = new Vector2(rend.size.x / 2, rend.size.y);
        }

        _modifiersButton = Object.Instantiate(_smallRolesButton, _smallRolesButton.transform.parent);
        _modifiersButton.OnClick = new ButtonClickedEvent();
        _modifiersButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(3, false);
            }));
        _modifiersButton.OnMouseOver = new UnityEvent();
        _modifiersButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(3, true);
            }));

        _modifiersButton.buttonText.text = "Modifiers";
        pos.x = -2.27f;
        _modifiersButton.transform.localPosition = pos;
        _modifiersButton.name = "ModifiersButton";

        UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
    }

    private static void UpdateText(GameSettingMenu menu, GameOptionsMenu settings, RolesSettingsMenu roles)
    {
        if (_text is not null && SelectedModIdx == 0)
        {
            _text.text = "Default";
            _text.fontSizeMax = 3.2f;
        }
        else if (_text is not null)
        {
            _text.fontSizeMax = 2.3f;
            SelectedMod = MiraPluginManager.Instance.RegisteredPlugins()[SelectedModIdx - 1];

            var name = SelectedMod.MiraPlugin.OptionsTitleText;
            _text.text = name[..Math.Min(name.Length, 25)];
        }

        _modifiersButton.gameObject.SetActive(false);
        _smallRolesButton.gameObject.SetActive(false);
        menu.RoleSettingsButton.gameObject.SetActive(false);
        if (SelectedModIdx != 0 &&
            SelectedMod?.OptionGroups.Exists(x => x.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true) == true)
        {
            _modifiersButton.gameObject.SetActive(true);
            _smallRolesButton.gameObject.SetActive(true);
        }
        else
        {
            menu.RoleSettingsButton.gameObject.SetActive(true);
            if (SelectedModIdx == 0 && _modifiersTab.gameObject.active)
            {
                menu.ChangeTab(0, false);
            }
        }

        //menu.RoleSettingsButton.transform.localPosition = _roleBtnOgPos;
        //menu.GameSettingsButton.gameObject.SetActive(true);
        //menu.RoleSettingsButton.gameObject.SetActive(true);

        // TODO: reimplement this
        /*
        if (SelectedModIdx != 0)
        {
            if (SelectedMod?.Options.Where(x=>x.AdvancedRole==null).ToList().Count == 0)
            {
                menu.GameSettingsButton.gameObject.SetActive(false);
            }

            if (SelectedMod?.CustomRoles.Count == 0)
            {
                menu.RoleSettingsButton.gameObject.SetActive(false);
            }

            if (!menu.GameSettingsButton.gameObject.active && menu.RoleSettingsButton.gameObject.active)
            {
                menu.RoleSettingsButton.transform.localPosition = menu.GameSettingsButton.transform.localPosition;
            }
        }*/

        if (roles.roleChances != null && SelectedMod != null && SelectedMod.CustomRoles.Count != 0)
        {
            if (roles.advancedSettingChildren is not null)
            {
                foreach (var child in roles.advancedSettingChildren)
                {
                    child.gameObject.DestroyImmediate();
                }

                roles.advancedSettingChildren.Clear();
                roles.advancedSettingChildren = null;
            }

            foreach (var header in roles.RoleChancesSettings.transform
                         .GetComponentsInChildren<CategoryHeaderEditRole>())
            {
                header.gameObject.DestroyImmediate();
            }

            foreach (var roleChance in roles.roleChances)
            {
                roleChance.gameObject.DestroyImmediate();
            }

            roles.roleChances.Clear();
            roles.AdvancedRolesSettings.gameObject.SetActive(false);
            roles.RoleChancesSettings.gameObject.SetActive(true);
            roles.SetQuotaTab();
            roles.scrollBar.ScrollToTop();
        }

        if (settings.Children != null && SelectedMod?.OptionGroups.Count != 0)
        {
            foreach (var child in settings.Children)
            {
                if (child.TryCast<GameOptionsMapPicker>())
                {
                    continue;
                }

                if (!child.gameObject)
                {
                    continue;
                }

                child.gameObject.DestroyImmediate();
            }

            foreach (var header in settings.settingsContainer.GetComponentsInChildren<CategoryHeaderMasked>())
            {
                header.gameObject.DestroyImmediate();
            }

            settings.Children.Clear();
            settings.Children = null;

            settings.Initialize();
            settings.scrollBar.ScrollToTop();
        }
    }
}
