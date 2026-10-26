using System;
using System.Reflection;
using AssetShards;
using GTFO.API.Impl;
using GTFO.API.NativePatches;
using GTFO.API.Resources;
using HarmonyLib;
using Il2CppInterop.Runtime;

namespace GTFO.API.Patches
{
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

            public static void Prefix(string assetPath)
            {
                if (AssetAPI.AllCustomAssets.TryGet(assetPath, out var assetRef))
                {
                    assetRef.GetAsset();
                }
            }
        }

        [HarmonyPatch(typeof(AssetShardManager_Native), nameof(AssetShardManager_Native.GetLoadedAsset__T__String_Boolean))]
        private static class GetLoadedAssetT
        {
            public static void Prefix(IntPtr n_assetPath)
            {
                var assetPath = IL2CPP.Il2CppStringToManaged(n_assetPath);
                if (AssetAPI.AllCustomAssets.TryGet(assetPath, out var assetRef))
                {
                    assetRef.GetAsset();
                }
            }
        }
    }
}
