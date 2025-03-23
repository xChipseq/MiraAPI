using AmongUs.Data;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(NameplatesTab))]
public static class NameplatesTabPatches
{
    private static readonly SortedList<string, List<NamePlateData>> SortedNameplates = new(new ControllableComparer<string>(["vanilla"], [], StringComparer.InvariantCulture));

    internal static void AddRange(IEnumerable<(string Key, NamePlateData Visor)> data)
    {
        foreach (var item in data)
        {
            if (!SortedNameplates.ContainsKey(item.Key)) SortedNameplates.Add(item.Key, []);
            SortedNameplates[item.Key].Add(item.Visor);
        }
    }

    [HarmonyPatch(nameof(NameplatesTab.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePrefix(NameplatesTab __instance)
    {
        __instance.plateId = HatManager.Instance.GetNamePlateById(DataManager.Player.Customization.namePlate).ProdId;

        if (!SortedNameplates.ContainsKey("Vanilla")) AddRange(DestroyableSingleton<HatManager>.Instance.GetUnlockedNamePlates().Select(x => ("Vanilla", x)));
        GenerateHats(__instance);

        return false;
    }

    private static void GenerateHats(NameplatesTab __instance)
    {
        foreach (ColorChip instanceColorChip in __instance.ColorChips) instanceColorChip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        __instance.scroller.Inner.GetComponentsInChildren<TextMeshPro>().Do(x => x.gameObject.Destroy());

        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);

        int hatIndex = 0;

        foreach ((string groupName, List<NamePlateData> visors) in SortedNameplates)
        {
            hatIndex = (hatIndex + 1) / 2 * 2; // yes it looks redundant, but consider hatindex = 0, symbolically it would be 4, the computer calculates 0.
            var text = Object.Instantiate(groupNameText, __instance.scroller.Inner);
            text.enabled = true;
            text.gameObject.transform.localScale = Vector3.one;
            text.GetComponent<TextTranslatorTMP>().Destroy();
            text.EnableStencilMasking();
            text.text = $"{groupName}";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3f;
            text.fontSizeMax = 3f;
            text.fontSizeMin = 0f;
            float xLerp = __instance.XRange.Lerp(0.5f);
            float yLerp = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
            text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);

            hatIndex += 2;
            foreach (var visor in visors.OrderBy(HatManager.Instance.allNamePlates.IndexOf))
            {
                float hatXposition = __instance.XRange.Lerp(hatIndex % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                float hatYposition = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
                GenerateColorChip(__instance, new Vector2(hatXposition, hatYposition), visor);
                hatIndex += 1;
            }
        }

        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIndex + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
        __instance.currentNameplateIsEquipped = true;
    }

    private static void GenerateColorChip(NameplatesTab __instance, Vector2 position, NamePlateData namePlate)
    {
        var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
        colorChip.gameObject.name = namePlate.ProductId;
        colorChip.Button.OnClick.AddListener((Action)(() => __instance.ClickEquip()));
        colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectNameplate(namePlate)));
        colorChip.Button.OnMouseOut.AddListener((Action)(() => __instance.SelectNameplate(HatManager.Instance.GetNamePlateById(DataManager.Player.Customization.NamePlate))));
        colorChip.Button.ClickMask = __instance.scroller.Hitbox;
        colorChip.ProductId = namePlate.ProdId;

        var x = (NamePlateViewData viewdata) =>
        {
            colorChip.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = viewdata?.Image;
            //(colorChip as NameplateChip).image.sprite = viewdata?.Image;
        };
        __instance.StartCoroutine(AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>(__instance, namePlate.GetAssetReference(), x));
        colorChip.transform.localPosition = new Vector3(position.x, position.y, -1f);
        colorChip.Tag = namePlate;
        __instance.ColorChips.Add(colorChip);
    }
}
