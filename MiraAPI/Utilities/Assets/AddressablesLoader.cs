using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// The component for loading Addressables, and cosmetics.
/// </summary>
public class AddressablesLoader
{
    private static readonly List<(string Location, string ProviderSuffix)> CatalogLocations = [];
    private static readonly List<string> LoadedLocations = [];

    private static readonly List<string> RegisteredHatKeys = [];
    private static readonly List<string> RegisteredVisorKeys = [];
    private static readonly List<string> RegisteredNameplateKeys = [];
    private static readonly List<string> RegisteredSkinKeys = [];

    /// <summary>
    /// Registers a specific addressables package to load asynchronously at the start of the game, when possible.
    /// </summary>
    /// <param name="location">The location, remote or otherwise, of the addressables.</param>
    /// <param name="providerSuffix">The suffix of the provider for an addressables package.</param>
    public static void RegisterToLoad(string location, string providerSuffix = "")
    {
        CatalogLocations.Add((location, providerSuffix));
    }

    public static void RegisterHats(string addressables_key)
    {
        RegisteredHatKeys.Add(addressables_key);
    }

    public static void RegisterSkins(string addressables_key)
    {
        RegisteredSkinKeys.Add(addressables_key);
    }

    public static void RegisterNameplates(string addressables_key)
    {
        RegisteredNameplateKeys.Add(addressables_key);
    }

    public static void RegisterVisors(string addressables_key)
    {
        RegisteredVisorKeys.Add(addressables_key);
    }

    internal static void LoadAll()
    {
        foreach (var (location, providerSuffix) in CatalogLocations)
        {
            Coroutines.Start(CoLoadAddressables(location, providerSuffix));
        }

        Coroutines.Start(LoadCosmetics());
    }

    internal static IEnumerator CoLoadAddressables(string location, string suffix = "")
    {
        while (!AmongUsClient.Instance) yield return null;
        // Load the local/remote content catalog
        var catalogOperation = Addressables.LoadContentCatalog(location, suffix);
        yield return catalogOperation;

        // Check for errors
        if (catalogOperation.Status != AsyncOperationStatus.Succeeded)
        {
            Logger<MiraApiPlugin>.Error($"Failed to load catalog {location}.");
        }

        Logger<MiraApiPlugin>.Info($"Loaded addressables {location}.");
        LoadedLocations.Add(location);
    }

    [HideFromIl2Cpp]
    internal static IEnumerator LoadCosmetics()
    {
        while (!AmongUsClient.Instance || CatalogLocations.Select(x=>x.Location).Any(x=>!LoadedLocations.Contains(x))) yield return null;

        var hatBehaviours = DiscoverData<HatData>(RegisteredHatKeys);
        var skinBehaviours = DiscoverData<SkinData>(RegisteredSkinKeys);
        var namePlateBehaviours = DiscoverData<NamePlateData>(RegisteredNameplateKeys);
        var visorBehaviours = DiscoverData<VisorData>(RegisteredVisorKeys);

        var hatData = new List<HatData>();
        hatData.AddRange(DestroyableSingleton<HatManager>.Instance.allHats);
        hatData.ForEach(x => x.StoreName = "Vanilla");
        DestroyableSingleton<HatManager>.Instance.allHats = PrepareArray(hatData, hatBehaviours);

        var skinData = new List<SkinData>();
        skinData.AddRange(DestroyableSingleton<HatManager>.Instance.allSkins);
        DestroyableSingleton<HatManager>.Instance.allSkins = PrepareArray(skinData, skinBehaviours);

        var visorData = new List<VisorData>();
        visorData.AddRange(DestroyableSingleton<HatManager>.Instance.allVisors);
        DestroyableSingleton<HatManager>.Instance.allVisors = PrepareArray(visorData, visorBehaviours);

        var namePlateData = new List<NamePlateData>();
        namePlateData.AddRange(DestroyableSingleton<HatManager>.Instance.allNamePlates);
        DestroyableSingleton<HatManager>.Instance.allNamePlates = PrepareArray(namePlateData, namePlateBehaviours);
    }

    private static T[] PrepareArray<T>(List<T> data, List<T> behaviours) where T : CosmeticData
    {
        var count = data.Count;
        for (int i = 0; i < behaviours.Count; i++)
        {
            behaviours[i].displayOrder = count + i;
            data.Add(behaviours[i]);
        }
        return data.ToArray();
    }

    private static List<T> DiscoverData<T>(List<string> tags)
    {
        var behaviours = new List<T>();

        foreach (var tag in tags)
        {
            try
            {
                var all_locations = Addressables.LoadResourceLocationsAsync(tag).WaitForCompletion();
                var assets = Addressables.LoadAssetsAsync<T>(all_locations, null, false).WaitForCompletion();
                var array = new Il2CppSystem.Collections.Generic.List<T>(assets.Pointer);
                behaviours.AddRange(array.ToArray());
            }
            catch
            {
                Logger<MiraApiPlugin>.Error($"Failed to find tag {tag}");
            }
        }
        return behaviours;
    }
}
