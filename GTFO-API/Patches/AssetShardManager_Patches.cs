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
            __result = AssetAPI.Internal_GetLoadedAsset(assetPath, logErrors: true, logExceptions: true);
            return false;
        }
    }

    [HarmonyPatch(typeof(AssetShardManager_Native), nameof(AssetShardManager_Native.GetLoadedAsset__T__String_Boolean))]
    private static class GetLoadedAssetT
    {
        public static bool Prefix(IntPtr n_assetPath, ref IntPtr __result)
        {
            var assetPath = IL2CPP.Il2CppStringToManaged(n_assetPath);

            var asset = AssetAPI.Internal_GetLoadedAsset(assetPath, logErrors: true, logExceptions: true);
            if (asset != null)
                __result = asset.Pointer;
            else
                __result = IntPtr.Zero;

            return false;
        }
    }
}
