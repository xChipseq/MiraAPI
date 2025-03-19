using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(HatsTab))]
public static class HatsTabPatches
{
    private static SortedList<string, List<HatData>> sortedHats = [];

    [HarmonyPatch(nameof(HatsTab.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePrefix(HatsTab __instance)
    {
        __instance.currentHat = HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat);
        var allHats = HatManager.Instance.GetUnlockedHats().ToImmutableList();

        if (sortedHats.Count == 0)
        {
            var comparer = new ControllableComparer<string>(["vanilla"], [], StringComparer.InvariantCulture);
            sortedHats = new SortedList<string, List<HatData>>(comparer);
            foreach (var hat in allHats)
            {
                if (!sortedHats.ContainsKey(hat.StoreName)) sortedHats[hat.StoreName] = [];
                sortedHats[hat.StoreName].Add(hat);
            }
        }

        GenerateHats(__instance);

        return false;
    }

    private static void GenerateHats(HatsTab __instance)
    {
        foreach (ColorChip instanceColorChip in __instance.ColorChips) instanceColorChip.gameObject.Destroy();
        __instance.ColorChips.Clear();
        __instance.scroller.Inner.GetComponentsInChildren<TextMeshPro>().Do(x => x.gameObject.Destroy());

        var groupNameText = __instance.GetComponentInChildren<TextMeshPro>(false);

        int hatIndex = 0;

        foreach ((string groupName, List<HatData> hats) in sortedHats)
        {
            hatIndex = (hatIndex + 4) / 5 * 5; // yes it looks redundant, but consider hatindex = 0, symbolically it would be 4, the computer calculates 0.
            var text = Object.Instantiate(groupNameText, __instance.scroller.Inner);
            text.gameObject.transform.localScale = Vector3.one;
            text.GetComponent<TextTranslatorTMP>().Destroy();
            text.text = $"{groupName}";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3f;
            text.fontSizeMax = 3f;
            text.fontSizeMin = 0f;
            float xLerp = __instance.XRange.Lerp(0.5f);
            float yLerp = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
            text.transform.localPosition = new Vector3(xLerp, yLerp, -1f);

            hatIndex += 5;
            foreach (var hat in hats.OrderBy(HatManager.Instance.allHats.IndexOf))
            {
                float hatXposition = __instance.XRange.Lerp(hatIndex % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                float hatYposition = __instance.YStart - hatIndex / __instance.NumPerRow * __instance.YOffset;
                GenerateColorChip(__instance, new Vector2(hatXposition, hatYposition), hat);
                hatIndex += 1;
            }
        }

        __instance.scroller.ContentYBounds.max = -(__instance.YStart - (hatIndex + 1) / __instance.NumPerRow * __instance.YOffset) - 3f;
        __instance.currentHatIsEquipped = true;
    }

    private static void GenerateColorChip(HatsTab __instance, Vector2 position, HatData hat)
    {
        var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
        colorChip.gameObject.name = hat.ProductId;
        colorChip.Button.OnClick.AddListener((Action)(() => __instance.ClickEquip()));
        colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectHat(hat)));
        colorChip.Button.OnMouseOut.AddListener((Action)(() => __instance.SelectHat(HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat))));
        colorChip.Inner.SetHat(hat, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
        colorChip.Inner.transform.GetChild(0).GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        colorChip.transform.localPosition = new Vector3(position.x, position.y, -1f);
        colorChip.Inner.transform.localPosition = hat.ChipOffset + new Vector2(0f, -0.3f);
        colorChip.Tag = hat;
        __instance.ColorChips.Add(colorChip);
    }
}
