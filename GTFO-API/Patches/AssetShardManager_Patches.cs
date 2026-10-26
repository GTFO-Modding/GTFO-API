using System;
using System.Reflection;
using AssetShards;
using GTFO.API.Impl;
using GTFO.API.NativePatches;
using GTFO.API.Resources;
using HarmonyLib;
using Il2CppInterop.Runtime;

namespace GTFO.API.Patches;

[HarmonyPatch(typeof(AssetShardManager))]
internal class AssetShardManager_Patches
{
    [HarmonyPatch(nameof(AssetShardManager.Setup))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    public static void Setup_Postfix()
    {
        if (APIStatus.Asset.Created) return;
        APIStatus.CreateApi<AssetAPI_Impl>(nameof(APIStatus.Asset));
    }

    [HarmonyPatch]
    private static class GetLoadedAsset
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.FirstMethod(typeof(AssetShardManager), IsTargetMethod);
        }

        public static bool IsTargetMethod(MethodBase method)
        {
            return method.Name == nameof(AssetShardManager.GetLoadedAsset) && !method.IsGenericMethod;
        }

        public static bool Prefix(string assetPath, ref UnityEngine.Object __result)
        {
            __result = GetAsset(assetPath);
            return false;
        }
    }

    [HarmonyPatch(typeof(AssetShardManager_Native), nameof(AssetShardManager_Native.GetLoadedAsset__T__String_Boolean))]
    private static class GetLoadedAssetT
    {
        public static bool Prefix(IntPtr n_assetPath, ref IntPtr __result)
        {
            var assetPath = IL2CPP.Il2CppStringToManaged(n_assetPath);

            var asset = GetAsset(assetPath);
            if (asset != null)
                __result = asset.Pointer;
            else
                __result = IntPtr.Zero;

            return false;
        }
    }

    private static UnityEngine.Object GetAsset(string assetPath)
    {
        if (AssetAPI.AllCustomAssets.TryGet(assetPath, out var assetRef))
        {
            var loadedCustomAsset = assetRef.Asset;
            if (loadedCustomAsset == null)
                APILogger.Error("AssetShard", $"Custom Asset: '{assetPath}' did not loaded!");

            return loadedCustomAsset;
        }

        if (AssetShardManager.s_loadedAssetsLookup.TryGetValue(assetPath.ToUpper(), out var asset))
        {
            if (asset == null)
                APILogger.Error("AssetShard", $"Asset: '{assetPath}' exists, but value is null!");

            return asset;
        }

        APILogger.Error("AssetShard", $"Asset: '{assetPath}' did not loaded!");
        return null;
    }
}
