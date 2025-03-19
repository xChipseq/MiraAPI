using HarmonyLib;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiraAPI.Patches.Menu;

[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatches
{
    [HarmonyPatch(nameof(MainMenuManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix()
    {
        AddressablesLoader.LoadAll();
    }
}
