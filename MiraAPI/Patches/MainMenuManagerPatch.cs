using System;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TMPro;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(MainMenuManager))]

public class MainMenuManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MainMenuManager.Start))]
    public static void StartPatch()
    {
        TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(CustomRoleManager.SpriteAsset);
        /*TMP_Text.OnSpriteAssetRequest += DelegateSupport.ConvertDelegate<Il2CppSystem.Func<int, string, TMP_SpriteAsset>>(new Func<int, string, TMP_SpriteAsset>((index, str) =>
        {
            var asset = Helpers.RegisteredSpriteAssets.First(x => x.name.Contains(str));
            return asset != null ? asset : null;
        }));*/
    }
}
