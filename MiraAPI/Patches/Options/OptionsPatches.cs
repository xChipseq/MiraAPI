using HarmonyLib;
using Il2CppSystem;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Options;

[HarmonyPatch]
public static class OptionsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Initialize))]
    public static bool ToggleInit(ToggleOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.TitleText.text =
            TranslationController.Instance.GetString(__instance.Title, Array.Empty<Object>());

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
    // UpdateValue was inlined.
    public static bool ToggleUpdate(ToggleOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.CheckMark.enabled = !__instance.CheckMark.enabled;
        __instance.OnValueChanged.Invoke(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.UpdateValue))]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
    public static bool UpdateValuePrefix(OptionBehaviour __instance)
    {
        return !__instance.IsCustom();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.AdjustButtonsActiveState))]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.AdjustButtonsActiveState))]
    public static bool AdjustButtonsPrefix(OptionBehaviour __instance)
    {
        return !__instance.IsCustom();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
    public static bool NumberInit(NumberOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.TitleText.text = TranslationController.Instance.GetString(__instance.Title, Array.Empty<Object>());
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
    public static bool NumberIncrease(NumberOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.Value += __instance.Increment;
        if (__instance.Value > __instance.ValidRange.max)
        {
            __instance.Value = __instance.ValidRange.min;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
    public static bool NumberDecrease(NumberOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.Value -= __instance.Increment;
        if (__instance.Value < __instance.ValidRange.min)
        {
            __instance.Value = __instance.ValidRange.max;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public static bool StringOptionIncrease(StringOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.Value += 1;
        if (__instance.Value >= __instance.Values.Length)
        {
            __instance.Value = 0;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public static bool StringOptionDecrease(StringOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.Value -= 1;
        if (__instance.Value < 0)
        {
            __instance.Value = __instance.Values.Length - 1;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public static bool StringInit(StringOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.TitleText.text = TranslationController.Instance.GetString(__instance.Title, Array.Empty<Object>());
        __instance.ValueText.text = TranslationController.Instance.GetString(__instance.Values[__instance.Value], Array.Empty<Object>());

        return false;
    }
}
