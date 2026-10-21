using System.Reflection;
using AssetShards;
using GTFO.API.Impl;
using GTFO.API.Resources;
using HarmonyLib;

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
                    var asset = assetRef.Asset; // Kinda ugly /shrug
                    APILogger.Verbose("Asset", $"AssetRequest: {asset.name}");
                }
            }
        }

        [HarmonyPatch]
        private static class GetLoadedAssetT
        {
            public static MethodBase TargetMethod()
            {
                var method = AccessTools.FirstMethod(typeof(AssetShardManager), IsTargetMethod);
                return method.MakeGenericMethod([typeof(UnityEngine.GameObject)]);
            }

            public static bool IsTargetMethod(MethodBase method)
            {
                return method.Name == nameof(AssetShardManager.GetLoadedAsset) && method.IsGenericMethod;
            }

            public static void Prefix(string assetPath)
            {
                if (AssetAPI.AllCustomAssets.TryGet(assetPath, out var assetRef))
                {
                    var asset = assetRef.Asset; // Kinda ugly /shrug
                    APILogger.Verbose("Asset", $"AssetRequest: {asset.name}");
                }
            }
        }
    }
}
